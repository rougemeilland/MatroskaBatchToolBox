using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Palmtree.Numerics;

namespace MatroskaBatchToolBox.Model
{
    internal class Rectangle
    {
        private static readonly Regex _rectanglePattern;

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

        public static bool TryParse(string text, [NotNullWhen(true)] out Rectangle? rectangle)
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
                        match.Groups["left"].Value.ParseAsInt32(),
                        match.Groups["top"].Value.ParseAsInt32(),
                        match.Groups["width"].Value.ParseAsInt32(),
                        match.Groups["height"].Value.ParseAsInt32());
                return true;
            }
        }
    }
}
