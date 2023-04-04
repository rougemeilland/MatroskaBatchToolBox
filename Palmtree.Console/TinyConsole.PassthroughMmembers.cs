using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Palmtree
{
    partial class TinyConsole
    {
        /// <summary>
        /// コンソールウィンドウ領域の左端の位置を、コンソールバッファに対する相対位置として取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準出力と標準エラー出力がともにリダイレクトされています。</item>
        /// </list>
        /// </exception>
        public static int WindowLeft
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Console.WindowLeft;
        }

        /// <summary>
        /// コンソールウィンドウ領域の上端の位置を、コンソールバッファに対する相対位置として取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準出力と標準エラー出力がともにリダイレクトされています。</item>
        /// </list>
        /// </exception>
        public static int WindowTop
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Console.WindowTop;
        }

        /// <summary>
        /// 標準入力ストリームを取得します。
        /// </summary>
        public static TextReader In
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Console.In;
        }

        /// <summary>
        /// 標準出力ストリームを取得します。
        /// </summary>
        public static TextWriter Out
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Console.Out;
        }

        /// <summary>
        /// 標準エラー出力ストリームを取得します。
        /// </summary>
        public static TextWriter Error
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Console.Error;
        }

        /// <summary>
        /// 標準入力ストリームがリダイレクトされているかどうかを示す値を取得します。
        /// </summary>
        public static bool IsInputRedirected
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Console.IsInputRedirected;
        }

        /// <summary>
        /// 標準出力ストリームがリダイレクトされているかどうかを示す値を取得します。
        /// </summary>
        public static bool IsOutputRedirected
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Console.IsOutputRedirected;
        }

        /// <summary>
        /// 標準エラー出力ストリームがリダイレクトされているかどうかを示す値を取得します。
        /// </summary>
        public static bool IsErrorRedirected
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Console.IsErrorRedirected;
        }

        /// <summary>
        /// コンソールが入力内容の読み取り時に使用するエンコーディングを取得または設定します。
        /// </summary>
        public static Encoding InputEncoding
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Console.InputEncoding;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Console.InputEncoding = value;
        }

        /// <summary>
        /// コンソールが出力内容の書き込み時に使用するエンコーディングを取得または設定します。
        /// </summary>
        public static Encoding OutputEncoding
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Console.OutputEncoding;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Console.OutputEncoding = value;
        }

        /// <summary>
        /// 標準入力ストリームを取得します。
        /// </summary>
        /// <returns>
        /// 標準入力ストリームです。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Stream OpenStandardInput() => Console.OpenStandardInput();

        /// <summary>
        /// 標準出力ストリームを取得します。
        /// </summary>
        /// <returns>
        /// 標準出力ストリームです。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Stream OpenStandardOutput() => Console.OpenStandardOutput();

        /// <summary>
        /// 標準エラー出力ストリームを取得します。
        /// </summary>
        /// <returns>
        /// 標準エラー出力ストリームです。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Stream OpenStandardError() => Console.OpenStandardError();

        /// <summary>
        /// 指定した <see cref="TextReader"/> を <see cref="In"/> プロパティに設定します。
        /// </summary>
        /// <param name="newIn">
        /// 新しい標準入力であるストリームです。
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetIn(TextReader newIn) => Console.SetIn(newIn);

        /// <summary>
        /// 指定した <see cref="TextWriter"/> を <see cref="Out"/> プロパティに設定します。
        /// </summary>
        /// <param name="newOut">
        /// 新しい標準出力であるストリームです。
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetOut(TextWriter newOut) => Console.SetOut(newOut);

        /// <summary>
        /// 指定した <see cref="TextWriter"/> を <see cref="Error"/> プロパティに設定します。
        /// </summary>
        /// <param name="newError">
        /// 新しい標準エラー出力であるストリームです。
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetError(TextWriter newError) => Console.SetError(newError);

        /// <summary>
        /// キーが押されたかどうか、つまり、押されたキーが入力ストリームに存在するかどうかを示す値を取得します。
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準入力がリダイレクトされています。</item>
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
        public static bool KeyAvailable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Console.KeyAvailable;
        }

        /// <summary>
        /// ユーザーによって押された次の文字キーまたはファンクション キーを取得します。
        /// 押されたキーは、コンソール ウィンドウに表示されます。
        /// </summary>
        /// <returns>
        /// 押されたコンソール キーに対応する <see cref="ConsoleKey"/> 定数と (もし存在する場合は) Unicode 文字を記述する <see cref="ConsoleKeyInfo"/> オブジェクトです。
        /// <see cref="ConsoleKeyInfo"/> オブジェクトは、1 つ以上の Shift、Alt、Ctrl の各修飾子キーがコンソール キーと同時に押されたかどうかを <see cref="ConsoleModifiers"/> 値のビットごとの組み合わせで記述します。
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準入力がリダイレクトされています。</item>
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConsoleKeyInfo ReadKey() => Console.ReadKey();

        /// <summary>
        /// ユーザーによって押された次の文字キーまたはファンクション キーを取得します。
        /// 押されたキーは、オプションでコンソール ウィンドウに表示されます。
        /// </summary>
        /// <param name="intercept">
        /// 押されたキーをコンソール ウィンドウに表示するかどうかを決定します。
        /// 押されたキーを表示しない場合は true。それ以外の場合は false です。
        /// </param>
        /// <returns>
        /// 押されたコンソール キーに対応する <see cref="ConsoleKey"/> 定数と (もし存在する場合は) Unicode 文字を記述する <see cref="ConsoleKeyInfo"/> オブジェクトです。
        /// <see cref="ConsoleKeyInfo"/> オブジェクトは、1 つ以上の Shift、Alt、Ctrl の各修飾子キーがコンソール キーと同時に押されたかどうかを <see cref="ConsoleModifiers"/> 値のビットごとの組み合わせで記述します。
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準入力がリダイレクトされています。</item>
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConsoleKeyInfo ReadKey(bool intercept) => Console.ReadKey(intercept);

        /// <summary>
        /// Control 修飾キー (Ctrl) と C コンソール キー (c) または Break キーが同時に押された場合 (Ctrl + C または Ctrl + Break) に発生するイベントです。
        /// </summary>
        public static event ConsoleCancelEventHandler? CancelKeyPress
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            add => Console.CancelKeyPress += value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            remove => Console.CancelKeyPress -= value;
        }

        /// <summary>
        /// Control修飾子キーとCコンソール キー (Ctrl+C)の組み合わせが、通常の入力として扱われるか、オペレーティングシステムによって処理される割り込みとして扱われるかを示す値を取得または設定します。
        /// </summary>
        public static bool TreatControlCAsInput
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Console.TreatControlCAsInput;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Console.TreatControlCAsInput = value;
        }

        /// <summary>
        /// 標準入力ストリームから次の文字を読み取ります。
        /// </summary>
        /// <returns>
        /// 入力ストリームから文字が読み込めた場合は、その文字を表す<see cref="int"/>値です。
        /// 次の文字がない場合は -1 です。
        /// </returns>
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Read() => Console.Read();

        /// <summary>
        /// 標準入力ストリームから次の 1 行分の文字を読み取ります。
        /// </summary>
        /// <returns>
        /// 入力ストリームから次の行が読み込めた場合は、その行を表す<see cref="string"/>値です。
        /// 次の行がない場合は null です。
        /// </returns>
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? ReadLine() => Console.ReadLine();
    }
}
