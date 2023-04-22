using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MatroskaBatchToolBox.Properties;
using Palmtree;

namespace MatroskaBatchToolBox
{
    [Guid("F6CDDD48-C454-4CF1-B454-92533E5638DE")]
    public class Program
    {
        private enum ActionMode
        {
            None = 0,
            NormalizeAudio,
            ConvertVideo,
        }

        private const string _optionStringNormalizeAudio = "--normalize-audio";
        private const string _optionStringResizeResolution = "--convert-video";
        private static readonly string _applicationUniqueId;
        private static readonly object _lockConsoleObject;
        private static readonly TimeSpan _maximumTimeForProgressUpdate;
        private static string _previousProgressText;
        private static bool _cancelRequested;
        private static bool _completed;

        static Program()
        {
            _applicationUniqueId = $"{nameof(MatroskaBatchToolBox)}.{typeof(Program).GUID}";
            _lockConsoleObject = new object();
            _maximumTimeForProgressUpdate = TimeSpan.FromMinutes(1);
            _previousProgressText = "";
            _cancelRequested = false;
            _completed = false;
        }

        public static void Main(string[] args)
        {
            // このプロセスでは Ctrl+C を無視する。
            TinyConsole.CancelKeyPress += (sender, e) => e.Cancel = true;

            // コマンドの入出力エンコーディングを UTF8 にする
            TinyConsole.InputEncoding = Encoding.UTF8;
            TinyConsole.OutputEncoding = Encoding.UTF8;

            TinyConsole.Title = Process.GetCurrentProcess().ProcessName;

            try
            {
                var actionMode = ActionMode.None;
                foreach (var arg in args)
                {
                    if (arg == _optionStringNormalizeAudio)
                    {
                        actionMode = ActionMode.NormalizeAudio;
                        break;
                    }

                    if (arg == _optionStringResizeResolution)
                    {
                        actionMode = ActionMode.ConvertVideo;
                        break;
                    }
                }

                if (actionMode == ActionMode.None)
                    actionMode = ActionMode.NormalizeAudio;

                var applicationLock = new Mutex(false, _applicationUniqueId);
                if (!applicationLock.WaitOne(0, false))
                {
                    TinyConsole.ForegroundColor = ConsoleColor.Red;
                    TinyConsole.WriteLine(string.Format(Resource.AlreadyRunnningMessasgeText, nameof(MatroskaBatchToolBox)));
                    TinyConsole.ResetColor();
                    return;
                }

                TinyConsole.WriteLine(
                    actionMode switch
                    {
                        ActionMode.NormalizeAudio => Resource.AudioNormalizationProcessStartMessageText,
                        ActionMode.ConvertVideo => Resource.VideoConversionStartMessageText,
                        _ => throw Validation.GetFailErrorException($"Unexpected {nameof(ActionMode)} value: {actionMode}"),
                    });
                TinyConsole.WriteLine();

                _ = Task.Run(() =>
                {
                    while (true)
                    {
                        var keyInfo = TinyConsole.ReadKey(true);
                        if (keyInfo.Key == ConsoleKey.Q)
                        {
                            ExternalCommand.AbortExternalCommands();
                            lock (_lockConsoleObject)
                            {
                                if (!_completed)
                                {
                                    _cancelRequested = true;
                                    TinyConsole.CursorVisible = ConsoleCursorVisiblity.NormalMode;
                                    TinyConsole.WriteLine();
                                    TinyConsole.WriteLine();
                                    var color = TinyConsole.ForegroundColor;
                                    TinyConsole.ForegroundColor = ConsoleColor.Yellow;
                                    TinyConsole.WriteLine(Resource.Q_KeyPressedMessasgeText);
                                    TinyConsole.WriteLine();
                                    TinyConsole.ForegroundColor = color;
                                }
                            }

                            break;
                        }
                    }
                });

                var localSettingsCache = new LocalSettingsCache();

                var progressState = new ProgressState(CreateSourceFileList(args, actionMode));
                progressState.WriteProgressText(PrintProgress);

                var degreeOfParallelism = Settings.GlobalSettings.DegreeOfParallelism;
                if (degreeOfParallelism > Environment.ProcessorCount)
                    degreeOfParallelism = Environment.ProcessorCount;
                if (degreeOfParallelism < 1)
                    degreeOfParallelism = 1;

                var workerTasks = new List<Task>();
                for (var count = 0; count < degreeOfParallelism; ++count)
                    workerTasks.Add(Task.Run(worker));

                foreach (var workerTask in workerTasks)
                {
                    while (!workerTask.Wait(_maximumTimeForProgressUpdate))
                        progressState.WriteProgressText(PrintProgress);
                }

                progressState.WriteProgressText(PrintProgress);

                lock (_lockConsoleObject)
                {
                    _completed = true;
                }

                TinyConsole.CursorVisible = ConsoleCursorVisiblity.NormalMode;
                TinyConsole.WriteLine();
                TinyConsole.WriteLine();
                TinyConsole.Beep();
                TinyConsole.WriteLine(Resource.ProcessCompletedMessageText);
                _ = TinyConsole.ReadLine();

                void worker()
                {
                    while (progressState.TryGetNextSourceFile(out int sourceFileId, out FileInfo? sourceFile))
                    {
                        if (_cancelRequested)
                            break;

                        var progress =
                            new Progress<double>(percent =>
                            {
                                progressState.UpdateProgress(sourceFileId, percent);
                                progressState.WriteProgressText(PrintProgress);
                            });
                        try
                        {
                            var localSettings = localSettingsCache[sourceFile.Directory];
                            var actionResult =
                                sourceFile.Exists && !localSettings.DoNotConvert
                                ? actionMode switch
                                {
                                    ActionMode.NormalizeAudio => MatroskaAction.NormalizeMovieFile(localSettings, sourceFile, progress),
                                    ActionMode.ConvertVideo => MatroskaAction.ResizeMovieFile(localSettings, sourceFile, progress),
                                    _ => ActionResult.Skipped,
                                }
                                : ActionResult.Skipped;
                            progressState.CompleteSourceFile(sourceFileId, actionResult);
                            progressState.WriteProgressText(PrintProgress);
                        }
                        catch (Exception ex)
                        {
                            // ここに到達することはないはず
                            throw Validation.GetFailErrorException("Unreachable code", ex);

                        }
                        finally
                        {
                        }
                    }
                }
            }
            catch (AggregateException ex)
            {
                TinyConsole.CursorVisible = ConsoleCursorVisiblity.NormalMode;
                TinyConsole.WriteLine();
                TinyConsole.WriteLine();
                TinyConsole.ForegroundColor = ConsoleColor.Red;
                TinyConsole.WriteLine($"Fatal error occured.");
                TinyConsole.ForegroundColor = ConsoleColor.White;
                ExternalCommand.ReportAggregateException(ex);
                TinyConsole.WriteLine();
                TinyConsole.Beep();
                TinyConsole.WriteLine("Press ENTER key to exit.");
                _ = TinyConsole.ReadLine();
            }
            catch (Exception ex)
            {
                TinyConsole.CursorVisible = ConsoleCursorVisiblity.NormalMode;
                TinyConsole.WriteLine();
                TinyConsole.WriteLine();
                TinyConsole.ForegroundColor = ConsoleColor.Red;
                TinyConsole.WriteLine($"Fatal error occured.");
                TinyConsole.ForegroundColor = ConsoleColor.White;
                ExternalCommand.ReportException(ex);
                TinyConsole.WriteLine();
                TinyConsole.Beep();
                TinyConsole.WriteLine("Press ENTER key to exit.");
                _ = TinyConsole.ReadLine();
            }
        }

        private static IEnumerable<FileInfo> CreateSourceFileList(string[] args, ActionMode actionMode)
        {
            var sourceFileList =
                EnumerateSourceFile(args.Where(arg => !arg.StartsWith("--", StringComparison.Ordinal)))
                .OrderBy(file => file.FullName)
                .ToList();
            if (actionMode != ActionMode.ConvertVideo)
                return sourceFileList;

            // "--convert-video" モードの際に、親ディレクトリに処理方法を指定するという仕様の都合上、単純変換対象のファイルが先に処理されやすい。
            // 進捗の母数はソースファイルのサイズの合計で、単純変換は非常に高速に終了するため、その後のサイズ変換の進捗や終了予定時刻に非現実的な値が表示されやすい問題がある。
            // そのため、"--convert-video" モードでは「サイズ変更対象になりそうなソースファイル」と「単純変換対象になりそうなファイル」がそれぞれ大量に連続して処理されにくいように
            // 適当に処理順序を変えた sourceFileList を再作成する。

            var sourceQueueWithSimpleConversion = new Queue<FileInfo>();
            var sourceQueueWithComplexConversion = new Queue<FileInfo>();
            foreach (var sourceFile in sourceFileList)
            {
                var sourceFileDirectoryName = Path.GetFileName(Path.GetDirectoryName(sourceFile.FullName) ?? ".");
                if (sourceFileDirectoryName.StartsWith("==", StringComparison.Ordinal))
                    sourceQueueWithSimpleConversion.Enqueue(sourceFile);
                else
                    sourceQueueWithComplexConversion.Enqueue(sourceFile);
            }

            var totalCountOfSourceFilesWithSimpleConversion = sourceQueueWithSimpleConversion.Count;
            var totalCountOfSourceFilesWithComplexConversion = sourceQueueWithComplexConversion.Count;
            var modifiedSourceFileList = new List<FileInfo>();
            while (sourceQueueWithSimpleConversion.Count > 0 && sourceQueueWithComplexConversion.Count > 0)
            {
                if (sourceQueueWithSimpleConversion.Count * totalCountOfSourceFilesWithComplexConversion > sourceQueueWithComplexConversion.Count * totalCountOfSourceFilesWithSimpleConversion)
                    modifiedSourceFileList.Add(sourceQueueWithSimpleConversion.Dequeue());
                else
                    modifiedSourceFileList.Add(sourceQueueWithComplexConversion.Dequeue());
            }

            while (sourceQueueWithSimpleConversion.Count > 0)
                modifiedSourceFileList.Add(sourceQueueWithSimpleConversion.Dequeue());
            while (sourceQueueWithComplexConversion.Count > 0)
                modifiedSourceFileList.Add(sourceQueueWithComplexConversion.Dequeue());
            return modifiedSourceFileList;
        }

        private static IEnumerable<FileInfo> EnumerateSourceFile(IEnumerable<string> args)
        {
            foreach (var arg in args)
            {
                if (_cancelRequested)
                    break;

                FileInfo? file;
                try
                {
                    file = new FileInfo(arg);
                    if (!IsSourceFile(arg) || !file.Exists || file.Length <= 0)
                        file = null;

                }
                catch (Exception)
                {
                    file = null;
                }

                if (file is not null)
                    yield return file;

                DirectoryInfo? directoryInfo;
                try
                {
                    directoryInfo = new DirectoryInfo(arg);
                    if (!directoryInfo.Exists)
                        directoryInfo = null;
                }
                catch (Exception)
                {
                    directoryInfo = null;
                }

                if (directoryInfo is not null)
                {
                    foreach (var childFile in directoryInfo.EnumerateFiles("*.*", SearchOption.AllDirectories))
                    {
                        if (childFile is not null && IsSourceFile(childFile.FullName) && childFile.Length > 0)
                        {
                            if (_cancelRequested)
                                break;

                            yield return childFile;
                        }
                    }
                }
            }
        }

        private static bool IsSourceFile(string sourceFilePath)
            => !(Path.GetFileName(sourceFilePath) ?? ".").StartsWith(".", StringComparison.Ordinal) &&
                Path.GetExtension(sourceFilePath).ToUpperInvariant().IsAnyOf(".MKV", ".MP4", ".WMV", ".AVI");

        private static bool IsRequestedCancellation()
        {
            lock (_lockConsoleObject)
            {
                return _cancelRequested;
            }
        }

        private static void PrintProgress(string progressText)
        {
            lock (_lockConsoleObject)
            {
                if (_completed)
                    return;

                if (_cancelRequested)
                {
                    TinyConsole.CursorVisible = ConsoleCursorVisiblity.Invisible;
                    TinyConsole.Write(".");
                    return;
                }

                if (progressText != _previousProgressText)
                {
                    TinyConsole.CursorVisible = ConsoleCursorVisiblity.Invisible;
                    TinyConsole.Write($"  {progressText}");
                    TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
                    _previousProgressText = progressText;
                }
            }
        }
    }
}
