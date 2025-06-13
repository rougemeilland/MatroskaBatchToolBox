using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MatroskaBatchToolBox.Properties;
using Palmtree;
using Palmtree.IO;
using Palmtree.IO.Console;

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
        private static readonly string _applicationUniqueId = $"{nameof(MatroskaBatchToolBox)}.{typeof(Program).GUID}";
#if NET9_0_OR_GREATER
        private static readonly Lock _lockConsoleObject = new();
#else
        private static readonly object _lockConsoleObject = new();
#endif
        private static readonly TimeSpan _maximumTimeForProgressUpdate = TimeSpan.FromMinutes(1);
        private static readonly CompositeFormat _alreadyRunnningMessasgeText = CompositeFormat.Parse(Resource.AlreadyRunnningMessasgeText);

        private static string _previousProgressText = "";
        private static bool _cancelRequested;
        private static bool _completed;

        public static void Main(string[] args)
        {
            if (TinyConsole.InputEncoding.CodePage != Encoding.UTF8.CodePage || TinyConsole.OutputEncoding.CodePage != Encoding.UTF8.CodePage)
            {
                if (OperatingSystem.IsWindows())
                    TinyConsole.WriteLog(LogCategory.Warning, "The encoding of standard input or output is not UTF8. Consider running the command \"chcp 65001\".");
                else
                    TinyConsole.WriteLog(LogCategory.Warning, "The encoding of standard input or standard output is not UTF8.");
            }

            // このプロセスでは Ctrl+C を無視する。
            TinyConsole.CancelKeyPress += (sender, e) => e.Cancel = true;

            // コマンドの入出力エンコーディングを UTF8 にする
            TinyConsole.InputEncoding = Encoding.UTF8;
            TinyConsole.OutputEncoding = Encoding.UTF8;
            TinyConsole.DefaultTextWriter = ConsoleTextWriterType.StandardError;

            TinyConsole.Title = Process.GetCurrentProcess().ProcessName;

            var success = false;
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
                    TinyConsole.WriteLine(string.Format(CultureInfo.InvariantCulture, _alreadyRunnningMessasgeText, nameof(MatroskaBatchToolBox)));
                    TinyConsole.ResetColor();
                    return;
                }

                TinyConsole.WriteLine(
                    actionMode switch
                    {
                        ActionMode.NormalizeAudio => Resource.AudioNormalizationProcessStartMessageText,
                        ActionMode.ConvertVideo => Resource.VideoConversionStartMessageText,
                        _ => throw Validation.GetFailErrorException(),
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
                success = true;

                void worker()
                {
                    while (progressState.TryGetNextSourceFile(out var sourceFileId, out var sourceFile))
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
                                    ActionMode.NormalizeAudio => MatroskaAction.NormalizeMovieFile(sourceFile, progress),
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
                            throw Validation.GetFailErrorException(ex);

                        }
                        finally
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TinyConsole.CursorVisible = ConsoleCursorVisiblity.NormalMode;
                TinyConsole.WriteLine();
                TinyConsole.WriteLine();
                TinyConsole.WriteLog(ex);
                TinyConsole.WriteLine("Press ENTER key to exit.");
                _ = TinyConsole.ReadLine();
            }

            if (success)
                TinyConsole.WriteLine(Resource.ProcessCompletedMessageText);
            else
                TinyConsole.WriteLine("Press ENTER key to exit.");

            TinyConsole.Beep();
            _ = TinyConsole.ReadLine();

        }

        private static IEnumerable<FilePath> CreateSourceFileList(string[] args, ActionMode actionMode)
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

            var sourceQueueWithSimpleConversion = new Queue<FilePath>();
            var sourceQueueWithComplexConversion = new Queue<FilePath>();
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
            var modifiedSourceFileList = new List<FilePath>();
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
            return modifiedSourceFileList.AsEnumerable();
        }

        private static IEnumerable<FilePath> EnumerateSourceFile(IEnumerable<string> args)
        {
            foreach (var file in args.EnumerateFilesFromArgument(true))
            {
                if (_cancelRequested)
                    break;
                if (IsSourceFile(file.FullName) && file.Length > 0)
                    yield return file;
            }
        }

        private static bool IsSourceFile(string sourceFilePath)
            => !Path.GetFileName(sourceFilePath).StartsWith('.')
                && Path.GetExtension(sourceFilePath).ToUpperInvariant().IsAnyOf(".MKV", ".MP4", ".WMV", ".AVI");

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
