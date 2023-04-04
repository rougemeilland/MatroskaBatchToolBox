using System;
using System.Globalization;
using System.Text;

namespace Palmtree.Terminal.StringExpansion
{
    internal class ExpansionNumberParameter
        : ExpansionParameter
    {
        private readonly int _value;

        public ExpansionNumberParameter(int value) => _value = value;
        public ExpansionNumberParameter(char value) => _value = value;
        public ExpansionNumberParameter(bool value) => _value = value ? 1 : 0;

        public override int AsNumber() => _value;
        public override bool AsBool() => _value != 0;
        public override string AsString() => throw new ExpansionBadArgumentExceptionException("Cannot call \"AsString()\" on numeric type values.");

        protected override string Format(string width, string precision, string typeSpec)
        {
            if (!width.TryParse(out int widthValue))
            {
                widthValue =
                    string.IsNullOrEmpty(width)
                    ? -1
                    : throw new ExpansionBadArgumentExceptionException($"Invalid format of {nameof(width)}.: \"{width}\"");
            }

            if (!precision.TryParse(out int precisionValue))
            {
                precisionValue =
                    string.IsNullOrEmpty(precision)
                    ? -1
                    : throw new ExpansionBadArgumentExceptionException($"Invalid format of {nameof(precision)}.: \"{precision}\"");
            }

            return
                typeSpec switch
                {
                    "c" =>
                        PadText(
                            new string((char)(byte)_value, 1),
                            widthValue,
                            width.Length > 0 && width[0] == '0' ? '0' : ' '),
                    "d" =>
                        Format(
                            _value < 0 ? "-" : "",
                            (_value < 0 ? checked(-_value) : _value).ToString("D", CultureInfo.InvariantCulture.NumberFormat),
                            widthValue,
                            precisionValue,
                            width.Length > 0 && width[0] == '0'),
                    "o" =>
                        Format(
                            "",
                            Convert.ToString(_value, 8),
                            widthValue,
                            precisionValue,
                            width.Length > 0 && width[0] == '0'),
                    "u" =>
                        Format(
                            "",
                            unchecked((uint)_value).ToString("D", CultureInfo.InvariantCulture.NumberFormat),
                            widthValue,
                            precisionValue,
                            width.Length > 0 && width[0] == '0'),
                    "x" or "X" =>
                        Format(
                            "",
                            _value.ToString(typeSpec, CultureInfo.InvariantCulture.NumberFormat),
                            widthValue,
                            precisionValue,
                            width.Length > 0 && width[0] == '0'),
                    _ => throw new ExpansionBadArgumentExceptionException($"Not supported type spec.: \"{typeSpec}\""),
                };
        }

        private static string FromByteToString(byte data)
        {
            Span<byte> buffer = stackalloc byte[1];
            buffer[0] = data;
            return Encoding.ASCII.GetString(buffer);
        }

        private static string Format(string sign, string valueText, int width, int precision, bool zeroPadding)
            => precision >= 0
                ? PadText($"{sign}{PadText(valueText, precision, '0')}", width, ' ')
                : width < 0
                ? $"{sign}{valueText}"
                : zeroPadding
                ? $"{sign}{PadText(valueText, (width - sign.Length).Maximum(0), '0')}"
                : PadText($"{sign}{valueText}", width, ' ');

        private static string PadText(string text, int length, char paddingChar)
            => length > text.Length ? $"{new string(paddingChar, length - text.Length)}{text}" : text;
    }
}
