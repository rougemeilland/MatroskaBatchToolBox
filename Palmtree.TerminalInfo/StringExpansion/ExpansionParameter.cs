using System.Text.RegularExpressions;

namespace Palmtree.Terminal.StringExpansion
{
    internal abstract class ExpansionParameter
    {
        private static readonly Regex _formatSpecPattern;

        static ExpansionParameter()
            => _formatSpecPattern =
                new Regex(
                    @"^(?<width>\d+)?(\.(?<precision>\d+))?(?<type>[cdosuxX])$",
                    RegexOptions.Compiled);

        public abstract int AsNumber();
        public abstract bool AsBool();
        public abstract string AsString();

        public string Format(string formatSpec)
        {
            var match = _formatSpecPattern.Match(formatSpec);
            return
                match.Success
                ? Format(
                    match.Groups["width"].Success
                    ? match.Groups["width"].Value
                    : "",
                    match.Groups["precision"].Success
                    ? match.Groups["precision"].Value
                    : "",
                    match.Groups["type"].Value)
                : throw new ExpansionStringSyntaxErrorExceptionException($"Invalid format spec. \"{formatSpec}\"");
        }

        protected abstract string Format(string width, string precision, string typeSpec);
    }
}
