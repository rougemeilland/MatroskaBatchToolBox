using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MatroskaBatchToolBox.Model
{
    internal class Rectangle
    {
        private static Regex _rectanglePattern;

        static Rectangle()
        {
            _rectanglePattern = new Regex(@"^\((?<left>\d+),(?<top>\d+)\)-(?<width>\d+)x(?<height>\d+)$", RegexOptions.Compiled);
            DefaultValue = new Rectangle(-1, -1, -1, -1);
        }

        public Rectangle(int left, int top, int width, int height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }

        public int Left { get; }
        public int Top { get; }
        public int Width { get; }
        public int Height { get; }
        public bool IsValid => Left >= 0 && Top >= 0 && Width >= 0 && Height >= 0;
        public static Rectangle DefaultValue { get; }

        public static bool TryParse(string text, [MaybeNullWhen(false)] out Rectangle rectangle)
        {
            var match = _rectanglePattern.Match(text);
            if (!match.Success)
            {
                rectangle = null;
                return false;
            }
            else
            {
                rectangle =
                    new Rectangle(
                        int.Parse(match.Groups["left"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat),
                        int.Parse(match.Groups["top"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat),
                        int.Parse(match.Groups["width"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat),
                        int.Parse(match.Groups["height"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat));
                return true;
            }
        }
    }
}
