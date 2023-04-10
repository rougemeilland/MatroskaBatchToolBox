﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Palmtree;

namespace Experiment
{
    public static partial class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<保留中>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:不要な抑制を削除します", Justification = "<保留中>")]
        public static int Main(string[] args)
        {
            var thisCommandName = typeof(Program).Assembly.GetName().Name;
            try
            {
                var location = ExecuteWhichCommand(args[0]);
                TinyConsole.WriteLine(location ?? "(null)");
                return 0;
            }
            catch (Exception ex)
            {
                TinyConsole.ForegroundColor = ConsoleColor.Red;
                TinyConsole.Error.WriteLine($"{thisCommandName}:ERROR: {ex.Message}");
                return 1;
            }
            finally
            {
                TinyConsole.ResetColor();
            }
        }

        private static string? ExecuteWhichCommand(string targetCommandName)
        {
            // コマンドのパス名を解決するコマンドの情報を取得する
            //   Windows の場合: where.exe
            //   UNIX の場合: which
            var (whichCommandDirs, whichCommandName, whichCommandOptions) = GetWhichCommandInfo(targetCommandName);

            // コマンドのパス名を解決するコマンドのフルパスを求める
            var whichCommandPath =
                whichCommandDirs
                .Select(dir => Path.Combine(dir, whichCommandName))
                .Where(File.Exists)
                .FirstOrDefault()
                ?? throw new FileNotFoundException($"\"{whichCommandName}\" command is not found.");

            // コマンドのパス名を解決するコマンドを起動する
            var startInfo = new ProcessStartInfo
            {
                Arguments = string.Join(" ", whichCommandOptions.Select(option => option.CommandLineArgumentEncode())),
                CreateNoWindow = false,
                FileName = whichCommandPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            using var process = Process.Start(startInfo) ?? throw new Exception($"Could not start \"{whichCommandName}\" command.");

            // 標準出力の最初の1行を読み込む (これが見つかったコマンドのフルパス名のはず)
            var foundPath = process.StandardOutput.ReadLine();

            // 標準出力の2行目以降は読み捨てる
            _ = process.StandardOutput.ReadToEnd();

            // 標準エラー出力を読み込み
            var errorMessage = process.StandardError.ReadToEnd();

            // コマンドのパス名を解決するコマンドの終了を待機する
            process.WaitForExit();

            var result = string.IsNullOrEmpty(foundPath) ? null : foundPath;
            Validation.Assert(result is null || File.Exists(result), "result is null || File.Exists(result)");

            // プロセスの終了コードを判別して復帰する
            //   0: 指定されたコマンドが見つかった場合 (Windows/UNIX 共通)
            //   1: 指定されたコマンドが見つからなかった場合 (Windows/UNIX 共通)
            //   2: その他の異常が発生した場合 (Windows/UNIX 共通)
            return
                process.ExitCode switch
                {
                    0 => result,
                    1 => null,
                    _ => throw new Exception($"\"{whichCommandName}\" command terminated abnormally.: exit-code={process.ExitCode}, message=\"{errorMessage}\""),
                };
        }

        private static (IEnumerable<string> whichCommandDirs, string whichCommandName, IEnumerable<string> whichCommandOptions) GetWhichCommandInfo(string targetCommandName)
            => OperatingSystem.IsWindows()
                ? (new[] { Environment.GetFolderPath(Environment.SpecialFolder.System) }.AsEnumerable(), "where.exe", new[] { targetCommandName }.AsEnumerable())
                : (new[] { "/usr/bin", "/bin" }.AsEnumerable(), "which", new[] { "-a", targetCommandName }.AsEnumerable());
    }
}
