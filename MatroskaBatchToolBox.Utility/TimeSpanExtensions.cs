#define NEED_HIGH_PRECISION_FOR_TIME
using System;
using System.Text.RegularExpressions;
using Palmtree;

namespace MatroskaBatchToolBox.Utility
{
    public static class TimeSpanExtensions
    {
        private static readonly Regex _strictTimePattern;
        private static readonly Regex _lazyTimePattern;

        static TimeSpanExtensions()
        {
            _strictTimePattern = new Regex(@"^(?<hour>\d+):(?<minute>\d+):(?<second>\d+(\.\d+)?)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            _lazyTimePattern = new Regex(@"^(((?<hour>\d+):)?(?<minute>\d+):)?(?<second>\d+(\.\d+)?)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        public static TimeSpan ParseAsTimeSpan(this string s, bool strict)
        {
            if (strict)
            {
                var match = _strictTimePattern.Match(s);
                if (!match.Success)
                    throw new FormatException($"Time string is expected.: \"{s}\"");

                var hour = match.Groups["hour"].Value.ParseAsInt32();

                var minute = match.Groups["minute"].Value.ParseAsInt32();
                if (minute >= 60)
                    throw new FormatException($"Time string in time format is expected. (Minute value out of range): \"{s}\"");

                var second = match.Groups["second"].Value.ParseAsDouble();
                return second < 60
                    ? TimeSpan.FromSeconds((hour * 60.0 + minute) * 60.0 + second)
                    : throw new FormatException($"Time string in time format is expected. (Second value out of range): \"{s}\"");
            }
            else
            {
                var match = _lazyTimePattern.Match(s);
                if (!match.Success)
                    throw new FormatException($"Time string is expected.: \"{s}\"");

                var totalSeconds = match.Groups["second"].Value.ParseAsDouble();

                if (match.Groups["minute"].Success)
                {
                    var minute = match.Groups["minute"].Value.ParseAsInt32();
                    totalSeconds += minute * 60;
                }

                if (match.Groups["hour"].Success)
                {
                    var hour = match.Groups["hour"].Value.ParseAsInt32();
                    totalSeconds += hour * (60 * 60);
                }

                return TimeSpan.FromSeconds(totalSeconds);
            }
        }

        public static bool TryParse(this string s, bool strict, out TimeSpan value)
        {
            if (strict)
            {
                var match = _strictTimePattern.Match(s);
                if (!match.Success)
                {
                    value = TimeSpan.Zero;
                    return false;
                }

                var hour = match.Groups["hour"].Value.ParseAsInt32();

                var minute = match.Groups["minute"].Value.ParseAsInt32();
                if (minute >= 60)
                {
                    value = TimeSpan.Zero;
                    return false;
                }

                var second = match.Groups["second"].Value.ParseAsDouble();
                if (second >= 60)
                {
                    value = TimeSpan.Zero;
                    return false;
                }

                value = TimeSpan.FromSeconds((hour * 60.0 + minute) * 60.0 + second);
                return true;
            }
            else
            {
                var match = _lazyTimePattern.Match(s);
                if (!match.Success)
                {
                    value = TimeSpan.Zero;
                    return false;
                }

                var totalSeconds = match.Groups["second"].Value.ParseAsDouble();

                if (match.Groups["minute"].Success)
                {
                    var minute = match.Groups["minute"].Value.ParseAsInt32();
                    totalSeconds += minute * 60;
                }

                if (match.Groups["hour"].Success)
                {
                    var hour = match.Groups["hour"].Value.ParseAsInt32();
                    totalSeconds += hour * (60 * 60);
                }

                value = TimeSpan.FromSeconds(totalSeconds);
                return true;
            }
        }

        public static string FormatTime(this TimeSpan time, int precision)
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

        public static TimeSpan FromTimeCountToTimeSpan(this long timeValue, long timeBaseNumerator, long timeBaseDenominator)
        {
            if (timeValue < 0)
                throw new ArgumentOutOfRangeException(nameof(timeValue));

#if NEED_HIGH_PRECISION_FOR_TIME
            var ticksPerSeconds = TimeSpan.TicksPerSecond;
            (ticksPerSeconds, timeBaseDenominator) = Numerics.Reduce(ticksPerSeconds, timeBaseDenominator);
            (timeValue, timeBaseDenominator) = Numerics.Reduce(timeValue, timeBaseDenominator);
            (timeBaseNumerator, timeBaseDenominator) = Numerics.Reduce(timeBaseNumerator, timeBaseDenominator);
            return TimeSpan.FromTicks(checked(ticksPerSeconds * timeValue * timeBaseNumerator / timeBaseDenominator));
#else
            return TimeSpan.FromSeconds((double)timeValue * timeBaseNumerator / timeBaseDenominator);
#endif
        }

        public static long FromTimeSpanToTimeCount(this TimeSpan time, long timeBaseNumerator, long timeBaseDenominator)
        {
            if (time < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(time));

#if NEED_HIGH_PRECISION_FOR_TIME
            var ticks = time.Ticks;
            var ticksPerSeconds = TimeSpan.TicksPerSecond;
            (ticks, timeBaseNumerator) = Numerics.Reduce(ticks, timeBaseNumerator);
            (ticks, ticksPerSeconds) = Numerics.Reduce(ticks, ticksPerSeconds);
            (timeBaseDenominator, timeBaseNumerator) = Numerics.Reduce(timeBaseDenominator, timeBaseNumerator);
            (timeBaseDenominator, ticksPerSeconds) = Numerics.Reduce(timeBaseDenominator, ticksPerSeconds);
            return checked(ticks * timeBaseDenominator / timeBaseNumerator / ticksPerSeconds);
#else
            return Convert.ToInt64(time.TotalSeconds * timeBaseDenominator / timeBaseNumerator);
#endif
        }
    }
}
