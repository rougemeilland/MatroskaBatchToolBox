namespace Palmtree
{
    partial class TinyConsole
    {
        /// <summary>
        /// 現在の行終端記号をコンソールに書き込みます。
        /// </summary>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void WriteLine()
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.WriteLine();
        }

        /// <summary>
        /// 指定した <see cref="object"/> のテキスト形式をコンソールに書き込み、続けて現在の行終端記号を書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void WriteLine(object? value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.WriteLine(value);
        }

        /// <summary>
        /// 指定した <see cref="bool"/> 値のテキスト形式をコンソールに書き込み、続けて現在の行終端記号を書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void WriteLine(bool value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.WriteLine(value);
        }

        /// <summary>
        /// 指定した UNICODE 文字をコンソールに書き込み、続けて現在の行終端記号を書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void WriteLine(char value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.WriteLine(value);
        }

        /// <summary>
        /// 指定した <see cref="int"/> 値のテキスト形式をコンソールに書き込み、続けて現在の行終端記号を書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void WriteLine(int value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.WriteLine(value);
        }

        /// <summary>
        /// 指定した <see cref="uint"/> 値のテキスト形式をコンソールに書き込み、続けて現在の行終端記号を書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void WriteLine(uint value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.WriteLine(value);
        }

        /// <summary>
        /// 指定した <see cref="long"/> 値のテキスト形式をコンソールに書き込み、続けて現在の行終端記号を書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void WriteLine(long value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.WriteLine(value);
        }

        /// <summary>
        /// 指定した <see cref="ulong"/> 値のテキスト形式をコンソールに書き込み、続けて現在の行終端記号を書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void WriteLine(ulong value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.WriteLine(value);
        }

        /// <summary>
        /// 指定した <see cref="float"/> 値のテキスト形式をコンソールに書き込み、続けて現在の行終端記号を書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void WriteLine(float value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.WriteLine(value);
        }

        /// <summary>
        /// 指定した <see cref="double"/>値のテキスト形式をコンソールに書き込み、続けて現在の行終端記号を書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void WriteLine(double value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.WriteLine(value);
        }

        /// <summary>
        /// 指定した <see cref="decimal"/> 値のテキスト形式をコンソールに書き込み、続けて現在の行終端記号を書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void WriteLine(decimal value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.WriteLine(value);
        }

        /// <summary>
        /// 指定した<see cref="string"/> 値をコンソールに書き込み、続けて現在の行終端記号を書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void WriteLine(string? value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.WriteLine(value);
        }

        /// <summary>
        /// 指定した UNICODE 文字配列をコンソールに書き込み、続けて現在の行終端記号を書き込みます。
        /// </summary>
        /// <param name="buffer">書き込む UNICODE 文字配列です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void WriteLine(char[]? buffer)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.WriteLine(buffer);
        }

        /// <summary>
        /// 指定した UNICODE 文字の部分配列をコンソールに書き込み、続けて現在の行終端記号を書き込みます。
        /// </summary>
        /// <param name="buffer">UNICODE 文字の配列です。</param>
        /// <param name="index"><paramref name="buffer"/> 内の開始位置です。</param>
        /// <param name="count">書き込む文字数です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void WriteLine(char[] buffer, int index, int count)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.WriteLine(buffer, index, count);
        }

        /// <summary>
        /// 指定した書式情報を使用して、指定した <see cref="object"/> 値のテキスト表現をコンソールに書き込み、続けて現在の行終端記号を書き込みます。
        /// </summary>
        /// <param name="format">複合書式設定文字列です。</param>
        /// <param name="arg0"><paramref name="format"/> を使用して書き込むオブジェクトです。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void WriteLine(string format, object? arg0)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.WriteLine(format, arg0);
        }

        /// <summary>
        /// 指定した書式情報を使用して、指定した <see cref="object"/> 値のテキスト表現をコンソールに書き込み、続けて現在の行終端記号を書き込みます。
        /// </summary>
        /// <param name="format">複合書式設定文字列です。</param>
        /// <param name="arg0"><paramref name="format"/> を使用して書き込む最初のオブジェクトです。</param>
        /// <param name="arg1"><paramref name="format"/> を使用して書き込む 2 番目のオブジェクト。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void WriteLine(string format, object? arg0, object? arg1)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.WriteLine(format, arg0, arg1);
        }

        /// <summary>
        /// 指定した書式情報を使用して、指定した <see cref="object"/> 値のテキスト表現をコンソールに書き込み、続けて現在の行終端記号を書き込みます。
        /// </summary>
        /// <param name="format">複合書式設定文字列です。</param>
        /// <param name="arg0"><paramref name="format"/> を使用して書き込む最初のオブジェクトです。</param>
        /// <param name="arg1"><paramref name="format"/> を使用して書き込む 2 番目のオブジェクト。</param>
        /// <param name="arg2"><paramref name="format"/> を使用して書き込む 3 番目のオブジェクトです。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void WriteLine(string format, object? arg0, object? arg1, object? arg2)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.WriteLine(format, arg0, arg1, arg2);
        }

        /// <summary>
        /// 指定した書式情報を使用して、指定した <see cref="object"/> 配列のテキスト表現をコンソールに書き込み、続けて現在の行終端記号を書き込みます。
        /// </summary>
        /// <param name="format">複合書式設定文字列です。</param>
        /// <param name="arg"><paramref name="format"/> を使用して書き込むオブジェクトの配列です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void WriteLine(string format, params object?[] arg)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.WriteLine(format, arg);
        }
    }
}
