namespace Palmtree.Terminal.StringExpansion
{
    internal class ExpansionStringParameter
        : ExpansionParameter
    {
        private readonly string _value;

        public ExpansionStringParameter(string value) => _value = value;

        public override int AsNumber() => throw new ExpansionBadArgumentExceptionException("Cannot call \"AsNumber()\" on numeric type values.");
        public override bool AsBool() => throw new ExpansionBadArgumentExceptionException("Cannot call \"AsBool()\" on numeric type values.");
        public override string AsString() => _value;

        protected override string Format(string width, string precision, string typeSpec)
        {
            if (typeSpec != "s")
                throw new ExpansionBadArgumentExceptionException($"Not supported type spec.: \"{typeSpec}\"");

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

            var valueText = _value;
            if (precisionValue >= 0)
                valueText = valueText[..precisionValue.Minimum(valueText.Length)];
            if (widthValue >= 0)
                valueText = $"{new string(width[0] == '0' ? '0' : ' ', (widthValue - valueText.Length).Maximum(0))}{valueText}";
            return valueText;
        }
    }
}
