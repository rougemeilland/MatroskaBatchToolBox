using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Utility.Models.Json;

namespace Utility
{
    public static class Command
    {
        private static readonly TimeSpan _childProcessCancellationInterval;
        private static bool _requestedCancellation;

        static Command()
        {
            _childProcessCancellationInterval = TimeSpan.FromSeconds(10);
            _requestedCancellation = false;
        }

        public static void AbortExternalCommands()
        {
            _requestedCancellation = true;
        }

        public static (CommandResult result, MovieInformation? streams) GetMovieInformation(FileInfo ffprobeCommandFile, FileInfo inFile, Action<string, string> logger)
        {
            var commandParameter = new StringBuilder();
            commandParameter.Append("-hide_banner");
            commandParameter.Append(" -v error");
            commandParameter.Append(" -print_format json");
            commandParameter.Append(" -show_chapters");
            commandParameter.Append(" -show_streams");
            commandParameter.Append($" -i \"{inFile.FullName}\"");
            var standardOutputTextLines = new List<string>();
            var (cancelled, exitCode) =
                Command.ExecuteCommand(
                    ffprobeCommandFile,
                    commandParameter.ToString(),
                    Encoding.UTF8,
                    (type, text) =>
                    {
                        if (type == OutputStreamType.StandardOutput)
                            standardOutputTextLines.Add(text);
                    },
                    logger,
                    proc =>
                    {
                        proc.StandardInput.Write("q");
                    });
            if (cancelled)
            {
                logger("INFO", $"ffprobe aborted at user request.");
                return (CommandResult.Cancelled, null);
            }
            logger("INFO", $"ffprobe exited with exit code {exitCode}.");
            if (exitCode != 0)
                throw new Exception($"ffprobe failed. (exit code {exitCode})");

            string jsonText = string.Join("\r\n", standardOutputTextLines);
            try
            {
                var movieInformationContainer =
                    JsonSerializer.Deserialize<MovieInformationContainer>(
                        jsonText,
                        new JsonSerializerOptions { AllowTrailingCommas = true })
                    ?? throw new Exception("ffprobe returned no information.");
                return (CommandResult.Completed, new MovieInformation(movieInformationContainer));
            }
            catch (Exception ex)
            {
                throw new Exception($"The information returned by ffprobe is in an unknown format.: \"{jsonText}\"", ex);
            }
        }

        public static (bool cancelled, int exitCode) ExecuteCommand(FileInfo programFile, string args, Encoding intputOutputEncoding, Action<OutputStreamType, string>? OutputReader, Action<string, string> logger, Action<Process>? childProcessCcanceller)
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
                    StandardInputEncoding = intputOutputEncoding,
                    StandardOutputEncoding = intputOutputEncoding,
                    StandardErrorEncoding = intputOutputEncoding,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                };
#if DEBUG && false
            System.Diagnostics.Debug.WriteLine($"{nameof(MatroskaBatchToolBox)}:INFO: A child process has been started.: \"{info.FileName}\" {info.Arguments}");
#endif
            logger("INFO", $"A child process has been started.: \"{info.FileName}\" {info.Arguments}");
            var process =
                Process.Start(info)
                ?? throw new Exception("Could not start process");
            try
            {
                process.PriorityClass = ProcessPriorityClass.BelowNormal;
                var cancelled = false;
                var cancellationWatcherTask =
                    Task.Run(() =>
                    {
                        if (childProcessCcanceller is not null)
                        {
                            while (!_requestedCancellation && !process.WaitForExit(1000)) ;
                            if (_requestedCancellation)
                            {
#if DEBUG && false
                            System.Diagnostics.Debug.WriteLine($"{nameof(MatroskaBatchToolBox)}:INFO: Detected cancellation for \"{info.FileName}\"({process.Id}).");
#endif
                                try
                                {
                                    process.PriorityClass = ProcessPriorityClass.Normal;
                                    while (true)
                                    {
                                        try
                                        {
                                            childProcessCcanceller(process);
                                        }
                                        catch (Exception)
                                        {
                                        }
#if DEBUG && false
                                        System.Diagnostics.Debug.WriteLine($"{nameof(MatroskaBatchToolBox)}:INFO: Requested to cancel child process \"{info.FileName}\"({process.Id}).");
#endif
                                        if (process.WaitForExit((int)_childProcessCancellationInterval.TotalMilliseconds))
                                            break;
                                    }
                                }
                                catch (Exception)
                                {
                                }
                                finally
                                {
                                    cancelled = true;
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
                logger("INFO", $"Total process time: {process.TotalProcessorTime.TotalSeconds:F2}[sec]");
                return (cancelled, process.ExitCode);
            }
            finally
            {
                process.Dispose();
            }
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
                        cache += new string(buffer, 0, length);
#if DEBUG && false
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
                                    cache = cache[(endOfLine + 1)..];
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
