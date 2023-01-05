using MatroskaBatchToolBox.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace MatroskaBatchToolBox
{
    [Guid("F6CDDD48-C454-4CF1-B454-92533E5638DE")]
    public class Program
    {
        private enum ActionMode
        {
            None = 0,
            NormalizeAudio,
            ResizeVideo,
        }

        private const string _optionStringNormalizeAudio = "--normalize-audio";
        private const string _optionStringResizeResolution = "--change-resolution";
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
                    if (string.Equals(arg, _optionStringNormalizeAudio, StringComparison.InvariantCulture))
                    {
                        actionMode = ActionMode.NormalizeAudio;
                        break;
                    }
                    if (string.Equals(arg, _optionStringResizeResolution, StringComparison.InvariantCulture))
                    {
                        actionMode = ActionMode.ResizeVideo;
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
                        ActionMode.ResizeVideo => Resource.VideoResizingProcessStartMessageText,
                        _ => throw new Exception("internal error"),
                    });
                Console.WriteLine();

                Task.Run(() =>
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

                var sourceFileList =
                    EnumerateSourceFile(args.Where(arg => !arg.StartsWith("--", StringComparison.InvariantCulture)))
                    .OrderBy(file => file.FullName)
                    .ToList();


                var progressState = new ProgressState(sourceFileList);
                progressState.WriteProgressText(PrintProgress);

                var degreeOfParallelism = Settings.CurrentSettings.DegreeOfParallelism;
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
                Console.ReadLine();

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
                            var actionResult =
                                actionMode switch
                                {
                                    ActionMode.NormalizeAudio => MatroskaAction.NormalizeMovieFile(sourceFile, progress),
                                    ActionMode.ResizeVideo => MatroskaAction.ResizeMovieFile(sourceFile, progress),
                                    _ => ActionResult.Skipped,
                                };
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
                Console.ReadLine();
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
                Console.ReadLine();
            }
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
        {
            if ((Path.GetFileName(sourceFilePath) ?? ".").StartsWith(".", StringComparison.InvariantCulture))
                return false;
            var extension = Path.GetExtension(sourceFilePath).ToUpperInvariant();
            return
                string.Equals(extension, ".MKV", StringComparison.InvariantCulture) ||
                string.Equals(extension, ".MP4", StringComparison.InvariantCulture) ||
                string.Equals(extension, ".WMV", StringComparison.InvariantCulture) ||
                string.Equals(extension, ".AVI", StringComparison.InvariantCulture);
        }

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

                if (!string.Equals(progressText, _previousProgressText, StringComparison.InvariantCulture))
                {
                    Console.CursorVisible = false;
                    var (leftPos0, topPos0) = Console.GetCursorPosition();
                    Console.Write($"  {progressText}");
                    var (leftPos1, topPos1) = Console.GetCursorPosition();
                    var currentProgressTextLength = (leftPos1 - leftPos0) + (topPos1 - topPos0) * Console.WindowWidth;
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
