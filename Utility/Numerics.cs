#define USE_SYSTEM_BIT_OPERATION
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
#if USE_SYSTEM_BIT_OPERATION
using System.Numerics;
#endif

namespace Utility
{
    public static class Numerics
    {
        private static readonly Regex _rationalNumberPattern;

        static Numerics()
            => _rationalNumberPattern = new Regex(@"^(?<numerator>\d+)/(?<denominator>\d+)$", RegexOptions.Compiled);

        #region GreatestCommonDivisor

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GreatestCommonDivisor(uint u, uint v)
        {
            if (u == 0)
                return v != 0 ? v : throw new ArgumentException($"Both {nameof(u)} and {nameof(v)} are 0. The greatest common divisor of 0 and 0 is undefined.");
            else if (v == 0)
                return u;

#if DEBUG
            if (u == 0 || v == 0)
            {
                // このルートへの到達はあり得ないはず。
                throw new Exception("An unexpected code was executed.");
            }
#endif

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
#if DEBUG
                if (u == 0 || v == 0)
                {
                    // このルートへの到達はあり得ないはず。
                    throw new Exception("An unexpected code was executed.");
                }
#endif

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GreatestCommonDivisor(ulong u, ulong v)
        {
            if (u == 0)
                return v != 0 ? v : throw new ArgumentException($"Both {nameof(u)} and {nameof(v)} are 0. The greatest common divisor of 0 and 0 is undefined.");
            else if (v == 0)
                return u;

#if DEBUG
            if (u == 0 || v == 0)
            {
                // このルートへの到達はあり得ないはず。
                throw new Exception("An unexpected code was executed.");
            }
#endif

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
#if DEBUG
                if (u == 0 || v == 0)
                {
                    // このルートへの到達はあり得ないはず。
                    throw new Exception("An unexpected code was executed.");
                }
#endif

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int newU, int newV) Reduce(int u, int v)
        {
            var gcd = GreatestCommonDivisor(u, v);
            return (u / gcd, v / gcd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (uint newU, uint newV) Reduce(uint u, uint v)
        {
            var gcd = GreatestCommonDivisor(u, v);
            return (u / gcd, v / gcd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (long newU, long newV) Reduce(long u, long v)
        {
            var gcd = GreatestCommonDivisor(u, v);
            return (u / gcd, v / gcd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (ulong newU, ulong newV) Reduce(ulong u, ulong v)
        {
            var gcd = GreatestCommonDivisor(u, v);
            return (u / gcd, v / gcd);
        }

        #endregion

        #region ParseAs

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ParseAsInt32(this string s, NumberStyles numberStyles = NumberStyles.None)
            => int.Parse(s, numberStyles, CultureInfo.InvariantCulture.NumberFormat);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ParseAsUint32(this string s, NumberStyles numberStyles = NumberStyles.None)
            => uint.Parse(s, numberStyles, CultureInfo.InvariantCulture.NumberFormat);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ParseAsInt64(this string s, NumberStyles numberStyles = NumberStyles.None)
            => long.Parse(s, numberStyles, CultureInfo.InvariantCulture.NumberFormat);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ParseAsUint64(this string s, NumberStyles numberStyles = NumberStyles.None)
            => ulong.Parse(s, numberStyles, CultureInfo.InvariantCulture.NumberFormat);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ParseAsDouble(this string s, NumberStyles numberStyles = NumberStyles.None)
            => double.Parse(s, numberStyles | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat);

        public static (int numerator, int denominator) ParseAsInt32Fraction(this string s)
        {
            var match = _rationalNumberPattern.Match(s);
            return
                match.Success
                    ? (match.Groups["numerator"].Value.ParseAsInt32(), match.Groups["denominator"].Value.ParseAsInt32())
                    : throw new FormatException($"Expected a string in fraction format.: \"{s}\"");
        }

        public static (uint numerator, uint denominator) ParseAsUInt32Fraction(this string s)
        {
            var match = _rationalNumberPattern.Match(s);
            return
                match.Success
                    ? (match.Groups["numerator"].Value.ParseAsUint32(), match.Groups["denominator"].Value.ParseAsUint32())
                    : throw new FormatException($"Expected a string in fraction format.: \"{s}\"");
        }

        public static (long numerator, long denominator) ParseAsInt64Fraction(this string s)
        {
            var match = _rationalNumberPattern.Match(s);
            return
                match.Success
                ? (match.Groups["numerator"].Value.ParseAsInt64(), match.Groups["denominator"].Value.ParseAsInt64())
                : throw new FormatException($"Expected a string in fraction format.: \"{s}\"");
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(this string s, out int value)
            => int.TryParse(s, NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture.NumberFormat, out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(this string s, out uint value)
            => uint.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out value);

        public static bool TryParse(this string s, out long value)
            => long.TryParse(s, NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture.NumberFormat, out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(this string s, out ulong value)
            => ulong.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(this string s, out double value)
            => double.TryParse(s, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out value);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Minimum(this int x, int y)
            => x >= y ? y : x;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Minimum(this uint x, uint y)
            => x >= y ? y : x;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Minimum(this long x, long y)
            => x >= y ? y : x;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Minimum(this ulong x, ulong y)
            => x >= y ? y : x;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VALUE_T Minimum<VALUE_T>(this VALUE_T x, VALUE_T y)
            where VALUE_T : IComparable<VALUE_T>
            => x.CompareTo(y) >= 0 ? y : x;

        #endregion

        #region Mamimunm

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Mamimunm(this int x, int y)
            => x >= y ? x : y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Mamimunm(this uint x, uint y)
            => x >= y ? x : y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Mamimunm(this long x, long y)
            => x >= y ? x : y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Mamimunm(this ulong x, ulong y)
            => x >= y ? x : y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VALUE_T Mamimunm<VALUE_T>(this VALUE_T x, VALUE_T y)
            where VALUE_T : IComparable<VALUE_T>
            => x.CompareTo(y) >= 0 ? x : y;

        #endregion

        #region private methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint AbsoluteWithoutCheck(this int x)
            => x < 0 ? checked((uint)x) : (uint)x;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong AbsoluteWithoutCheck(this long x)
            => x < 0 ? checked((ulong)x) : (ulong)x;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ToInt32WithCheck(this uint x)
            => checked((int)x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long ToInt64WithCheck(this ulong x)
            => checked((long)x);

        #endregion
    }
}
