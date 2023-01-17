using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CalcVMAF
{

    public static class Program
    {
        private class ProgramOption
        {
            public string? Scale { get; set; }
            public bool? Log { get; set; }
        }

        public static int Main(string[] args)
        {
            var vmafScorePattern = new Regex(@" VMAF score: (?<vmafScore>\d+\.\d+)[\r\n]", RegexOptions.Compiled);
            var baseDirectoryPath = Path.GetDirectoryName(typeof(Program).Assembly.Location) ?? ".";
            var ffmpegExecutablePath = FindFFmpegExecutablePath(baseDirectoryPath);
            if (string.IsNullOrEmpty(ffmpegExecutablePath))
            {
                Console.Error.WriteLine("'ffmpeg' executable not found.");
                return 1;
            }
            var (originalMovieFilePath, encodedMovieFilePath, option) = ParseArguments(args);
            if (string.IsNullOrEmpty(originalMovieFilePath) || string.IsNullOrEmpty(encodedMovieFilePath))
                return 1;
            var commandParameter = new StringBuilder();
            commandParameter.Append("-hide_banner");
            commandParameter.Append($" -i \"{originalMovieFilePath}\"");
            commandParameter.Append($" -i \"{encodedMovieFilePath}\"");
            if (string.IsNullOrEmpty(option.Scale))
                commandParameter.Append($" -filter_complex \"libvmaf=n_threads=4\"");
            else
                commandParameter.Append($" -filter_complex \"scale={option.Scale},[1]libvmaf=n_threads=4\"");
            commandParameter.Append(" -an -sn");
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                commandParameter.Append(" -f NULL -");
            else
                commandParameter.Append(" -f null /dev/null");
            var processStartInfo = new ProcessStartInfo
            {
                Arguments = commandParameter.ToString(),
                CreateNoWindow = true,
                FileName = ffmpegExecutablePath,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                StandardErrorEncoding = Encoding.UTF8,
                StandardInputEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8,
                UseShellExecute = false,
            };
            var vmafScore = "";
            using (var process = Process.Start(processStartInfo))
            {
                if (process is null)
                {
                    Console.Error.WriteLine("'Failed to start ffmpeg.");
                    return 1;
                }
                _ = Task.Run(() =>
                {
                    while (true)
                    {
                        var c = Console.Read();
                        if (c < 0)
                            break;
                        process.StandardInput.Write(char.ConvertFromUtf32(c));
                    }
                });
                var standardOutputProcessTask = Task.Run(() =>
                {
                    var buffer = new char[1024];
                    while (true)
                    {
                        var length = process.StandardOutput.Read(buffer, 0, buffer.Length);
                        if (length <= 0)
                            break;
                        Console.Write(new string(buffer, 0, length));
                    }
                });
                var standardErrorProcessTask = Task.Run(() =>
                {
                    var buffer = new char[1024];
                    var cache = "";
                    while (true)
                    {
                        var length = process.StandardError.Read(buffer, 0, buffer.Length);
                        if (length <= 0)
                            break;
                        var blockText = new string(buffer, 0, length);
                        Console.Error.Write(blockText);
                        cache = cache + blockText;
                        foreach (var match in vmafScorePattern.Matches(cache).Cast<Match>())
                        {
                            vmafScore = match.Groups["vmafScore"].Value;
                            if (!(option.Log ?? false))
                            Console.WriteLine(vmafScore);
                        }
                        var indexOfLastNewLine = cache.LastIndexOfAny(new[] { '\r', '\n' });
                        if (indexOfLastNewLine >= 0)
                            cache = cache.Substring(indexOfLastNewLine + 1);
                    }
                });
                standardOutputProcessTask.Wait();
                standardErrorProcessTask.Wait();
                process.WaitForExit();
                if (option.Log ?? false)
                {
                    foreach (var logFilePath in Directory.EnumerateFiles(Path.GetDirectoryName(encodedMovieFilePath) ?? ".", $"{Path.GetFileName(encodedMovieFilePath)}.vmaf-*.log"))
                        File.Delete(logFilePath);
                    using (var writer = new StreamWriter($"{encodedMovieFilePath}.vmaf-{vmafScore}.log"))
                        writer.WriteLine($"VMAF score: {vmafScore}");
                }
                return process.ExitCode;
            }
        }

        private static string? FindFFmpegExecutablePath(string baseDirectoryPath)
        {
            var ffmpegExecutablePath1 = Path.Combine(baseDirectoryPath, "ffmpeg");
            var ffmpegExecutablePath2 = Path.Combine(baseDirectoryPath, "ffmpeg.exe");
            try
            {
                if (File.Exists(ffmpegExecutablePath1))
                    return ffmpegExecutablePath1;
                else if (File.Exists(ffmpegExecutablePath2))
                    return ffmpegExecutablePath1;
                else
                    return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static (string originalMovieFilePath, string encodedMovieFilePath, ProgramOption option) ParseArguments(string[] args)
        {
            string? originalMovieFilePath = null;
            string? encodedMovieFilePath = null;
            var option = new ProgramOption();

            for (var index = 0; index < args.Length; ++index)
            {
                if (string.Equals(args[index], "--scale", StringComparison.InvariantCulture))
                {
                    if (!string.IsNullOrEmpty(option.Scale))
                    {
                        Console.Error.WriteLine("Duplicate '--scale' option specified.");
                        return ("", "", new ProgramOption());
                    }
                    if (index + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("'--scale' option requires a value.");
                        return ("", "", new ProgramOption());
                    }
                    var value = args[index + 1];
                    ++index;
                    if (!Regex.IsMatch(value, @"^\d+x\d+$"))
                    {
                        Console.Error.WriteLine($"The '--scale' option value must be a resolution string.: \"{value}\"");
                        return ("", "", new ProgramOption());
                    }
                    option.Scale = value;
                }
                else if (string.Equals(args[index], "--log", StringComparison.InvariantCulture))
                {
                    if (option.Log is not null)
                    {
                        Console.Error.WriteLine("Duplicate '--log' option specified.");
                        return ("", "", new ProgramOption());
                    }
                    option.Log = true;
                }
                else if (args[index].StartsWith("-", StringComparison.InvariantCulture) ||
                         args[index].StartsWith("--", StringComparison.InvariantCulture))
                {
                    Console.Error.WriteLine($"An unsupported option was specified.: \"{args[index]}\"");
                    return ("", "", new ProgramOption());
                }
                else if (string.IsNullOrEmpty(originalMovieFilePath))
                {
                    try
                    {
                        if (!File.Exists(args[index]))
                        {
                            Console.Error.WriteLine($"Original movie file does not exist.: \"{args[index]}\"");
                            return ("", "", new ProgramOption());
                        }
                    }
                    catch (Exception)
                    {
                        Console.Error.WriteLine($"Original movie file does not exist.: \"{args[index]}\"");
                        return ("", "", new ProgramOption());
                    }
                    originalMovieFilePath = args[index];
                }
                else if (string.IsNullOrEmpty(encodedMovieFilePath))
                {
                    try
                    {
                        if (!File.Exists(args[index]))
                        {
                            Console.Error.WriteLine($"Encoded movie file does not exist.: \"{args[index]}\"");
                            return ("", "", new ProgramOption());
                        }
                    }
                    catch (Exception)
                    {
                        Console.Error.WriteLine($"Encoded movie file does not exist.: \"{args[index]}\"");
                        return ("", "", new ProgramOption());
                    }
                    encodedMovieFilePath = args[index];
                }
                else
                {
                    Console.Error.WriteLine($"There is an error in the command line arguments.: {nameof(CalcVMAF)} {string.Join(" ", args)}");
                    return ("", "", new ProgramOption());
                }
            }
            if (string.IsNullOrEmpty(originalMovieFilePath))
            {
                Console.Error.WriteLine($"Original movie file is not specified.: {nameof(CalcVMAF)} {string.Join(" ", args)}");
                return ("", "", new ProgramOption());
            }
            if (string.IsNullOrEmpty(encodedMovieFilePath))
            {
                Console.Error.WriteLine($"Encoded movie file is not specified.: {nameof(CalcVMAF)} {string.Join(" ", args)}");
                return ("", "", new ProgramOption());
            }
            return (originalMovieFilePath, encodedMovieFilePath, option);
        }
    }
}
