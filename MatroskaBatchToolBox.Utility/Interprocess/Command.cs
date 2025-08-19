using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;
using Palmtree.IO;
using Palmtree.IO.Console;

namespace MatroskaBatchToolBox.Utility.Interprocess
{
    public static partial class Command
    {
        private sealed class BinaryInputRedirector
            : IChildProcessInputRedirectable
        {
            private readonly Func<ReadOnlyMemory<byte>> _binaryWriter;

            public BinaryInputRedirector(Func<ReadOnlyMemory<byte>> binaryWriter)
            {
                _binaryWriter = binaryWriter;
            }

            Action IChildProcessInputRedirectable.GetInputRedirector(StreamWriter writer)
                => () =>
                {
                    while (true)
                    {
                        var buffer = _binaryWriter();
                        if (buffer.Length <= 0)
                            break;
                        writer.BaseStream.Write(buffer.Span);
                    }

                    writer.BaseStream.Flush();
                    writer.BaseStream.Close();
                };
        }

        private sealed class TextInputRedirector
            : IChildProcessInputRedirectable
        {
            private readonly Func<string?> _textWriter;

            public TextInputRedirector(Func<string?> textWriter)
            {
                _textWriter = textWriter;
            }

            Action IChildProcessInputRedirectable.GetInputRedirector(StreamWriter writer)
                => () =>
                {
                    while (true)
                    {
                        var textLine = _textWriter();
                        if (textLine is null)
                            break;
                        writer.WriteLine(textLine);
                    }

                    writer.Flush();
                    writer.Close();
                };
        }

        private sealed class NullInputRedirector
            : IChildProcessInputRedirectable
        {
            public NullInputRedirector()
            {
            }

            Action IChildProcessInputRedirectable.GetInputRedirector(StreamWriter writer)
                => () => { };
        }

        private sealed class BinaryOutputRedirector
            : IChildProcessOutputRedirectable
        {
            private readonly Action<ReadOnlyMemory<byte>> _binaryReader;

            public BinaryOutputRedirector(Action<ReadOnlyMemory<byte>> binaryReader)
            {
                _binaryReader = binaryReader;
            }

            Action IChildProcessOutputRedirectable.GetOutputRedirector(StreamReader reader)
                => () =>
                {
                    var buffer = new byte[_ioBufferSize];
                    while (true)
                    {
                        var length = reader.BaseStream.Read(buffer);
                        if (length <= 0)
                            break;
                        _binaryReader(buffer.AsMemory()[length..]);
                    }
                };
        }

        private sealed class TextOutputRedirector
            : IChildProcessOutputRedirectable
        {
            private readonly Action<string> _textReader;

            public TextOutputRedirector(Action<string> textReader)
            {
                _textReader = textReader;
            }

            Action IChildProcessOutputRedirectable.GetOutputRedirector(StreamReader reader)
                => () =>
                {
                    while (true)
                    {
                        var textLine = reader.ReadLine();
                        if (textLine is null)
                            break;
                        _textReader(textLine);
                    }
                };
        }

        private sealed class ChildProcessCanceller
            : IChildProcessCancellable
        {
            private readonly Action<Process> _canceller;

            public ChildProcessCanceller(Action<Process> canceller)
            {
                _canceller = canceller;
            }

            void IChildProcessCancellable.CancelChildProcess(Process process) => _canceller(process);
        }

        private sealed class FfmpegLogState
        {
            public FfmpegLogState()
            {
                DetectedToQuit = false;
                MaximumDurationSeconds = null;
            }

            public bool DetectedToQuit { get; set; }
            public double? MaximumDurationSeconds { get; set; }
        }

        private const int _ioBufferSize = 64 * 1024;
        private static readonly TimeSpan _childProcessCancellationInterval = TimeSpan.FromSeconds(10);
        private static readonly Encoding _defaultInputOutputEncoding = new UTF8Encoding(false);
        private static bool _requestedCancellation;

        public static void AbortExternalCommands()
            => _requestedCancellation = true;

        public static MovieInformation GetMovieInformation(
            string? inFileFormat,
            FilePath inFile,
            MovieInformationType requestedInfo,
            Action<LogCategory, string> logger)
        {
            var ffprobeCommandFile =
                new FilePath(
                    ProcessUtility.WhereIs("ffprobe")
                    ?? throw new ApplicationException("ffprobe command is not installed."));

            var commandParameters = new List<string>
            {
                "-hide_banner",
                "-v error",
                "-analyzeduration 100M",
                "-probesize 100M",
                "-print_format json",
            };
            if ((requestedInfo & MovieInformationType.Format) != MovieInformationType.None)
                commandParameters.Add("-show_format");
            if ((requestedInfo & MovieInformationType.Streams) != MovieInformationType.None)
                commandParameters.Add("-show_streams");
            if ((requestedInfo & MovieInformationType.Chapters) != MovieInformationType.None)
                commandParameters.Add("-show_chapters");
            if (inFileFormat is not null)
                commandParameters.Add($"-f {inFileFormat}");
            commandParameters.Add($"-i \"{inFile.FullName}\"");
            var standardOutputTextLines = new List<string>();
            var exitCode =
                ExecuteCommand(
                    ffprobeCommandFile,
                    string.Join(" ", commandParameters),
                    null,
                    null,
                    null,
                    GetTextOutputRedirector(standardOutputTextLines.Add),
                    null,
                    logger,
                    null);
            logger(LogCategory.Information, $"ffprobe exited with exit code {exitCode}.");
            if (exitCode != 0)
                throw new ApplicationException($"ffprobe failed. (exit code {exitCode})");
            var jsonText = string.Join("\r\n", standardOutputTextLines);
            try
            {
                return MovieInformation.ParseFromJson(jsonText);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"The information returned by ffprobe is in an unknown format.: \"{jsonText}\"", ex);
            }
        }

        public static int ExecuteFfmpeg(
            string args,
            IChildProcessInputRedirectable? standardInputRedirector,
            IChildProcessOutputRedirectable? standardOutputRedirector,
            Action<string> ffmpegLogRedirector,
            Action<LogCategory, string> logger,
            IProgress<double> progressReporter)
        {
            var ffmpegCommandFile =
                new FilePath(
                    ProcessUtility.WhereIs("pffmpeg")
                    ?? throw new ApplicationException("ffmpeg command is not installed."));

            var logState = new FfmpegLogState();
            return
                ExecuteCommand(
                    ffmpegCommandFile,
                    args,
                    null,
                    null,
                    standardInputRedirector,
                    standardOutputRedirector,
                    GetTextOutputRedirector(lineText =>
                    {
                        if (lineText.StartsWith("[q] command received. Exiting.", StringComparison.Ordinal))
                        {
                            // ffmpeg が q の入力を検出して終了ログである場合

                            logState.DetectedToQuit = true;
                            return;
                        }
                        else if (lineText.StartsWith("frame=", StringComparison.Ordinal))
                        {
                            // 進捗状況のログである場合

                            if (logState.MaximumDurationSeconds is null)
                            {
                                // MaximumDurationSeconds が未設定で進捗の計算ができないので何もしない
                                return;
                            }

                            // 進捗状況の解析
                            var match = GetFfmpegConversionProgressPattern().Match(lineText);
                            if (!match.Success)
                            {
                                // 解析に失敗した場合は何もしない
                                return;
                            }

                            // 進捗率の計算
                            var progress = match.Groups["time"].Value.ParseAsTimeSpan(TimeParsingMode.LazyMode).TotalSeconds / logState.MaximumDurationSeconds.Value;
                            if (progress < 0)
                                progress = 0;
                            if (progress > 1)
                                progress = 1;

                            // 進捗率を報告する
                            progressReporter.Report(progress);
                            return;
                        }
                        else
                        {
                            // その他のログである場合

                            // ログを報告する
                            ffmpegLogRedirector(lineText);

                            // ログの中に Duration 値が含まれているか探す
                            var durationMatch = GetFfmpegConversionDurationPattern().Match(lineText);
                            if (durationMatch.Success)
                            {
                                // ログの中に Duration 値が含まれている場合

                                var duration = durationMatch.Groups["time"].Value.ParseAsTimeSpan(TimeParsingMode.LazyMode).TotalSeconds;
                                if (logState.MaximumDurationSeconds is null || duration > logState.MaximumDurationSeconds.Value)
                                {
                                    // Duration 値が初めて見つかったか、または以前に見つかった Duration 値より大きい場合は MaximumDurationSeconds を更新する
                                    logState.MaximumDurationSeconds = duration;
                                }
                            }
                        }
                    }),
                    logger,
                    standardInputRedirector is not null
                    ? null // 標準入力がリダイレクトされている場合はキャンセルができない
                    : GetChildProcessCanceller(process => process.StandardInput.Write("q")));
        }

        public static int ExecuteCommand(
            FilePath programFile,
            string args,
            Encoding? intpuEncoding,
            Encoding? outputEncoding,
            IChildProcessInputRedirectable? standardInputRedirector,
            IChildProcessOutputRedirectable? standardOutputRedirector,
            IChildProcessOutputRedirectable? standardErrorRedirector,
            Action<LogCategory, string> messageReporter,
            IChildProcessCancellable? childProcessCcanceller)
        {
            standardOutputRedirector ??= new TextOutputRedirector(TinyConsole.WriteLine);
            standardErrorRedirector ??= new TextOutputRedirector(TinyConsole.WriteLine);

            Validation.Assert(standardOutputRedirector is not null);
            Validation.Assert(standardErrorRedirector is not null);

            var processStartInfo =
                new ProcessStartInfo
                {
                    Arguments = args,
                    FileName = programFile.FullName,
                    CreateNoWindow = false,
                    RedirectStandardInput = standardInputRedirector is not null,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardInputEncoding = standardInputRedirector is not null ? ( intpuEncoding ?? _defaultInputOutputEncoding) : null,
                    StandardOutputEncoding = outputEncoding ?? _defaultInputOutputEncoding,
                    StandardErrorEncoding = outputEncoding ?? _defaultInputOutputEncoding,
                    UseShellExecute = false,
                };
            var process =
                Process.Start(processStartInfo)
                ?? throw new ApplicationException("Could not start process");
            try
            {
                messageReporter(LogCategory.Information, $"Child process started.: id={process.Id} \"{process.StartInfo.FileName}\" {process.StartInfo.Arguments}");
                var cancelled = false;
                var cancellationWatcherTask =
                    Task.Run(() =>
                    {
                        if (childProcessCcanceller is not null)
                        {
                            while (!_requestedCancellation && !process.WaitForExit(1000))
                            {
                            }

                            if (_requestedCancellation)
                            {
                                try
                                {
                                    process.PriorityClass = ProcessPriorityClass.Normal;
                                    while (true)
                                    {
                                        try
                                        {
                                            childProcessCcanceller.CancelChildProcess(process);
                                        }
                                        catch (Exception)
                                        {
                                        }

                                        if (process.WaitForExit((int)_childProcessCancellationInterval.TotalMilliseconds))
                                            break;
                                    }
                                }
                                catch (Exception)
                                {
                                }
                                finally
                                {
                                    cancelled = true;
                                }
                            }
                        }
                    });

                var standardInputRedirectorTask =
                    standardInputRedirector is not null
                    ? Task.Run(standardInputRedirector.GetInputRedirector(process.StandardInput))
                    : null;
                try
                {
                    using var standardOutputRedirectorTask = Task.Run(standardOutputRedirector.GetOutputRedirector(process.StandardOutput));
                    using var standardErrorRedirectorTask = Task.Run(standardErrorRedirector.GetOutputRedirector(process.StandardError));
                    standardInputRedirectorTask?.Wait();
                    standardOutputRedirectorTask.Wait();
                    standardErrorRedirectorTask.Wait();
                    process.WaitForExit();
                }
                finally
                {
                    standardInputRedirectorTask?.Dispose();
                }

                messageReporter(LogCategory.Information, $"Child process exited.: id={process.Id}, process-total-time={process.TotalProcessorTime.TotalSeconds:F2}[sec], \"{process.StartInfo.FileName}\" {process.StartInfo.Arguments}");

                return
                    !cancelled
                    ? process.ExitCode
                    : throw new OperationCanceledException($"Child process canceled.: \"{process.StartInfo.FileName} {process.StartInfo.Arguments}\"");
            }
            finally
            {
                process.Dispose();
            }
        }

        public static IChildProcessInputRedirectable GetBinaryInputRedirector(Func<ReadOnlyMemory<byte>> binaryWriter)
           => new BinaryInputRedirector(binaryWriter);

        public static IChildProcessInputRedirectable GetTextInputRedirector(Func<string?> textwriter)
           => new TextInputRedirector(textwriter);

        public static IChildProcessInputRedirectable GetNullInputRedirector()
           => new NullInputRedirector();

        public static IChildProcessOutputRedirectable GetBinaryOutputRedirector(Action<ReadOnlyMemory<byte>> binaryReader)
           => new BinaryOutputRedirector(binaryReader);

        public static IChildProcessOutputRedirectable GetTextOutputRedirector(Action<string> textReader)
           => new TextOutputRedirector(textReader);

        public static IChildProcessCancellable GetChildProcessCanceller(Action<Process> childProcessCanceller)
            => new ChildProcessCanceller(childProcessCanceller);

        [GeneratedRegex(@"\s*(Duration|DURATION)\s*:\s*(?<time>\d+:\d+:\d+(\.\d+)?)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)]
        private static partial Regex GetFfmpegConversionDurationPattern();

        [GeneratedRegex(@" time=(?<time>\d+:\d+:\d+(\.\d+)?) ", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)]
        private static partial Regex GetFfmpegConversionProgressPattern();
    }
}
