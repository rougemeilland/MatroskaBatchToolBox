#define USE_WIN32_API_TO_CONSOLE_OPERATION_FOR_WINDOWS
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Palmtree.Terminal;

namespace Palmtree
{
    /// <summary>
    /// コンソールの操作を行うクラスです。
    /// </summary>
    public static partial class TinyConsole
    {
        private enum CharacterSet
        {
            Primary,
            Alternative,
        }

#if USE_WIN32_API_TO_CONSOLE_OPERATION_FOR_WINDOWS
        private const bool _useAnsiEscapeCodeEvenOnWindows = false;
#else
        private const bool _useAnsiEscapeCodeEvenOnWindows = true;
#endif
        private const char _alternativeCharacterSetMapMinimumKey = '\u0020';
        private const char _alternativeCharacterSetMapMaximumKey = '\u007e';
        private static readonly bool _isWindows = OperatingSystem.IsWindows();
        private static readonly IntPtr _consoleOutputHandle;
        private static readonly int _consoleOutputFileNo;
        private static readonly TextWriter _consoleTextWriter;
        private static readonly TextWriter? _escapeCodeWriter;
        private static readonly ConsoleColor _defaultBackgrouongColor = Console.BackgroundColor;
        private static readonly ConsoleColor _defaultForegrouongColor = Console.ForegroundColor;
        private static readonly TerminalInfo? ___thisTerminalInfo = TerminalInfo.GetTerminalInfo(true);
        private static readonly Regex _cprResponsePattern = new(@"^\e\[(?<row>\d*)(;(?<column>\d*))?R", RegexOptions.Compiled);
        private static readonly ConsoleTextReader? _consoleTextReader = null;
        private static readonly ConsoleInputStream? _consoleInputStream = null;
        private static readonly char[] _alternativeCharacterSetMap;
        private static ConsoleColor _currentBackgrouongColor = Console.BackgroundColor;
        private static ConsoleColor _currentForegrouongColor = Console.ForegroundColor;
        private static CharacterSet _currentCharSet;

        private static TerminalInfo ThisTerminalInfo
            => ___thisTerminalInfo is not null
                ? ___thisTerminalInfo
                : throw new InvalidOperationException("Terminal information not found.");

        static TinyConsole()
        {
            if (!Console.IsOutputRedirected)
            {
                _consoleOutputHandle = _isWindows ? InterOpWindows.GetStdHandle(InterOpWindows.STD_OUTPUT_HANDLE) : InterOpWindows.INVALID_HANDLE_VALUE;
                _consoleOutputFileNo = !_isWindows ? InterOpUnix.GetStandardFileNo(InterOpUnix.STANDARD_FILE_OUT) : -1;
                _consoleTextWriter =
                    new StreamWriter(Console.OpenStandardOutput(), Console.OutputEncoding.RemovePreamble(), 256, true) { AutoFlush = true };
                _escapeCodeWriter = _consoleTextWriter;
            }
            else if (!Console.IsErrorRedirected)
            {
                _consoleOutputHandle = _isWindows ? InterOpWindows.GetStdHandle(InterOpWindows.STD_ERROR_HANDLE) : InterOpWindows.INVALID_HANDLE_VALUE;
                _consoleOutputFileNo = !_isWindows ? InterOpUnix.GetStandardFileNo(InterOpUnix.STANDARD_FILE_ERR) : -1;
                _consoleTextWriter =
                    new StreamWriter(Console.OpenStandardError(), Console.OutputEncoding.RemovePreamble(), 256, true) { AutoFlush = true };
                _escapeCodeWriter = _consoleTextWriter;
            }
            else
            {
                _consoleOutputHandle = InterOpWindows.INVALID_HANDLE_VALUE;
                _consoleOutputFileNo = -1;
                _consoleTextWriter =
                        new StreamWriter(Console.OpenStandardError(), Console.OutputEncoding.RemovePreamble(), 256, true) { AutoFlush = true };
                _escapeCodeWriter = null;
            }

            _alternativeCharacterSetMap = Array.Empty<char>();
            _currentCharSet = CharacterSet.Primary;

            if (___thisTerminalInfo is not null)
            {
                var acs = ___thisTerminalInfo.AcsChars;
                if (acs is not null)
                {
                    _alternativeCharacterSetMap = new char[_alternativeCharacterSetMapMaximumKey - _alternativeCharacterSetMapMinimumKey + 1];
                    Array.Fill(_alternativeCharacterSetMap, '\u0000');
                    for (var index = 0; index + 1 < acs.Length; index += 2)
                        _alternativeCharacterSetMap[acs[index] - _alternativeCharacterSetMapMinimumKey] = acs[index + 1];
                }
            }

            if (_isWindows && _consoleOutputHandle != InterOpWindows.INVALID_HANDLE_VALUE)
            {
                // Windows プラットフォームであり、かつ
                // コンソール出力ハンドルが有効である (つまり標準出力と標準エラー出力のどちらかがリダイレクトされていない) 場合

                // コンソールモードに ENABLE_VIRTUAL_TERMINAL_PROCESSING フラグを立てる (エスケープコードを解釈可能にする)

                if (!InterOpWindows.GetConsoleMode(_consoleOutputHandle, out var mode))
                    throw new Exception("Failed to get console mode.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

                if ((mode & InterOpWindows.ENABLE_VIRTUAL_TERMINAL_PROCESSING) == 0)
                {
                    mode |= InterOpWindows.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
                    if (!InterOpWindows.SetConsoleMode(_consoleOutputHandle, mode))
                        throw new Exception("Failed to set console mode.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
                }
            }

            if (_isWindows)
            {
                if (!Console.IsInputRedirected)
                {
                    _consoleTextReader = new ConsoleTextReader(Console.OpenStandardInput(), Console.InputEncoding);
                    Console.SetIn(_consoleTextReader);
                }
            }
            else
            {
                if (!Console.IsInputRedirected)
                {
                    _consoleInputStream = new ConsoleInputStream(Console.OpenStandardInput());
                    Console.SetIn(new StreamReader(_consoleInputStream, Console.InputEncoding));
                    var keypadXmit = ThisTerminalInfo.KeypadXmit;
                    if (keypadXmit is not null)
                    {
                        var keypadXmitBytes = Encoding.ASCII.GetBytes($"{keypadXmit}\0");
                        unsafe
                        {
                            fixed (byte* keypadXmitBytesPointer = keypadXmitBytes)
                            {
                                InterOpUnix.SetKeypadXmit(keypadXmitBytesPointer);
                            }
                        }
                    }
                }
            }

            if (!ImplementWithWin32Api && _escapeCodeWriter is not null && ___thisTerminalInfo is not null)
            {
                var exitAltCharsetMode = ___thisTerminalInfo.ExitAltCharsetMode;
                if (exitAltCharsetMode is not null)
                    _escapeCodeWriter.Write(exitAltCharsetMode);
            }
        }

        #region BackgroundColor / ForegroundColor / ResetColor

        /// <summary>
        /// コンソールの文字の前景色を取得/設定します。
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準出力と標準エラー出力がともにリダイレクトされています。</item>
        /// <item>ターミナルが文字の前景色の変更をサポートしていません。</item>
        /// </list>
        /// </exception>
        public static ConsoleColor BackgroundColor
        {
            get
            {
                if (ImplementWithWin32Api)
                {
                    if (_consoleOutputHandle == InterOpWindows.INVALID_HANDLE_VALUE)
                        throw new InvalidOperationException("Since both standard output and standard error output are redirected, it is not possible to get console attributes.");

                    if (!InterOpWindows.GetConsoleScreenBufferInfo(_consoleOutputHandle, out var consoleInfo))
                        throw new InvalidOperationException("Failed to get console screen buffer info.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

                    (_currentBackgrouongColor, _) = InterOpWindows.FromConsoleAttributeToConsoleColors(consoleInfo.wAttributes);
                    return _currentBackgrouongColor;
                }
                else
                {
                    return
                        _escapeCodeWriter is not null
                        ? _currentBackgrouongColor
                        : throw new InvalidOperationException("Since both standard output and standard error output are redirected, it is not possible to get console attributes.");
                }
            }

            set => SetBackgroundColorCore(value);
        }

        /// <summary>
        /// コンソールの文字の前景色を取得/設定します。
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準出力と標準エラー出力がともにリダイレクトされています。</item>
        /// <item>ターミナルが文字の前景色の変更をサポートしていません。</item>
        /// </list>
        /// </exception>
        public static ConsoleColor ForegroundColor
        {
            get
            {
                if (ImplementWithWin32Api)
                {
                    if (_consoleOutputHandle == InterOpWindows.INVALID_HANDLE_VALUE)
                        throw new InvalidOperationException("Since both standard output and standard error output are redirected, it is not possible to get console attributes.");

                    if (!InterOpWindows.GetConsoleScreenBufferInfo(_consoleOutputHandle, out var consoleInfo))
                        throw new InvalidOperationException("Failed to get console screen buffer info.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

                    (_, _currentForegrouongColor) = InterOpWindows.FromConsoleAttributeToConsoleColors(consoleInfo.wAttributes);
                    return _currentBackgrouongColor;
                }
                else
                {
                    return
                        _escapeCodeWriter is not null
                        ? _currentForegrouongColor
                        : throw new InvalidOperationException("Since both standard output and standard error output are redirected, it is not possible to get console attributes.");
                }
            }

            set => SetForegroundColorCore(value);
        }

        /// <summary>
        /// コンソールの文字の前景色と背景色を初期値に変更します。
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準出力と標準エラー出力がともにリダイレクトされています。</item>
        /// <item>ターミナルが文字の前景色・背景色の初期化をサポートしていません。</item>
        /// </list>
        /// </exception>
        public static void ResetColor()
        {
            if (ImplementWithWin32Api)
            {
                if (_consoleOutputHandle == InterOpWindows.INVALID_HANDLE_VALUE)
                    throw new InvalidOperationException("Both standard output and standard error output are redirected, so console attributes cannot be changed.");

                var consoleAtrribute = InterOpWindows.FromConsoleColorsToConsoleAttribute(_defaultBackgrouongColor, _defaultForegrouongColor);
                if (!InterOpWindows.SetConsoleTextAttribute(_consoleOutputHandle, consoleAtrribute))
                    throw new InvalidOperationException("Failed to set console text attribute.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

                _currentBackgrouongColor = _defaultBackgrouongColor;
                _currentForegrouongColor = _defaultForegrouongColor;
            }
            else
            {
                var resetColorEscapeCode = ThisTerminalInfo.ResetColor;
                if (resetColorEscapeCode is not null)
                {
                    WriteAnsiEscapeCodeToConsole(
                        resetColorEscapeCode,
                        () => throw new InvalidOperationException("Both standard output and standard error output are redirected, so console attributes cannot be changed."));
                }
                else
                {
                    SetBackgroundColorCore(_defaultBackgrouongColor);
                    SetForegroundColorCore(_defaultForegrouongColor);
                }
            }
        }

        #endregion

        #region WindowWidth

        /// <summary>
        /// コンソールウィンドウの桁数を取得します。
        /// </summary>
        public static int WindowWidth => GetWindowSizeCore().windowWidth;

        #endregion

        #region WindowHeight

        /// <summary>
        /// コンソールウィンドウの行数を取得します。
        /// </summary>
        public static int WindowHeight => GetWindowSizeCore().windowHeight;

        #endregion

        #region Title

        /// <summary>
        /// ウィンドウタイトルを設定します。
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準出力と標準エラー出力がともにリダイレクトされています。</item>
        /// <item>ターミナルがウィンドウのタイトルの変更をサポートしていません。</item>
        /// </list>
        /// </exception>
        public static string Title
        {
            set
            {
                if (ImplementWithWin32Api)
                {
                    Console.Title = value;
                }
                else
                {
                    WriteAnsiEscapeCodeToConsole(
                        ThisTerminalInfo.SetTitle(value)
                        ?? throw new InvalidOperationException("This terminal does not define the capability to change the window title."),
                        () => throw new InvalidOperationException("Since both standard output and standard error output are redirected, it is not possible to set the title of the cursor."));
                }
            }
        }

        #endregion

        #region Beep

        /// <summary>
        /// コンソールから BEEP 音を鳴らします。
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準出力と標準エラー出力がともにリダイレクトされています。</item>
        /// <item>ターミナルがBEEP音をサポートしていません。</item>
        /// </list>
        /// </exception>
        public static void Beep()
        {
            if (ImplementWithWin32Api)
            {
                Console.Beep();
            }
            else
            {
                WriteAnsiEscapeCodeToConsole(
                    ThisTerminalInfo.Bell ?? throw new InvalidOperationException("This terminal does not define the \"bell\" capability."),
                    () => throw new InvalidOperationException("Since both standard output and standard error output are redirected, it cannot beep."));
            }
        }

        #endregion

        #region Clear

        /// <summary>
        /// コンソールバッファを消去します。
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準出力と標準エラー出力がともにリダイレクトされています。</item>
        /// <item>ターミナルがコンソールバッファの消去をサポートしていません。</item>
        /// </list>
        /// </exception>

        public static void Clear()
        {
            if (ImplementWithWin32Api)
            {
                if (_consoleOutputHandle == InterOpWindows.INVALID_HANDLE_VALUE)
                    throw new InvalidOperationException("Since both standard output and standard error output are redirected, the console screen cannot be cleared.");

                if (!InterOpWindows.GetConsoleScreenBufferInfo(_consoleOutputHandle, out var consoleInfo))
                    throw new InvalidOperationException("Failed to get console screen buffer info.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

                if (!InterOpWindows.SetConsoleCursorPosition(_consoleOutputHandle, new InterOpWindows.COORD { X = 0, Y = 0 }))
                    throw new InvalidOperationException("Failed to set cursor position.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

                ClearScreenCore(0, 0, consoleInfo.dwSize.X * consoleInfo.dwSize.Y, consoleInfo.wAttributes);

                // Windows ターミナルなどのターミナルでは Win32 API のみではコンソールバッファが消去されないため、エスケープコードも併用する。
                var eraseScrollBufferEscapeSequence = ThisTerminalInfo.EraseScrollBuffer;
                if (eraseScrollBufferEscapeSequence is not null)
                {
                    WriteAnsiEscapeCodeToConsole(
                        eraseScrollBufferEscapeSequence,
                        () => throw new InvalidOperationException("Since both standard output and standard error output are redirected, the console screen cannot be cleared."));
                }
            }
            else
            {
                WriteAnsiEscapeCodeToConsole(
                    ThisTerminalInfo.ClearBuffer
                        ?? ThisTerminalInfo.ClearScreen
                        ?? throw new InvalidOperationException("This terminal does not define the capability to clear the console buffer."),
                    () => throw new InvalidOperationException("Since both standard output and standard error output are redirected, the console screen cannot be cleared."));
            }
        }

        #endregion

        #region Erase

        /// <summary>
        /// コンソールバッファまたはコンソールウィンドウの全体または一部を消去します。
        /// </summary>
        /// <param name="eraseMode">
        /// 消去の方法を示す<see cref="ConsoleEraseMode"/>値です。
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準出力と標準エラー出力がともにリダイレクトされています。</item>
        /// <item><paramref name="eraseMode"/>で指定された方法での消去をターミナルがサポートしていません。</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="eraseMode"/>の値がサポートされていません。
        /// </exception>
        public static void Erase(ConsoleEraseMode eraseMode)
        {
            if (ImplementWithWin32Api)
            {
                if (_consoleOutputHandle == InterOpWindows.INVALID_HANDLE_VALUE)
                    throw new InvalidOperationException("Since both standard output and standard error output are redirected, it is not possible to delete console characters.");

                if (!InterOpWindows.GetConsoleScreenBufferInfo(_consoleOutputHandle, out var consoleInfo))
                    throw new InvalidOperationException("Failed to get console buffer info.", Marshal.GetExceptionForHR(Marshal.GetLastWin32Error()));

                var screenWidth = consoleInfo.srWindow.Right - consoleInfo.srWindow.Left + 1;
                switch (eraseMode)
                {
                    case ConsoleEraseMode.FromCursorToEndOfScreen:
                        ClearScreenCore(
                            consoleInfo.dwCursorPosition.X,
                            consoleInfo.dwCursorPosition.Y,
                            consoleInfo.srWindow.Right - consoleInfo.dwCursorPosition.X + 1,
                            consoleInfo.wAttributes);
                        for (var row = consoleInfo.dwCursorPosition.Y + 1; row <= consoleInfo.srWindow.Bottom; row++)
                            ClearScreenCore(consoleInfo.srWindow.Left, row, screenWidth, consoleInfo.wAttributes);
                        break;
                    case ConsoleEraseMode.FromBeggingOfScreenToCursor:
                        for (var row = consoleInfo.srWindow.Top; row <= consoleInfo.dwCursorPosition.Y - 1; row++)
                            ClearScreenCore(consoleInfo.srWindow.Left, row, screenWidth, consoleInfo.wAttributes);
                        ClearScreenCore(
                            consoleInfo.srWindow.Left,
                            consoleInfo.dwCursorPosition.Y,
                            consoleInfo.dwCursorPosition.X - consoleInfo.srWindow.Left + 1,
                            consoleInfo.wAttributes);
                        break;
                    case ConsoleEraseMode.EntireScreen:
                    {
                        // カーソルをホームポジションに設定
                        if (!InterOpWindows.SetConsoleCursorPosition(_consoleOutputHandle, new InterOpWindows.COORD { X = consoleInfo.srWindow.Left, Y = consoleInfo.srWindow.Top }))
                            throw new InvalidOperationException("Failed to set cursor position.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

                        for (var row = consoleInfo.srWindow.Top; row <= consoleInfo.srWindow.Bottom; row++)
                            ClearScreenCore(consoleInfo.srWindow.Left, row, screenWidth, consoleInfo.wAttributes);
                        break;
                    }
                    default:
                    {
                        int startX;
                        int startY;
                        int length;
                        switch (eraseMode)
                        {
                            case ConsoleEraseMode.EntireConsoleBuffer:
                            {
                                // カーソルをホームポジションに設定
                                if (!InterOpWindows.SetConsoleCursorPosition(_consoleOutputHandle, new InterOpWindows.COORD { X = 0, Y = 0 }))
                                    throw new InvalidOperationException("Failed to set cursor position.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

                                startX = 0;
                                startY = 0;
                                length = consoleInfo.dwSize.X * consoleInfo.dwSize.Y;
                                break;
                            }
                            case ConsoleEraseMode.FromCursorToEndOfLine:
                                startX = consoleInfo.dwCursorPosition.X;
                                startY = consoleInfo.dwCursorPosition.Y;
                                length = screenWidth - consoleInfo.dwCursorPosition.X;
                                break;
                            case ConsoleEraseMode.FromBeggingOfLineToCursor:
                                startX = consoleInfo.srWindow.Left;
                                startY = consoleInfo.dwCursorPosition.Y;
                                length = consoleInfo.dwCursorPosition.X + 1;
                                break;
                            case ConsoleEraseMode.EntireLine:
                                startX = consoleInfo.srWindow.Left;
                                startY = consoleInfo.dwCursorPosition.Y;
                                length = screenWidth;
                                break;
                            default:
                                throw new ArgumentException($"Invalid value: {eraseMode}", nameof(eraseMode));
                        }

                        ClearScreenCore(startX, startY, length, consoleInfo.wAttributes);

                        if (eraseMode == ConsoleEraseMode.EntireConsoleBuffer)
                        {
                            // Windows ターミナルなどのターミナルでは Win32 API のみではコンソールバッファが消去されないため、エスケープコードも併用する。
                            var eraseScrollBufferEscapeSequence = ThisTerminalInfo.EraseScrollBuffer;
                            if (eraseScrollBufferEscapeSequence is not null)
                                WriteAnsiEscapeCodeToConsole(eraseScrollBufferEscapeSequence, () => { });
                        }

                        break;
                    }
                }
            }
            else
            {
                WriteAnsiEscapeCodeToConsole(
                    eraseMode switch
                    {
                        ConsoleEraseMode.FromCursorToEndOfScreen => ThisTerminalInfo.ClrEos ?? throw new InvalidOperationException("This terminal does not support the capability \"clr_eos\"."),
                        ConsoleEraseMode.FromBeggingOfScreenToCursor => ThisTerminalInfo.EraseInDisplay1 ?? throw new InvalidOperationException("This terminal does not support the capability to erase from the beginning of the screen to the cursor position."),
                        ConsoleEraseMode.EntireScreen => ThisTerminalInfo.ClearScreen ?? throw new InvalidOperationException("This terminal does not support the capability \"clear_screen\"."),
                        ConsoleEraseMode.EntireConsoleBuffer => ThisTerminalInfo.ClearBuffer ?? throw new InvalidOperationException("This terminal doesn't support the capability to clear the console buffer."),
                        ConsoleEraseMode.FromCursorToEndOfLine => ThisTerminalInfo.ClrEol ?? throw new InvalidOperationException("This terminal does not support the capability \"clr_eol\"."),
                        ConsoleEraseMode.FromBeggingOfLineToCursor => ThisTerminalInfo.ClrBol ?? throw new InvalidOperationException("This terminal does not support the capability \"clr_bol\"."),
                        ConsoleEraseMode.EntireLine => ThisTerminalInfo.EraseInLine2 ?? throw new InvalidOperationException("This terminal does not support the capability to erase entire lines."),
                        _ => throw new ArgumentException($"Invalid erase mode.: {eraseMode}", nameof(eraseMode)),
                    },
                    () => throw new InvalidOperationException("Since both standard output and standard error output are redirected, it is not possible to delete console characters."));
            }
        }

        #endregion

        #region CursorVisible

        /// <summary>
        /// カーソルの可視性を <see cref="ConsoleCursorVisiblity"/> 列挙体で設定します。
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準出力と標準エラー出力がともにリダイレクトされています。</item>
        /// <item>カーソルの可視性の変更をターミナルがサポートしていません。</item>
        /// </list>
        /// </exception>
        public static ConsoleCursorVisiblity CursorVisible
        {
            set
            {
                if (value.IsNoneOf(ConsoleCursorVisiblity.Invisible, ConsoleCursorVisiblity.NormalMode, ConsoleCursorVisiblity.HighVisibilityMode))
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (ImplementWithWin32Api)
                {
                    if (_consoleOutputHandle == InterOpWindows.INVALID_HANDLE_VALUE)
                        throw new InvalidOperationException("Since both standard output and standard error are redirected, it is not possible to change the visibility of the cursor.");

                    if (!InterOpWindows.GetConsoleCursorInfo(_consoleOutputHandle, out var cursorInfo))
                        throw new InvalidOperationException("Failed to get console cursor info.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

                    (cursorInfo.bVisible, cursorInfo.dwSize) =
                        value switch
                        {
                            ConsoleCursorVisiblity.Invisible => (false, 1U),
                            ConsoleCursorVisiblity.NormalMode => (true, 25U),
                            ConsoleCursorVisiblity.HighVisibilityMode => (true, 100U),
                            _ => throw Validation.GetFailErrorException($"Unexpected value \"{value}\""),
                        };
                    if (!InterOpWindows.SetConsoleCursorInfo(_consoleOutputHandle, ref cursorInfo))
                        throw new InvalidOperationException("Failed to set console cursor info.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
                }
                else
                {
                    WriteAnsiEscapeCodeToConsole(
                        value switch
                        {
                            ConsoleCursorVisiblity.Invisible => ThisTerminalInfo.CursorInvisible,
                            ConsoleCursorVisiblity.NormalMode => ThisTerminalInfo.CursorNormal,
                            ConsoleCursorVisiblity.HighVisibilityMode => ThisTerminalInfo.CursorVisible ?? ThisTerminalInfo.CursorNormal,
                            _ => throw Validation.GetFailErrorException($"Unexpected value \"{value}\""),
                        }
                        ?? throw new ArgumentException($"This terminal does not support {value}."),
                        () => throw new InvalidOperationException("Since both standard output and standard error are redirected, it is not possible to change the visibility of the cursor."));
                }
            }
        }

        #endregion

        #region GetCursorPosition

        /// <summary>
        /// カーソルの位置を取得します。
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <term>Left</term>
        /// <description>コンソールウィンドウの左端からの桁数です。</description>
        /// </item>
        /// <item>
        /// <term>Top</term>
        /// <description>コンソールウィンドウの上端からの行数です。</description>
        /// </item>
        /// </list>
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準出力と標準エラー出力がともにリダイレクトされています。</item>
        /// <item>標準入力がリダイレクトされています。</item>
        /// <item>ターミナルがカーソル位置の取得をサポートしていません。</item>
        /// </list>
        /// </exception>
        /// <remarks>
        /// 異なるタスク/スレッドで以下のメソッド/プロパティの何れかが同時に使用された場合、予期しない動作をすることがあります。
        /// <list type="bullet">
        /// <item><see cref="CursorLeft"/></item>
        /// <item><see cref="CursorTop"/></item>
        /// <item><see cref="GetCursorPosition()"/></item>
        /// <item><see cref="KeyAvailable"/></item>
        /// <item><see cref="Read()"/></item>
        /// <item><see cref="ReadKey()"/></item>
        /// <item><see cref="ReadKey(bool)"/></item>
        /// <item><see cref="ReadLine()"/></item>
        /// </list>
        /// </remarks>
        public static (int Left, int Top) GetCursorPosition() => GetCursorPositionCore();

        #endregion

        #region SetCursorPosition

        /// <summary>
        /// カーソルの位置を設定します。
        /// </summary>
        /// <param name="left">コンソールバッファの左端からの桁数です。</param>
        /// <param name="top">コンソールバッファの上端からの桁数です。</param>
        public static void SetCursorPosition(int left, int top) => SetCursorPositionCore(left, top);

        #endregion

        #region CursorLeft

        /// <summary>
        /// コンソールウィンドウの左端からカーソル位置までの桁数を取得または設定します。
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準出力と標準エラー出力がともにリダイレクトされています。</item>
        /// <item>標準入力がリダイレクトされています。</item>
        /// <item>ターミナルがカーソル位置の取得/設定をサポートしていません。</item>
        /// </list>
        /// </exception>
        /// <remarks>
        /// 異なるタスク/スレッドで以下のメソッド/プロパティの何れかが同時に使用された場合、予期しない動作をすることがあります。
        /// <list type="bullet">
        /// <item><see cref="CursorLeft"/></item>
        /// <item><see cref="CursorTop"/></item>
        /// <item><see cref="GetCursorPosition()"/></item>
        /// <item><see cref="KeyAvailable"/></item>
        /// <item><see cref="Read()"/></item>
        /// <item><see cref="ReadKey()"/></item>
        /// <item><see cref="ReadKey(bool)"/></item>
        /// <item><see cref="ReadLine()"/></item>
        /// </list>
        /// </remarks>
        public static int CursorLeft
        {
            get => GetCursorPositionCore().left;
            set => SetCursorPositionCore(value, GetCursorPositionCore().top);
        }

        #endregion

        #region CursorTop

        /// <summary>
        /// コンソールウィンドウの上端からカーソル位置までの行数を取得または設定します。
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準出力と標準エラー出力がともにリダイレクトされています。</item>
        /// <item>標準入力がリダイレクトされています。</item>
        /// <item>ターミナルがカーソル位置の取得/設定をサポートしていません。</item>
        /// </list>
        /// </exception>
        /// <remarks>
        /// 異なるタスク/スレッドで以下のメソッド/プロパティの何れかが同時に使用された場合、予期しない動作をすることがあります。
        /// <list type="bullet">
        /// <item><see cref="CursorLeft"/></item>
        /// <item><see cref="CursorTop"/></item>
        /// <item><see cref="GetCursorPosition()"/></item>
        /// <item><see cref="KeyAvailable"/></item>
        /// <item><see cref="Read()"/></item>
        /// <item><see cref="ReadKey()"/></item>
        /// <item><see cref="ReadKey(bool)"/></item>
        /// <item><see cref="ReadLine()"/></item>
        /// </list>
        /// </remarks>
        public static int CursorTop
        {
            get => GetCursorPositionCore().top;
            set => SetCursorPositionCore(GetCursorPositionCore().left, value);
        }

        #endregion

        #region CursorUp

        /// <summary>
        /// カーソルを指定された行数だけ上に移動します。
        /// </summary>
        /// <param name="n">
        /// カーソルを移動する行数です。
        /// </param>
        /// <remarks>
        /// コンソールウィンドウの上端を超えて移動することはできません。
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準出力と標準エラー出力がともにリダイレクトされています。</item>
        /// <item>カーソルの行の移動をターミナルがサポートしていません。</item>
        /// </list>
        /// </exception>
        public static void CursorUp(int n)
            => MoveCursorVertically(-n, () => throw new InvalidOperationException("Since both standard output and standard error output are redirected, the cursor position cannot be changed."));

        #endregion

        #region CursorDown

        /// <summary>
        /// カーソルを指定された行数だけ下に移動します。
        /// </summary>
        /// <param name="n">
        /// カーソルを移動する行数です。
        /// </param>
        /// <remarks>
        /// コンソールウィンドウの下端を超えて移動することはできません。
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準出力と標準エラー出力がともにリダイレクトされています。</item>
        /// <item>カーソルの行の移動をターミナルがサポートしていません。</item>
        /// </list>
        /// </exception>
        public static void CursorDown(int n)
            => MoveCursorVertically(n, () => throw new InvalidOperationException("Since both standard output and standard error output are redirected, the cursor position cannot be changed."));

        #endregion

        #region CursorBack

        /// <summary>
        /// カーソルを指定された桁数だけ左に移動します。
        /// </summary>
        /// <param name="n">
        /// カーソルを移動する桁数です。
        /// </param>
        /// <remarks>
        /// コンソールウィンドウの左端を超えて移動することはできません。
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準出力と標準エラー出力がともにリダイレクトされています。</item>
        /// <item>カーソルの桁の移動をターミナルがサポートしていません。</item>
        /// </list>
        /// </exception>
        public static void CursorBack(int n)
            => MoveCursorHorizontally(-n, () => throw new InvalidOperationException("Since both standard output and standard error output are redirected, the cursor position cannot be changed."));

        #endregion

        #region CursorForward

        /// <summary>
        /// カーソルを指定された桁数だけ右に移動します。
        /// </summary>
        /// <param name="n">
        /// カーソルを移動する桁数です。
        /// </param>
        /// <remarks>
        /// コンソールウィンドウの右端を超えて移動することはできません。
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準出力と標準エラー出力がともにリダイレクトされています。</item>
        /// <item>カーソルの桁の移動をターミナルがサポートしていません。</item>
        /// </list>
        /// </exception>
        public static void CursorForward(int n)
            => MoveCursorHorizontally(n, () => throw new InvalidOperationException("Since both standard output and standard error output are redirected, the cursor position cannot be changed."));

        #endregion

        #region Terminal

        /// <summary>
        /// 現在使用中のターミナルの情報を取得します。
        /// </summary>
        public static TerminalInfo Terminal
            => ThisTerminalInfo
                ?? throw new InvalidOperationException("Information about the terminal currently in use cannot be found.");

        #endregion

        #region OutputEscapeCode

        /// <summary>
        /// 指定されたエスケープコードをターミナルに出力します。
        /// </summary>
        /// <param name="escapeCode">
        /// ターミナルに出力するエスケープコードです。
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準出力と標準エラー出力がともにリダイレクトされています。</item>
        /// </list>
        /// </exception>
        /// <remarks>
        /// <list type="bullet">
        /// <item><paramref name="escapeCode"/>で与えられたエスケープコードが正しいかどうかはチェックされません。</item>
        /// <item>ターミナルの種類によりどのエスケープコードがサポートされているかは異なります。実行環境によっては期待した結果を生まない可能性があることを忘れないでください。</item>
        /// </list>
        /// </remarks>
        public static void OutputEscapeCode(string escapeCode)
            => WriteAnsiEscapeCodeToConsole(
                escapeCode,
                () => throw new InvalidOperationException("Since both standard output and standard error output are redirected, it is not possible to output the escape code."));

        #endregion

        #region private methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetBackgroundColorCore(ConsoleColor value)
        {
            if (ImplementWithWin32Api)
            {
                if (_consoleOutputHandle == InterOpWindows.INVALID_HANDLE_VALUE)
                    throw new InvalidOperationException("Both standard output and standard error output are redirected, so console attributes cannot be changed.");

                if (!InterOpWindows.GetConsoleScreenBufferInfo(_consoleOutputHandle, out var consoleInfo))
                    throw new InvalidOperationException("Failed to get console screen buffer info.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

                var consoleAtrribute =
                    InterOpWindows.FromConsoleColorsToConsoleAttribute(
                        value,
                        InterOpWindows.FromConsoleAttributeToConsoleColors(consoleInfo.wAttributes).foregroundColor);
                if (!InterOpWindows.SetConsoleTextAttribute(_consoleOutputHandle, consoleAtrribute))
                    throw new InvalidOperationException("Failed to set console text attribute.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
            }
            else
            {

                WriteAnsiEscapeCodeToConsole(
                    ThisTerminalInfo.SetABackground(value.ToAnsiColor16())
                    ?? ThisTerminalInfo.SetBackground(value.ToColor8())
                    ?? throw new InvalidOperationException("This terminal does not define the capability to change the text background color."),
                    () => throw new InvalidOperationException("Both standard output and standard error output are redirected, so console attributes cannot be changed."));

            }

            _currentBackgrouongColor = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetForegroundColorCore(ConsoleColor value)
        {
            if (ImplementWithWin32Api)
            {
                if (_consoleOutputHandle == InterOpWindows.INVALID_HANDLE_VALUE)
                    throw new InvalidOperationException("Both standard output and standard error output are redirected, so console attributes cannot be changed.");

                if (!InterOpWindows.GetConsoleScreenBufferInfo(_consoleOutputHandle, out var consoleInfo))
                    throw new InvalidOperationException("Failed to get console screen buffer info.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

                var consoleAtrribute =
                    InterOpWindows.FromConsoleColorsToConsoleAttribute(
                        InterOpWindows.FromConsoleAttributeToConsoleColors(consoleInfo.wAttributes).backgroundColor,
                        value);
                if (!InterOpWindows.SetConsoleTextAttribute(_consoleOutputHandle, consoleAtrribute))
                    throw new InvalidOperationException("Failed to set console text attribute.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

            }
            else
            {
                WriteAnsiEscapeCodeToConsole(
                    ThisTerminalInfo.SetAForeground(value.ToAnsiColor16())
                        ?? ThisTerminalInfo.SetForeground(value.ToColor8())
                        ?? throw new InvalidOperationException("This terminal does not define the capability to change the foreground color of characters."),
                    () => throw new InvalidOperationException("Both standard output and standard error output are redirected, so console attributes cannot be changed."));
            }

            _currentForegrouongColor = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (int windowWidth, int windowHeight) GetWindowSizeCore()
        {
            if (_isWindows)
            {
                if (_consoleOutputHandle == InterOpWindows.INVALID_HANDLE_VALUE)
                    throw new InvalidOperationException("Since both standard output and standard error output are redirected, it is not possible to get window size.");

                if (!InterOpWindows.GetConsoleScreenBufferInfo(_consoleOutputHandle, out var consoleInfo))
                    throw new InvalidOperationException("Failed to get console screen buffer info.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

                return (consoleInfo.srWindow.Right - consoleInfo.srWindow.Left + 1, consoleInfo.srWindow.Bottom - consoleInfo.srWindow.Top + 1);
            }
            else
            {
                if (_consoleOutputFileNo < 0)
                    throw new InvalidOperationException("Since both standard output and standard error output are redirected, it is not possible to get window size.");

                if (InterOpUnix.GetWindowSize(_consoleOutputFileNo, out var windowSize, out _) == 0)
                    return (windowSize.Col, windowSize.Row);

                return (ThisTerminalInfo.Columns ?? throw new InvalidOperationException("The terminal does not have the capability \"columns\" defined."), ThisTerminalInfo.Lines ?? throw new InvalidOperationException("The terminal does not have the capability \"lines\" defined."));
            }
        }

#if false // プラットフォーム依存の機能であるためサポートしないこととする
        private static void SetWindowSizeCore(int windowWidth, int windowHeight)
        {
            if (_isWindows)
            {
                if (_consoleOutputHandle == InterOpWindows.INVALID_HANDLE_VALUE)
                    throw new InvalidOperationException("Since both standard output and standard error output are redirected, it is not possible to set window size.");

                if (!InterOpWindows.GetConsoleScreenBufferInfo(_consoleOutputHandle, out var consoleInfo))
                    throw new InvalidOperationException("Failed to get console screen buffer info.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

                var largestConsoleBufferSize = InterOpWindows.GetLargestConsoleWindowSize(_consoleOutputHandle);
                if (largestConsoleBufferSize.X == 0 && largestConsoleBufferSize.Y == 0)
                    throw new InvalidOperationException("Failed to get largest console buffer size.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

                // 新たなコンソールウィンドウのサイズを初期化する。
                var newConsoleBufferSize = new InterOpWindows.COORD { X = consoleInfo.dwSize.X, Y = consoleInfo.dwSize.Y };

                // 新たなコンソールウィンドウの桁数をチェックする
                try
                {
                    if (windowWidth < 0)
                        throw new ArgumentOutOfRangeException(nameof(windowWidth));
                    if (checked(consoleInfo.srWindow.Left + windowWidth) > consoleInfo.dwSize.X)
                    {
                        // 新たなコンソールウィンドウの桁が現在のコンソールバッファからはみ出している場合

                        // 新たなコンソールバッファの桁数を求める
                        newConsoleBufferSize.X = checked((short)(consoleInfo.srWindow.Left + windowWidth));
                        if (newConsoleBufferSize.X > largestConsoleBufferSize.X)
                            throw new ArgumentOutOfRangeException(nameof(windowWidth));
                    }
                }
                catch (OverflowException)
                {
                    throw new ArgumentOutOfRangeException(nameof(windowWidth));
                }

                // 新たなコンソールウィンドウの行数をチェックする
                try
                {
                    if (windowHeight < 0)
                        throw new ArgumentOutOfRangeException(nameof(windowHeight));
                    if (checked(consoleInfo.srWindow.Top + windowHeight) > consoleInfo.dwSize.Y)
                    {
                        // 新たなコンソールウィンドウの行が現在のコンソールバッファからはみ出している場合

                        // 新たなコンソールバッファの行数を求める
                        newConsoleBufferSize.Y = checked((short)(consoleInfo.srWindow.Top + windowHeight));
                        if (newConsoleBufferSize.Y > largestConsoleBufferSize.Y)
                            throw new ArgumentOutOfRangeException(nameof(windowHeight));
                    }
                }
                catch (OverflowException)
                {
                    throw new ArgumentOutOfRangeException(nameof(windowHeight));
                }

                if (newConsoleBufferSize.X != consoleInfo.dwSize.X || newConsoleBufferSize.Y != consoleInfo.dwSize.Y)
                {
                    // コンソールバッファのサイズを変更する必要がある場合

                    // コンソールバッファのサイズを変更する
                    if (!InterOpWindows.SetConsoleScreenBufferSize(_consoleOutputHandle, newConsoleBufferSize))
                        throw new InvalidOperationException("Failed to resize console buffer.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
                }

                var success = false;
                try
                {
                    // コンソールウィンドウのサイズを変更する
                    var windowRect =
                        new InterOpWindows.SMALL_RECT
                        {
                            Left = consoleInfo.srWindow.Left,
                            Top = consoleInfo.srWindow.Top,
                            Right = (short)(consoleInfo.srWindow.Left + windowWidth - 1),
                            Bottom = (short)(consoleInfo.srWindow.Top + windowHeight - 1),
                        };
                    if (!InterOpWindows.SetConsoleWindowInfo(_consoleOutputHandle, true, ref windowRect))
                        throw new InvalidOperationException("Failed to resize console window.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

                    // 成功フラグをセットする
                    success = true;
                }
                finally
                {
                    if (!success && (newConsoleBufferSize.X != consoleInfo.dwSize.X || newConsoleBufferSize.Y != consoleInfo.dwSize.Y))
                    {
                        // コンソールウィンドウのサイズの変更に失敗しており、かつコンソールバッファのサイズが変更されている場合

                        // コンソールバッファのサイズを元に戻す。(エラーは無視する)
                        _ = InterOpWindows.SetConsoleScreenBufferSize(_consoleOutputHandle, consoleInfo.dwSize);
                    }
                }
            }
            else
            {
                if (_consoleOutputFileNo < 0)
                    throw new InvalidOperationException("Since both standard output and standard error output are redirected, it is not possible to get window size.");

                if (windowWidth is < 0 or > ushort.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(windowWidth));
                if (windowHeight is < 0 or > ushort.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(windowHeight));

                var windowSize =
                    new InterOpUnix.WinSize
                    {
                        Col = (ushort)windowWidth,
                        Row = (ushort)windowHeight,
                        XPixel = default,
                        YPixel = default,
                    };

                var result = InterOpUnix.SetWindowSize(_consoleOutputFileNo, ref windowSize, out var errno);
                if (result != 0)
                {
                    Console.Error.WriteLine($"result={result}, errno={errno}");
                    if (errno == InterOpUnix.ENOTSUP)
                        throw new PlatformNotSupportedException("Resizing the console window is not supported.");
                    else
                        throw new InvalidOperationException($"Failed to set window size.: errno={errno}");
                }
            }
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (int left, int top) GetCursorPositionCore()
        {
            if (ImplementWithWin32Api)
            {
                if (_consoleOutputHandle == InterOpWindows.INVALID_HANDLE_VALUE)
                    throw new InvalidOperationException("Since both standard output and standard error output are redirected, it is not possible to get the cursor position.");

                if (!InterOpWindows.GetConsoleScreenBufferInfo(_consoleOutputHandle, out var consoleInfo))
                    throw new InvalidOperationException("Failed to get console screen buffer info.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

                return (consoleInfo.dwCursorPosition.X - consoleInfo.srWindow.Left, consoleInfo.dwCursorPosition.Y - consoleInfo.srWindow.Top);
            }
            else if (_isWindows)
            {
                return GetCursorPositionForWindows();
            }
            else
            {
                return GetCursorPositionForUnix();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetCursorPositionCore(int left, int top)
        {
            if (ImplementWithWin32Api)
            {
                if (_consoleOutputHandle == InterOpWindows.INVALID_HANDLE_VALUE)
                    throw new InvalidOperationException("Cursor position cannot be set because both standard output and standard error output are redirected.");

                if (!InterOpWindows.GetConsoleScreenBufferInfo(_consoleOutputHandle, out var consoleInfo))
                    throw new InvalidOperationException("Failed to get console information.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

                if (left.IsOutOfClosedInterval(0, consoleInfo.srWindow.Right - consoleInfo.srWindow.Left))
                    throw new ArgumentOutOfRangeException(nameof(left), left, $"Invalid value for \"{nameof(left)}\" parameter.: {nameof(left)}={left}");

                if (left.IsOutOfClosedInterval(0, consoleInfo.srWindow.Bottom - consoleInfo.srWindow.Top))
                    throw new ArgumentOutOfRangeException(nameof(top), top, $"Invalid value for \"{nameof(top)}\" parameter.: {nameof(top)}={top}");

                if (!InterOpWindows.SetConsoleCursorPosition(
                    _consoleOutputHandle,
                    new InterOpWindows.COORD
                    {
                        X = checked((short)(left + consoleInfo.srWindow.Left)),
                        Y = checked((short)(top + consoleInfo.srWindow.Top)),
                    }))
                {
                    throw new InvalidOperationException("Failed to set cursor position.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
                }
            }
            else
            {
                WriteAnsiEscapeCodeToConsole(
                    ThisTerminalInfo.CursorAddress(top, left)
                    ?? throw new InvalidOperationException("This terminal does not define the capability \"cursor_address\"."),
                    () => throw new InvalidOperationException("Cursor position cannot be set because both standard output and standard error output are redirected."));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveCursorVertically(int n, Action errorHandler)
        {
            if (ImplementWithWin32Api)
            {
                if (_consoleOutputHandle == InterOpWindows.INVALID_HANDLE_VALUE)
                    throw new InvalidOperationException("Both standard output and standard error output are redirected, so the cursor position cannot be moved.");

                if (!InterOpWindows.GetConsoleScreenBufferInfo(_consoleOutputHandle, out var consoleInfo))
                    throw new InvalidOperationException("Failed to get console buffer info.", Marshal.GetExceptionForHR(Marshal.GetLastWin32Error()));

                if (!InterOpWindows.SetConsoleCursorPosition(
                    _consoleOutputHandle,
                    new InterOpWindows.COORD
                    {
                        X = consoleInfo.dwCursorPosition.X,
                        Y = checked((short)(consoleInfo.dwCursorPosition.Y + n).Maximum(consoleInfo.srWindow.Top).Minimum(consoleInfo.srWindow.Bottom)),
                    }))
                {
                    throw new InvalidOperationException("Failed to set console cursor position.", Marshal.GetExceptionForHR(Marshal.GetLastWin32Error()));
                }
            }
            else
            {
                if (n > 0)
                {
                    WriteAnsiEscapeCodeToConsole(
                        ThisTerminalInfo.ParmDownCursor(n) ?? throw new InvalidOperationException("This terminal does not define the capability \"parm_down_cursor\"."),
                        errorHandler);
                }
                else if (n < 0)
                {
                    WriteAnsiEscapeCodeToConsole(
                        ThisTerminalInfo.ParmUpCursor(checked(-n)) ?? throw new InvalidOperationException("This terminal does not define the capability \"parm_up_cursor\"."),
                        errorHandler);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MoveCursorHorizontally(int n, Action errorHandler)
        {
            if (ImplementWithWin32Api)
            {
                if (_consoleOutputHandle == InterOpWindows.INVALID_HANDLE_VALUE)
                    throw new InvalidOperationException("Both standard output and standard error output are redirected, so the cursor position cannot be moved.");

                if (!InterOpWindows.GetConsoleScreenBufferInfo(_consoleOutputHandle, out var consoleInfo))
                    throw new InvalidOperationException("Failed to get console buffer info.", Marshal.GetExceptionForHR(Marshal.GetLastWin32Error()));

                if (!InterOpWindows.SetConsoleCursorPosition(
                    _consoleOutputHandle,
                    new InterOpWindows.COORD
                    {
                        X = checked((short)(consoleInfo.dwCursorPosition.X + n).Maximum(consoleInfo.srWindow.Left).Minimum(consoleInfo.srWindow.Right)),
                        Y = consoleInfo.dwCursorPosition.Y
                    }))
                {
                    throw new InvalidOperationException("Failed to set console cursor position.", Marshal.GetExceptionForHR(Marshal.GetLastWin32Error()));
                }
            }
            else
            {
                if (n > 0)
                {
                    WriteAnsiEscapeCodeToConsole(
                        ThisTerminalInfo.ParmRightCursor(n) ?? throw new InvalidOperationException("This terminal does not define the capability \"parm_right_cursor\"."),
                        errorHandler);
                }
                else if (n < 0)
                {
                    WriteAnsiEscapeCodeToConsole(
                        ThisTerminalInfo.ParmLeftCursor(checked(-n)) ?? throw new InvalidOperationException("This terminal does not define the capability \"parm_left_cursor\"."),
                        errorHandler);
                }
            }
        }

        private static void WriteAnsiEscapeCodeToConsole(string ansiEscapeCode, Action errorHandler)
        {
            if (_escapeCodeWriter is not null)
                _escapeCodeWriter.Write(ansiEscapeCode);
            else
                errorHandler();
        }

        // Win32 API を使用する条件: 強制的に ANSI エスケープコードを使用する実装ではなく、かつプラットフォームが Windows である
        private static bool ImplementWithWin32Api
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !_useAnsiEscapeCodeEvenOnWindows && _isWindows;
        }

        private static void ClearScreenCore(int startX, int startY, int length, ushort attribute)
        {
            var startPosition =
                new InterOpWindows.COORD
                {
                    X = checked((short)startX),
                    Y = checked((short)startY),
                };
            if (!InterOpWindows.FillConsoleOutputCharacter(_consoleOutputHandle, (short)' ', (uint)length, startPosition, out _))
                throw new InvalidOperationException("Failed to clear console buffer characters.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

            if (!InterOpWindows.FillConsoleOutputAttribute(_consoleOutputHandle, attribute, (uint)length, startPosition, out _))
                throw new InvalidOperationException("Failed to clear console buffer attributes.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe byte ReadByteFromConsole()
        {
            byte data;
            int result = InterOpUnix.ReadStdin(&data, sizeof(byte));
            if (result < 0)
                throw new IOException($"I/O error (errno={Marshal.GetLastPInvokeError()})");

            // 特殊キーが押された場合は result == 0 になる可能性があることに注意

            Trace.Assert(result is (sizeof(byte)) or 0, $"ReadStdin(&data, 1) => {result}");
            return result == 0 ? (byte)0 : data;
        }

        private static void SetCharacterSet(CharacterSet charSet)
        {
            switch (charSet)
            {
                case CharacterSet.Primary:
                    if (_currentCharSet != charSet)
                    {
                        if (_escapeCodeWriter is null)
                            throw new InvalidOperationException("Since both standard output and standard error output are redirected, it is not possible to change the character set.");
                        var escapeCode = ThisTerminalInfo.ExitAltCharsetMode ?? throw new InvalidOperationException("The terminal does not define the capability \"exit_alt_charset_mode\".");
                        _escapeCodeWriter.Write(escapeCode);
                        _currentCharSet = charSet;
                    }

                    break;
                case CharacterSet.Alternative:
                    if (_currentCharSet != charSet)
                    {
                        if (_escapeCodeWriter is null)
                            throw new InvalidOperationException("Since both standard output and standard error output are redirected, it is not possible to change the character set.");
                        var escapeCode = ThisTerminalInfo.EnterAltCharsetMode ?? throw new InvalidOperationException("The terminal does not define the capability \"enter_alt_charset_mode\".");
                        _escapeCodeWriter.Write(escapeCode);
                        _currentCharSet = charSet;
                    }

                    break;
                default:
                    throw Validation.GetFailErrorException($"Unexpected {nameof(CharacterSet)} value: {charSet}");
            }
        }

        #endregion
    }
}
