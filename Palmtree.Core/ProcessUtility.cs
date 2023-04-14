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
        /// <list type="bullet">
        /// <item>
        /// <term>検索対象ファイルについて</term>
        /// <description>
        /// <list type="bullet">
        /// <item><term>Windows の場合</term><description><paramref name="targetCommandName"/> で指定された名前に拡張子 ".exe" が付加されたファイルを検索します。</description></item>
        /// <item><term>Linux の場合</term><description><paramref name="targetCommandName"/> で指定された名前のファイルを探します。ただし実行可能ではないファイルを除きます。</description></item>
        /// </list>
        /// </description>
        /// </item>
        /// <item>
        /// <term>検索場所について</term>
        /// <description>
        /// 以下の順で検索します。
        /// <list type="number">
        /// <item>アセンブリのあるディレクトリ (Windowsのみ)</item>
        /// <item>カレントディレクトリ</item>
        /// <item>PATH環境変数に設定されているディレクトリ</item>
        /// </list>
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        public static string? WhereIs(string targetCommandName)
        {
            if (OperatingSystem.IsWindows())
            {
                // Windows の where コマンドは PATH環境変数値の ';' を含むディレクトリ名を正しく認識できないため、where コマンドは使用しない。

                return WhereIsForWindows(targetCommandName);
            }
            else
            {
                return WhereIsForUnix(targetCommandName);
            }
        }

        private static string? WhereIsForWindows(string targetCommandName)
        {
            var commandFileName = $"{targetCommandName}.exe";
            return EnumerateExecutableDirectoriesForWindows()
                        .Select(dir => Path.Combine(dir, commandFileName))
                        .FirstOrDefault(File.Exists);
        }

        private static IEnumerable<string> EnumerateExecutableDirectoriesForWindows()
        {
            yield return AppContext.BaseDirectory;
            yield return Environment.CurrentDirectory;
            var pathEnvironment = Environment.GetEnvironmentVariable("PATH");
            if (pathEnvironment is not null)
            {
                var startPos = 0;
                var pathElement = new StringBuilder();
                while (true)
                {
                    // ';' または '"' を探す
                    var endPos = pathEnvironment.IndexOfAny(new char[] { ';', '"' }, startPos);
                    if (endPos < 0)
                    {
                        // ';' と '"' のどちらも見つからなかった場合

                        // 終端までを pathElement に追加する
                        _ = pathElement.Append(pathEnvironment[startPos..]);

                        // pathElement をディレクトリパス名として返し、繰り返しを終える
                        yield return pathElement.ToString();
                        yield break;
                    }

                    if (pathEnvironment[endPos] == '"')
                    {
                        // 先に見つかったのが '"' であった場合

                        // '"' の 1 文字前までの部分を pathElement に追加する
                        _ = pathElement.Append(pathEnvironment[startPos..endPos]);
                        startPos = endPos + 1;

                        // 閉じの '"' を探す
                        endPos = pathEnvironment.IndexOf('"', startPos);
                        if (endPos < 0)
                        {
                            // 閉じの '"' が見つからなかった場合

                            // 構文エラーなので、そのまま繰り返しを終える
                            yield break;
                        }

                        // 閉じの '"' の1つ前までを pathElement に追加する。
                        _ = pathElement.Append(pathEnvironment[startPos..(endPos - 1)]);
                        startPos = endPos + 1;
                    }
                    else
                    {
                        // 先に見つかったのが ';' であった場合
                        // ';' までの部分を pathElement に追加する
                        _ = pathElement.Append(pathEnvironment[startPos..endPos]);
                        startPos = endPos + 1;

                        // pathElement をディレクトリパス名として返す
                        yield return pathElement.ToString();

                        // pathElement をクリアする
                        _ = pathElement.Clear();
                    }
                }
            }
        }

        private static string? WhereIsForUnix(string targetCommandName)
        {
            var whichCommandName = "which";

            // コマンドのパス名を解決するコマンドのフルパスを求める
            var whichCommandPath =
                new[] { "/usr/bin", "/bin" }
                .Select(dir => Path.Combine(dir, whichCommandName))
                .Where(File.Exists)
                .FirstOrDefault()
                ?? throw new FileNotFoundException($"\"{whichCommandName}\" command is not found.");

            // コマンドのパス名を解決するコマンドを起動する
            var startInfo = new ProcessStartInfo
            {
                Arguments = string.Join(" ", new[] { "-a", targetCommandName }.Select(option => option.CommandLineArgumentEncode())),
                CreateNoWindow = false,
                FileName = whichCommandPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            var process = Process.Start(startInfo) ?? throw new Exception($"Could not start \"{whichCommandName}\" command.");

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
                Task.Run(process.StandardError.ReadToEnd);

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
    }
}
