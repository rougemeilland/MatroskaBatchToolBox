using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MatroskaBatchToolBox;

namespace EncoderBenchmarkTest
{

    public static class Program
    {
        private static readonly Regex _vmafScorePattern;
        private static readonly Regex _resolutionSpecInFileNamePattern;

        static Program()
        {
            _vmafScorePattern = new Regex(@"VMAF score\s*:\s*(?<vmafScore>\d+(\.\d+)?)", RegexOptions.Compiled);
            _resolutionSpecInFileNamePattern = new Regex(@"\[(?<resolutionWidth>\d+)x(?<resolutionHeight>\d+)( (?<aspectRatioWidth>\d+)(to|：)(?<aspectRatioHeight>\d+))?\]", RegexOptions.Compiled);
        }
        public static void Main(string[] args)
        {
            // 第1パラメタはffmpegの実行ファイルのフルパス
            // 第2パラメタ以降はテストデータ

            // それぞれの動画に対してエンコード時のCPU時間とVMAFスコアを表示する。

            if (args.Length < 2)
                throw new ArgumentException("引数が足りません");

            var ffmpegCommandPath = args[0];
            var logFilePath = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location) ?? ".", "benchmark.txt");
            using (var logWriter = new StreamWriter(logFilePath, false, Encoding.UTF8))
            {
                logWriter.WriteLine(string.Join("\t", new[] { "source file", "souce file length [bytes]", "encoder", "encoded file length [bytes]", "elapsed time [sec]", "VMAF score", "compression ratio", "command line" }));
                foreach (var item in new[]
                {
                    new { encoder = "libaom-av1", friendlyEncoderName = "libaom-av1(crf23)", parameter = "-crf 23 -cpu-used 5 -b:v 0" },
                    // new { encoder = "librav1e", friendlyEncoderName = "librav1e(qp23)", parameter = "-qp 23 -speed 5" }, // チューニングが困難そうなので除外
                    new { encoder = "libsvtav1", friendlyEncoderName = "libsvtav1(crf23)", parameter = "-crf 23 -preset 2" },
                    // new { encoder = "av1_nvenc", friendlyEncoderName = "av1_nvenc(qp23)", parameter = "-rc 0 -qp 23" }, // ドライバがサポートしていないので除外
                    new { encoder = "av1_qsv", friendlyEncoderName = "av1_qsv(q20)", parameter = "-pix_fmt nv12 -q:v 20" },
                    new { encoder = "libx265", friendlyEncoderName = "libx265(crf18)", parameter = "-crf 18" },
                    // new { encoder = "hevc_amf", friendlyEncoderName = "hevc_amf(qp18)", parameter = "-quality 0 -rc 0 -qp_p 18 -qp_i 18" }, // 対象ハードウェアが AMD Radeon シリーズなので除外
                    // new { encoder = "hevc_mf", friendlyEncoderName = "hevc_mf(quality18)", parameter = "-rate_control 3 -quality 18" }, // 実際の使用に関する情報がないので除外
                    // new { encoder = "hevc_nvenc", friendlyEncoderName = "hevc_nvenc(qp18)", parameter = "-rc 0 -qp 18" }, // ドライバがサポートしていないので除外
                    new { encoder = "hevc_qsv", friendlyEncoderName = "hevc_qsv(q20)", parameter = "-pix_fmt nv12 -q:v 20" },
                    //new { encoder = "libx264", friendlyEncoderName = "libx264(crf18)", parameter = "-crf 18" }, // 要求する品質を保つために圧縮率が悪いので除外
                    //new { encoder = "h264_amf", friendlyEncoderName = "h264_amf(qp18)", parameter = "-quality 2 -rc 0 -qp_i 18 -qp_p 18 -qp_b 18" }, // 要求する品質を保つために圧縮率が悪いので除外
                    //new { encoder = "h264_mf", friendlyEncoderName = "h264_mf(quality18)", parameter = "-rate_control 3 -quality 18" }, // 要求する品質を保つために圧縮率が悪いので除外
                    //new { encoder = "h264_nvenc", friendlyEncoderName = "h264_nvenc(cq18)", parameter = "-rc 0 -cq 18" }, // 要求する品質を保つために圧縮率が悪いので除外
                    //new { encoder = "h264_qsv", friendlyEncoderName = "h264_qsv", parameter = "" }, // 要求する品質を保つために圧縮率が悪いので除外
                })
                {
                    foreach (var sourceFilePath in args.Skip(1).OrderBy(arg => Path.GetFileName(arg)))
                    {
                        var sourceFile = new FileInfo(sourceFilePath);
                        if (sourceFile.Exists)
                        {
                            var sourceFileLength = sourceFile.Length;
                            var encodedFile = new FileInfo(Path.GetTempFileName());
                            encodedFile.MoveTo(Path.Combine(encodedFile.DirectoryName ?? ".", Path.GetFileNameWithoutExtension(encodedFile.Name) + ".mkv"));
                            try
                            {
                                var match = _resolutionSpecInFileNamePattern.Match(sourceFile.Name);
                                if (!match.Success)
                                    throw new Exception($"The filename does not contain a resolution specification.: \"{sourceFile.Name}\"");

                                int resolutionWidth = int.Parse(match.Groups["resolutionWidth"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
                                if (resolutionWidth <= 0)
                                    throw new Exception($"There is an error in the resolution specified in the file name.: \"{sourceFile.Name}\"");
                                int resolutionHeight = int.Parse(match.Groups["resolutionHeight"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
                                if (resolutionHeight <= 0)
                                    throw new Exception($"There is an error in the resolution specified in the file name.: \"{sourceFile.Name}\"");
                                int aspectRatioWidth;
                                int aspectRatioHeight;
                                if (match.Groups["aspectRatioWidth"].Success && match.Groups["aspectRatioHeight"].Success)
                                {
                                    aspectRatioWidth = int.Parse(match.Groups["aspectRatioWidth"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
                                    if (aspectRatioWidth <= 0)
                                        throw new Exception($"There is an error in the aspect ratio specified in the file name.: \"{sourceFile.Name}\"");
                                    aspectRatioHeight = int.Parse(match.Groups["aspectRatioHeight"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
                                    if (aspectRatioHeight <= 0)
                                        throw new Exception($"There is an error in the aspect ratio specified in the file name.: \"{sourceFile.Name}\"");
                                }
                                else
                                {
                                    var gcd = ExtendedMath.GreatestCommonDivisor(resolutionWidth, resolutionHeight);
                                    aspectRatioWidth = resolutionWidth / gcd;
                                    aspectRatioHeight = resolutionHeight / gcd;
                                }
                                try
                                {
                                    var (elapsedTime, fileSize, commandLine) = ConvertMovieFile(ffmpegCommandPath, sourceFile, encodedFile, resolutionWidth, resolutionHeight, aspectRatioWidth, aspectRatioHeight, item.encoder, item.parameter);
                                    Console.WriteLine(new string('-', 40));
                                    var vmafScore = CalculateVMAF(ffmpegCommandPath, sourceFile, encodedFile, resolutionWidth, resolutionHeight);
                                    var resultItems = new[] { Path.GetFileName(sourceFilePath) ?? "???", sourceFileLength.ToString("N0"), item.friendlyEncoderName, encodedFile.Length.ToString("N0"), elapsedTime.TotalSeconds.ToString("F2"), vmafScore.ToString("F6"), ((double)encodedFile.Length / sourceFileLength).ToString("F6"), commandLine};
                                    logWriter.WriteLine(string.Join("\t", resultItems));
                                    logWriter.Flush();
                                    Console.WriteLine(string.Join(", ", resultItems));
                                    Console.WriteLine(new string('=', 40));
                                }
                                catch (Exception)
                                {
                                }
                            }
                            finally
                            {
                                encodedFile.Delete();
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Complete");
            Console.Beep();
            Console.ReadLine();
        }

        private static (TimeSpan cpuTime, long encodedFileLength, string commandLine) ConvertMovieFile(string ffmpegCommandPath, FileInfo sourceFile, FileInfo encodedFile, int resolutionWidth, int resolutionHeight, int aspectRatioWidth, int aspectRatioHeight, string encoder, string encoderDependentParameters)
        {
            var commandParameter = new StringBuilder();
            commandParameter.Append("-hide_banner");
            commandParameter.Append(" -y");
            commandParameter.Append($" -i \"{sourceFile.FullName}\"");
            commandParameter.Append($" -s {resolutionWidth}x{resolutionHeight}");
            commandParameter.Append($" -aspect {aspectRatioWidth}:{aspectRatioHeight}");
            commandParameter.Append($" -c:v {encoder}");
            if (!string.IsNullOrEmpty(encoderDependentParameters))
                commandParameter.Append($" {encoderDependentParameters}");
            commandParameter.Append($" -g 240");
            commandParameter.Append($" -an");
            commandParameter.Append($" -sn");
            commandParameter.Append($" \"{encodedFile.FullName}\"");
            var summaryOfCommandLine = $"{Path.GetFileName(ffmpegCommandPath)} {commandParameter.ToString().Replace(sourceFile.FullName, sourceFile.Name).Replace(encodedFile.FullName, encodedFile.Name)}";
            Console.WriteLine($"commandLine: {summaryOfCommandLine}");
            var totalProcessorTime = ExecuteFFmpegCommand(ffmpegCommandPath, commandParameter.ToString());
            var encodedFileLength = encodedFile.Length;
            return (totalProcessorTime, encodedFileLength, summaryOfCommandLine);
        }

        private static double CalculateVMAF(string ffmpegCommandPath, FileInfo sourceFile, FileInfo encodedFile, int resolutionWidth, int resolutionHeight)
        {
            var commandParameter = new StringBuilder();
            commandParameter.Append("-hide_banner");
            commandParameter.Append($" -i \"{sourceFile.FullName}\"");
            commandParameter.Append($" -i \"{encodedFile.FullName}\"");
            commandParameter.Append($" -filter_complex \"scale={resolutionWidth}x{resolutionHeight},[1]libvmaf\"");
            commandParameter.Append(" -an -sn");
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                commandParameter.Append(" -f NULL -");
            else
                commandParameter.Append(" -f null /dev/null");
            double? vmafScore = null;
            var _ =
                ExecuteFFmpegCommand(
                    ffmpegCommandPath,
                    commandParameter.ToString(),
                    textLine =>
                    {
                        var match = _vmafScorePattern.Match(textLine);
                        if (match.Success && double.TryParse(match.Groups["vmafScore"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out double vmafScoreValue))
                            vmafScore = vmafScoreValue;   
                    });
            if (vmafScore is null)
                throw new Exception("VMAF score was not reported.");
            return vmafScore.Value;
        }

        private static TimeSpan ExecuteFFmpegCommand(string ffmpegCommandPath, string commandParameter, Action<string>? textLineHander = null)
        {
            var processStartInfo = new ProcessStartInfo()
            {
                FileName = ffmpegCommandPath,
                Arguments = commandParameter,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardError = true,
                StandardErrorEncoding = Encoding.UTF8,
            };
            var process = Process.Start(processStartInfo);
            if (process is null)
                throw new Exception("Failed to start child process.");
            try
            {
                var cancelled = false;
                var standardErrorProcessingTask =
                    Task.Run(() =>
                    {
                        while (true)
                        {
                            var lineText = process.StandardError.ReadLine();
                            if (lineText is null)
                                break;
                            if (string.Equals(lineText, "[q] command received. Exiting.", StringComparison.InvariantCulture))
                            {
                                cancelled = true;
                                break;
                            }
                            if (textLineHander is not null)
                            {
                                try
                                {
                                    textLineHander(lineText);
                                }
                                catch (Exception)
                                {
                                }
                            }
                            if (lineText.StartsWith("frame=", StringComparison.InvariantCulture))
                                Console.Write(lineText + "\r");
                            else
                                Console.WriteLine(lineText);
                        }
                    });
                standardErrorProcessingTask.Wait();
                process.WaitForExit();
                if (cancelled)
                    throw new Exception("A child process was aborted by a user.");
                var exitCode = process.ExitCode;
                if (exitCode != 0)
                    throw new Exception($"Process terminated abnormally. : exitCode={exitCode}");
                return process.TotalProcessorTime;
            }
            finally
            {
                process.Dispose();
            }
        }
    }
}
