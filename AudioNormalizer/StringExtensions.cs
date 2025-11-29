using System;
using System.Linq;
using System.Text;
using Palmtree;

namespace AudioNormalizer
{
    internal static class StringExtensions
    {
        private static readonly char[] _kid3EscapeCharacters = ['\\', '\"'];

#if DEBUG
        static StringExtensions()
        {
            Validation.Assert(char.IsWhiteSpace(' ') == true);
            Validation.Assert(char.IsWhiteSpace('\t') == true);
            Validation.Assert(char.IsWhiteSpace('\r') == true);
            Validation.Assert(char.IsWhiteSpace('\n') == true);
            Validation.Assert("".EncodeKid3CommandLineArgument() == "");
            Validation.Assert("a".EncodeKid3CommandLineArgument() == "a");
            Validation.Assert("aaaa".EncodeKid3CommandLineArgument() == "aaaa");
            Validation.Assert("\\".EncodeKid3CommandLineArgument() == "\\\\");
            Validation.Assert("\\\\".EncodeKid3CommandLineArgument() == "\\\\\\\\");
            Validation.Assert(" ".EncodeKid3CommandLineArgument() == "\" \"");
            Validation.Assert("  ".EncodeKid3CommandLineArgument() == "\"  \"");
            Validation.Assert("\\先頭にバックスラッシュ".EncodeKid3CommandLineArgument() == "\\\\先頭にバックスラッシュ");
            Validation.Assert("末尾にバックスラッシュ\\".EncodeKid3CommandLineArgument() == "末尾にバックスラッシュ\\\\");
            Validation.Assert("途中に\\バックスラッシュ".EncodeKid3CommandLineArgument() == "途中に\\\\バックスラッシュ");
            Validation.Assert("\"先頭にダブルクォート".EncodeKid3CommandLineArgument() == "\\\"先頭にダブルクォート");
            Validation.Assert("末尾にダブルクォート\"".EncodeKid3CommandLineArgument() == "末尾にダブルクォート\\\"");
            Validation.Assert("途中に\"ダブルクォート".EncodeKid3CommandLineArgument() == "途中に\\\"ダブルクォート");
            Validation.Assert(" 先頭に空白".EncodeKid3CommandLineArgument() == "\" 先頭に空白\"");
            Validation.Assert("末尾に空白 ".EncodeKid3CommandLineArgument() == "\"末尾に空白 \"");
            Validation.Assert("途中に 空白".EncodeKid3CommandLineArgument() == "\"途中に 空白\"");
        }
#endif

        public static string EncodeKid3CommandLineArgument(this string arg, bool quoteAlways = false)
        {
            var sb = new StringBuilder();
            var index = 0;
            while (index < arg.Length)
            {
                var newIndex = arg.IndexOfAny(_kid3EscapeCharacters, index);
                if (newIndex < 0)
                {
                    _ = sb.Append(arg.AsSpan(index));
                    break;
                }

                _ = sb.Append(arg[index..newIndex]);
                _ = sb.Append('\\');
                _ = sb.Append(arg[newIndex]);
                index = newIndex + 1;
            }

            var newArg = sb.ToString();
            return
                quoteAlways || newArg.Any(char.IsWhiteSpace)
                ? $"\"{newArg}\""
                : newArg;
        }
    }
}
