using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MatroskaBatchToolBox.Utility;
using Palmtree;
using Palmtree.IO;
using Palmtree.IO.Console;
using Palmtree.Numerics;

namespace EncoderBenchmarkTest
{

    public static partial class Program
    {
        private static readonly string[] _columnNames = ["source file", "souce file length [bytes]", "encoder", "encoded file length [bytes]", "elapsed time [sec]", "VMAF score", "compression ratio", "command line"];

        public static void Main(string[] args)
        {
            ProcessUtility.SetupCurrentProcessPriority();

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

            // 第1パラメタはffmpegの実行ファイルのフルパス
            // 第2パラメタ以降はテストデータ

            // それぞれの動画に対してエンコード時のCPU時間とVMAFスコアを表示する。

            if (args.Length < 2)
                throw new ArgumentException("引数が足りません");

            var ffmpegCommandPath = args[0];
            var logFilePath = typeof(Program).Assembly.GetBaseDirectory().GetFile("benchmark.txt");
            using (var logWriter = new StreamWriter(logFilePath.FullName, false, Encoding.UTF8))
            {
                logWriter.WriteLine(string.Join("\t", _columnNames));
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
                        var sourceFile = new FilePath(sourceFilePath);
                        if (sourceFile.Exists)
                        {
                            var sourceFileLength = sourceFile.Length;
                            var encodedFile = FilePath.CreateTemporaryFile();
                            encodedFile.MoveTo(encodedFile.Directory.GetFile(Path.GetFileNameWithoutExtension(encodedFile.Name) + ".mkv"));
                            try
                            {
                                var match = GetResolutionSpecInFileNamePattern().Match(sourceFile.Name);
                                if (!match.Success)
                                    throw new ApplicationException($"The filename does not contain a resolution specification.: \"{sourceFile.Name}\"");

                                var resolutionWidth = match.Groups["resolutionWidth"].Value.ParseAsInt32();
                                if (resolutionWidth <= 0)
                                    throw new ApplicationException($"There is an error in the resolution specified in the file name.: \"{sourceFile.Name}\"");
                                var resolutionHeight = match.Groups["resolutionHeight"].Value.ParseAsInt32();
                                if (resolutionHeight <= 0)
                                    throw new ApplicationException($"There is an error in the resolution specified in the file name.: \"{sourceFile.Name}\"");
                                int aspectRatioWidth;
                                int aspectRatioHeight;
                                if (match.Groups["aspectRatioWidth"].Success && match.Groups["aspectRatioHeight"].Success)
                                {
                                    aspectRatioWidth = match.Groups["aspectRatioWidth"].Value.ParseAsInt32();
                                    if (aspectRatioWidth <= 0)
                                        throw new ApplicationException($"There is an error in the aspect ratio specified in the file name.: \"{sourceFile.Name}\"");
                                    aspectRatioHeight = match.Groups["aspectRatioHeight"].Value.ParseAsInt32();
                                    if (aspectRatioHeight <= 0)
                                        throw new ApplicationException($"There is an error in the aspect ratio specified in the file name.: \"{sourceFile.Name}\"");
                                }
                                else
                                {
                                    var gcd = resolutionWidth.GreatestCommonDivisor(resolutionHeight);
                                    aspectRatioWidth = resolutionWidth / gcd;
                                    aspectRatioHeight = resolutionHeight / gcd;
                                }

                                try
                                {
                                    var (elapsedTime, fileSize, commandLine) = ConvertMovieFile(ffmpegCommandPath, sourceFile, encodedFile, resolutionWidth, resolutionHeight, aspectRatioWidth, aspectRatioHeight, item.encoder, item.parameter);
                                    TinyConsole.WriteLine(new string('-', 40));
                                    var vmafScore = CalculateVmaf(ffmpegCommandPath, sourceFile, encodedFile, resolutionWidth, resolutionHeight);
                                    var resultItems =
                                        new[]
                                        {
                                            Path.GetFileName(sourceFilePath) ?? "???",
                                            sourceFileLength.ToString("N0", CultureInfo.InvariantCulture),
                                            item.friendlyEncoderName,
                                            encodedFile.Length.ToString("N0", CultureInfo.InvariantCulture),
                                            elapsedTime.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture),
                                            vmafScore.ToString("F6", CultureInfo.InvariantCulture),
                                            ((double)encodedFile.Length / sourceFileLength).ToString("F6", CultureInfo.InvariantCulture),
                                            commandLine,
                                        };
                                    logWriter.WriteLine(string.Join("\t", resultItems));
                                    logWriter.Flush();
                                    TinyConsole.WriteLine(string.Join(", ", resultItems));
                                    TinyConsole.WriteLine(new string('=', 40));
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

            TinyConsole.WriteLine("Complete");
            TinyConsole.Beep();
            _ = TinyConsole.ReadLine();
        }

        private static (TimeSpan cpuTime, ulong encodedFileLength, string commandLine) ConvertMovieFile(string ffmpegCommandPath, FilePath sourceFile, FilePath encodedFile, int resolutionWidth, int resolutionHeight, int aspectRatioWidth, int aspectRatioHeight, string encoder, string encoderDependentParameters)
        {
            var commandParameter = new StringBuilder();
            _ = commandParameter.Append("-hide_banner");
            _ = commandParameter.Append(" -y");
            _ = commandParameter.Append(CultureInfo.InvariantCulture, $" -i \"{sourceFile.FullName}\"");
            _ = commandParameter.Append(CultureInfo.InvariantCulture, $" -s {resolutionWidth}x{resolutionHeight}");
            _ = commandParameter.Append(CultureInfo.InvariantCulture, $" -aspect {aspectRatioWidth}:{aspectRatioHeight}");
            _ = commandParameter.Append(CultureInfo.InvariantCulture, $" -c:v {encoder}");
            if (!string.IsNullOrEmpty(encoderDependentParameters))
                _ = commandParameter.Append(CultureInfo.InvariantCulture, $" {encoderDependentParameters}");
            _ = commandParameter.Append(" -g 240");
            _ = commandParameter.Append(" -an");
            _ = commandParameter.Append(" -sn");
            _ = commandParameter.Append(CultureInfo.InvariantCulture, $" \"{encodedFile.FullName}\"");
            var summaryOfCommandLine = $"{Path.GetFileName(ffmpegCommandPath)} {commandParameter.ToString().Replace(sourceFile.FullName, sourceFile.Name).Replace(encodedFile.FullName, encodedFile.Name)}";
            TinyConsole.WriteLine($"commandLine: {summaryOfCommandLine}");
            var totalProcessorTime = ExecuteFfmpegCommand(ffmpegCommandPath, commandParameter.ToString());
            var encodedFileLength = encodedFile.Length;
            return (totalProcessorTime, encodedFileLength, summaryOfCommandLine);
        }

        private static double CalculateVmaf(string ffmpegCommandPath, FilePath sourceFile, FilePath encodedFile, int resolutionWidth, int resolutionHeight)
        {
            var commandParameter = new StringBuilder();
            _ = commandParameter.Append("-hide_banner");
            _ = commandParameter.Append(CultureInfo.InvariantCulture, $" -i \"{sourceFile.FullName}\"");
            _ = commandParameter.Append(CultureInfo.InvariantCulture, $" -i \"{encodedFile.FullName}\"");
            _ = commandParameter.Append(CultureInfo.InvariantCulture, $" -filter_complex \"scale={resolutionWidth}x{resolutionHeight},[1]libvmaf\"");
            _ = commandParameter.Append(" -an -sn");
            _ = OperatingSystem.IsWindows()
                ? commandParameter.Append(" -f NULL -")
                : commandParameter.Append(" -f null /dev/null");
            double? vmafScore = null;
            _ =
               ExecuteFfmpegCommand(
                   ffmpegCommandPath,
                   commandParameter.ToString(),
                   lineText =>
                   {
                       var match = GetVmafScorePattern().Match(lineText);
                       if (match.Success && match.Groups["vmafScore"].Value.TryParse(out double vmafScoreValue))
                           vmafScore = vmafScoreValue;
                   });
            return
                vmafScore is not null
                ? vmafScore.Value
                : throw new ApplicationException("VMAF score was not reported.");
        }

        private static TimeSpan ExecuteFfmpegCommand(string ffmpegCommandPath, string commandParameter, Action<string>? lineTextHander = null)
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
            var process =
                Process.Start(processStartInfo)
                ?? throw new ApplicationException("Failed to start child process.");
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
                            if (lineText == "[q] command received. Exiting.")
                            {
                                cancelled = true;
                                break;
                            }

                            if (lineTextHander is not null)
                            {
                                try
                                {
                                    lineTextHander(lineText);
                                }
                                catch (Exception)
                                {
                                }
                            }

                            if (lineText.StartsWith("frame=", StringComparison.Ordinal))
                                TinyConsole.Write(lineText + "\r");
                            else
                                TinyConsole.WriteLine(lineText);
                        }
                    });
                standardErrorProcessingTask.Wait();
                process.WaitForExit();
                if (cancelled)
                    throw new ApplicationException("A child process was aborted by a user.");
                var exitCode = process.ExitCode;
                return
                    exitCode == 0
                    ? process.TotalProcessorTime
                    : throw new ApplicationException($"Process terminated abnormally. : exitCode={exitCode}");
            }
            finally
            {
                process.Dispose();
            }
        }

        [GeneratedRegex(@"VMAF score\s*:\s*(?<vmafScore>\d+(\.\d+)?)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)]
        private static partial Regex GetVmafScorePattern();

        [GeneratedRegex(@"\[(?<resolutionWidth>\d+)x(?<resolutionHeight>\d+)( (?<aspectRatioWidth>\d+)(to|：)(?<aspectRatioHeight>\d+))?\]", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)]
        private static partial Regex GetResolutionSpecInFileNamePattern();
    }
}
