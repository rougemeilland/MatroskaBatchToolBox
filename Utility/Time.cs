#define NEED_HIGH_PRECISION_FOR_TIME
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Utility
{
    public static class Time
    {
        private static readonly Regex _strictTimePattern;
        private static readonly Regex _lazyTimePattern;

        static Time()
        {
            _strictTimePattern = new Regex(@"^(?<hour>\d+):(?<minute>\d+):(?<second>\d+(\.\d+)?)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            _lazyTimePattern = new Regex(@"^(((?<hour>\d+):)?(?<minute>\d+):)?(?<second>\d+(\.\d+)?)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        public static TimeSpan? ParseTime(string s, bool strict)
        {
            if (strict)
            {
                var match = _strictTimePattern.Match(s);
                if (!match.Success)
                    return null;

                var hour = int.Parse(match.Groups["hour"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);

                var minute = int.Parse(match.Groups["minute"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
                if (minute >= 60)
                    return null;

                var second = double.Parse(match.Groups["second"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat);
                if (second >= 60)
                    return null;

                return TimeSpan.FromSeconds((hour * 60.0 + minute) * 60.0 + second);
            }
            else
            {
                var match = _lazyTimePattern.Match(s);
                if (!match.Success)
                    return null;

                var totalSeconds = double.Parse(match.Groups["second"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat);

                if (match.Groups["minute"].Success)
                {
                    var minute = int.Parse(match.Groups["minute"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
                    totalSeconds += minute * 60;
                }

                if (match.Groups["hour"].Success)
                {
                    var hour = int.Parse(match.Groups["hour"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
                    totalSeconds += hour * (60 * 60);
                }

                return TimeSpan.FromSeconds(totalSeconds);
            }
        }

        public static string FormatTime(TimeSpan time, int precision)
        {
            if (time < TimeSpan.Zero)
                throw new ArgumentException("internal error (time < 0)");
            if (precision < 0)
                throw new ArgumentException("internal error (precision < 0)");
            if (precision > 9)
                throw new ArgumentException($"internal error (precision > 9)");

            var secondFormat = precision <= 0 ? "D2" : $"00.{new string('0', precision)}";
            var totalSeconds = (decimal)time.TotalSeconds;
            var totalMinutes = (int)Math.Floor(totalSeconds / 60);
            var second = totalSeconds - totalMinutes * 60;
            var minute = totalMinutes % 60;
            var hour = totalMinutes / 60;
            return $"{hour:D2}:{minute:D2}:{second.ToString(secondFormat)}";
        }

        public static TimeSpan FromTimeCountToTimeSpan(long timeValue, long baseTimeNumerator, long baseTimeDenominator)
        {
            if (timeValue < 0)
                throw new Exception("internal error (timeValue < 0)");

#if NEED_HIGH_PRECISION_FOR_TIME
            // longの有効桁数は18桁+αで、チャプターの時間の計算において baseTimeDenominator で想定される桁数は最低でも 9(+1) 桁。
            // 1 秒を表す timeValue が 9(+1) 桁であることを考えると、10 秒以上の計算を行うと、途中でオーバーフローを起こすことはほぼ確実であり、運用に支障をきたす。。
            // double 型を使用すればオーバーフローは回避できるが、精度が犠牲になる。(double型の仮数部の精度は 自明な先頭ビットを含めても53ビットしかなく、long 型に劣る)
            // そのため、精度の確保のために途中計算を BigInteger 型で行う。
            // まぁ、想定される運用がチャプターの時間の計算であり、チャプターのタイミングにナノセコンド単位の精度が求められることはないはずであるが…
            var ticks = (System.Numerics.BigInteger)TimeSpan.TicksPerSecond * timeValue * baseTimeNumerator / baseTimeDenominator;
            if (ticks > long.MaxValue)
                throw new OverflowException($"\"{(double)timeValue * baseTimeNumerator / baseTimeDenominator}\" seconds cannot be represented in long type.: timeValue={timeValue}, baseTimeNumerator={baseTimeNumerator}, baseTimeDenominator={baseTimeDenominator}");
            return TimeSpan.FromTicks((long)ticks);
#else
            return TimeSpan.FromSeconds((double)timeValue * baseTimeNumerator / baseTimeDenominator);
#endif
        }

        public static long FromTimeSpanToTimeCount(TimeSpan time, long baseTimeNumerator, long baseTimeDenominator)
        {
            if (time < TimeSpan.Zero)
                throw new Exception("internal error (time < TimeSpan.Zero)");

#if NEED_HIGH_PRECISION_FOR_TIME
            var timeCount = (System.Numerics.BigInteger)time.Ticks * baseTimeDenominator / baseTimeNumerator / TimeSpan.TicksPerSecond;
            if (timeCount > long.MaxValue)
                throw new OverflowException($"\"{time.TotalSeconds}\" seconds cannot be represented on the timescale \"{baseTimeNumerator}/{baseTimeDenominator}\".");
            return (long)timeCount;
#else
            return Convert.ToInt64(time.TotalSeconds * baseTimeDenominator / baseTimeNumerator);
#endif
        }
    }
}
