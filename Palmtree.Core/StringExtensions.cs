using System.Linq;

namespace Palmtree
{
    /// <summary>
    /// 文字列の拡張メソッドのクラスです。
    /// </summary>
    public static class StringExtensions
    {
        static StringExtensions()
        {
            Validation.Assert('\u0007' == '\a', "'\u0007' == '\a'");
            Validation.Assert('\u0008' == '\b', "'\u0008' == '\b'");
            Validation.Assert('\u0009' == '\t', "'\u0009' == '\t'");
            Validation.Assert('\u000a' == '\n', "'\u000a' == '\n'");
            Validation.Assert('\u000b' == '\v', "'\u000b' == '\v'");
            Validation.Assert('\u000c' == '\f', "'\u000c' == '\f'");
            Validation.Assert('\u000c' == '\f', "'\u000c' == '\f'");
            Validation.Assert('\u000d' == '\r', "'\u000d' == '\r'");
        }

        /// <summary>
        /// 指定された文字列を JSON 形式でエンコードします。
        /// </summary>
        /// <param name="s">エンコード対象の文字列です。</param>
        /// <returns>エンコードされた文字列です。</returns>
        public static string JsonEncode(this string s)
            => string.Concat(
                s.Select(c =>
                    c switch
                    {
                        '\u0000' or '\u0001' or '\u0002' or '\u0003' or '\u0004' or '\u0005' or '\u0006' or '\u0007' or '\u000b' or '\u000e' or '\u000f' or '\u0010' or '\u0011' or '\u0012' or '\u0013' or '\u0014' or '\u0015' or '\u0016' or '\u0017' or '\u0018' or '\u0019' or '\u001a' or '\u001b' or '\u001c' or '\u001d' or '\u001e' or '\u001f' or '\u007f'
                            => $"\\u{(int)c:x4}",
                        '\u0008' => "\\b",
                        '\u0009' => "\\t",
                        '\u000a' => "\\n",
                        '\u000c' => "\\f",
                        '\u000d' => "\\r",
                        '\"' => "\\\"",
                        '\\' => "\\\\",
                        '/' => "\\/",
                        _ => c.ToString(),
                    }));

        /// <summary>
        /// 指定された文字列を C# の文字列リテラル形式でエンコードします。
        /// </summary>
        /// <param name="s">エンコード対象の文字列です。</param>
        /// <returns>エンコードされた文字列です。</returns>
        public static string CSharpEncode(this string s)
            => string.Concat(
                s.Select(c =>
                    c switch
                    {
                        '\u0000' or '\u0001' or '\u0002' or '\u0003' or '\u0004' or '\u0005' or '\u0006' or '\u000e' or '\u000f' or '\u0010' or '\u0011' or '\u0012' or '\u0013' or '\u0014' or '\u0015' or '\u0016' or '\u0017' or '\u0018' or '\u0019' or '\u001a' or '\u001b' or '\u001c' or '\u001d' or '\u001e' or '\u001f' or '\u007f'
                            => $"\\u{(int)c:x4}",
                        '\u0007' => "\\a",
                        '\u0008' => "\\b",
                        '\u0009' => "\\t",
                        '\u000a' => "\\n",
                        '\u000b' => "\\v",
                        '\u000c' => "\\f",
                        '\u000d' => "\\r",
                        '\"' => "\\\"",
                        '\\' => "\\\\",
                        _ => c.ToString(),
                    }));
    }
}
