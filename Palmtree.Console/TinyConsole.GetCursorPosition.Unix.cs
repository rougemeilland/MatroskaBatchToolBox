using System;
using System.Diagnostics;
using System.Linq;
using Palmtree;

namespace Palmtree
{
    partial class TinyConsole
    {
        // The following source file was referenced.
        //   https://github.com/dotnet/runtime/blob/main/src/libraries/System.Console/src/System/ConsolePal.Unix.cs

        private static bool _everReceivedCursorPositionResponse = false;
        private static bool _firstCursorPositionRequest = true;

        private static (int left, int top) GetCursorPositionForUnix()
        {
            if (Console.IsInputRedirected)
                throw new InvalidOperationException("Unable to get cursor position because standard input is redirected.");

            Validation.Assert(_consoleInputStream is not null, "_consoleInputStream is not null");

            int readBytesPos = 0;
            Span<byte> buffer = stackalloc byte[256];

            UnixNativeInterOp.InitializeConsoleBeforeRead(minChars: (byte)(_everReceivedCursorPositionResponse ? 1 : 0), decisecondsTimeout: (byte)(_firstCursorPositionRequest ? 100 : 10));
            try
            {
                // CPR リクエストの送信
                WriteAnsiEscapeCodeToConsole(
                    ThisTerminalInfo.CursorPositionReport ?? throw new InvalidOperationException("This terminal does not define the capability to get the cursor position."),
                    () => throw new InvalidOperationException("Since both standard output and standard error output are redirected, it is not possible to get the cursor position."));

                // CPR レスポンスの受信開始

                var b = ReadByteFromConsole();
                while (true)
                {
                    // '\u001b' が見つかるまでコンソールから読み込む
                    while (true)
                    {
                        if (b == '\u001b')
                            break;
                        _consoleInputStream.AppendToCache(b);
                        b = ReadByteFromConsole();
                    }

                    buffer[readBytesPos++] = b;
                    Trace.Assert(readBytesPos < buffer.Length);

                    // この時点で b == '\u001b', 未読データの先頭は "\u001b" の次

                    b = ReadByteFromConsole();
                    if (b == '[')
                    {
                        buffer[readBytesPos++] = b;
                        Trace.Assert(readBytesPos < buffer.Length);

                        // この時点で b == '[', 未読データの先頭は "\u001b\[" の次

                        // 0個以上の数字列を読み込む
                        while (true)
                        {
                            b = ReadByteFromConsole();
                            if (b is < (byte)'0' or > (byte)'9')
                                break;
                            buffer[readBytesPos++] = b;
                            Trace.Assert(readBytesPos < buffer.Length);
                        }

                        // この時点で、 b は数字以外の文字であり、未読データの先頭は "\u001b\[\d*" の次

                        if (b == ';')
                        {
                            buffer[readBytesPos++] = b;
                            Trace.Assert(readBytesPos < buffer.Length);

                            // この時点で、 b は数字以外の文字であり、未読データの先頭は "\u001b\[\d*;" の次

                            // 0個以上の数字列を読み込む
                            while (true)
                            {
                                b = ReadByteFromConsole();
                                if (b is < (byte)'0' or > (byte)'9')
                                    break;
                                buffer[readBytesPos++] = b;
                                Trace.Assert(readBytesPos < buffer.Length);
                            }

                            // この時点で、 b は数字以外の文字であり、未読データの先頭は "\u001b\[\d*;\d*" の次
                        }

                        if (b == 'R')
                        {
                            buffer[readBytesPos++] = b;
                            Trace.Assert(readBytesPos < buffer.Length);

                            var readBytes = buffer[..readBytesPos];

                            // CPR レスポンスの受信完了

                            try
                            {
                                // CPR レスポンスの解析をする

                                // row は '[' の次から 非数字の前まで
                                var rowStartIndex = readBytes.IndexOf((byte)'[');
                                Trace.Assert(rowStartIndex >= 0);
                                ++rowStartIndex;
                                var rowEndIndex = readBytes[rowStartIndex..].IndexOf(v => v is < (byte)'0' or > (byte)'9');
                                Trace.Assert(rowEndIndex >= 0);
                                var top = ParseRowColumn(readBytes, rowStartIndex, rowEndIndex);

                                // column は ';' の次から 'R' の前まで (';'が省略されている場合は column = 1)
                                int left;
                                var columnStartIndex = readBytes.IndexOf((byte)';');
                                if (columnStartIndex < 0)
                                {
                                    left = 1;
                                }
                                else
                                {
                                    ++columnStartIndex;
                                    var columnEndIndex = readBytes[columnStartIndex..].IndexOf((byte)'R');
                                    Trace.Assert(rowEndIndex >= 0);
                                    left = ParseRowColumn(readBytes, columnStartIndex, columnEndIndex);
                                }

                                // 正常復帰
                                _everReceivedCursorPositionResponse = true;
                                return (left - 1, top - 1);
                            }
                            catch (Exception ex)
                            {
                                throw
                                    new InvalidOperationException(
                                        $"The format of the response message from the terminal is invalid.: \"{string.Concat(readBytes.ToArray().Select(data => data is >= 0x20 and <= 0x7e ? ((char)data).ToString() : $"\\u{data:x4}"))}\"",
                                        ex);
                            }
                        }
                    }

                    // この時点で解析は失敗。b はreadBytes[] に未格納、未読データの先頭は b の値の次の文字
                    // b はもしかしたら '\u001b' かもしれないので、readBytes には含めない。

                    // 解析に失敗したバイト列を入力ストリームに戻す
                    _consoleInputStream.AppendToCache(buffer[..readBytesPos]);

                    // 入力バッファをクリアする
                    readBytesPos = 0;
                }
            }
            finally
            {
                UnixNativeInterOp.UninitializeConsoleAfterRead();
                _firstCursorPositionRequest = false;
            }

            static int ParseRowColumn(Span<byte> buffer, int startIndex, int endIndex)
            {
                if (endIndex <= startIndex)
                    return 1;
                try
                {
                    var value = 0;
                    for (var index = startIndex; index < endIndex; ++index)
                    {
                        value = checked(value * 10 + (buffer[index] - '0'));
                    }

                    return
                        value > 0
                        ? value
                        : throw new Exception($"The row or column value is less than 1.");
                }
                catch (OverflowException ex)
                {
                    throw new OverflowException("Row or column value too large.", ex);
                }
            }
        }
    }
}
