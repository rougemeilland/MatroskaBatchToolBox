using System;
using System.Collections.Generic;

namespace Palmtree.Terminal
{
    /// <summary>
    /// 8 種類の色を示す構造体です。
    /// </summary>
    public readonly struct Color8
        : IEquatable<Color8>
    {
        private readonly byte _colorNumber;

        private Color8(int colorCode)
        {
            if (colorCode.IsOutOfHalfClosedInterval(0, 8))
                throw new ArgumentOutOfRangeException(nameof(colorCode));

            _colorNumber = checked((byte)colorCode);
        }

        /// <summary>
        /// 黒色を意味する <see cref="Color8"/> 値です。
        /// </summary>
        public readonly static Color8 Black = new(0);

        /// <summary>
        /// 青色を意味する <see cref="Color8"/> 値です。
        /// </summary>
        public readonly static Color8 Blue = new(1);

        /// <summary>
        /// 緑色を意味する <see cref="Color8"/> 値です。
        /// </summary>
        public readonly static Color8 Green = new(2);

        /// <summary>
        /// シアンを意味する <see cref="Color8"/> 値です。
        /// </summary>
        public readonly static Color8 Cyan = new(3);

        /// <summary>
        /// 赤色を意味する <see cref="Color8"/> 値です。
        /// </summary>
        public readonly static Color8 Red = new(4);

        /// <summary>
        /// マジェンタを意味する <see cref="Color8"/> 値です。
        /// </summary>
        public readonly static Color8 Magenta = new(5);

        /// <summary>
        /// 黄色を意味する <see cref="Color8"/> 値です。
        /// </summary>
        public readonly static Color8 Yellow = new(6);

        /// <summary>
        /// 白色を意味する <see cref="Color8"/> 値です。
        /// </summary>
        public readonly static Color8 White = new(7);

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
            => ((_colorNumber & 0x04) != 0 ? (byte)0x80 : (byte)0x00,
                (_colorNumber & 0x02) != 0 ? (byte)0x80 : (byte)0x00,
                (_colorNumber & 0x01) != 0 ? (byte)0x80 : (byte)0x00);

        /// <summary>
        /// 値を別の <see cref="Color8"/> 値と等しいかどうか調べます。
        /// </summary>
        /// <param name="other">
        /// 別の <see cref="Color8"/> 値です。
        /// </param>
        /// <returns>
        /// 値が <paramref name="other"/> と等しいなら true、そうではないなら false です。
        /// </returns>
        public bool Equals(Color8 other)
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
                Equals((Color8)other);

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
            => _colorNumber switch
            {
                0 => "Black",
                1 => "Blue",
                2 => "Green",
                3 => "Cyan",
                4 => "Red",
                5 => "Magenta",
                6 => "Yellow",
                7 => "White",
                _ => throw Validation.GetFailErrorException($"Unexpected {nameof(_colorNumber)} value: {_colorNumber}"),
            };

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
        /// 作成された <see cref="Color8"/> 値です。
        /// </returns>
        public static Color8 FromRgb(byte r, byte g, byte b)
            => new(((r >> 7) << 2) | ((g >> 7) << 1) | (b >> 7));

        /// <summary>
        /// <see cref="Color8"/> 型のすべての値を列挙します。
        /// </summary>
        /// <returns>
        /// <see cref="Color8"/> 値の列挙子です。
        /// </returns>
        public static IEnumerable<Color8> GetValues()
        {
            for (var index = 0; index < 8; ++index)
                yield return new Color8(index);
        }

        /// <summary>
        /// 2つの <see cref="Color8"/> 値が等しいかどうか調べます。
        /// </summary>
        /// <param name="left">
        /// 左側の <see cref="Color8"/> 値です。
        /// </param>
        /// <param name="right">
        /// 右側の <see cref="Color8"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="left"/> と <paramref name="right"/> が等しいなら true、そうではないなら false です。
        /// </returns>
        public static bool operator ==(Color8 left, Color8 right) => left.Equals(right);

        /// <summary>
        /// 2つの <see cref="Color8"/> 値が等しくないかどうか調べます。
        /// </summary>
        /// <param name="left">
        /// 左側の <see cref="Color8"/> 値です。
        /// </param>
        /// <param name="right">
        /// 右側の <see cref="Color8"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="left"/> と <paramref name="right"/> が等しくないなら true、そうではないなら false です。
        /// </returns>
        public static bool operator !=(Color8 left, Color8 right) => !left.Equals(right);

        /// <summary>
        /// <see cref="Color8"/> 値を <see cref="int"/> 値に変換します。
        /// </summary>
        /// <param name="value">
        /// 変換する <see cref="Color8"/> 値です。
        /// </param>
        /// <returns>
        /// 変換された <see cref="int"/> 値です。
        /// </returns>
        public static explicit operator int(Color8 value) => value._colorNumber;

        /// <summary>
        /// <see cref="Color8"/> 値を <see cref="Color16"/> 値に変換します。
        /// </summary>
        /// <param name="value">
        /// 変換する <see cref="Color8"/> 値です。
        /// </param>
        /// <returns>
        /// 変換された <see cref="Color16"/> 値です。
        /// </returns>
        public static explicit operator Color16(Color8 value)
            => value._colorNumber switch
            {
                0 => Color16.Black,
                1 => Color16.BrightBlue,
                2 => Color16.BrightGreen,
                3 => Color16.BrightCyan,
                4 => Color16.BrightRed,
                5 => Color16.BrightMagenta,
                6 => Color16.BrightYellow,
                7 => Color16.BrightWhite,
                _ => throw Validation.GetFailErrorException($"Unexpected {nameof(value._colorNumber)} value: {value._colorNumber}"),
            };

        /// <summary>
        /// <see cref="Color8"/> 値を <see cref="Color88"/> 値に変換します。
        /// </summary>
        /// <param name="value">
        /// 変換する <see cref="Color8"/> 値です。
        /// </param>
        /// <returns>
        /// 変換された <see cref="Color88"/> 値です。
        /// </returns>
        public static explicit operator Color88(Color8 value)
            => value._colorNumber switch
            {
                0 => Color88.Black,
                1 => Color88.BrightBlue,
                2 => Color88.BrightGreen,
                3 => Color88.BrightCyan,
                4 => Color88.BrightRed,
                5 => Color88.BrightMagenta,
                6 => Color88.BrightYellow,
                7 => Color88.BrightWhite,
                _ => throw Validation.GetFailErrorException($"Unexpected {nameof(value._colorNumber)} value: {value._colorNumber}"),
            };

        /// <summary>
        /// <see cref="Color8"/> 値を <see cref="Color256"/> 値に変換します。
        /// </summary>
        /// <param name="value">
        /// 変換する <see cref="Color8"/> 値です。
        /// </param>
        /// <returns>
        /// 変換された <see cref="Color256"/> 値です。
        /// </returns>
        public static explicit operator Color256(Color8 value)
            => value._colorNumber switch
            {
                0 => Color256.Black,
                1 => Color256.BrightBlue,
                2 => Color256.BrightGreen,
                3 => Color256.BrightCyan,
                4 => Color256.BrightRed,
                5 => Color256.BrightMagenta,
                6 => Color256.BrightYellow,
                7 => Color256.BrightWhite,
                _ => throw Validation.GetFailErrorException($"Unexpected {nameof(value._colorNumber)} value: {value._colorNumber}"),
            };
    }
}
