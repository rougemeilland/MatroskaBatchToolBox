#define USE_SYSTEM_BIT_OPERATION
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
#if USE_SYSTEM_BIT_OPERATION
using System.Numerics;
#endif

namespace Palmtree
{
    /// <summary>
    /// 数値演算のクラスです。
    /// </summary>
    public static class Numerics
    {
        private static readonly Regex _rationalNumberPattern;

        static Numerics()
            => _rationalNumberPattern = new Regex(@"^(?<numerator>-?\d+)/(?<denominator>\d+)$", RegexOptions.Compiled);

        #region GreatestCommonDivisor

        /// <summary>
        /// 2つの整数の最大公約数を計算します。
        /// </summary>
        /// <param name="u">最初の整数です。</param>
        /// <param name="v">2 番目の整数です。</param>
        /// <returns><paramref name="u"/>と<paramref name="v"/>の最大公約数です。この値は常に正の整数です。 </returns>
        /// <exception cref="OverflowException">計算中にオーバーフローが発生しました。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GreatestCommonDivisor(int u, int v)
        {
            // checked / unchecked を行っている理由:
            //   checked コンテキスト内部で int.MinValue (0x80000000) の値を符号反転しようとするとオーバーフロー例外が発生するため、明示的に unchecked をしている。
            //   符号なし GCD が 0x80000000 であった場合、int 型に安全に変換することはできないので、checked を指定してオーバーフローチェックを行っている。
            //     ※ 符号なし u と 符号なし v は共に 0x80000000 以下であるので、GCDが 0x80000000 を超えることはない。
            //        GCDが 0x80000000 になるのは u と v が共に int.MinValue (0x80000000) である場合だけ。

            var unsignedGcd = GreatestCommonDivisor(u.AbsoluteWithoutCheck(), v.AbsoluteWithoutCheck());
            return unsignedGcd.ToInt32WithCheck();
        }

        /// <summary>
        /// 2つの整数の最大公約数を計算します。
        /// </summary>
        /// <param name="u">最初の整数です。</param>
        /// <param name="v">2 番目の整数です。</param>
        /// <returns><paramref name="u"/>と<paramref name="v"/>の最大公約数です。この値は常に正の整数です。 </returns>
        /// <exception cref="OverflowException">計算中にオーバーフローが発生しました。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GreatestCommonDivisor(uint u, uint v)
        {
            if (u == 0)
                return v != 0 ? v : throw new ArgumentException($"Both {nameof(u)} and {nameof(v)} are 0. The greatest common divisor of 0 and 0 is undefined.");
            else if (v == 0)
                return u;

            System.Diagnostics.Debug.Assert(u != 0 && v != 0);

            // この時点で u と v は共に正数

            // 「互減法」アルゴリズムを使用して最大公約数を求める。
            // 乗算/除算/剰余算を一切使わなくて済むので互除法よりも極めて高速に計算できる。
            // [出典] 「準数値算法 算術演算」 著)Donald Ervin Knuth , 出版)サイエンス社
            //
            // GCDの以下の性質を利用して、u および v をだんだん小さくしていくアルゴリズムである。
            // 1) w が u と v の公約数であるとき、GCD(u / w, v / w) * w == GCD(u, v)
            // 2) 素数 w が u の因数であるが v の因数ではないとき、 GCD(u / w, v) == GCD(u, v)
            // 3) u > v のとき、 GCD(u - v, v) == GCD(u, v)
            // 4) u > 0 && v > 0 && u == v のとき、 GCD(u, v) == u

            var k = 0;

            // u と v のどちらかが奇数になるまで続ける
#if USE_SYSTEM_BIT_OPERATION
            {
                var zeroBitCount = BitOperations.TrailingZeroCount(u).Minimum(BitOperations.TrailingZeroCount(v));
                u >>= zeroBitCount;
                v >>= zeroBitCount;
                k += zeroBitCount;
            }
#else
            while ((u & 1) == 0 && (v & 1) == 0)
            {
                u >>= 1;
                v >>= 1;
                ++k;
            }
#endif

            // この時点で、u と v は共に正で、かつ少なくとも u と v のどちらかが奇数で、かつ、 u << k が元の u に等しく、v << k が元の v に等しい

            // 最大公約数が求まるまで繰り返す。
            while (true)
            {
                System.Diagnostics.Debug.Assert(u != 0 && v != 0);

                // この時点で、u と v は共に正で、かつ少なくとも u と v のどちらかが奇数(2回目以降はともに奇数)である。

                if (u == v)
                {
                    // u == v の場合

                    // u を k ビットだけ左シフトした値を最終的な GCD として復帰
                    return u << k;
                }

                if (u < v)
                {
                    // u < v なら u と v を入れ替える。
                    (v, u) = (u, v);
                }

                // この時点で、u と v は共に正で、かつ u > v かつ 少なくとも u と v のどちらかが奇数(2回目以降はともに奇数)である。

                u -= v;

                // 2回目以降ではこの時点で u は偶数

                // u が奇数になるまで u を右シフトする
#if USE_SYSTEM_BIT_OPERATION
                u >>= BitOperations.TrailingZeroCount(u);
#else
                while ((u & 1) == 0)
                    u >>= 1;
#endif
            }
        }

        /// <summary>
        /// 2つの整数の最大公約数を計算します。
        /// </summary>
        /// <param name="u">最初の整数です。</param>
        /// <param name="v">2 番目の整数です。</param>
        /// <returns><paramref name="u"/>と<paramref name="v"/>の最大公約数です。この値は常に正の整数です。 </returns>
        /// <exception cref="OverflowException">計算中にオーバーフローが発生しました。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GreatestCommonDivisor(long u, long v)
        {
            // checked / unchecked を行っている理由:
            //   checked コンテキスト内部で long.MinValue (0x8000000000000000) の値を符号反転しようとするとオーバーフロー例外が発生するため、明示的に unchecked をしている。
            //   符号なし GCD が 0x8000000000000000 であった場合、long 型に安全に変換することはできないので、checked を指定してオーバーフローチェックを行っている。
            //     ※ 符号なし u と 符号なし v は共に 0x8000000000000000 以下であるので、GCDが 0x8000000000000000 を超えることはない。
            //        GCDが 0x8000000000000000 になるのは u と v が共に long.MinValue (0x8000000000000000) である場合だけ。
            var unsignedGcd = GreatestCommonDivisor(u.AbsoluteWithoutCheck(), v.AbsoluteWithoutCheck());
            return unsignedGcd.ToInt64WithCheck();
        }

        /// <summary>
        /// 2つの整数の最大公約数を計算します。
        /// </summary>
        /// <param name="u">最初の整数です。</param>
        /// <param name="v">2 番目の整数です。</param>
        /// <returns><paramref name="u"/>と<paramref name="v"/>の最大公約数です。この値は常に正の整数です。 </returns>
        /// <exception cref="OverflowException">計算中にオーバーフローが発生しました。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GreatestCommonDivisor(ulong u, ulong v)
        {
            if (u == 0)
                return v != 0 ? v : throw new ArgumentException($"Both {nameof(u)} and {nameof(v)} are 0. The greatest common divisor of 0 and 0 is undefined.");
            else if (v == 0)
                return u;

            System.Diagnostics.Debug.Assert(u != 0 && v != 0);

            // この時点で u と v は共に正数

            // 「互減法」アルゴリズムを使用して最大公約数を求める。
            // 乗算/除算/剰余算を一切使わなくて済むので互除法よりも極めて高速に計算できる。
            // [出典] 「準数値算法 算術演算」 著)Donald Ervin Knuth , 出版)サイエンス社
            //
            // GCDの以下の性質を利用して、u および v をだんだん小さくしていくアルゴリズムである。
            // 1) w が u と v の公約数であるとき、GCD(u / w, v / w) * w == GCD(u, v)
            // 2) 素数 w が u の因数であるが v の因数ではないとき、 GCD(u / w, v) == GCD(u, v)
            // 3) u > v のとき、 GCD(u - v, v) == GCD(u, v)
            // 4) u > 0 && v > 0 && u == v のとき、 GCD(u, v) == u

            var k = 0;

            // u と v のどちらかが奇数になるまで続ける
#if USE_SYSTEM_BIT_OPERATION
            {
                var zeroBitCount = BitOperations.TrailingZeroCount(u).Minimum(BitOperations.TrailingZeroCount(v));
                u >>= zeroBitCount;
                v >>= zeroBitCount;
                k += zeroBitCount;
            }
#else
            while ((u & 1) == 0 && (v & 1) == 0)
            {
                u >>= 1;
                v >>= 1;
                ++k;
            }
#endif

            // この時点で、u と v は共に正で、かつ少なくとも u と v のどちらかが奇数で、かつ、 u << k が元の u に等しく、v << k が元の v に等しい

            // 最大公約数が求まるまで繰り返す。
            while (true)
            {
                System.Diagnostics.Debug.Assert(u != 0 && v != 0);

                // この時点で、u と v は共に正で、かつ少なくとも u と v のどちらかが奇数(2回目以降はともに奇数)である。

                if (u == v)
                {
                    // u == v の場合

                    // u を k ビットだけ左シフトした値を最終的な GCD として復帰
                    return u << k;
                }

                if (u < v)
                {
                    // u < v なら u と v を入れ替える。
                    (v, u) = (u, v);
                }

                // この時点で、u と v は共に正で、かつ u > v かつ 少なくとも u と v のどちらかが奇数(2回目以降はともに奇数)である。

                u -= v;

                // 2回目以降ではこの時点で u は偶数

                // u が奇数になるまで u を右シフトする
#if USE_SYSTEM_BIT_OPERATION
                u >>= BitOperations.TrailingZeroCount(u);
#else
                while ((u & 1) == 0)
                    u >>= 1;
#endif
            }
        }

        #endregion

        #region Reduce

        /// <summary>
        /// 2つの整数を約分します。
        /// </summary>
        /// <param name="u">最初の整数です。</param>
        /// <param name="v">2 番目の整数です。</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><term>newU</term><description>約分された <paramref name="u"/>です。</description></item>
        /// <item><term>newV</term><description>約分された <paramref name="v"/>です。</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="OverflowException">計算中にオーバーフローが発生しました。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int newU, int newV) Reduce(int u, int v)
        {
            var gcd = GreatestCommonDivisor(u, v);
            return (u / gcd, v / gcd);
        }

        /// <summary>
        /// 2つの整数を約分します。
        /// </summary>
        /// <param name="u">最初の整数です。</param>
        /// <param name="v">2 番目の整数です。</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><term>newU</term><description>約分された <paramref name="u"/>です。</description></item>
        /// <item><term>newV</term><description>約分された <paramref name="v"/>です。</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="OverflowException">計算中にオーバーフローが発生しました。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (uint newU, uint newV) Reduce(uint u, uint v)
        {
            var gcd = GreatestCommonDivisor(u, v);
            return (u / gcd, v / gcd);
        }

        /// <summary>
        /// 2つの整数を約分します。
        /// </summary>
        /// <param name="u">最初の整数です。</param>
        /// <param name="v">2 番目の整数です。</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><term>newU</term><description>約分された <paramref name="u"/>です。</description></item>
        /// <item><term>newV</term><description>約分された <paramref name="v"/>です。</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="OverflowException">計算中にオーバーフローが発生しました。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (long newU, long newV) Reduce(long u, long v)
        {
            var gcd = GreatestCommonDivisor(u, v);
            return (u / gcd, v / gcd);
        }

        /// <summary>
        /// 2つの整数を約分します。
        /// </summary>
        /// <param name="u">最初の整数です。</param>
        /// <param name="v">2 番目の整数です。</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><term>newU</term><description>約分された <paramref name="u"/>です。</description></item>
        /// <item><term>newV</term><description>約分された <paramref name="v"/>です。</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="OverflowException">計算中にオーバーフローが発生しました。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (ulong newU, ulong newV) Reduce(ulong u, ulong v)
        {
            var gcd = GreatestCommonDivisor(u, v);
            return (u / gcd, v / gcd);
        }

        #endregion

        #region ParseAs

        /// <summary>
        /// 文字列で表現された数値を <see cref="CultureInfo.InvariantCulture"/> の書式情報に基づいて <see cref="int"/> 型に変換します。
        /// </summary>
        /// <param name="s">変換元の文字列です。</param>
        /// <param name="numberStyles">変換方法のオプションである <see cref="NumberStyles"/> 列挙体です。</param>
        /// <returns>変換された <see cref="int"/> 値です。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ParseAsInt32(this string s, NumberStyles numberStyles = NumberStyles.None)
            => int.Parse(s, numberStyles | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture.NumberFormat);

        /// <summary>
        /// 文字列で表現された数値を <see cref="CultureInfo.InvariantCulture"/> の書式情報に基づいて <see cref="uint"/> 型に変換します。
        /// </summary>
        /// <param name="s">変換元の文字列です。</param>
        /// <param name="numberStyles">変換方法のオプションである <see cref="NumberStyles"/> 列挙体です。</param>
        /// <returns>変換された <see cref="uint"/> 値です。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ParseAsUint32(this string s, NumberStyles numberStyles = NumberStyles.None)
            => uint.Parse(s, numberStyles, CultureInfo.InvariantCulture.NumberFormat);

        /// <summary>
        /// 文字列で表現された数値を <see cref="CultureInfo.InvariantCulture"/> の書式情報に基づいて <see cref="long"/> 型に変換します。
        /// </summary>
        /// <param name="s">変換元の文字列です。</param>
        /// <param name="numberStyles">変換方法のオプションである <see cref="NumberStyles"/> 列挙体です。</param>
        /// <returns>変換された <see cref="long"/> 値です。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ParseAsInt64(this string s, NumberStyles numberStyles = NumberStyles.None)
            => long.Parse(s, numberStyles | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture.NumberFormat);

        /// <summary>
        /// 文字列で表現された数値を <see cref="CultureInfo.InvariantCulture"/> の書式情報に基づいて <see cref="ulong"/> 型に変換します。
        /// </summary>
        /// <param name="s">変換元の文字列です。</param>
        /// <param name="numberStyles">変換方法のオプションである <see cref="NumberStyles"/> 列挙体です。</param>
        /// <returns>変換された <see cref="ulong"/> 値です。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ParseAsUint64(this string s, NumberStyles numberStyles = NumberStyles.None)
            => ulong.Parse(s, numberStyles, CultureInfo.InvariantCulture.NumberFormat);

        /// <summary>
        /// 固定小数点形式の文字列で表現された数値を <see cref="CultureInfo.InvariantCulture"/> の書式情報に基づいて <see cref="double"/> 型に変換します。
        /// </summary>
        /// <param name="s">変換元の文字列です。</param>
        /// <param name="numberStyles">変換方法のオプションである <see cref="NumberStyles"/> 列挙体です。</param>
        /// <returns>変換された <see cref="double"/> 値です。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ParseAsDouble(this string s, NumberStyles numberStyles = NumberStyles.None)
            => double.Parse(s, numberStyles | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat);

        /// <summary>
        /// 分数形式の文字列で表現された数値を <see cref="CultureInfo.InvariantCulture"/> の書式情報に基づいて <see cref="int"/> 型の分子と分母に変換します。
        /// </summary>
        /// <param name="s">変換元の文字列です。</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><term>numerator</term><description>変換された分子である<see cref="int"/>値です。</description></item>
        /// <item><term>denominator</term><description>変換された分母である<see cref="int"/>値です。</description></item>
        /// </list>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int numerator, int denominator) ParseAsInt32Fraction(this string s)
        {
            var match = _rationalNumberPattern.Match(s);
            return
                match.Success
                    ? (match.Groups["numerator"].Value.ParseAsInt32(), match.Groups["denominator"].Value.ParseAsInt32())
                    : throw new FormatException($"Expected a string in fraction format.: \"{s}\"");
        }

        /// <summary>
        /// 分数形式の文字列で表現された数値を <see cref="CultureInfo.InvariantCulture"/> の書式情報に基づいて <see cref="uint"/> 型の分子と分母に変換します。
        /// </summary>
        /// <param name="s">変換元の文字列です。</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><term>numerator</term><description>変換された分子である<see cref="uint"/>値です。</description></item>
        /// <item><term>denominator</term><description>変換された分母である<see cref="uint"/>値です。</description></item>
        /// </list>
        /// </returns>
        public static (uint numerator, uint denominator) ParseAsUInt32Fraction(this string s)
        {
            var match = _rationalNumberPattern.Match(s);
            return
                match.Success
                    ? (match.Groups["numerator"].Value.ParseAsUint32(), match.Groups["denominator"].Value.ParseAsUint32())
                    : throw new FormatException($"Expected a string in fraction format.: \"{s}\"");
        }

        /// <summary>
        /// 分数形式の文字列で表現された数値を <see cref="CultureInfo.InvariantCulture"/> の書式情報に基づいて <see cref="long"/> 型の分子と分母に変換します。
        /// </summary>
        /// <param name="s">変換元の文字列です。</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><term>numerator</term><description>変換された分子である<see cref="long"/>値です。</description></item>
        /// <item><term>denominator</term><description>変換された分母である<see cref="long"/>値です。</description></item>
        /// </list>
        /// </returns>
        public static (long numerator, long denominator) ParseAsInt64Fraction(this string s)
        {
            var match = _rationalNumberPattern.Match(s);
            return
                match.Success
                ? (match.Groups["numerator"].Value.ParseAsInt64(), match.Groups["denominator"].Value.ParseAsInt64())
                : throw new FormatException($"Expected a string in fraction format.: \"{s}\"");
        }

        /// <summary>
        /// 分数形式の文字列で表現された数値を <see cref="CultureInfo.InvariantCulture"/> の書式情報に基づいて <see cref="ulong"/> 型の分子と分母に変換します。
        /// </summary>
        /// <param name="s">変換元の文字列です。</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><term>numerator</term><description>変換された分子である<see cref="ulong"/>値です。</description></item>
        /// <item><term>denominator</term><description>変換された分母である<see cref="ulong"/>値です。</description></item>
        /// </list>
        /// </returns>
        public static (ulong numerator, ulong denominator) ParseAsUInt64Fraction(this string s)
        {
            var match = _rationalNumberPattern.Match(s);
            return
                match.Success
                    ? (match.Groups["numerator"].Value.ParseAsUint64(), match.Groups["denominator"].Value.ParseAsUint64())
                    : throw new FormatException($"Expected a string in fraction format.: \"{s}\"");
        }

        #endregion

        #region TryParse

        /// <summary>
        /// 文字列で表現された数値を <see cref="CultureInfo.InvariantCulture"/> の書式情報に基づいて <see cref="int"/> 値に変換します。
        /// </summary>
        /// <param name="s">変換元の文字列です。</param>
        /// <param name="value">変換結果の数値です。</param>
        /// <returns>true である場合は変換に成功したことを意味し、更に <paramref name="value"/> に変換結果が格納されます。
        /// false は変換に失敗したことを意味します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(this string s, out int value)
            => int.TryParse(s, NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture.NumberFormat, out value);

        /// <summary>
        /// 文字列で表現された数値を <see cref="CultureInfo.InvariantCulture"/> の書式情報に基づいて <see cref="uint"/> 値に変換します。
        /// </summary>
        /// <param name="s">変換元の文字列です。</param>
        /// <param name="value">変換結果の数値です。</param>
        /// <returns>true である場合は変換に成功したことを意味し、更に <paramref name="value"/> に変換結果が格納されます。
        /// false は変換に失敗したことを意味します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(this string s, out uint value)
            => uint.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out value);

        /// <summary>
        /// 文字列で表現された数値を <see cref="CultureInfo.InvariantCulture"/> の書式情報に基づいて <see cref="long"/> 値に変換します。
        /// </summary>
        /// <param name="s">変換元の文字列です。</param>
        /// <param name="value">変換結果の数値です。</param>
        /// <returns>true である場合は変換に成功したことを意味し、更に <paramref name="value"/> に変換結果が格納されます。
        /// false は変換に失敗したことを意味します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(this string s, out long value)
            => long.TryParse(s, NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture.NumberFormat, out value);

        /// <summary>
        /// 文字列で表現された数値を <see cref="CultureInfo.InvariantCulture"/> の書式情報に基づいて <see cref="ulong"/> 値に変換します。
        /// </summary>
        /// <param name="s">変換元の文字列です。</param>
        /// <param name="value">変換結果の数値です。</param>
        /// <returns>true である場合は変換に成功したことを意味し、更に <paramref name="value"/> に変換結果が格納されます。
        /// false は変換に失敗したことを意味します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(this string s, out ulong value)
            => ulong.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out value);

        /// <summary>
        /// 固定小数点形式の文字列で表現された数値を <see cref="CultureInfo.InvariantCulture"/> の書式情報に基づいて <see cref="double"/> 値に変換します。
        /// </summary>
        /// <param name="s">変換元の文字列です。</param>
        /// <param name="value">変換結果の数値です。</param>
        /// <returns>true である場合は変換に成功したことを意味し、更に <paramref name="value"/> に変換結果が格納されます。
        /// false は変換に失敗したことを意味します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(this string s, out double value)
            => double.TryParse(s, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out value);

        /// <summary>
        /// 分数形式の文字列で表現された数値を <see cref="CultureInfo.InvariantCulture"/> の書式情報に基づいて <see cref="int"/> 型の分子と分母に変換します。
        /// </summary>
        /// <param name="s">変換元の文字列です。</param>
        /// <param name="numerator">変換結果の分子です。</param>
        /// <param name="denominator">変換結果の分母です。</param>
        /// <returns>true である場合は変換に成功したことを意味し、更に <paramref name="numerator"/>および <paramref name="denominator"/> に変換結果が格納されます。
        /// false は変換に失敗したことを意味します。
        /// </returns>
        public static bool TryParse(this string s, out int numerator, out int denominator)
        {
            var match = _rationalNumberPattern.Match(s);
            if (!match.Success)
            {
                numerator = default;
                denominator = default;
                return false;
            }

            numerator = match.Groups["numerator"].Value.ParseAsInt32();
            denominator = match.Groups["denominator"].Value.ParseAsInt32();
            return true;
        }

        /// <summary>
        /// 分数形式の文字列で表現された数値を <see cref="CultureInfo.InvariantCulture"/> の書式情報に基づいて <see cref="uint"/> 型の分子と分母に変換します。
        /// </summary>
        /// <param name="s">変換元の文字列です。</param>
        /// <param name="numerator">変換結果の分子です。</param>
        /// <param name="denominator">変換結果の分母です。</param>
        /// <returns>true である場合は変換に成功したことを意味し、更に <paramref name="numerator"/>および <paramref name="denominator"/> に変換結果が格納されます。
        /// false は変換に失敗したことを意味します。
        /// </returns>
        public static bool TryParse(this string s, out uint numerator, out uint denominator)
        {
            var match = _rationalNumberPattern.Match(s);
            if (!match.Success)
            {
                numerator = default;
                denominator = default;
                return false;
            }

            numerator = uint.Parse(match.Groups["numerator"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
            denominator = uint.Parse(match.Groups["denominator"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
            return true;
        }

        /// <summary>
        /// 分数形式の文字列で表現された数値を <see cref="CultureInfo.InvariantCulture"/> の書式情報に基づいて <see cref="long"/> 型の分子と分母に変換します。
        /// </summary>
        /// <param name="s">変換元の文字列です。</param>
        /// <param name="numerator">変換結果の分子です。</param>
        /// <param name="denominator">変換結果の分母です。</param>
        /// <returns>true である場合は変換に成功したことを意味し、更に <paramref name="numerator"/>および <paramref name="denominator"/> に変換結果が格納されます。
        /// false は変換に失敗したことを意味します。
        /// </returns>
        public static bool TryParse(this string s, out long numerator, out long denominator)
        {
            var match = _rationalNumberPattern.Match(s);
            if (!match.Success)
            {
                numerator = default;
                denominator = default;
                return false;
            }

            numerator = match.Groups["numerator"].Value.ParseAsInt64();
            denominator = match.Groups["denominator"].Value.ParseAsInt64();
            return true;
        }

        /// <summary>
        /// 分数形式の文字列で表現された数値を <see cref="CultureInfo.InvariantCulture"/> の書式情報に基づいて <see cref="ulong"/> 型の分子と分母に変換します。
        /// </summary>
        /// <param name="s">変換元の文字列です。</param>
        /// <param name="numerator">変換結果の分子です。</param>
        /// <param name="denominator">変換結果の分母です。</param>
        /// <returns>true である場合は変換に成功したことを意味し、更に <paramref name="numerator"/>および <paramref name="denominator"/> に変換結果が格納されます。
        /// false は変換に失敗したことを意味します。
        /// </returns>
        public static bool TryParse(this string s, out ulong numerator, out ulong denominator)
        {
            var match = _rationalNumberPattern.Match(s);
            if (!match.Success)
            {
                numerator = default;
                denominator = default;
                return false;
            }

            numerator = ulong.Parse(match.Groups["numerator"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
            denominator = ulong.Parse(match.Groups["denominator"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
            return true;
        }

        #endregion

        #region Minimum

        /// <summary>
        /// 2 つの値のうち小さい方の値を取得します。
        /// </summary>
        /// <param name="value">1 つ目の値です。</param>
        /// <param name="otherValue">2 つ目の値です。</param>
        /// <returns>
        /// <paramref name="value"/> と <paramref name="otherValue"/> のうち、小さい方の値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Minimum(this int value, int otherValue)
            => value >= otherValue ? otherValue : value;

        /// <summary>
        /// 2 つの値のうち小さい方の値を取得します。
        /// </summary>
        /// <param name="value">1 つ目の値です。</param>
        /// <param name="otherValue">2 つ目の値です。</param>
        /// <returns>
        /// <paramref name="value"/> と <paramref name="otherValue"/> のうち、小さい方の値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Minimum(this uint value, uint otherValue)
            => value >= otherValue ? otherValue : value;

        /// <summary>
        /// 2 つの値のうち小さい方の値を取得します。
        /// </summary>
        /// <param name="value">1 つ目の値です。</param>
        /// <param name="otherValue">2 つ目の値です。</param>
        /// <returns>
        /// <paramref name="value"/> と <paramref name="otherValue"/> のうち、小さい方の値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Minimum(this long value, long otherValue)
            => value >= otherValue ? otherValue : value;

        /// <summary>
        /// 2 つの値のうち小さい方の値を取得します。
        /// </summary>
        /// <param name="value">1 つ目の値です。</param>
        /// <param name="otherValue">2 つ目の値です。</param>
        /// <returns>
        /// <paramref name="value"/> と <paramref name="otherValue"/> のうち、小さい方の値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Minimum(this ulong value, ulong otherValue)
            => value >= otherValue ? otherValue : value;

        /// <summary>
        /// 2 つの値のうち小さい方の値を取得します。
        /// </summary>
        /// <typeparam name="VALUE_T">値の型です。</typeparam>
        /// <param name="value">1 つ目の値です。</param>
        /// <param name="otherValue">2 つ目の値です。</param>
        /// <returns>
        /// <paramref name="value"/> と <paramref name="otherValue"/> のうち、小さい方の値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VALUE_T Minimum<VALUE_T>(this VALUE_T value, VALUE_T otherValue)
            where VALUE_T : IComparable
            => value.CompareTo(otherValue) >= 0 ? otherValue : value;

        #endregion

        #region Maximum

        /// <summary>
        /// 2 つの値のうち大きい方の値を取得します。
        /// </summary>
        /// <param name="value">1 つ目の値です。</param>
        /// <param name="otherValue">2 つ目の値です。</param>
        /// <returns>
        /// <paramref name="value"/> と <paramref name="otherValue"/> のうち、大きい方の値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Maximum(this int value, int otherValue)
            => value >= otherValue ? value : otherValue;

        /// <summary>
        /// 2 つの値のうち大きい方の値を取得します。
        /// </summary>
        /// <param name="value">1 つ目の値です。</param>
        /// <param name="otherValue">2 つ目の値です。</param>
        /// <returns>
        /// <paramref name="value"/> と <paramref name="otherValue"/> のうち、大きい方の値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Maximum(this uint value, uint otherValue)
            => value >= otherValue ? value : otherValue;

        /// <summary>
        /// 2 つの値のうち大きい方の値を取得します。
        /// </summary>
        /// <param name="value">1 つ目の値です。</param>
        /// <param name="otherValue">2 つ目の値です。</param>
        /// <returns>
        /// <paramref name="value"/> と <paramref name="otherValue"/> のうち、大きい方の値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Maximum(this long value, long otherValue)
            => value >= otherValue ? value : otherValue;

        /// <summary>
        /// 2 つの値のうち大きい方の値を取得します。
        /// </summary>
        /// <param name="value">1 つ目の値です。</param>
        /// <param name="otherValue">2 つ目の値です。</param>
        /// <returns>
        /// <paramref name="value"/> と <paramref name="otherValue"/> のうち、大きい方の値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Maximum(this ulong value, ulong otherValue)
            => value >= otherValue ? value : otherValue;

        /// <summary>
        /// 2 つの値のうち大きい方の値を取得します。
        /// </summary>
        /// <typeparam name="VALUE_T">値の型です。</typeparam>
        /// <param name="value">1 つ目の値です。</param>
        /// <param name="otherValue">2 つ目の値です。</param>
        /// <returns>
        /// <paramref name="value"/> と <paramref name="otherValue"/> のうち、大きい方の値です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VALUE_T Maximum<VALUE_T>(this VALUE_T value, VALUE_T otherValue)
            where VALUE_T : IComparable
            => value.CompareTo(otherValue) >= 0 ? value : otherValue;

        #endregion

        #region IsInClosedInterval

        /// <summary>
        /// ある値が指定された範囲内にあるかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// 調べる値の型です。
        /// </typeparam>
        /// <param name="value">
        /// 調べる値です。
        /// </param>
        /// <param name="minimumValue">
        /// 範囲の最大値です。
        /// </param>
        /// <param name="maximumValue">
        /// 範囲の最小値です。
        /// </param>
        /// <returns>
        /// <paramref name="minimumValue"/> &lt;= <paramref name="value"/> &lt;= <paramref name="maximumValue"/> ならば true、
        /// そうではないのなら false を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInClosedInterval<VALUE_T>(this VALUE_T value, VALUE_T minimumValue, VALUE_T maximumValue)
            where VALUE_T : IComparable
            => value is null
                ? minimumValue is null
                : value.CompareTo(minimumValue) >= 0 && value.CompareTo(maximumValue) <= 0;

        #endregion

        #region IsOutOfClosedInterval

        /// <summary>
        /// ある値が指定された範囲外にあるかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// 調べる値の型です。
        /// </typeparam>
        /// <param name="value">
        /// 調べる値です。
        /// </param>
        /// <param name="minimumValue">
        /// 範囲の最大値です。
        /// </param>
        /// <param name="maximumValue">
        /// 範囲の最小値です。
        /// </param>
        /// <returns>
        /// <paramref name="minimumValue"/> &lt;= <paramref name="value"/> &lt;= <paramref name="maximumValue"/> ならば false、
        /// そうではないのなら true を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOutOfClosedInterval<VALUE_T>(this VALUE_T value, VALUE_T minimumValue, VALUE_T maximumValue)
            where VALUE_T : IComparable
            => !value.IsInClosedInterval(minimumValue, maximumValue);

        #endregion

        #region IsInHalfClosedInterval

        /// <summary>
        /// ある値が指定された範囲内にあるかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// 調べる値の型です。
        /// </typeparam>
        /// <param name="value">
        /// 調べる値です。
        /// </param>
        /// <param name="minimumValue">
        /// 範囲の最大値です。
        /// </param>
        /// <param name="maximumValue">
        /// 範囲の最小値です。
        /// </param>
        /// <returns>
        /// <paramref name="minimumValue"/> &lt;= <paramref name="value"/> &lt; <paramref name="maximumValue"/> ならば true、
        /// そうではないのなら false を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInHalfClosedInterval<VALUE_T>(this VALUE_T value, VALUE_T minimumValue, VALUE_T maximumValue)
            where VALUE_T : IComparable
            => value is null
                ? minimumValue is null
                : value.CompareTo(minimumValue) >= 0 && value.CompareTo(maximumValue) < 0;

        #endregion

        #region IsOutOfHalfClosedInterval

        /// <summary>
        /// ある値が指定された範囲外にあるかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// 調べる値の型です。
        /// </typeparam>
        /// <param name="value">
        /// 調べる値です。
        /// </param>
        /// <param name="minimumValue">
        /// 範囲の最大値です。
        /// </param>
        /// <param name="maximumValue">
        /// 範囲の最小値です。
        /// </param>
        /// <returns>
        /// <paramref name="minimumValue"/> &lt;= <paramref name="value"/> &lt; <paramref name="maximumValue"/> ならば false、
        /// そうではないのなら true を返します。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOutOfHalfClosedInterval<VALUE_T>(this VALUE_T value, VALUE_T minimumValue, VALUE_T maximumValue)
            where VALUE_T : IComparable
            => !value.IsInHalfClosedInterval(minimumValue, maximumValue);

        #endregion

        #region private methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint AbsoluteWithoutCheck(this int x)
            => x < 0 ? unchecked((uint)-x) : (uint)x;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong AbsoluteWithoutCheck(this long x)
            => x < 0 ? unchecked((ulong)-x) : (ulong)x;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ToInt32WithCheck(this uint x)
            => checked((int)x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long ToInt64WithCheck(this ulong x)
            => checked((long)x);

        #endregion
    }
}
