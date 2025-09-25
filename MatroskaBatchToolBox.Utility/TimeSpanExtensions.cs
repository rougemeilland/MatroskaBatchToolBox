#define NEED_HIGH_PRECISION_FOR_TIME
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Palmtree;
using Palmtree.Numerics;

namespace MatroskaBatchToolBox.Utility
{
    public static partial class TimeSpanExtensions
    {
        public static TimeSpan ParseAsTimeSpan(this string s, TimeParsingMode parsingMode)
        {
            switch (parsingMode)
            {
                case TimeParsingMode.StrictForLongTimeFormat:
                {
                    var match = GetStrictLongTimePattern().Match(s);
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
                case TimeParsingMode.StrictForShortTimeFormat:
                {
                    var match = GetStrictShortTimePattern().Match(s);
                    if (!match.Success)
                        throw new FormatException($"Time string is expected.: \"{s}\"");

                    var minute = match.Groups["minute"].Value.ParseAsInt32();

                    var second = match.Groups["second"].Value.ParseAsDouble();
                    return second < 60
                        ? TimeSpan.FromSeconds(minute * 60.0 + second)
                        : throw new FormatException($"Time string in time format is expected. (Second value out of range): \"{s}\"");
                }
                case TimeParsingMode.StrictForVeryShortTimeFormat:
                {
                    var match = GetStrictVeryShortTimePattern().Match(s);
                    if (!match.Success)
                        throw new FormatException($"Time string is expected.: \"{s}\"");

                    var second = match.Groups["second"].Value.ParseAsDouble();
                    return TimeSpan.FromSeconds(second);
                }
                case TimeParsingMode.LazyMode:
                {
                    var match = GetLazyTimePattern().Match(s);
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
                case TimeParsingMode.Expression:
                {
                    var result = TimeSpan.Zero;
                    var offset = 0;
                    while (offset < s.Length)
                    {
                        var op = '\0';
                        if (offset > 0)
                        {
                            op = s[offset];
                            if (op is not '+' and not '-')
                                throw new FormatException($"Time string is expected.: \"{s}\"");

                            ++offset;
                            if (offset >= s.Length)
                                throw new FormatException($"Time string is expected.: \"{s}\"");
                        }

                        var match = GetPartialLazyTimePattern().Match(s[offset..]);
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

                        switch (op)
                        {
                            case '\0':
                                Validation.Assert(offset <= 0);
                                result = TimeSpan.FromSeconds(totalSeconds);
                                break;
                            case '+':
                                Validation.Assert(offset > 0);
                                result += TimeSpan.FromSeconds(totalSeconds);
                                break;
                            case '-':
                                Validation.Assert(offset > 0);
                                result -= TimeSpan.FromSeconds(totalSeconds);
                                break;
                            default:
                                break;
                        }

                        offset += match.Length;
                    }

                    return result;
                }
                default:
                    throw new ArgumentException($"Invalid {nameof(parsingMode)} value.: \"{parsingMode}\"", nameof(parsingMode));
            }
        }

        public static bool TryParse(this string s, TimeParsingMode parsingMode, out TimeSpan value)
        {
            switch (parsingMode)
            {
                case TimeParsingMode.StrictForLongTimeFormat:
                {
                    var match = GetStrictLongTimePattern().Match(s);
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
                case TimeParsingMode.StrictForShortTimeFormat:
                {
                    var match = GetStrictShortTimePattern().Match(s);
                    if (!match.Success)
                    {
                        value = TimeSpan.Zero;
                        return false;
                    }

                    var minute = match.Groups["minute"].Value.ParseAsInt32();

                    var second = match.Groups["second"].Value.ParseAsDouble();
                    if (second >= 60)
                    {
                        value = TimeSpan.Zero;
                        return false;
                    }

                    value = TimeSpan.FromSeconds(minute * 60.0 + second);
                    return true;
                }
                case TimeParsingMode.StrictForVeryShortTimeFormat:
                {
                    var match = GetStrictVeryShortTimePattern().Match(s);
                    if (!match.Success)
                    {
                        value = TimeSpan.Zero;
                        return false;
                    }

                    var second = match.Groups["second"].Value.ParseAsDouble();
                    value = TimeSpan.FromSeconds(second);
                    return true;
                }
                case TimeParsingMode.LazyMode:
                {
                    var match = GetLazyTimePattern().Match(s);
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
                case TimeParsingMode.Expression:
                {
                    var result = TimeSpan.Zero;
                    var offset = 0;
                    while (offset < s.Length)
                    {
                        var op = '\0';
                        if (offset > 0)
                        {
                            op = s[offset];
                            if (op is not '+' and not '-')
                            {
                                value = TimeSpan.Zero;
                                return false;
                            }

                            ++offset;
                            if (offset >= s.Length)
                            {
                                value = TimeSpan.Zero;
                                return false;
                            }
                        }

                        var match = GetPartialLazyTimePattern().Match(s[offset..]);
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

                        switch (op)
                        {
                            case '\0':
                                Validation.Assert(offset <= 0);
                                result = TimeSpan.FromSeconds(totalSeconds);
                                break;
                            case '+':
                                Validation.Assert(offset > 0);
                                result += TimeSpan.FromSeconds(totalSeconds);
                                break;
                            case '-':
                                Validation.Assert(offset > 0);
                                result -= TimeSpan.FromSeconds(totalSeconds);
                                break;
                            default:
                                break;
                        }

                        offset += match.Length;
                    }

                    value = result;
                    return true;
                }
                default:
                    throw new ArgumentException($"Invalid {nameof(parsingMode)} value.: \"{parsingMode}\"", nameof(parsingMode));
            }
        }

        public static string FormatTime(this TimeSpan time, TimeFormatType format, int precision)
        {
            if (time < TimeSpan.Zero)
                throw new ArgumentException("internal error (time < 0)");
            if (precision < 0)
                throw new ArgumentException("internal error (precision < 0)");
            if (precision > 9)
                throw new ArgumentException($"internal error (precision > 9)");

            var secondFormat = precision <= 0 ? "D2" : $"00.{new string('0', precision)}";
            var totalSeconds = (decimal)time.TotalSeconds;
            switch (format)
            {
                case TimeFormatType.LongFormat:
                {
                    var totalMinutes = (int)Math.Floor(totalSeconds / 60);
                    var second = totalSeconds - totalMinutes * 60;
                    var minute = totalMinutes % 60;
                    var totalHours = totalMinutes / 60;
                    return $"{totalHours:D2}:{minute:D2}:{second.ToString(secondFormat, CultureInfo.InvariantCulture)}";
                }
                case TimeFormatType.ShortFormat:
                {
                    var totalMinutes = (int)Math.Floor(totalSeconds / 60);
                    var second = totalSeconds - totalMinutes * 60;
                    return $"{totalMinutes:D2}:{second.ToString(secondFormat, CultureInfo.InvariantCulture)}";
                }
                case TimeFormatType.OnlySeconds:
                    return $"{totalSeconds.ToString(secondFormat, CultureInfo.InvariantCulture)}";
                case TimeFormatType.LazyFormat:
                {
                    var totalMinutes = (int)Math.Floor(totalSeconds / 60);
                    var second = totalSeconds - totalMinutes * 60;
                    var minute = totalMinutes % 60;
                    var totalHours = totalMinutes / 60;
                    if (totalHours > 0)
                        return $"{totalHours:D2}:{minute:D2}:{second.ToString(secondFormat, CultureInfo.InvariantCulture)}";
                    else if (totalMinutes > 0)
                        return $"{totalMinutes:D2}:{second.ToString(secondFormat, CultureInfo.InvariantCulture)}";
                    else
                        return $"{totalSeconds.ToString(secondFormat, CultureInfo.InvariantCulture)}";
                }
                default:
                    throw new ArgumentException($"Invalid {nameof(format)} value.: \"{format}\"", nameof(format));
            }
        }

        public static TimeSpan FromTimeCountToTimeSpan(this long timeValue, long timeBaseNumerator, long timeBaseDenominator)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(timeValue);

#if NEED_HIGH_PRECISION_FOR_TIME
            var ticksPerSeconds = TimeSpan.TicksPerSecond;
            (ticksPerSeconds, timeBaseDenominator) = ticksPerSeconds.Reduce(timeBaseDenominator);
            (timeValue, timeBaseDenominator) = timeValue.Reduce(timeBaseDenominator);
            (timeBaseNumerator, timeBaseDenominator) = timeBaseNumerator.Reduce(timeBaseDenominator);
            return TimeSpan.FromTicks(checked(ticksPerSeconds * timeValue * timeBaseNumerator / timeBaseDenominator));
#else
            return TimeSpan.FromSeconds((double)timeValue * timeBaseNumerator / timeBaseDenominator);
#endif
        }

        public static long FromTimeSpanToTimeCount(this TimeSpan time, long timeBaseNumerator, long timeBaseDenominator)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(time, TimeSpan.Zero);

#if NEED_HIGH_PRECISION_FOR_TIME
            var ticks = time.Ticks;
            var ticksPerSeconds = TimeSpan.TicksPerSecond;
            (ticks, timeBaseNumerator) = ticks.Reduce(timeBaseNumerator);
            (ticks, ticksPerSeconds) = ticks.Reduce(ticksPerSeconds);
            (timeBaseDenominator, timeBaseNumerator) = timeBaseDenominator.Reduce(timeBaseNumerator);
            (timeBaseDenominator, ticksPerSeconds) = timeBaseDenominator.Reduce(ticksPerSeconds);
            return checked(ticks * timeBaseDenominator / timeBaseNumerator / ticksPerSeconds);
#else
            return Convert.ToInt64(time.TotalSeconds * timeBaseDenominator / timeBaseNumerator);
#endif
        }

        [GeneratedRegex(@"^(?<hour>\d+):(?<minute>\d+):(?<second>\d+(\.\d+)?)$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex GetStrictLongTimePattern();

        [GeneratedRegex(@"^(?<minute>\d+):(?<second>\d+(\.\d+)?)$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex GetStrictShortTimePattern();

        [GeneratedRegex(@"^(?<second>\d+(\.\d+)?)$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex GetStrictVeryShortTimePattern();

        [GeneratedRegex(@"^(((?<hour>\d+):)?(?<minute>\d+):)?(?<second>\d+(\.\d+)?)$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex GetLazyTimePattern();

        [GeneratedRegex(@"^(((?<hour>\d+):)?(?<minute>\d+):)?(?<second>\d+(\.\d+)?)", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex GetPartialLazyTimePattern();
    }
}
