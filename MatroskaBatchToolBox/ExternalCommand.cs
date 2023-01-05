using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MatroskaBatchToolBox
{
    internal static class ExternalCommand
    {
        private enum OutputStreamType
        {
            StandardOutput,
            StandardError,
        }

        private const string _ffmpegPathEnvironmentVariableName = "FFMPEG_PATH";
        private static readonly object _loggingLockObject;
        private static readonly Regex _ffmpegNormalizeProgressPattern;
        private static readonly Regex _moveStreamInfoPattern;
        private static readonly Regex _moveStreamInfoAudioDetailPattern;
        private static readonly Regex _audioChannelLayoutPattern;
        private static readonly Regex _ffmpegConversionDurationPattern;
        private static readonly Regex _ffmpegConversionProgressPattern;
        private static bool _requestedCancellation;

        static ExternalCommand()
        {
            _loggingLockObject = new object();
            _ffmpegNormalizeProgressPattern = new Regex(@"^(((?<phase>Stream)\s+(?<currentStream>\d+)/(?<totalStream>\d+))|(?<phase>Second Pass)|(?<phase>File)):\s+(?<Percentage>\d+)%\|", RegexOptions.Compiled);
            _moveStreamInfoPattern = new Regex(@"^  Stream #0:(?<id>\d+)(?<language>\([^\)]+\))?: (?<streamType>Video|Audio|Subtitle): (?<detail>.*)$", RegexOptions.Compiled);
            _moveStreamInfoAudioDetailPattern = new Regex(@"^\s*(?<codec>[^ ,][^,]*[^ ,])\s*,\s*(?<samplingFrequency>[^ ,][^,]*[^ ,]) Hz\s*,\s*(?<channelLayout>[^ ,][^,]*[^ ,])\s*,", RegexOptions.Compiled);
            _audioChannelLayoutPattern = new Regex(@"(?<layoutType>[^\(]+)(\([^\)]+\))?", RegexOptions.Compiled);
            _ffmpegConversionDurationPattern = new Regex(@"\s*(Duration|DURATION)\s*:\s*(?<hours>\d+):(?<minutes>\d+):(?<seconds>[\d\.]+)", RegexOptions.Compiled);
            _ffmpegConversionProgressPattern = new Regex(@" time=(?<hours>\d+):(?<minutes>\d+):(?<seconds>[\d\.]+) ", RegexOptions.Compiled);
            _requestedCancellation = false;
#if false
#if DEBUG   
            if (!_ffmpegConversionDurationPattern.IsMatch("  Duration: 00:17:01.25, start: 0.000000, bitrate: 1103 kb/s"))
                throw new Exception();
            if (!_ffmpegConversionDurationPattern.IsMatch("      DURATION        : 00:17:01.187000000"))
                throw new Exception();
            if (!_ffmpegConversionProgressPattern.IsMatch("frame=  291 fps=0.0 q=28.0 size=     244kB time=00:00:14.82 bitrate= 134.7kbits/s speed=  29x    "))
                throw new Exception();
            throw new Exception();
#endif
#endif

        }

        public static void AbortExternalCommands()
        {
            _requestedCancellation = true;
        }

        public static void NormalizeAudioFile(FileInfo logFile, FileInfo inFile, FileInfo outFile, IProgress<double> progressReporter)
        {
            System.Environment.SetEnvironmentVariable(_ffmpegPathEnvironmentVariableName, Settings.CurrentSettings.FFmpegCommandFile.FullName);
            var commandParameter = $"\"{inFile.FullName}\" -o \"{outFile.FullName}\" -pr --keep-loudness-range-target --audio-codec {Settings.CurrentSettings.AudioCodec}";
            NormalizeAudioFile(logFile, commandParameter, progressReporter);
        }

        private static void NormalizeAudioFile(FileInfo logFile, string commandParameter, IProgress<double> progressReporter)
        {
            var totalStream = 1;
            var exitCode =
                ExecuteCommand(
                    Settings.CurrentSettings.FFmpegNormalizeCommandFile,
                    logFile,
                    commandParameter,
                    (type, text) =>
                    {
                        if (type != OutputStreamType.StandardError)
                            return;
                        if (string.IsNullOrEmpty(text))
                            return;
                        var match = _ffmpegNormalizeProgressPattern.Match(text);
                        if (!match.Success)
                        {
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
#if DEBUG
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
                            Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Settings.CurrentSettings.FFmpegCommandFile.Name))
                            .Where(proc => string.Equals(proc?.MainModule?.FileName ?? "", Settings.CurrentSettings.FFmpegCommandFile.FullName, StringComparison.InvariantCulture))
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
            if (exitCode != 0)
                throw new Exception("Failed in ffmpeg-normalize");
        }

        public static void CopyMovieFile(FileInfo logFile, FileInfo inFile, FileInfo outFile, IProgress<double> progressReporter)
        {
            var commandParameter = $"-y -i \"{inFile.FullName}\" -c:v copy -c:a copy -c:s copy \"{outFile}\"";
            var detectedToQuit = false;
            var maximumDurationSeconds = double.NaN;
            var exitCode =
                ExecuteCommand(
                    Settings.CurrentSettings.FFmpegCommandFile,
                    logFile,
                    commandParameter,
                    (type, text) => ProcessFFmpegOutput(logFile, text, ref detectedToQuit, ref maximumDurationSeconds, progressReporter),
                    proc =>
                    {
                        proc.StandardInput.Write("q");
                    });
            if (detectedToQuit)
            {
                Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: ffmpeg aborted at user request." });
                throw new Exception("ffmpeg aborted at user request.");
            }
            Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: ffmpeg exited with exit code {exitCode}." });
            if (exitCode != 0)
                throw new Exception($"ffmpeg failed. (exit code {exitCode})");
        }

        public static void ResizeMovieFile(FileInfo logFile, FileInfo inFile, string resolutionSpec, string aspectRateSpec, FileInfo outFile, IProgress<double> progressReporter)
        {
            var commandParameter = $"-y -i \"{inFile.FullName}\" -s {resolutionSpec} -aspect {aspectRateSpec} -c:a copy -c:s copy \"{outFile}\"";
            var detectedToQuit = false;
            var maximumDurationSeconds = double.NaN;
            var exitCode =
                ExecuteCommand(
                    Settings.CurrentSettings.FFmpegCommandFile,
                    logFile,
                    commandParameter,
                    (type, text) => ProcessFFmpegOutput(logFile, text, ref detectedToQuit, ref maximumDurationSeconds, progressReporter),
                    proc =>
                    {
                        proc.StandardInput.Write("q");
                    });
            if (detectedToQuit)
            {
                Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: ffmpeg aborted at user request." });
                throw new Exception("ffmpeg aborted at user request.");
            }
            Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: ffmpeg exited with exit code {exitCode}." });
            if (exitCode != 0)
                throw new Exception($"ffmpeg failed. (exit code {exitCode})");
        }

        private static void ProcessFFmpegOutput(FileInfo logFile, string lineText, ref bool detectedToQuit, ref double maximumDurationSeconds, IProgress<double> progressReporter)
        {
            if (string.Equals(lineText, "[q] command received. Exiting.", StringComparison.InvariantCulture))
            {
                detectedToQuit = true;
                return;
            }
            if (!lineText.StartsWith("frame=", StringComparison.InvariantCulture))
            {
                Log(logFile, new[] { lineText });

                var match = _ffmpegConversionDurationPattern.Match(lineText);
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

                // If the duration is parsed successfully and is greater than the maximumDurationSeconds, replace the maximumDurationSeconds.
                var duration = hours * 3600 + minutes * 60 + seconds;
                if (double.IsNaN(maximumDurationSeconds) || duration > maximumDurationSeconds)
                    maximumDurationSeconds = duration;

                return;
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

        private static int ExecuteCommand(FileInfo programFile, FileInfo logFile, string args, Action<OutputStreamType, string>? OutputReader, Action<Process>? childProcessCcanceller)
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
                    StandardErrorEncoding = Encoding.UTF8,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                };
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"{nameof(MatroskaBatchToolBox)}:INFO: A child process has been started.: \"{info.FileName}\" {info.Arguments}");
#endif
            Log(logFile, new[] { $"{nameof(MatroskaBatchToolBox)}: INFO: A child process has been started.: \"{info.FileName}\" {info.Arguments}" });
            var process = Process.Start(info);
            if (process is null)
                throw new Exception("Could not start process");

            var cancellationWatcherTask =
                Task.Run(() =>
                {
                    if (childProcessCcanceller is not null)
                    {
                        while (!_requestedCancellation && !process.WaitForExit(1000)) ;
                        if (_requestedCancellation)
                        {
#if DEBUG
                            System.Diagnostics.Debug.WriteLine($"{nameof(MatroskaBatchToolBox)}:INFO: Detected cancellation for \"{info.FileName}\"({process.Id}).");
#endif
                            try
                            {
                                childProcessCcanceller(process);
#if DEBUG
                                System.Diagnostics.Debug.WriteLine($"{nameof(MatroskaBatchToolBox)}:INFO: Requested to cancel child process \"{info.FileName}\"({process.Id}).");
#endif
                                process.WaitForExit();
                            }
                            catch (Exception)
                            {
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
            return process.ExitCode;
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
                        cache = cache + new string(buffer, 0, length);
#if DEBUG
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
                                    cache = cache.Substring(endOfLine + 1);
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
