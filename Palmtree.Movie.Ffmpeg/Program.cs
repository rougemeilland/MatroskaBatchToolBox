using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Palmtree.Movie.Ffmpeg
{
    public static class Program
    {
        private static readonly string _thisCommandName = typeof(Program).Assembly.GetName().Name ?? "???";
        private static readonly string _ffmpegCommandPath = ProcessUtility.WhereIs("ffmpeg") ?? "ffmpeg";
        private static readonly string _quitMessage = "[q] command received. Exiting.";

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
            if (!File.Exists(_ffmpegCommandPath))
                throw new FileNotFoundException("ffmpeg command file not found.");

            if (force && args.None(arg => arg == "-y"))
                args = args.Prepend("-y");
            var arguments = string.Join(" ", args.Select(arg => arg.CommandLineArgumentEncode()));
            var startInfo = new ProcessStartInfo
            {
                Arguments = arguments,
                FileName = _ffmpegCommandPath,
                UseShellExecute = false,
                RedirectStandardError = true,
                StandardErrorEncoding = Encoding.UTF8,
            };
            using var process = Process.Start(startInfo) ?? throw new Exception("Could not start ffmpeg.");
            var cancelled = ffmpegStandardErrorHandler(process.StandardError);
            process.WaitForExit();
            return cancelled ? -1 : process.ExitCode;
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

        private static bool ffmpegStandardErrorHandler(StreamReader reader)
        {
            var cancelled = false;
            var textCache = new char[1024];
            var textCacheLength = 0;
            var detectedEndOfStream = false;

            while (!IsEndOfStream())
            {
                if (cancelled)
                {
                    if (textCacheLength > 0)
                        _ = OutputFromCache(textCacheLength);

                    var length =ReadChars(reader, textCache, 0, textCache.Length);
                    if (length <= 0)
                        detectedEndOfStream = true;
                    else
                        TinyConsole.Write(textCache, 0, length);
                }
                else
                {
                    Validation.Assert(textCacheLength <= 0 || textCache[0] == _quitMessage[0] && textCacheLength < _quitMessage.Length, "textCacheLength <= 0 || textCache[0] == _quitMessage[0] && textCacheLength < _quitMessage.Length");

                    if (!ReadToCache())
                    {
                        _ = OutputFromCache(textCacheLength);
                    }
                    else
                    {
                        while (textCacheLength > 0)
                        {
                            var updated = FlushPrefix(0);
                            if (textCacheLength >= _quitMessage.Length)
                            {
                                if (textCache.Take(_quitMessage.Length).SequenceEqual(_quitMessage))
                                    cancelled = true;
                                else
                                    updated = FlushPrefix(1) || updated;
                            }
                            else
                            {
                                if (!textCache.Take(textCacheLength).SequenceEqual(_quitMessage.Take(textCacheLength)))
                                    updated = FlushPrefix(1) || updated;
                            }

                            if (!updated)
                                break;
                        }
                    }
                }
            }

            return cancelled;

            bool IsEndOfStream()
                => textCacheLength <= 0 && detectedEndOfStream;

            bool ReadToCache()
            {
                if (detectedEndOfStream)
                    return false;

                Validation.Assert(textCacheLength < textCache.Length, "textCacheLength < textCache.Length");

                var length = ReadChars(reader, textCache, textCacheLength, textCache.Length - textCacheLength);
                if (length <=  0)
                {
                    detectedEndOfStream = true;
                    return false;
                }
                else
                {
#if DEBUG
                    System.Diagnostics.Debug.Write(new string(textCache, textCacheLength, length));
#endif
                    textCacheLength += length;
                    return true;
                }
            }

            static int ReadChars(TextReader reader, char[] buffer, int offset, int count)
            {
                Validation.Assert(offset >= 0 && offset <= buffer.Length, "offset > 0 && offset <= buffer.Length");
                Validation.Assert(count >= 0 && offset + count <= buffer.Length, "count >= 0 && offset + count <= buffer.Length");
                for (var length = 0; length < count; ++length)
                {
                    var c = reader.Peek();
                    if (length > 0 && c < 0)
                        return length;
                    c = reader.Read();
                    if (c < 0)
                        return length;
                    buffer[offset + length] = (char)c;
                }

                return count;
            }

            bool FlushPrefix(int offset)
            {
                Validation.Assert(offset <= textCacheLength, "offset <=  textCacheLength");

                var foundIndex = Array.IndexOf(textCache, _quitMessage[0], offset, textCacheLength - offset);
                if (foundIndex < 0)
                    return OutputFromCache(textCacheLength);
                else
                    return OutputFromCache(foundIndex);
            }

            bool OutputFromCache(int length)
            {
                Validation.Assert(length <= textCacheLength, "length <= textCacheLength");
                if (length <= 0)
                    return false;
                TinyConsole.Write(textCache, 0, length);
                Array.Copy(textCache, length, textCache, 0, textCacheLength - length);
                textCacheLength -= length;
                return true;
            }
        }
    }
}
