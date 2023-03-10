using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MatroskaBatchToolBox.Properties;
using Utility;

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
        private static int _previousProgressTextLengthOnConsole;
        private static bool _cancelRequested;
        private static bool _completed;

        static Program()
        {
            _applicationUniqueId = $"{nameof(MatroskaBatchToolBox)}.{typeof(Program).GUID}";
            _lockConsoleObject = new object();
            _maximumTimeForProgressUpdate = TimeSpan.FromMinutes(1);
            _previousProgressText = "";
            _previousProgressTextLengthOnConsole = 0;
            _cancelRequested = false;
            _completed = false;
        }

        public static void Main(string[] args)
        {
            Console.Title = nameof(MatroskaBatchToolBox);

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
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(string.Format(Resource.AlreadyRunnningMessasgeText, nameof(MatroskaBatchToolBox)));
                    return;
                }

                Console.WriteLine(
                    actionMode switch
                    {
                        ActionMode.NormalizeAudio => Resource.AudioNormalizationProcessStartMessageText,
                        ActionMode.ConvertVideo => Resource.VideoConversionStartMessageText,
                        _ => throw new Exception("internal error"),
                    });
                Console.WriteLine();

                _ = Task.Run(() =>
                {
                    while (true)
                    {
                        var keyInfo = Console.ReadKey(true);
                        if (keyInfo.Key == ConsoleKey.Q)
                        {
                            ExternalCommand.AbortExternalCommands();
                            lock (_lockConsoleObject)
                            {
                                if (!_completed)
                                {
                                    _cancelRequested = true;
                                    Console.CursorVisible = true;
                                    Console.WriteLine();
                                    Console.WriteLine();
                                    var color = Console.ForegroundColor;
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine(Resource.Q_KeyPressedMessasgeText);
                                    Console.WriteLine();
                                    Console.ForegroundColor = color;
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

                if (!IsRequestedCancellation())
                    progressState.CheckCompletion();
                progressState.WriteProgressText(PrintProgress);

                lock (_lockConsoleObject)
                {
                    _completed = true;
                }

                Console.CursorVisible = true;
                Console.WriteLine();
                Console.WriteLine();
                Console.Beep();
                Console.WriteLine(Resource.ProcessCompletedMessageText);
                _ = Console.ReadLine();

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
                            throw new Exception("internal error", ex);

                        }
                        finally
                        {
                        }
                    }
                }
            }
            catch (AggregateException ex)
            {
                Console.CursorVisible = true;
                Console.WriteLine();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Fatal error occured.");
                Console.ForegroundColor = ConsoleColor.White;
                ExternalCommand.ReportAggregateException(ex);
                Console.WriteLine();
                Console.Beep();
                Console.WriteLine("Press ENTER key to exit.");
                _ = Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.CursorVisible = true;
                Console.WriteLine();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Fatal error occured.");
                Console.ForegroundColor = ConsoleColor.White;
                ExternalCommand.ReportException(ex);
                Console.WriteLine();
                Console.Beep();
                Console.WriteLine("Press ENTER key to exit.");
                _ = Console.ReadLine();
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
#if DEBUG && false
            System.Diagnostics.Debug.WriteLine("-----");
            System.Diagnostics.Debug.WriteLine("modifiedSourceFileList:");
            System.Diagnostics.Debug.Indent();
            foreach (var sourceFile in modifiedSourceFileList)
            {
                System.Diagnostics.Debug.WriteLine($"\"{sourceFile.FullName}\"");
            }
            System.Diagnostics.Debug.Unindent();
            System.Diagnostics.Debug.WriteLine("-----");
            if (modifiedSourceFileList.Count != sourceFileList.Count)
                throw new Exception();
#endif
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
                    Console.CursorVisible = false;
                    Console.Write(".");
                    return;
                }

                if (progressText != _previousProgressText)
                {
                    Console.CursorVisible = false;
                    var (leftPos0, topPos0) = Console.GetCursorPosition();
                    Console.Write($"  {progressText}");
                    var (leftPos1, topPos1) = Console.GetCursorPosition();
                    var currentProgressTextLength = leftPos1 - leftPos0 + (topPos1 - topPos0) * Console.WindowWidth;
                    if (_previousProgressTextLengthOnConsole > currentProgressTextLength)
                        Console.Write(new string(' ', _previousProgressTextLengthOnConsole - currentProgressTextLength));
                    Console.SetCursorPosition(leftPos0, topPos0);
                    _previousProgressText = progressText;
                    _previousProgressTextLengthOnConsole = currentProgressTextLength;
                }
            }
        }
    }
}
