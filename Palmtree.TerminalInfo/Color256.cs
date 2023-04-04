using System;
using System.Collections.Generic;
using System.Linq;

namespace Palmtree.Terminal
{
    /// <summary>
    /// 256 種類の色を示す構造体です。
    /// </summary>
    public readonly struct Color256
        : IEquatable<Color256>
    {
        private static readonly byte[] _rdbCubeTable = new[] { (byte)0x00, (byte)0x5f, (byte)0x87, (byte)0xaf, (byte)0xd7, (byte)0xff };
        private static readonly ushort[] _rdbCubeBoundaries;
        private static readonly byte[] _grayScaleTable = new[] { (byte)0x08, (byte)0x12, (byte)0x1c, (byte)0x26, (byte)0x30, (byte)0x3a, (byte)0x44, (byte)0x4e, (byte)0x58, (byte)0x62, (byte)0x6c, (byte)0x76, (byte)0x80, (byte)0x8a, (byte)0x94, (byte)0x9e, (byte)0xa8, (byte)0xb2, (byte)0xbc, (byte)0xc6, (byte)0xd0, (byte)0xda, (byte)0xe4, (byte)0xee };
        private readonly byte _colorNumber;

        static Color256()
        {
            _rdbCubeBoundaries = new ushort[_rdbCubeTable.Length + 1];
            _rdbCubeBoundaries[0] = 0;
            _rdbCubeBoundaries[1] = GetBoundary(_rdbCubeTable[0], _rdbCubeTable[1]);
            _rdbCubeBoundaries[2] = GetBoundary(_rdbCubeTable[1], _rdbCubeTable[2]);
            _rdbCubeBoundaries[3] = GetBoundary(_rdbCubeTable[2], _rdbCubeTable[3]);
            _rdbCubeBoundaries[4] = GetBoundary(_rdbCubeTable[3], _rdbCubeTable[4]);
            _rdbCubeBoundaries[5] = GetBoundary(_rdbCubeTable[4], _rdbCubeTable[5]);
            _rdbCubeBoundaries[6] = 256;
        }

        private Color256(int colorCode)
        {
            if (colorCode.IsOutOfHalfClosedInterval(0, 256))
                throw new ArgumentOutOfRangeException(nameof(colorCode));

            _colorNumber = checked((byte)colorCode);
        }

        /// <summary>
        /// 黒色を意味する <see cref="Color256"/> 値です。
        /// </summary>
        public readonly static Color256 Black = new(0);

        /// <summary>
        /// 赤色を意味する <see cref="Color256"/> 値です。
        /// </summary>
        public readonly static Color256 Red = new(1);

        /// <summary>
        /// 緑色を意味する <see cref="Color256"/> 値です。
        /// </summary>
        public readonly static Color256 Green = new(2);

        /// <summary>
        /// 黄色を意味する <see cref="Color256"/> 値です。
        /// </summary>
        public readonly static Color256 Yellow = new(3);

        /// <summary>
        /// 青色を意味する <see cref="Color256"/> 値です。
        /// </summary>
        public readonly static Color256 Blue = new(4);

        /// <summary>
        /// マジェンタを意味する <see cref="Color256"/> 値です。
        /// </summary>
        public readonly static Color256 Magenta = new(5);

        /// <summary>
        /// シアンを意味する <see cref="Color256"/> 値です。
        /// </summary>
        public readonly static Color256 Cyan = new(6);

        /// <summary>
        /// 白色を意味する <see cref="Color256"/> 値です。
        /// </summary>
        public readonly static Color256 White = new(7);

        /// <summary>
        /// 明るい黒色を意味する <see cref="Color256"/> 値です。
        /// </summary>
        public readonly static Color256 BrightBlack = new(8);

        /// <summary>
        /// 明るい赤色を意味する <see cref="Color256"/> 値です。
        /// </summary>
        public readonly static Color256 BrightRed = new(9);

        /// <summary>
        /// 明るい緑色を意味する <see cref="Color256"/> 値です。
        /// </summary>
        public readonly static Color256 BrightGreen = new(10);

        /// <summary>
        /// 明るい黄色を意味する <see cref="Color256"/> 値です。
        /// </summary>
        public readonly static Color256 BrightYellow = new(11);

        /// <summary>
        /// 明るい青色を意味する <see cref="Color256"/> 値です。
        /// </summary>
        public readonly static Color256 BrightBlue = new(12);

        /// <summary>
        /// 明るいマジェンタを意味する <see cref="Color256"/> 値です。
        /// </summary>
        public readonly static Color256 BrightMagenta = new(13);

        /// <summary>
        /// 明るいシアンを意味する <see cref="Color256"/> 値です。
        /// </summary>
        public readonly static Color256 BrightCyan = new(14);

        /// <summary>
        /// 明るい白色を意味する <see cref="Color256"/> 値です。
        /// </summary>
        public readonly static Color256 BrightWhite = new(15);

        /// <summary>
        /// 24 段階のグレースケールの配列です。要素番号が大きいほど明るくなります。
        /// </summary>
        public static readonly IReadOnlyArray<Color256> GrayScales =
            Enumerable.Repeat(232, 24)
            .Select(code => new Color256(code))
            .ToReadOnlyArray();

        /// <summary>
        /// 色の RGB 成分を取得します。
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        /// <item><term>r</term><description>赤成分の値です。 0 &lt;= r &lt;= 255</description></item>
        /// <item><term>g</term><description>緑成分の値です。 0 &lt;= g &lt;= 255</description></item>
        /// <item><term>b</term><description>青成分の値です。 0 &lt;= b &lt;= 255</description></item>
        /// </list>
        /// </returns>
        public (byte r, byte g, byte b) ToRgb()
        {
            if (_colorNumber < 16)
            {
                return Color16.ToRgbCore(_colorNumber);
            }
            else if (_colorNumber < 232)
            {
                var colorCode = _colorNumber - 16;
                var rIndex = colorCode / 36 % 6;
                var gIndex = colorCode / 6 % 6;
                var bIndex = colorCode % 6;
                return (_rdbCubeTable[rIndex], _rdbCubeTable[gIndex], _rdbCubeTable[bIndex]);
            }
            else
            {
                var grayScale = _grayScaleTable[_colorNumber - 232];
                return (grayScale, grayScale, grayScale);
            }
        }

        /// <summary>
        /// 値を別の <see cref="Color256"/> 値と等しいかどうか調べます。
        /// </summary>
        /// <param name="other">
        /// 別の <see cref="Color256"/> 値です。
        /// </param>
        /// <returns>
        /// 値が <paramref name="other"/> と等しいなら true、そうではないなら false です。
        /// </returns>
        public bool Equals(Color256 other)
            => _colorNumber == other._colorNumber;

        /// <summary>
        /// 値が別のオブジェクトと等しいかどうか調べます。
        /// </summary>
        /// <param name="other">
        /// 別のオブジェクトです。
        /// </param>
        /// <returns>
        /// 値が <paramref name="other"/> と等しいなら true、そうではないなら false です。
        /// </returns>
        public override bool Equals(object? other)
            => other != null &&
                GetType() == other.GetType() &&
                Equals((Color256)other);

        /// <summary>
        /// 値のハッシュコードを取得します。
        /// </summary>
        /// <returns>
        /// ハッシュコードである <see cref="int"/>値です。
        /// </returns>
        public override int GetHashCode()
            => _colorNumber.GetHashCode();

        /// <summary>
        /// 値の文字列表現を取得します。
        /// </summary>
        /// <returns>
        /// 値の文字列表現の <see cref="string"/> オブジェクトです。
        /// </returns>
        public override string ToString()
        {
            if (_colorNumber < 16)
            {
                return Color16.ToStringCore(_colorNumber);
            }
            else if (_colorNumber < 232)
            {
                var colorCode = _colorNumber - 16;
                var rIndex = colorCode / 36 % 6;
                var gIndex = colorCode / 6 % 6;
                var bIndex = colorCode % 6;
                return $"#{_rdbCubeTable[rIndex]:x2}{_rdbCubeTable[gIndex]:x2}{_rdbCubeTable[bIndex]:x2}";
            }
            else
            {
                var grayScale = _grayScaleTable[_colorNumber - 232];
                return $"#{grayScale:x2}{grayScale:x2}{grayScale:x2}";
            }
        }

        /// <summary>
        /// 与えられた RGB 値に最も近い <see cref="Color256"/> 構造体を作成します。
        /// </summary>
        /// <param name="r">
        /// 赤要素の値です。
        /// 0 &lt;= <paramref name="r"/> &lt;= 255
        /// </param>
        /// <param name="g">
        /// 緑要素の値です。
        /// 0 &lt;= <paramref name="g"/> &lt;= 255
        /// </param>
        /// <param name="b">
        /// 青要素の値です。
        /// 0 &lt;= <paramref name="b"/> &lt;= 255
        /// </param>
        /// <returns>
        /// 作成された <see cref="Color256"/> 値です。
        /// </returns>
        public static Color256 FromRgb(byte r, byte g, byte b)
            => new(
                EnumerateGrayScaleColors(r, g, b)
                .Concat(Color16.FindColor16CodeFromRgb(r, g, b))
                .Append(GetRgbCube(r, g, b))
                .MinBy(item => item.distance2)
                .colorNumber);

        /// <summary>
        /// <see cref="Color256"/> 型のすべての値を列挙します。
        /// </summary>
        /// <returns>
        /// <see cref="Color256"/> 値の列挙子です。
        /// </returns>
        public static IEnumerable<Color256> GetValues()
        {
            for (var index = 0; index < 256; ++index)
                yield return new Color256(index);
        }

        /// <summary>
        /// 2つの <see cref="Color256"/> 値が等しいかどうか調べます。
        /// </summary>
        /// <param name="left">
        /// 左側の <see cref="Color256"/> 値です。
        /// </param>
        /// <param name="right">
        /// 右側の <see cref="Color256"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="left"/> と <paramref name="right"/> が等しいなら true、そうではないなら false です。
        /// </returns>
        public static bool operator ==(Color256 left, Color256 right) => left.Equals(right);

        /// <summary>
        /// 2つの <see cref="Color256"/> 値が等しくないかどうか調べます。
        /// </summary>
        /// <param name="left">
        /// 左側の <see cref="Color256"/> 値です。
        /// </param>
        /// <param name="right">
        /// 右側の <see cref="Color256"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="left"/> と <paramref name="right"/> が等しくないなら true、そうではないなら false です。
        /// </returns>
        public static bool operator !=(Color256 left, Color256 right) => !left.Equals(right);

        /// <summary>
        /// <see cref="Color256"/> 値を <see cref="int"/> 値に変換します。
        /// </summary>
        /// <param name="value">
        /// 変換する <see cref="Color256"/> 値です。
        /// </param>
        public static explicit operator int(Color256 value) => value._colorNumber;

        private static ushort GetBoundary(byte value1, byte value2)
        {
            var sum = value1 + value2;
            if ((sum & 1) != 0)
                ++sum;
            return (ushort)(sum >> 1);
        }

        private static (int colorNumber, int distance2) GetRgbCube(byte r, byte g, byte b)
        {
            var rIndex = 0;
            while (rIndex < _rdbCubeTable.Length)
            {
                if (r >= _rdbCubeBoundaries[rIndex] && r < _rdbCubeBoundaries[rIndex + 1])
                    break;
                ++rIndex;
            }

            Validation.Assert(rIndex < _rdbCubeTable.Length, "rIndex < _rdbCubeTable.Length");

            var gIndex = 0;
            while (gIndex < _rdbCubeTable.Length)
            {
                if (g >= _rdbCubeBoundaries[gIndex] && g < _rdbCubeBoundaries[gIndex + 1])
                    break;
                ++gIndex;
            }

            Validation.Assert(gIndex < _rdbCubeTable.Length, "gIndex < _rdbCubeTable.Length");

            var bIndex = 0;
            while (bIndex < _rdbCubeTable.Length)
            {
                if (b >= _rdbCubeBoundaries[bIndex] && b < _rdbCubeBoundaries[bIndex + 1])
                    break;
                ++bIndex;
            }

            Validation.Assert(bIndex < _rdbCubeTable.Length, "bIndex < _rdbCubeTable.Length");

            var colorNumber = rIndex * 36 + gIndex * 6 + bIndex + 16;
            var rDistance = _rdbCubeTable[rIndex] - r;
            var gDistance = _rdbCubeTable[gIndex] - g;
            var bDistance = _rdbCubeTable[bIndex] - b;
            return (colorNumber, rDistance * rDistance + gDistance * gDistance + bDistance + bDistance);
        }

        private static IEnumerable<(int colorNumber, int distance2)> EnumerateGrayScaleColors(byte r, byte g, byte b)
        {
            for (var index = 0; index < _grayScaleTable.Length; ++index)
            {
                var colorNumber = index + 232;
                var rDistance = _grayScaleTable[index] - r;
                var gDistance = _grayScaleTable[index] - g;
                var bDistance = _grayScaleTable[index] - b;
                yield return (colorNumber, rDistance * rDistance + gDistance * gDistance + bDistance * bDistance);
            }
        }
    }
}
