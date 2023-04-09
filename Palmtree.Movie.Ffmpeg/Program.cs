using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Palmtree.Movie.Ffmpeg
{
    public static class Program
    {
        private static readonly string _thisCommandName = typeof(Program).Assembly.GetName().Name ?? "???";
        private static readonly string _baseDirectory = AppContext.BaseDirectory;

        public static int Main(string[] args)
        {
            var newArgs = MakeFfmpegCommandArguments(args, out var tempFilePath);
            try
            {
                var exitCode = ExecuteFfpegCommand(newArgs, tempFilePath is not null);
                if (exitCode == 0 && tempFilePath is not null)
                {
                    using var inStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
                    using var outStream = TinyConsole.OpenStandardOutput();
                    CopyStream(inStream, outStream);
                }

                return exitCode;
            }
            catch (Exception ex)
            {
                TinyConsole.ForegroundColor = ConsoleColor.Red;
                TinyConsole.Error.Write($"{_thisCommandName}:ERROR: {ex.Message}");
                TinyConsole.ResetColor();
                TinyConsole.WriteLine();
                return 1;
            }
            finally
            {
                if (tempFilePath is not null)
                    File.Delete(tempFilePath);
            }
        }

        private static string[] MakeFfmpegCommandArguments(string[] args, out string? tempFilePath)
        {
            var newArgs = new string[args.Length];
            tempFilePath = null;
            Array.Copy(args, newArgs, args.Length);
            for (var index = 0; index + 1 < args.Length; ++index)
            {
                if (newArgs[index] != "-i" && (newArgs[index + 1] == "-" || newArgs[index + 1] == "pipe:1"))
                {
                    tempFilePath = Path.GetTempFileName();
                    newArgs[index + 1] = tempFilePath;
                }
            }

            return newArgs;
        }

        private static int ExecuteFfpegCommand(IEnumerable<string> args, bool force)
        {
            if (force && args.None(arg => arg == "-y"))
                args = args.Prepend("-y");
            var arguments = string.Join(" ", args.Select(arg => arg.CommandLineArgumentEncode()));
            var startInfo = new ProcessStartInfo
            {
                Arguments = arguments,
                CreateNoWindow = false,
                FileName = GetFfmpegCommandFilePath(),
                UseShellExecute = false,
            };
            using var process = Process.Start(startInfo) ?? throw new Exception("Could not start ffmpeg.");
            process.WaitForExit();
            return process.ExitCode;
        }

        private static string GetFfmpegCommandFilePath()
            => EnumerateFfmpegCommandPath()
                .Where(File.Exists)
                .FirstOrDefault()
                ?? throw new FileNotFoundException("ffmpeg command file not found.");

        private static IEnumerable<string> EnumerateFfmpegCommandPath()
        {
            yield return "/bin/ffmpeg";
            yield return "/usr/bin/ffmpeg";
            var currentDirectory = Environment.CurrentDirectory;
            yield return Path.Combine(currentDirectory, "ffmpeg");
            yield return Path.Combine(currentDirectory, "ffmpeg.exe");
            yield return Path.Combine(_baseDirectory, "ffmpeg");
            yield return Path.Combine(_baseDirectory, "ffmpeg.exe");
        }

        private static void CopyStream(Stream inStream, Stream outStream)
        {
            var buffer = new byte[64 * 1024];
            while (true)
            {
                var length = inStream.Read(buffer, 0, buffer.Length);
                if (length <= 0)
                    break;
                outStream.Write(buffer, 0, length);
            }
        }
    }
}
