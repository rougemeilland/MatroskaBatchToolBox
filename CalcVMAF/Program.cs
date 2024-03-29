﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Palmtree.IO;
using Palmtree.IO.Console;

namespace CalcVmaf
{

    public static class Program
    {
        private class ProgramOption
        {
            public bool? Log { get; set; }
        }

        private static readonly char[] _anyOfCarrigeReturnOrNewLine = new[] { '\r', '\n' };

        public static int Main(string[] args)
        {
            // このプロセスでは Ctrl+C を無視する。
            TinyConsole.CancelKeyPress += (sender, e) => e.Cancel = true;

            // コマンドの入出力エンコーディングを UTF8 にする
            TinyConsole.InputEncoding = Encoding.UTF8;
            TinyConsole.OutputEncoding = Encoding.UTF8;
            TinyConsole.DefaultTextWriter = ConsoleTextWriterType.StandardError;

            var vmafScorePattern = new Regex(@" VMAF score: (?<vmafScore>\d+\.\d+)[\r\n]", RegexOptions.Compiled);
            var baseDirectoryPath = Path.GetDirectoryName(typeof(Program).Assembly.Location) ?? ".";
            var ffmpegExecutablePath = FindFfmpegExecutablePath(baseDirectoryPath);
            if (string.IsNullOrEmpty(ffmpegExecutablePath))
            {
                TinyConsole.Error.WriteLine("'ffmpeg' executable not found.");
                return 1;
            }

            var (originalMovieFile, encodedMovieFile, option) = ParseArguments(args);
            if (originalMovieFile is null || encodedMovieFile is null || option is null)
                return 1;
            var logFilePattern = new Regex($"^{encodedMovieFile.Name}\\.vmaf-.*\\.log$", RegexOptions.IgnoreCase);
            foreach (var oldLogFilePath in encodedMovieFile.Directory.EnumerateFiles().Where(f => logFilePattern.IsMatch(f.Name)))
                oldLogFilePath.Delete();
            var logFilePath = $"{encodedMovieFile.FullName}.vmaf.log";
            var logWriter = option.Log ?? false ? new StreamWriter(logFilePath) : null;
            try
            {
                var commandParameter = new StringBuilder();
                commandParameter.Append("-hide_banner");
                commandParameter.Append($" -i \"{originalMovieFile.FullName}\"");
                commandParameter.Append($" -i \"{encodedMovieFile.FullName}\"");
                commandParameter.Append($" -filter_complex \"[0:v]settb=1/AVTB,setpts=PTS-STARTPTS[original];[1:v]settb=1/AVTB,setpts=PTS-STARTPTS[1v];[original][1v]scale2ref=flags=bicubic,libvmaf=model=version=vmaf_v0.6.1\\\\:name=vmaf\\\\:n_threads=4:shortest=1:repeatlast=0\"");
#if false // -an をつけると、実行中の time と speed が正常に表示されなくなる。
            commandParameter.Append(" -an -sn");
#endif
                if (OperatingSystem.IsWindows())
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
                if (logWriter is not null)
                {
                    logWriter.WriteLine($"command: {processStartInfo.FileName} {processStartInfo.Arguments}");
                    logWriter.WriteLine(new string('=', 40));
                    logWriter.WriteLine("");
                }

                var vmafScore = "";
                var process = Process.Start(processStartInfo);
                if (process is null)
                {
                    TinyConsole.Error.WriteLine("'Failed to start ffmpeg.");
                    return 1;
                }

                try
                {
                    _ = Task.Run(() =>
                    {
                        while (true)
                        {
                            var c = TinyConsole.Read();
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
                            var blockText = new string(buffer, 0, length);
                            TinyConsole.Write(blockText);
                            logWriter?.Write(blockText);
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
                            TinyConsole.Error.Write(blockText);
                            logWriter?.Write(blockText);
                            cache += blockText;
                            foreach (var match in vmafScorePattern.Matches(cache).Cast<Match>())
                            {
                                vmafScore = match.Groups["vmafScore"].Value;
                                if (!(option.Log ?? false))
                                    TinyConsole.Out.WriteLine(vmafScore);
                            }

                            var indexOfLastNewLine = cache.LastIndexOfAny(_anyOfCarrigeReturnOrNewLine);
                            if (indexOfLastNewLine >= 0)
                                cache = cache[(indexOfLastNewLine + 1)..];
                        }
                    });
                    standardOutputProcessTask.Wait();
                    standardErrorProcessTask.Wait();
                    process.WaitForExit();
                    if ((option.Log ?? false) && logWriter is not null)
                    {
                        logWriter.WriteLine("");
                        logWriter.WriteLine(new string('=', 40));
                        logWriter.WriteLine($"Original file: \"{originalMovieFile.FullName}\"");
                        logWriter.WriteLine($"Original file size [bytes]: \"{originalMovieFile.Length:N0}\"");
                        logWriter.WriteLine($"Encoded file: \"{encodedMovieFile.FullName}\"");
                        logWriter.WriteLine($"Encoded file size [bytes]: \"{encodedMovieFile.Length:N0}\"");
                        logWriter.WriteLine($"Compression ratio (<Encoded file size> / <Original file size>): \"{100.0 * encodedMovieFile.Length / originalMovieFile.Length:F2}%\"");
                        logWriter.WriteLine($"VMAF score: {vmafScore}");
                        logWriter.Flush();
                        logWriter.Dispose();
                        logWriter = null;
                        File.Move(logFilePath, $"{encodedMovieFile.FullName}.vmaf-{vmafScore}.log", true);
                    }

                    return process.ExitCode;
                }
                finally
                {
                    process.Dispose();
                }
            }
            finally
            {
                logWriter?.Dispose();
            }
        }

        private static string? FindFfmpegExecutablePath(string baseDirectoryPath)
        {
            var ffmpegExecutablePath1 = Path.Combine(baseDirectoryPath, "ffmpeg");
            var ffmpegExecutablePath2 = Path.Combine(baseDirectoryPath, "ffmpeg.exe");
            try
            {
                return
                    File.Exists(ffmpegExecutablePath1)
                    ? ffmpegExecutablePath1
                    : File.Exists(ffmpegExecutablePath2)
                    ? ffmpegExecutablePath1
                    : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static (FilePath? originalMovieFilePath, FilePath? encodedMovieFilePath, ProgramOption? option) ParseArguments(string[] args)
        {
            FilePath? originalMovieFile = null;
            FilePath? encodedMovieFile = null;
            var option = new ProgramOption();

            for (var index = 0; index < args.Length; ++index)
            {
                if (args[index] == "--log")
                {
                    if (option.Log is not null)
                    {
                        TinyConsole.Error.WriteLine("Duplicate '--log' option specified.");
                        return (null, null, null);
                    }

                    option.Log = true;
                }
                else if (args[index].StartsWith('-') ||
                         args[index].StartsWith("--", StringComparison.Ordinal))
                {
                    TinyConsole.Error.WriteLine($"An unsupported option was specified.: \"{args[index]}\"");
                    return (null, null, null);
                }
                else if (originalMovieFile is null)
                {
                    FilePath file;
                    try
                    {
                        file = new FilePath(args[index]);
                        if (!file.Exists)
                        {
                            TinyConsole.Error.WriteLine($"Original movie file does not exist.: \"{args[index]}\"");
                            return (null, null, null);
                        }
                    }
                    catch (Exception)
                    {
                        TinyConsole.Error.WriteLine($"Original movie file does not exist.: \"{args[index]}\"");
                        return (null, null, null);
                    }

                    originalMovieFile = file;
                }
                else if (encodedMovieFile is null)
                {
                    FilePath file;
                    try
                    {
                        file = new FilePath(args[index]);
                        if (!file.Exists)
                        {
                            TinyConsole.Error.WriteLine($"Encoded movie file does not exist.: \"{args[index]}\"");
                            return (null, null, null);
                        }
                    }
                    catch (Exception)
                    {
                        TinyConsole.Error.WriteLine($"Encoded movie file does not exist.: \"{args[index]}\"");
                        return (null, null, null);
                    }

                    encodedMovieFile = file;
                }
                else
                {
                    TinyConsole.Error.WriteLine($"There is an error in the command line arguments.: {nameof(CalcVmaf)} {string.Join(" ", args)}");
                    return (null, null, null);
                }
            }

            if (originalMovieFile is null)
            {
                TinyConsole.Error.WriteLine($"Original movie file is not specified.: {nameof(CalcVmaf)} {string.Join(" ", args)}");
                return (null, null, null);
            }

            if (encodedMovieFile is null)
            {
                TinyConsole.Error.WriteLine($"Encoded movie file is not specified.: {nameof(CalcVmaf)} {string.Join(" ", args)}");
                return (null, null, null);
            }

            return (originalMovieFile, encodedMovieFile, option);
        }
    }
}
