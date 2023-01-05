using System;

namespace MatroskaBatchToolBox
{
    internal static class ExtendedMath
    {
        public static int GreatestCommonDivisor(int u, int v)
        {
            if (u < 0)
                throw new ArgumentException($"'{nameof(u)}' must be positive or 0.: {nameof(u)}={u}");
            if (v < 0)
                throw new ArgumentException($"'{nameof(v)}' must be positive or 0.: {nameof(v)}={v}");
            if (u <= 0 && v <= 0)
                throw new ArgumentException($"Either '{nameof(u)}' or '{nameof(v)}' must be positive.: {nameof(u)}={u}, {nameof(v)}={v}");

            // 「互減法」アルゴリズムを使用して最大公約数を求める。
            // 乗算/除算/剰余算を一切使わなくて済むので高速に計算できる。
            // ※ 「奇数になるまで右シフトをする」という演算をする箇所が何か所かあるので、System.Numerics.TrailingZeroCount() (x86 機械語の TZCNT に相当) を使った方が速いかどうかはともかく簡潔かもしれない。
            //   ただし、実際に System.Numerics.TrailingZeroCount() が高速になるのは Unsafe コードを使わないといけないかもしれない。

            var k = 0;

            // u と v のどちらかが奇数になるまで続ける
            while ((u & 1) == 0 && (v & 1) == 0)
            {
                u >>= 1;
                v >>= 1;
                ++k;
            }

            // この時点で、u と v のどちらかが奇数で、かつ、 u << k が元の u に等しく、v << k が元の v に等しい。

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
                if (u == v)
                {
                    // u == v の場合

                    // GCD を u として復帰
                    return u << k;
                }
                if (u < v)
                {
                    // u < v なら u と v を入れ替える。
                    (v, u) = (u, v);
                }

                // この時点で u > v かつ u と v はともに奇数である

                u -= v;

                // この時点で u は偶数

                // u が奇数になるまで u を右シフトする
                while ((u & 1) == 0)
                    u >>= 1;
            }
        }

        public static long GreatestCommonDivisor(long u, long v)
        {
            if (u < 0)
                throw new ArgumentException($"'{nameof(u)}' must be positive or 0.: {nameof(u)}={u}");
            if (v < 0)
                throw new ArgumentException($"'{nameof(v)}' must be positive or 0.: {nameof(v)}={v}");
            if (u <= 0 && v <= 0)
                throw new ArgumentException($"Either '{nameof(u)}' or '{nameof(v)}' must be positive.: {nameof(u)}={u}, {nameof(v)}={v}");

            var k = 0;
            while ((u & 1) == 0 && (v & 1) == 0)
            {
                u >>= 1;
                v >>= 1;
                ++k;
            }
            while (true)
            {
                if (u == 0 || v == 0)
                {
                    // このルートへの到達はあり得ないはず。
                    throw new Exception("An unexpected code was executed.");
                }
                if (u == v)
                {
                    // u == v の場合

                    // GCD を u として復帰
                    return u << k;
                }
                if (u < v)
                {
                    (v, u) = (u, v);
                }
                // この時点で u > v かつ u と v はともに奇数である

                u -= v;

                // この時点で u は偶数

                // u が奇数になるまで u を右シフトする
                while ((u & 1) == 0)
                    u >>= 1;
            }
        }
    }
}
