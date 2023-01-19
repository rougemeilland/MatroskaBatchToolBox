using MatroskaBatchToolBox.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static MatroskaBatchToolBox.Settings;

namespace MatroskaBatchToolBox
{
    internal static class ExternalCommand
    {
        public enum ExternalCommandResult
        {
            Completed,
            Cancelled,
            NotSupported,
        }

        private enum OutputStreamType
        {
            StandardOutput,
            StandardError,
        }

        private const string _ffmpegPathEnvironmentVariableName = "FFMPEG_PATH";
        private static readonly object _loggingLockObject;
        private static readonly Regex _ffmpegNormalizeProgressPattern;
        private static readonly Regex _logsToIgnoreInFFmpegNormalize;
        private static readonly Regex _libopusNotAvailableOnFFmpegNormalizeInLogs;
        private static readonly Regex _moveStreamInfoPattern;
        private static readonly Regex _moveStreamInfoAudioDetailPattern;
        private static readonly Regex _audioChannelLayoutPattern;
        private static readonly Regex _ffmpegConversionDurationPattern;
        private static readonly Regex _ffmpegConversionProgressPattern;
        private static readonly Regex _ffmpegVMAFScoreCalculationResultPattern;
        private static bool _requestedCancellation;

        static ExternalCommand()
        {
            _loggingLockObject = new object();
            _ffmpegNormalizeProgressPattern = new Regex(@"^(((?<phase>Stream)\s+(?<currentStream>\d+)/(?<totalStream>\d+))|(?<phase>Second Pass)|(?<phase>File)):\s+(?<Percentage>\d+)%\|", RegexOptions.Compiled);
            _logsToIgnoreInFFmpegNormalize = new Regex(@"^(frame|fps|stream_\d+_\d+_q|bitrate|total_size|out_time_us|out_time_ms|out_time|dup_frames|drop_frames|speed|progress)=", RegexOptions.Compiled);
            _libopusNotAvailableOnFFmpegNormalizeInLogs = new Regex(@"^\[libopus @ [0-9a-fA-F]+\] Invalid channel layout 5.1\(side\) for specified mapping family -1\.", RegexOptions.Compiled);
            _moveStreamInfoPattern = new Regex(@"^  Stream #0:(?<id>\d+)(?<language>\([^\)]+\))?: (?<streamType>Video|Audio|Subtitle): (?<detail>.*)$", RegexOptions.Compiled);
            _moveStreamInfoAudioDetailPattern = new Regex(@"^\s*(?<codec>[^ ,][^,]*[^ ,])\s*,\s*(?<samplingFrequency>[^ ,][^,]*[^ ,]) Hz\s*,\s*(?<channelLayout>[^ ,][^,]*[^ ,])\s*,", RegexOptions.Compiled);
            _audioChannelLayoutPattern = new Regex(@"(?<layoutType>[^\(]+)(\([^\)]+\))?", RegexOptions.Compiled);
            _ffmpegConversionDurationPattern = new Regex(@"\s*(Duration|DURATION)\s*:\s*(?<hours>\d+):(?<minutes>\d+):(?<seconds>[\d\.]+)", RegexOptions.Compiled);
            _ffmpegConversionProgressPattern = new Regex(@" time=(?<hours>\d+):(?<minutes>\d+):(?<seconds>[\d\.]+) ", RegexOptions.Compiled);
            _ffmpegVMAFScoreCalculationResultPattern = new Regex(@"^\[Parsed_libvmaf_\d+\s*@\s*[a-fA-F0-9]+\]\s*VMAF\s+score:\s*(?<vmafScoreValue>\d+(\.\d+)?)$", RegexOptions.Compiled);
            _requestedCancellation = false;
        }

        public static void AbortExternalCommands()
        {
            _requestedCancellation = true;
        }

        public static ExternalCommandResult NormalizeAudioFile(FileInfo logFile, FileInfo inFile, AudioEncoderType audioEncoder, FileInfo outFile, IProgress<double> progressReporter)
        {
            Environment.SetEnvironmentVariable(_ffmpegPathEnvironmentVariableName, Settings.GlobalSettings.FFmpegCommandFile.FullName);
            var commandParameter = $"\"{inFile.FullName}\" -o \"{outFile.FullName}\" -pr -v -d --keep-loudness-range-target --audio-codec {audioEncoder.ToCodecSpec()}";
            var totalStream = 1;
            var isNotAvailableCodec = false;
            var (cancelled, exitCode) =
                ExecuteCommand(
                    Settings.GlobalSettings.FFmpegNormalizeCommandFile,
                    logFile,
                    commandParameter,

                    // ffmpeg-normalize 自体のエンコーディングはおそらくデフォルトエンコーディング (端末のローカルエンコーディング 日本のPCなら shift_jis) だが、
                    // そこから呼び出される ffmpgのログは UTF-8になっている。
                    // エンコーディングによって内容に差異が発生するのはパス名にASCII文字以外が含まれている場合ぐらいなので、とりあえず UTF-8にしておく。
                    // したがって、音声の変換時に出力されるログの内容のうちパス名に関する部分は表示が乱れる可能性がある。
                    Encoding.UTF8,

                    (type, text) =>
                    {
                        if (type != OutputStreamType.StandardError)
                            return;
                        if (string.IsNullOrEmpty(text))
                            return;
                        if (audioEncoder == AudioEncoderType.Libopus && _libopusNotAvailableOnFFmpegNormalizeInLogs.IsMatch(text))
                        {
                            // libopus が一部のチャネルレイアウトをサポートしていないために発生する
                            isNotAvailableCodec = true;
                            return;
                        }
                        var match = _ffmpegNormalizeProgressPattern.Match(text);
                        if (!match.Success)
                        {
                            // 進行状況などで頻出して、かつトラブル分析用としては重要ではない項目はログに記録しない。
                            if (!_logsToIgnoreInFFmpegNormalize.IsMatch(text))
                                Log(logFile, new[] { text });
                            return;
                        }
                        var phaseText = match.Groups["phase"].Value;
                        var percentageText = match.Groups["Percentage"].Value;
                        int phase;
                        switch (phaseText)
                        {
                            case "Stream":
                                {
                                    var currentStreamText = match.Groups["currentStream"].Value;
                                    if (!int.TryParse(currentStreamText, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out int currentStream))
                                        return;
                                    var totalStreamText = match.Groups["totalStream"].Value;
                                    if (!int.TryParse(totalStreamText, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out totalStream))
                                        return;
                                    phase = currentStream - 1;
                                }
                                break;
                            case "Second Pass":
                                phase = totalStream;
                                break;
                            case "File":
                                return; // NOP
                            default:
                                return; // NOP
                        }
                        if (!int.TryParse(percentageText, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out int percentage))
                            return;

                        // この時点で、 phase は完了しているフェーズの数、 totalStream + 1 は全フェーズの数、現在のフェーズの完了状況(%)は percentage
                        var progress = (phase + percentage / 100.0) / (totalStream + 1);
#if DEBUG && false
                        if (progress < 0)
                            throw new Exception();
                        if (progress > 1.0)
                            throw new Exception();
#endif
                        progressReporter.Report(progress);
                    },
                    childProcess =>
                    {
                        // 標準入出力を閉じることで ffmpeg-normalize のプロセスは終了するが、ffmpeg-normalize から起動された ffmpeg は終わらないまま。
                        childProcess.StandardInput.Close();
                        childProcess.StandardOutput.Close();
                        childProcess.StandardError.Close();

                        // プロセス名が "ffmpeg" で、かつ メインモジュールファイルが "MatroskaBatchToolBox にバンドルされた ffmpeg" であるプロセスをすべて列挙する。
                        // ※ ffmpeg-normalize は、例え PATH 環境変数で他の場所にある ffmpeg のパスが与えられていたとしても、
                        //    環境変数 FFMPEG_PATH で与えられた ffmpeg を  優先的に実行するので、kill したとしても MatroskaBatchToolBox 以外の
                        //    実行中のプログラムに被害はないはず。
                        var targetProcesses =
                            Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Settings.GlobalSettings.FFmpegCommandFile.Name))
                            .Where(proc => string.Equals(proc?.MainModule?.FileName ?? "", Settings.GlobalSettings.FFmpegCommandFile.FullName, StringComparison.InvariantCulture))
                            .ToList();

                        // 列挙されたプロセスをすべて kill する。
                        // タイミングによって既に終了しているプロセスもあり得るが、終了しているプロセスを kill しようとすると例外が発生するため、例外はすべて無視する。
                        foreach (var proc in targetProcesses)
                        {
                            try
                            {
                                if (!proc.HasExited)
                                    proc.Kill();
                            }
                            catch (Exception)
                            {
                            }
                        }
                    });
            if (cancelled)
            {
                Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: ffmpeg-normalize aborted at user request." });
                return ExternalCommandResult.Cancelled;
            }
            Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: ffmpeg-normalize exited with exit code {exitCode}." });
            if (isNotAvailableCodec)
            {
                Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: ERROR: The specified movie's audio was not supported by libopus." });
                return ExternalCommandResult.NotSupported;
            }
            if (exitCode != 0)
                throw new Exception($"Failed in ffmpeg-normalize. (exit code: {exitCode})");
            return ExternalCommandResult.Completed;
        }

        public static (ExternalCommandResult result, MovieStreamInfos? streams) GetMovieStreamInfos(FileInfo logFile, FileInfo inFile)
        {
            var commandParameter = new StringBuilder();
            commandParameter.Append("-hide_banner");
            commandParameter.Append(" -v error");
            commandParameter.Append(" -print_format json");
            commandParameter.Append(" -show_streams");
            commandParameter.Append($" -i \"{inFile.FullName}\"");
            var standardOutputTextLines = new List<string>();
            var (cancelled, exitCode) =
                ExecuteCommand(
                    Settings.GlobalSettings.FFprobeCommandFile,
                    logFile,
                    commandParameter.ToString(),
                    Encoding.UTF8,
                    (type, text) =>
                    {
                        if (type == OutputStreamType.StandardOutput)
                            standardOutputTextLines.Add(text);
                    },
                    proc =>
                    {
                        proc.StandardInput.Write("q");
                    });
            if (cancelled)
            {
                Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: ffprobe aborted at user request." });
                return (ExternalCommandResult.Cancelled, null);
            }
            Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: ffprobe exited with exit code {exitCode}." });
            if (exitCode != 0)
                throw new Exception($"ffprobe failed. (exit code {exitCode})");

            string jsonText = string.Join("\r\n", standardOutputTextLines);
            try
            {
                var streams = JsonSerializer.Deserialize<MovieStreamInfos>(jsonText);
                if (streams is null)
                    throw new Exception("ffprobe returned no information.");
                return (ExternalCommandResult.Completed, streams);
            }
            catch (Exception ex)
            {
                throw new Exception($"The information returned by ffprobe is in an unknown format.: {jsonText}", ex);
            }
        }

        public static ExternalCommandResult ConvertMovieFile(Settings localSettings, FileInfo logFile, FileInfo inFile, string? aspectRateSpec, FileInfo outFile, IProgress<double> progressReporter)
        {
            var commandParameters = new List<string>
            {
                "-hide_banner",
                "-y",
                $"-i \"{inFile.FullName}\""
            };
            if (aspectRateSpec is not null)
                commandParameters.Add($"-aspect {aspectRateSpec}");
            commandParameters.Add("-c copy");
            if (!string.IsNullOrEmpty(localSettings.FFmpegOption))
                commandParameters.Add(localSettings.FFmpegOption);
            commandParameters.Add($"\"{outFile.FullName}\"");
            var detectedToQuit = false;
            var maximumDurationSeconds = double.NaN;
            var vmafScore = double.NaN; // この値は使用されない
            var (cancelled, exitCode) =
                ExecuteCommand(
                    GlobalSettings.FFmpegCommandFile,
                    logFile,
                    string.Join(" ", commandParameters),
                    Encoding.UTF8,
                    (type, text) => ProcessFFmpegOutput(logFile, text, ref detectedToQuit, ref maximumDurationSeconds, ref vmafScore, progressReporter),
                    proc =>
                    {
                        proc.StandardInput.Write("q");
                    });
            if (detectedToQuit || cancelled)
            {
                Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: ffmpeg aborted at user request." });
                return ExternalCommandResult.Cancelled;
            }
            Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: ffmpeg exited with exit code {exitCode}." });
            if (exitCode != 0)
                throw new Exception($"ffmpeg failed. (exit code {exitCode})");
            return ExternalCommandResult.Completed;
        }

        public static ExternalCommandResult ResizeMovieFile(Settings localSettings, FileInfo logFile, FileInfo inFile, MovieStreamInfos streams, string resolutionSpec, string aspectRateSpec, VideoEncoderType videoEncoderType, FileInfo outFile, IProgress<double> progressReporter)
        {
            var videoStreams = streams.EnumerateVideoStreams().ToArray();
            if (videoStreams.Any(stream => string.Equals(stream.CodecName, "png", StringComparison.InvariantCulture)))
            {
                var videoStreamsCountExceptPng = videoStreams.Where(stream => !string.Equals(stream.CodecName, "png", StringComparison.InvariantCulture)).Count();
                if (videoStreamsCountExceptPng <= 0)
                    throw new Exception("The input movie file does not have a video stream other than \"png\".");
                if (videoStreamsCountExceptPng > 1)
                    throw new Exception("The input movie file has multiple video streams other than \"png\".");
            }
            else
            {
                var videoStreamsCount = videoStreams.Count();
                if (videoStreamsCount <= 0)
                    throw new Exception("The input movie file does not have a video stream.");
                if (videoStreamsCount > 1)
                    throw new Exception("The input movie file has multiple video streams.");
            }
            var commandParameters = new List<string>
            {
                "-hide_banner",
                "-y",
                $"-i \"{inFile.FullName}\"",
                $"-s {resolutionSpec}",
                $"-aspect {aspectRateSpec}"
            };
            for (var index = 0; index < videoStreams.Length; ++index)
            {
                if (string.Equals(videoStreams[index].CodecName, "png", StringComparison.InvariantCulture))
                    commandParameters.Add($"-c:v:{index} copy");
                else
                {
                    commandParameters.Add($" -c:v:{index} {videoEncoderType.ToCodecSpec()}");
                    var encoderOptions = localSettings.GetEncodingOption(videoEncoderType);
                    if (!string.IsNullOrEmpty(encoderOptions))
                        commandParameters.Add($"{encoderOptions.Trim()}");
                }
            }
            commandParameters.Add("-c:a copy");
            commandParameters.Add("-c:s copy");
            commandParameters.Add("-g 240");
            if (!string.IsNullOrEmpty(localSettings.FFmpegOption))
                commandParameters.Add(localSettings.FFmpegOption);
            commandParameters.Add("-map 0");
            commandParameters.Add($"\"{outFile}\"");
            var detectedToQuit = false;
            var maximumDurationSeconds = double.NaN;
            var vmafScore = double.NaN; // この値は使用されない
            var (cancelled, exitCode) =
                ExecuteCommand(
                    GlobalSettings.FFmpegCommandFile,
                    logFile,
                    string.Join(" ", commandParameters),
                    Encoding.UTF8,
                    (type, text) => ProcessFFmpegOutput(logFile, text, ref detectedToQuit, ref maximumDurationSeconds, ref vmafScore, progressReporter),
                    proc =>
                    {
                        proc.StandardInput.Write("q");
                    }); ;
            if (detectedToQuit || cancelled)
            {
                Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: ffmpeg aborted at user request." });
                return ExternalCommandResult.Cancelled;
            }
            Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: ffmpeg exited with exit code {exitCode}." });
            if (exitCode != 0)
                throw new Exception($"ffmpeg failed. (exit code {exitCode})");
            return ExternalCommandResult.Completed;
        }

        public static ExternalCommandResult CalculateVMAFScoreFromMovieFile(FileInfo logFile, FileInfo originalFile, FileInfo modifiedFile, string resolutionSpec, out double vmafScore, IProgress<double> progressReporter)
        {
            var commandParameter = new StringBuilder();
            commandParameter.Append("-hide_banner");
            commandParameter.Append($" -i \"{originalFile.FullName}\"");
            commandParameter.Append($" -i \"{modifiedFile.FullName}\"");
            commandParameter.Append($" -filter_complex \"[0:v]settb=1/AVTB,setpts=PTS-STARTPTS[original];[1:v]settb=1/AVTB,setpts=PTS-STARTPTS[1v];[original][1v]scale2ref=flags=bicubic,libvmaf=model=version=vmaf_v0.6.1\\\\:name=vmaf\\\\:n_threads=4:shortest=1:repeatlast=0\"");
#if false // -sn をつけると、VMAFスコア自体は正常に計算されるものの、実行経過に表示される time と speed に異常な値が表示され、進行状況が正しく把握できない。
            commandParameter.Append(" -an -sn");
#endif
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                commandParameter.Append(" -f NULL -");
            else
                commandParameter.Append(" -f null /dev/null");
            var detectedToQuit = false;
            var maximumDurationSeconds = double.NaN;
            var vmafScoreValue = double.NaN;
            var (cancelled, exitCode) =
                ExecuteCommand(
                    GlobalSettings.FFmpegCommandFile,
                    logFile,
                    commandParameter.ToString(),
                    Encoding.UTF8,
                    (type, text) => ProcessFFmpegOutput(logFile, text, ref detectedToQuit, ref maximumDurationSeconds, ref vmafScoreValue, progressReporter),
                    proc =>
                    {
                        proc.StandardInput.Write("q");
                    });
            if (detectedToQuit || cancelled)
            {
                Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: ffmpeg aborted at user request." });
                vmafScore = double.NaN;
                return ExternalCommandResult.Cancelled;
            }
            Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: ffmpeg exited with exit code {exitCode}." });
            if (exitCode != 0)
                throw new Exception($"ffmpeg failed. (exit code {exitCode})");
            if (double.IsNaN(vmafScoreValue))
                throw new Exception("VMAF score was not reported by ffmpeg.");
            vmafScore = vmafScoreValue;
            return ExternalCommandResult.Completed;
        }

        public static void Log(FileInfo logFile, IEnumerable<string> testLines)
        {
            lock (_loggingLockObject)
            {
                File.AppendAllLines(logFile.FullName, testLines);
            }
        }

        public static void ReportAggregateException(AggregateException ex)
        {
            ReportException(ex);
            foreach (var ex2 in ex.InnerExceptions)
                ReportException(ex2);
        }

        public static void ReportAggregateException(FileInfo logFile, AggregateException ex)
        {
            ReportException(logFile, ex);
            foreach (var ex2 in ex.InnerExceptions)
                ReportException(logFile, ex2);
        }

        public static void ReportException(Exception ex)
        {
            Console.CursorVisible = true;
            Console.WriteLine("----------");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace ?? "");
            for (var innerEx = ex.InnerException; innerEx is not null; innerEx = innerEx.InnerException)
            {
                Console.WriteLine("----------");
                Console.WriteLine(innerEx.Message);
                Console.WriteLine(innerEx.StackTrace ?? "");
            }
            Console.WriteLine("----------");
        }

        public static void ReportException(FileInfo logFile, Exception ex)
        {
            Log(logFile, new[] { "----------", ex.Message, ex.StackTrace ?? "" });
            for (var innerEx = ex.InnerException; innerEx is not null; innerEx = innerEx.InnerException)
                Log(logFile, new[] { "----------", innerEx.Message, innerEx.StackTrace ?? "" });
            Log(logFile, new[] { "----------" });
        }

        private static void ProcessFFmpegOutput(FileInfo logFile, string lineText, ref bool detectedToQuit, ref double maximumDurationSeconds, ref double vmafCalculationResult, IProgress<double> progressReporter)
        {
            if (string.Equals(lineText, "[q] command received. Exiting.", StringComparison.InvariantCulture))
            {
                detectedToQuit = true;
                return;
            }
            if (!lineText.StartsWith("frame=", StringComparison.InvariantCulture))
            {
                Log(logFile, new[] { lineText });

                var durationMatch = _ffmpegConversionDurationPattern.Match(lineText);
                if (durationMatch.Success)
                {

                    if (!int.TryParse(durationMatch.Groups["hours"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out int hours) ||
                        !int.TryParse(durationMatch.Groups["minutes"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out int minutes) ||
                        !double.TryParse(durationMatch.Groups["seconds"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out double seconds))
                    {
                        // Ignore unknown format
                        return;
                    }

                    // If the duration is parsed successfully and is greater than the maximumDurationSeconds, replace the maximumDurationSeconds.
                    var duration = hours * 3600 + minutes * 60 + seconds;
                    if (double.IsNaN(maximumDurationSeconds) || duration > maximumDurationSeconds)
                        maximumDurationSeconds = duration;

                    return;
                }
                else
                {
                    var vmafSoreMatch = _ffmpegVMAFScoreCalculationResultPattern.Match(lineText);
                    if (vmafSoreMatch.Success)
                    {
                        var vmafScoreValueText = vmafSoreMatch.Groups["vmafScoreValue"].Value;
                        if (double.TryParse(vmafScoreValueText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out double vmafScoreValue) &&
                            vmafScoreValue >= 0.0 && vmafScoreValue <= 100.0)
                        {
                            vmafCalculationResult = vmafScoreValue;
                        }
                        else
                        {
                            // このルートには到達しないはず
                            return;
                        }
                    }
                    else
                    {
                        // Ignore unknown format
                        return;
                    }
                }
            }
            else
            {
                if (double.IsNaN(maximumDurationSeconds))
                {
                    // If the duration is not set, the progress cannot be calculated, so nothing is done.
                    return;
                }

                var match = _ffmpegConversionProgressPattern.Match(lineText);
                if (!match.Success)
                {
                    // Ignore unknown format
                    return;
                }

                if (!int.TryParse(match.Groups["hours"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out int hours) ||
                    !int.TryParse(match.Groups["minutes"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out int minutes) ||
                    !double.TryParse(match.Groups["seconds"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out double seconds))
                {
                    // Ignore unknown format
                    return;
                }

                var timeSeconds = hours * 3600 + minutes * 60 + seconds;

                var progress = timeSeconds / maximumDurationSeconds;
                if (progress < 0)
                    progress = 0;
                if (progress > 1)
                    progress = 1;

                progressReporter.Report(progress);
                return;
            }
        }

        private static (bool cancelled, int exitCode) ExecuteCommand(FileInfo programFile, FileInfo logFile, string args, Encoding intputOutputEncoding, Action<OutputStreamType, string>? OutputReader, Action<Process>? childProcessCcanceller)
        {
            var info =
                new ProcessStartInfo
                {
                    Arguments = args,
                    FileName = programFile.FullName,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    StandardInputEncoding = intputOutputEncoding,
                    StandardOutputEncoding = intputOutputEncoding,
                    StandardErrorEncoding = intputOutputEncoding,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                };
#if DEBUG && false
            System.Diagnostics.Debug.WriteLine($"{nameof(MatroskaBatchToolBox)}:INFO: A child process has been started.: \"{info.FileName}\" {info.Arguments}");
#endif
            Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: A child process has been started.: \"{info.FileName}\" {info.Arguments}" });
            var process = Process.Start(info);
            if (process is null)
                throw new Exception("Could not start process");
            try
            {
                process.PriorityClass = ProcessPriorityClass.BelowNormal;
                var cancelled = false;
                var cancellationWatcherTask =
                    Task.Run(() =>
                    {
                        if (childProcessCcanceller is not null)
                        {
                            while (!_requestedCancellation && !process.WaitForExit(1000)) ;
                            if (_requestedCancellation)
                            {
#if DEBUG && false
                            System.Diagnostics.Debug.WriteLine($"{nameof(MatroskaBatchToolBox)}:INFO: Detected cancellation for \"{info.FileName}\"({process.Id}).");
#endif
                                try
                                {
                                    childProcessCcanceller(process);
#if DEBUG && false
                                System.Diagnostics.Debug.WriteLine($"{nameof(MatroskaBatchToolBox)}:INFO: Requested to cancel child process \"{info.FileName}\"({process.Id}).");
#endif
                                    process.WaitForExit();
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

                var standardOutputReaderTask =
                    Task.Run(() =>
                    {
                        // 同じ子プロセスの標準出力と標準エラーからの OutputReader() 呼び出しが(できるだけ)混じらないように、ロックオブジェクトには子プロセスのオブジェクトを指定する。
                        ProcessChildOutput(process, OutputReader, OutputStreamType.StandardOutput, process.StandardOutput);
                    });

                var standardErrorReaderTask =
                    Task.Run(() =>
                    {
                        // 同じ子プロセスの標準出力と標準エラーからの OutputReader() 呼び出しが(できるだけ)混じらないように、ロックオブジェクトには子プロセスのオブジェクトを指定する。
                        ProcessChildOutput(process, OutputReader, OutputStreamType.StandardError, process.StandardError);
                    });
                standardErrorReaderTask.Wait();
                standardOutputReaderTask.Wait();
                cancellationWatcherTask.Wait();
                process.WaitForExit();
                Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: Total process time: {process.TotalProcessorTime.TotalSeconds:F2}[sec]" });
                return (cancelled, process.ExitCode);
            }
            finally
            {
                process.Dispose();
            }
        }

        private static void ProcessChildOutput(object lockObject, Action<OutputStreamType, string>? OutputReader, OutputStreamType streamType, StreamReader streamReader)
        {
            if (OutputReader is not null)
            {
                var buffer = new char[256];
                var cache = "";
                try
                {
                    while (true)
                    {
                        // 単純に streamReader.ReadLine() を使わない理由は、子プロセスが改行で終わらない文字列を出力して
                        // そのまま待ちに入ってしまっても ReadLne() でそれを受け取ることができないから。
                        // 例：何らかの標準入力からの入力を待ち合わせるプロンプトを表示しているなど。
                        // 以下の実装ならば少なくともデバッグウィンドウには改行を待つことなく表示される。
                        var length = streamReader.Read(buffer, 0, buffer.Length);
                        if (length <= 0)
                            break;
                        cache += new string(buffer, 0, length);
#if DEBUG && false
                        System.Diagnostics.Debug.Write(new string(buffer, 0, buffer.Length));
#endif
                        lock (lockObject)
                        {
                            // cache の中からの行の切り出しを繰り返す。
                            while (true)
                            {
                                // 最初の改行文字を探す
                                var endOfLine = cache.IndexOfAny(new[] { '\r', '\n' });
                                if (endOfLine < 0)
                                {
                                    // 改行文字が見つからなかった場合

                                    // 次の読み込みで含まれる文字列も同じ行に含まれなければならないので、OutputReader() は呼び出さずにループを中断する。
                                    break;
                                }
                                if (cache[endOfLine] == '\r')
                                {
                                    // 見つかった改行文字が '\r' だった場合
                                    if (cache.Length <= endOfLine + 1)
                                    {
                                        // 見つかった改行文字が '\r' で、かつ、それが cache の最後の文字だった場合

                                        // 次に読み込まれる文字が '\n' かどうかで挙動を変えないといけないため、OutputReader() は呼び出さずにループを中断する。
                                        break;
                                    }
                                    else if (cache[endOfLine + 1] == '\n')
                                    {
                                        // '\r' の次が '\n' だった場合

                                        // '\r' と '\n' を1つの改行とみなして、OutputReader()の呼び出しと cache の更新を行う。
                                        try
                                        {
                                            OutputReader(streamType, cache[..endOfLine]);
                                        }
                                        catch (Exception)
                                        {
                                        }
                                        cache = cache[(endOfLine + 2)..];
                                    }
                                    else
                                    {
                                        // '\r' の次が '\n' 以外だった場合

                                        // '\r' だけを1つの改行とみなして、OutputReader()の呼び出しと cache の更新を行う。
                                        try
                                        {
                                            OutputReader(streamType, cache[..endOfLine]);
                                        }
                                        catch (Exception)
                                        {
                                        }
                                        cache = cache[(endOfLine + 1)..];
                                    }
                                }
                                else if (cache[endOfLine] == '\n')
                                {
                                    // 見つかった改行文字が '\n' だった場合

                                    // '\n' を1つの改行とみなして、OutputReader()の呼び出しと cache の更新を行う。
                                    try
                                    {
                                        OutputReader(streamType, cache[..endOfLine]);
                                    }
                                    catch (Exception)
                                    {
                                    }
                                    cache = cache[(endOfLine + 1)..];
                                }
                                else
                                    throw new Exception("internal error");
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
