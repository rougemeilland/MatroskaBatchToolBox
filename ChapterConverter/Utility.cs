using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ChapterConverter
{
    internal static class Utility
    {
        private static Regex _strictTimePattern;
        private static Regex _lazyTimePattern;

        static Utility()
        {
            _strictTimePattern = new Regex(@"(?<hour>\d+):(?<minute>\d+):(?<second>\d+(\.\d+)?)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            _lazyTimePattern = new Regex(@"(((?<hour>\d+):)?(?<minute>\d+):)?(?<second>\d+(\.\d+)?)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
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

            var secondFormat = precision <= 0 ? "D2" :  $"00.{new string('0', precision)}";
            var totalSeconds = (decimal)time.TotalSeconds;
            var totalMinutes = (int)Math.Floor(totalSeconds / 60);
            var second = totalSeconds - totalMinutes * 60;
            var minute = totalMinutes % 60;
            var hour = totalMinutes / 60;
            return $"{hour:D2}:{minute:D2}:{second.ToString(secondFormat)}";
        }
    }
}
