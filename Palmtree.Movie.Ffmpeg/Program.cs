using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

// fffmpeg コマンドとの違い
// + 常に -nostdin が指定される。
// + 標準出力への出力が常に可能 (ffmpeg では標準出力への出力が可能かどうかはコーデック/コンテナ依存)
// + 中断方法は Ctrl+C 押下のみ。 (q キー押下による中断は不可)

namespace Palmtree.Movie.Ffmpeg
{
    public static class Program
    {
        private static readonly string _thisCommandName = typeof(Program).Assembly.GetName().Name ?? "???";
        private static readonly string _ffmpegCommandPath = ProcessUtility.WhereIs("ffmpeg") ?? "ffmpeg";

        public static int Main(string[] args)
        {
            // このプロセスでは Ctrl+C を無視する。
            // 子プロセスの ffmpeg は Ctrl+C を受け付けて、非ゼロの exit code で終了する。
            TinyConsole.CancelKeyPress += (sender, e) => e.Cancel = true;

            // コマンドの入出力エンコーディングを UTF8 にする
            TinyConsole.InputEncoding = Encoding.UTF8;
            TinyConsole.OutputEncoding = Encoding.UTF8;

            var newArgs = MakeFfmpegCommandArguments(args, out var tempFilePath);
            try
            {
                var exitCode = ExecuteFfpegCommand(newArgs, tempFilePath is not null);
                if (exitCode == 0 && tempFilePath is not null)
                {
                    using var inStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
                    using var outStream = TinyConsole.OpenStandardOutput();
                    inStream.CopyTo(outStream);
                }

                return exitCode;
            }
            catch (Exception ex)
            {
                TinyConsole.ForegroundColor = ConsoleColor.Red;
                TinyConsole.Error.Write($"\r{_thisCommandName}:ERROR: {ex.Message}");
                TinyConsole.ResetColor();
                try
                {
                    TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
                }
                catch (Exception)
                {
                }

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
            if (!File.Exists(_ffmpegCommandPath))
                throw new FileNotFoundException("ffmpeg command file not found.");

            if (force && args.None(arg => arg == "-y"))
                args = args.Prepend("-y");
            if (args.None(arg => arg == "-nostdin"))
                args = args.Prepend("-nostdin");

            var startInfo = new ProcessStartInfo
            {
                Arguments = string.Join(" ", args.Select(arg => arg.CommandLineArgumentEncode())),
                FileName = _ffmpegCommandPath,
                UseShellExecute = false,
            };
            using var process = Process.Start(startInfo) ?? throw new Exception("Could not start ffmpeg.");
            process.WaitForExit();
            return process.ExitCode;
        }
    }
}
