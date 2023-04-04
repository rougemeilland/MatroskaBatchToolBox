using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Palmtree
{
    partial class TinyConsole
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:条件式に変換します", Justification = "<保留中>")]
        private static (int left, int top) GetCursorPositionForWindows()
        {
            if (Console.IsInputRedirected)
                throw new InvalidOperationException("Unable to get cursor position because standard input is redirected.");

            if (_consoleTextReader is null)
                throw new Exception("internal error (_consoleTextReader is null)");

            int readBytesPos = 0;
            Span<char> buffer = stackalloc char[256];

            lock (Console.In)
            {
                WriteAnsiEscapeCodeToConsole(
                    ThisTerminalInfo.CursorPositionReport ?? throw new InvalidOperationException("This terminal does not define the capability to get the cursor position."),
                    () => throw new InvalidOperationException("Since both standard output and standard error output are redirected, it is not possible to get the cursor position."));

                var b = ReadCharFromConsole();
                while (true)
                {
                    // '\u001b' が見つかるまでコンソールから読み込む
                    while (true)
                    {
                        if (b == '\u001b')
                            break;
                        _consoleTextReader.AppendToCache(b);
                        b = ReadCharFromConsole();
                    }

                    buffer[readBytesPos++] = b;
                    Trace.Assert(readBytesPos < buffer.Length);

                    // この時点で b == '\u001b', 未読データの先頭は "\u001b" の次

                    b = ReadCharFromConsole();
                    if (b == '[')
                    {
                        buffer[readBytesPos++] = b;
                        Trace.Assert(readBytesPos < buffer.Length);

                        // この時点で b == '[', 未読データの先頭は "\u001b\[" の次

                        // 0個以上の数字列を読み込む
                        while (true)
                        {
                            b = ReadCharFromConsole();
                            if (b is < '0' or > '9')
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
                                b = ReadCharFromConsole();
                                if (b is < '0' or > '9')
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

                            // この時点で、 b は 'R'、未読データの先頭は "\u001b\[\d*(;\d*)?R" の次

                            var cprResponse = new string(buffer[..readBytesPos]);
                            try
                            {
                                var match = _cprResponsePattern.Match(cprResponse);
                                Trace.Assert(match.Success);

                                var matchGroupRow = match.Groups["row"];
                                var row =
                                    !matchGroupRow.Success || string.IsNullOrEmpty(matchGroupRow.Value)
                                    ? 1
                                    : int.Parse(matchGroupRow.Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
                                if (row < 1)
                                    throw new Exception($"The row value is less than 1.");

                                var matchGroupColumn = match.Groups["column"];
                                var column =
                                    !matchGroupColumn.Success || string.IsNullOrEmpty(matchGroupColumn.Value)
                                    ? 1
                                    : int.Parse(matchGroupColumn.Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
                                if (column < 1)
                                    throw new Exception($"The column value is less than 1.");

                                return (column - 1, row - 1);
                            }
                            catch (Exception ex)
                            {
                                throw
                                    new InvalidOperationException(
                                        $"The format of the response message from the terminal is invalid.: \"{string.Concat(cprResponse.Select(c => c is >= '\u0020' and <= '\u007e' or >= '\u0080' ? c.ToString() : $"\\u{(int)c:x4}"))}\"",
                                        ex);
                            }
                        }
                    }

                    // この時点で解析は失敗。b はreadBytes[] に未格納、未読データの先頭は b の値の次の文字
                    // b はもしかしたら '\u001b' かもしれないので、readBytes には含めない。

                    // 解析に失敗したバイト列を入力ストリームに戻す
                    _consoleTextReader.AppendToCache(buffer[..readBytesPos]);

                    // 入力バッファをクリアする
                    readBytesPos = 0;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static char ReadCharFromConsole()
            {
                return Console.ReadKey(true).KeyChar;
            }
        }
    }
}
