using System;

namespace Palmtree
{
    partial class TinyConsole
    {
        /// <summary>
        /// 指定した <see cref="object"/> のテキスト形式をコンソールに書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void Write(object? value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.Write(value);
        }

        /// <summary>
        /// 指定した <see cref="bool"/> 値のテキスト形式をコンソールに書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void Write(bool value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.Write(value);
        }

        /// <summary>
        /// 指定した UNICODE 文字値をコンソールに書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void Write(char value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.Write(value);
        }

        /// <summary>
        /// 指定した <see cref="int"/> 値のテキスト形式をコンソールに書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void Write(int value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.Write(value);
        }

        /// <summary>
        /// 指定した <see cref="uint"/> 値のテキスト形式をコンソールに書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void Write(uint value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.Write(value);
        }

        /// <summary>
        /// 指定した <see cref="long"/> 値のテキスト形式をコンソールに書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void Write(long value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.Write(value);
        }

        /// <summary>
        /// 指定した <see cref="ulong"/> 値のテキスト形式をコンソールに書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void Write(ulong value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.Write(value);
        }

        /// <summary>
        /// 指定した <see cref="float"/> 値のテキスト形式をコンソールに書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void Write(float value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.Write(value);
        }

        /// <summary>
        /// 指定した <see cref="double"/> 値のテキスト形式をコンソールに書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void Write(double value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.Write(value);
        }

        /// <summary>
        /// 指定した <see cref="decimal"/> 値のテキスト形式をコンソールに書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void Write(decimal value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.Write(value);
        }

        /// <summary>
        /// 指定した <see cref="string"/> 値のテキスト形式をコンソールに書き込みます。
        /// </summary>
        /// <param name="value">書き込む値です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void Write(string? value)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.Write(value);
        }

        /// <summary>
        /// 指定した UNICODE 文字配列をコンソールに書き込みます。
        /// </summary>
        /// <param name="buffer">Unicode 文字配列です。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void Write(char[]? buffer)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.Write(buffer);
        }

        /// <summary>
        /// 指定した UNICODE 文字の部分配列をコンソールに書き込みます。
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
        public static void Write(char[] buffer, int index, int count)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.Write(buffer, index, count);
        }

        /// <summary>
        /// 指定した書式情報を使用して、指定した <see cref="object"/> 値のテキスト表現をコンソールに書き込みます。
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
        public static void Write(string format, object? arg0)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.Write(format, arg0);
        }

        /// <summary>
        /// 指定した書式情報を使用して、指定した <see cref="object"/> 値のテキスト表現をコンソールに書き込みます。
        /// </summary>
        /// <param name="format">複合書式設定文字列です。</param>
        /// <param name="arg0"><paramref name="format"/> を使用して書き込む最初のオブジェクトです。</param>
        /// <param name="arg1"><paramref name="format"/> を使用して書き込む 2 番目のオブジェクトです。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void Write(string format, object? arg0, object? arg1)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.Write(format, arg0, arg1);
        }

        /// <summary>
        /// 指定した書式情報を使用して、指定した <see cref="object"/> 値のテキスト表現をコンソールに書き込みます。
        /// </summary>
        /// <param name="format">複合書式設定文字列です。</param>
        /// <param name="arg0"><paramref name="format"/> を使用して書き込む最初のオブジェクトです。</param>
        /// <param name="arg1"><paramref name="format"/> を使用して書き込む 2 番目のオブジェクトです。</param>
        /// <param name="arg2"><paramref name="format"/> を使用して書き込む 3 番目のオブジェクトです。</param>
        /// <remarks>
        /// 実際の出力先は以下の通りです。
        /// <list type="bullet">
        /// <item>標準出力がリダイレクトされていない場合は、標準出力ストリーム</item>
        /// <item>標準出力がリダイレクトされている場合は、標準エラー出力ストリーム</item>
        /// </list>
        /// </remarks>
        public static void Write(string format, object? arg0, object? arg1, object? arg2)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.Write(format, arg0, arg1, arg2);
        }

        /// <summary>
        /// 指定された書式情報を使用して、指定した <see cref="object"/> 配列のテキスト表現をコンソールに書き込みます。
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
        public static void Write(string format, params object?[] arg)
        {
            SetCharacterSet(CharacterSet.Primary);
            _consoleTextWriter.Write(format, arg);
        }

        /// <summary>
        /// 代替文字 (グラフィックス文字) をコンソールに書き込みます。
        /// </summary>
        /// <param name="altChar">
        /// 代替文字である <see cref="AlternativeChar"/> 値です。
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>標準エラー出力の両方がリダイレクトされています。</item>
        /// </list>
        /// </exception>
        /// <remarks>
        /// <list type="bullet">
        /// ターミナルによってはすべての代替文字がサポートされているとは限りません。ターミナルがサポートしていない代替文字は'?'として表示されます。
        /// </list>
        /// </remarks>
        public static void Write(AlternativeChar altChar)
        {
            if (_escapeCodeWriter is null)
                throw new InvalidOperationException("Since both standard error output is redirected, the alternate characters cannot be displayed.");

            var key = (char)altChar;
            var c =
                key.IsInHalfClosedInterval(_alternativeCharacterSetMapMinimumKey, (char)(_alternativeCharacterSetMapMinimumKey + _alternativeCharacterSetMap.Length))
                ? _alternativeCharacterSetMap[key - _alternativeCharacterSetMapMinimumKey]
                : '\0';
            if (c == '\0')
            {
                SetCharacterSet(CharacterSet.Primary);
                _escapeCodeWriter.Write('?');
            }
            else
            {
                SetCharacterSet(CharacterSet.Alternative);
                _escapeCodeWriter.Write(c);
            }
        }
    }
}
