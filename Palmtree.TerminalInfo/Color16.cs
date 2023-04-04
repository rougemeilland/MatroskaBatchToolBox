using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Palmtree.Terminal
{
    /// <summary>
    /// 16 種類の色を示す構造体です。
    /// </summary>
    public readonly struct Color16
        : IEquatable<Color16>
    {
        private static readonly uint[] _colorCodes = new[]
        {
            0x000000U,
            0x800000U,
            0x008000U,
            0x808000U,
            0x000080U,
            0x800080U,
            0x008080U,
            0xc0c0c0U,
            0x808080U,
            0xff0000U,
            0x00ff00U,
            0xffff00U,
            0x0000ffU,
            0xff00ffU,
            0x00ffffU,
            0xffffffU,
        };

        private readonly byte _colorNumber;

        private Color16(int colorCode)
        {
            if (colorCode.IsOutOfHalfClosedInterval(0, 16))
                throw new ArgumentOutOfRangeException(nameof(colorCode));

            _colorNumber = checked((byte)colorCode);
        }

        /// <summary>
        /// 黒色を意味する <see cref="Color16"/> 値です。
        /// </summary>
        public readonly static Color16 Black = new(0);

        /// <summary>
        /// 赤色を意味する <see cref="Color16"/> 値です。
        /// </summary>
        public static readonly Color16 Red = new(1);

        /// <summary>
        /// 緑色を意味する <see cref="Color16"/> 値です。
        /// </summary>
        public readonly static Color16 Green = new(2);

        /// <summary>
        /// 黄色を意味する <see cref="Color16"/> 値です。
        /// </summary>
        public readonly static Color16 Yellow = new(3);

        /// <summary>
        /// 青色を意味する <see cref="Color16"/> 値です。
        /// </summary>
        public readonly static Color16 Blue = new(4);

        /// <summary>
        /// マジェンタを意味する <see cref="Color16"/> 値です。
        /// </summary>
        public readonly static Color16 Magenta = new(5);

        /// <summary>
        /// シアンを意味する <see cref="Color16"/> 値です。
        /// </summary>
        public readonly static Color16 Cyan = new(6);

        /// <summary>
        /// 白色を意味する <see cref="Color16"/> 値です。
        /// </summary>
        public readonly static Color16 White = new(7);

        /// <summary>
        /// 明るい黒色を意味する <see cref="Color16"/> 値です。
        /// </summary>
        public readonly static Color16 BrightBlack = new(8);

        /// <summary>
        /// 明るい赤色を意味する <see cref="Color16"/> 値です。
        /// </summary>
        public readonly static Color16 BrightRed = new(9);

        /// <summary>
        /// 明るい緑色を意味する <see cref="Color16"/> 値です。
        /// </summary>
        public readonly static Color16 BrightGreen = new(10);

        /// <summary>
        /// 明るい黄色を意味する <see cref="Color16"/> 値です。
        /// </summary>
        public readonly static Color16 BrightYellow = new(11);

        /// <summary>
        /// 明るい青色を意味する <see cref="Color16"/> 値です。
        /// </summary>
        public readonly static Color16 BrightBlue = new(12);

        /// <summary>
        /// 明るいマジェンタを意味する <see cref="Color16"/> 値です。
        /// </summary>
        public readonly static Color16 BrightMagenta = new(13);

        /// <summary>
        /// 明るいシアンを意味する <see cref="Color16"/> 値です。
        /// </summary>
        public readonly static Color16 BrightCyan = new(14);

        /// <summary>
        /// 明るい白色を意味する <see cref="Color16"/> 値です。
        /// </summary>
        public static readonly Color16 BrightWhite = new(15);

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
            => ToRgbCore(_colorNumber);

        /// <summary>
        /// 値を別の <see cref="Color16"/> 値と等しいかどうか調べます。
        /// </summary>
        /// <param name="other">
        /// 別の <see cref="Color16"/> 値です。
        /// </param>
        /// <returns>
        /// 値が <paramref name="other"/> と等しいなら true、そうではないなら false です。
        /// </returns>
        public bool Equals(Color16 other)
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
                Equals((Color16)other);

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
            => ToStringCore(_colorNumber);

        /// <summary>
        /// 与えられた RGB 値に最も近い <see cref="Color16"/> 構造体を作成します。
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
        /// 作成された <see cref="Color16"/> 値です。
        /// </returns>
        public static Color16 FromRgb(byte r, byte g, byte b)
            => new(
                FindColor16CodeFromRgb(r, g, b)
                .MinBy(item => item.distance2)
                .colorNumber);

        /// <summary>
        /// <see cref="Color16"/> 型のすべての値を列挙します。
        /// </summary>
        /// <returns>
        /// <see cref="Color16"/> 値の列挙子です。
        /// </returns>
        public static IEnumerable<Color16> GetValues()
        {
            for (var index = 0; index < 16; ++index)
                yield return new Color16(index);
        }

        /// <summary>
        /// 2つの <see cref="Color16"/> 値が等しいかどうか調べます。
        /// </summary>
        /// <param name="left">
        /// 左側の <see cref="Color16"/> 値です。
        /// </param>
        /// <param name="right">
        /// 右側の <see cref="Color16"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="left"/> と <paramref name="right"/> が等しいなら true、そうではないなら false です。
        /// </returns>
        public static bool operator ==(Color16 left, Color16 right) => left.Equals(right);

        /// <summary>
        /// 2つの <see cref="Color16"/> 値が等しくないかどうか調べます。
        /// </summary>
        /// <param name="left">
        /// 左側の <see cref="Color16"/> 値です。
        /// </param>
        /// <param name="right">
        /// 右側の <see cref="Color16"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="left"/> と <paramref name="right"/> が等しくないなら true、そうではないなら false です。
        /// </returns>
        public static bool operator !=(Color16 left, Color16 right) => !left.Equals(right);

        /// <summary>
        /// <see cref="Color16"/> 値を <see cref="int"/> 値に変換します。
        /// </summary>
        /// <param name="value">
        /// 変換する <see cref="Color16"/> 値です。
        /// </param>
        public static explicit operator int(Color16 value) => value._colorNumber;

        /// <summary>
        /// <see cref="Color16"/> 値を <see cref="Color88"/> 値に変換します。
        /// </summary>
        /// <param name="value">
        /// 変換する <see cref="Color16"/> 値です。
        /// </param>
        /// <returns>
        /// 変換された <see cref="Color88"/> 値です。
        /// </returns>
        public static explicit operator Color88(Color16 value)
            => value._colorNumber switch
            {
                0 => Color88.Black,
                1 => Color88.Red,
                2 => Color88.Green,
                3 => Color88.Yellow,
                4 => Color88.Blue,
                5 => Color88.Magenta,
                6 => Color88.Cyan,
                7 => Color88.White,
                8 => Color88.BrightBlack,
                9 => Color88.BrightRed,
                10 => Color88.BrightGreen,
                11 => Color88.BrightYellow,
                12 => Color88.BrightBlue,
                13 => Color88.BrightMagenta,
                14 => Color88.BrightCyan,
                15 => Color88.BrightWhite,
                _ => throw Validation.GetFailErrorException($"Unexpected {nameof(value._colorNumber)} value: {value._colorNumber}"),
            };

        /// <summary>
        /// <see cref="Color16"/> 値を <see cref="Color256"/> 値に変換します。
        /// </summary>
        /// <param name="value">
        /// 変換する <see cref="Color16"/> 値です。
        /// </param>
        /// <returns>
        /// 変換された <see cref="Color256"/> 値です。
        /// </returns>
        public static explicit operator Color256(Color16 value)
            => value._colorNumber switch
            {
                0 => Color256.Black,
                1 => Color256.Red,
                2 => Color256.Green,
                3 => Color256.Yellow,
                4 => Color256.Blue,
                5 => Color256.Magenta,
                6 => Color256.Cyan,
                7 => Color256.White,
                8 => Color256.BrightBlack,
                9 => Color256.BrightRed,
                10 => Color256.BrightGreen,
                11 => Color256.BrightYellow,
                12 => Color256.BrightBlue,
                13 => Color256.BrightMagenta,
                14 => Color256.BrightCyan,
                15 => Color256.BrightWhite,
                _ => throw Validation.GetFailErrorException($"Unexpected {nameof(value._colorNumber)} value: {value._colorNumber}"),
            };

        internal static IEnumerable<(int colorNumber, int distance2)> FindColor16CodeFromRgb(byte r, byte g, byte b)
            => _colorCodes
                .Select((code, colorNumber) =>
                {
                    var rDistance = ((int)code >> 16) - r;
                    var gDistance = (((int)code >> 8) & 0xff) - g;
                    var bDistance = ((int)code & 0xff) - b;
                    return (colorNumber, rDistance * rDistance + gDistance * gDistance + bDistance * bDistance);
                });

        internal static (byte r, byte g, byte b) ToRgbCore(byte colorNumber)
            => ((byte)(_colorCodes[colorNumber] >> 16),
                (byte)(_colorCodes[colorNumber] >> 8),
                (byte)(_colorCodes[colorNumber] >> 0));

        internal static string ToStringCore(byte colorNumber)
            => colorNumber switch
            {
                0 => "Black",
                1 => "Red",
                2 => "Green",
                3 => "Yellow",
                4 => "Blue",
                5 => "Magenta",
                6 => "Cyan",
                7 => "White",
                8 => "BrightBlack",
                9 => "BrightRed",
                10 => "BrightGreen",
                11 => "BrightYellow",
                12 => "BrightBlue",
                13 => "BrightMagenta",
                14 => "BrightCyan",
                15 => "BrightWhite",
                _ => throw Validation.GetFailErrorException($"Unexpected {nameof(colorNumber)} value: {colorNumber}"),
            };
    }
}
