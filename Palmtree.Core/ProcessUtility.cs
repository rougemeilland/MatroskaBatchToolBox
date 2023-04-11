using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Palmtree
{
    /// <summary>
    /// プロセス / 外部コマンドのヘルパークラスです。
    /// </summary>
    public class ProcessUtility
    {
        /// <summary>
        /// ファイルシステムから指定されたコマンドを探します。
        /// </summary>
        /// <param name="targetCommandName">
        /// 探すコマンドの名前である <see cref="string"/> オブジェクトです。
        /// </param>
        /// <returns>
        /// <paramref name="targetCommandName"/> で指定されたコマンドが見つかった場合、そのフルパス名が返ります。
        /// 見つからなかった場合、null が返ります。
        /// </returns>
        /// <exception cref="FileNotFoundException">
        /// コマンドを探すためのコマンドが見つかりませんでした。
        /// これは Windows の場合は "where.exe" であり、UNIX の場合は "which" です。
        /// </exception>
        /// <remarks>
        /// このメソッドはファイルシステムの以下の場所からコマンドを探します。
        /// <list type="bullet">
        /// <item>カレントディレクトリ</item>
        /// <item>PATH環境変数に設定されているディレクトリ</item>
        /// </list>
        /// </remarks>
        public static string? WhereIs(string targetCommandName)
        {
            // Windows のPATH環境変数 ';' 区切り、パス名に';' が含まれている場合はダブルクォートでくくられる。
            // TODO: コマンド呼び出しなしで実装する。1)このアセンブリがある場所, 2)カレントディレクトリ, 3)PATH環境変数で定義されているディレクトリ
            // TODO: Windows の場合は exe com bat も含む

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

            // 標準出力を読み込むタスクの起動
            var standardOutputProcessingTask =
                Task.Run(() =>
                {
                    // 標準出力の最初の1行を読み込む (これが見つかったコマンドのフルパス名のはず)
                    var firstLine = process.StandardOutput.ReadLine();

                    // 標準出力の2行目以降は読み捨てる
                    _ = process.StandardOutput.ReadToEnd();

                    // 最初の1行のみを返す。
                    return firstLine;
                });

            // 標準エラー出力を読み込むタスクの起動
            var standardErrorProcessingTask =
                Task.Run(() => process.StandardError.ReadToEnd());

            // 標準出力読み込みタスクの結果の取得
            var foundPath = standardOutputProcessingTask.Result;

            // 標準エラー出力読み込みタスクの結果の取得
            var errorMessage = standardErrorProcessingTask.Result;

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
