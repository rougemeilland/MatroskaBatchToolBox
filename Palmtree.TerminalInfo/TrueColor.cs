using System;
using System.Collections.Generic;

namespace Palmtree.Terminal
{
    /// <summary>
    /// 16777216 種類 (24bit) の色を示す構造体です。
    /// </summary>
    public readonly struct TrueColor
        : IEquatable<TrueColor>
    {
        private readonly byte _redComponent;
        private readonly byte _greenComponent;
        private readonly byte _blueComponent;

        private TrueColor(byte r, byte g, byte b)
        {
            _redComponent = r;
            _greenComponent = g;
            _blueComponent = b;
        }

        /// <summary>
        /// 黒色を意味する <see cref="TrueColor"/> 値です。
        /// </summary>
        public readonly static TrueColor Black = new(0x00, 0x00, 0x00);

        /// <summary>
        /// 赤色を意味する <see cref="TrueColor"/> 値です。
        /// </summary>
        public readonly static TrueColor Red = new(0x80, 0x00, 0x00);

        /// <summary>
        /// 緑色を意味する <see cref="TrueColor"/> 値です。
        /// </summary>
        public readonly static TrueColor Green = new(0x00, 0x80, 0x00);

        /// <summary>
        /// 黄色を意味する <see cref="TrueColor"/> 値です。
        /// </summary>
        public readonly static TrueColor Yellow = new(0x80, 0x80, 0x00);

        /// <summary>
        /// 青色を意味する <see cref="TrueColor"/> 値です。
        /// </summary>
        public readonly static TrueColor Blue = new(0x00, 0x00, 0x80);

        /// <summary>
        /// マジェンタを意味する <see cref="TrueColor"/> 値です。
        /// </summary>
        public readonly static TrueColor Magenta = new(0x80, 0x00, 0x80);

        /// <summary>
        /// シアンを意味する <see cref="TrueColor"/> 値です。
        /// </summary>
        public readonly static TrueColor Cyan = new(0x00, 0x80, 0x80);

        /// <summary>
        /// 白色を意味する <see cref="TrueColor"/> 値です。
        /// </summary>
        public readonly static TrueColor White = new(0xc0, 0xc0, 0xc0);

        /// <summary>
        /// 明るい黒色を意味する <see cref="TrueColor"/> 値です。
        /// </summary>
        public readonly static TrueColor BrightBlack = new(0x80, 0x80, 0x80);

        /// <summary>
        /// 明るい赤色を意味する <see cref="TrueColor"/> 値です。
        /// </summary>
        public readonly static TrueColor BrightRed = new(0xff, 0x00, 0x00);

        /// <summary>
        /// 明るい緑色を意味する <see cref="TrueColor"/> 値です。
        /// </summary>
        public readonly static TrueColor BrightGreen = new(0x00, 0xff, 0x00);

        /// <summary>
        /// 明るい黄色を意味する <see cref="TrueColor"/> 値です。
        /// </summary>
        public readonly static TrueColor BrightYellow = new(0xff, 0xff, 0x00);

        /// <summary>
        /// 明るい青色を意味する <see cref="TrueColor"/> 値です。
        /// </summary>
        public readonly static TrueColor BrightBlue = new(0x00, 0x00, 0xff);

        /// <summary>
        /// 明るいマジェンタを意味する <see cref="TrueColor"/> 値です。
        /// </summary>
        public readonly static TrueColor BrightMagenta = new(0xff, 0x00, 0xff);

        /// <summary>
        /// 明るいシアンを意味する <see cref="TrueColor"/> 値です。
        /// </summary>
        public readonly static TrueColor BrightCyan = new(0x00, 0xff, 0xff);

        /// <summary>
        /// 明るい白色を意味する <see cref="TrueColor"/> 値です。
        /// </summary>
        public readonly static TrueColor BrightWhite = new(0xff, 0xff, 0xff);

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
            => (_redComponent, _greenComponent, _blueComponent);

        /// <summary>
        /// 値を別の <see cref="TrueColor"/> 値と等しいかどうか調べます。
        /// </summary>
        /// <param name="other">
        /// 別の <see cref="TrueColor"/> 値です。
        /// </param>
        /// <returns>
        /// 値が <paramref name="other"/> と等しいなら true、そうではないなら false です。
        /// </returns>
        public bool Equals(TrueColor other)
            => _redComponent == other._redComponent &&
                _greenComponent == other._greenComponent &&
                _blueComponent == other._blueComponent;

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
                Equals((TrueColor)other);

        /// <summary>
        /// 値のハッシュコードを取得します。
        /// </summary>
        /// <returns>
        /// ハッシュコードである <see cref="int"/>値です。
        /// </returns>
        public override int GetHashCode()
            => HashCode.Combine(_redComponent, _greenComponent, _blueComponent);

        /// <summary>
        /// 値の文字列表現を取得します。
        /// </summary>
        /// <returns>
        /// 値の文字列表現の <see cref="string"/> オブジェクトです。
        /// </returns>
        public override string ToString()
            => $"#{_redComponent:x2}{_greenComponent:x2}{_blueComponent:x2}";

        /// <summary>
        /// 与えられた RGB 値から <see cref="TrueColor"/> 構造体を作成します。
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
        /// 作成された <see cref="TrueColor"/> 値です。
        /// </returns>
        public static TrueColor FromRgb(byte r, byte g, byte b)
            => new(r, g, b);

        /// <summary>
        /// <see cref="TrueColor"/> 型のすべての値を列挙します。
        /// </summary>
        /// <returns>
        /// <see cref="TrueColor"/> 値の列挙子です。
        /// </returns>
        public static IEnumerable<TrueColor> GetValues()
        {
            for (var r = 0; r < 256; ++r)
            {
                for (var g = 0; g < 256; ++g)
                {
                    for (var b = 0; b < 256; ++b)
                    {
                        yield return new TrueColor((byte)r, (byte)g, (byte)b);
                    }
                }
            }
        }

        /// <summary>
        /// 2つの <see cref="TrueColor"/> 値が等しいかどうか調べます。
        /// </summary>
        /// <param name="left">
        /// 左側の <see cref="TrueColor"/> 値です。
        /// </param>
        /// <param name="right">
        /// 右側の <see cref="TrueColor"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="left"/> と <paramref name="right"/> が等しいなら true、そうではないなら false です。
        /// </returns>
        public static bool operator ==(TrueColor left, TrueColor right) => left.Equals(right);

        /// <summary>
        /// 2つの <see cref="TrueColor"/> 値が等しくないかどうか調べます。
        /// </summary>
        /// <param name="left">
        /// 左側の <see cref="TrueColor"/> 値です。
        /// </param>
        /// <param name="right">
        /// 右側の <see cref="TrueColor"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="left"/> と <paramref name="right"/> が等しくないなら true、そうではないなら false です。
        /// </returns>
        public static bool operator !=(TrueColor left, TrueColor right) => !left.Equals(right);

        /// <summary>
        /// <see cref="TrueColor"/> 値を <see cref="int"/> 値に変換します。
        /// </summary>
        /// <param name="value">
        /// 変換する <see cref="TrueColor"/> 値です。
        /// </param>
        public static explicit operator int(TrueColor value)
            => (value._redComponent << 16) | (value._greenComponent << 8) | (value._blueComponent << 0);
    }
}
