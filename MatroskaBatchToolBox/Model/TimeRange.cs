using System;
using System.Diagnostics.CodeAnalysis;
using MatroskaBatchToolBox.Utility;

namespace MatroskaBatchToolBox.Model
{
    internal sealed class TimeRange
    {
        static TimeRange()
        {
            DefaultValue = new TimeRange("", null, "", null);
        }

        private TimeRange(string start, TimeSpan? startTime, string end, TimeSpan? endTime)
        {
            Start = start;
            StartTime = startTime;
            End = end;
            EndTime = endTime;
        }

        public string Start { get; }
        public TimeSpan? StartTime { get; }
        public string End { get; }
        public TimeSpan? EndTime { get; }
        public bool IsValid => StartTime is not null && EndTime is not null;
        public static TimeRange DefaultValue { get; }

        public static bool TryParse(string text, [NotNullWhen(true)] out TimeRange? timeRange)
        {
            var timeSpecs = text.Split('-');
            if (timeSpecs.Length != 2)
            {
                timeRange = default;
                return false;
            }

            var startTimeText = timeSpecs[0];
            var endTimeText = timeSpecs[1];

            // 開始時間と終了時間がともに省略されていた場合はエラーとする
            if (string.IsNullOrEmpty(startTimeText) && string.IsNullOrEmpty(endTimeText))
            {
                timeRange = default;
                return false;
            }

            // 開始時間が空でなく、かつ書式が誤っている場合はエラーとする。
            var (success1, startTime) = ParseTime(startTimeText);
            if (!success1)
            {
                timeRange = default;
                return false;
            }

            // 終了時間が空でなく、かつ書式が誤っている場合はエラーとする。
            var (success2, endTime) = ParseTime(endTimeText);
            if (!success2)
            {
                timeRange = default;
                return false;
            }

            // 少なくとも開始時間と終了時間のどちらかに正しい指定がされている場合は正常復帰する。
            timeRange = new TimeRange(startTimeText, startTime, endTimeText, endTime);
            return true;
        }

        private static (bool success, TimeSpan? time) ParseTime(string s)
            => string.IsNullOrEmpty(s)
            ? (true, null)
            : s.TryParse(TimeParsingMode.LazyMode, out var time)
            ? (true, time)
            : (false, null);
    }
}
