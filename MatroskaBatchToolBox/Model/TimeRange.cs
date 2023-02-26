using System.Diagnostics.CodeAnalysis;
using Utility;

namespace MatroskaBatchToolBox.Model
{
    internal class TimeRange
    {
        static TimeRange()
        {
            DefaultValue = new TimeRange("", "");
        }

        private TimeRange(string start, string end)
        {
            Start = start;
            End = end;
        }

        public string Start { get; }
        public string End { get; }
        public bool IsValid => !string.IsNullOrEmpty(Start) || !string.IsNullOrEmpty(End);
        public static TimeRange DefaultValue{get;}

        public static bool TryParse(string text, [MaybeNullWhen(false)] out TimeRange timeRange)
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
            if (!string.IsNullOrEmpty(startTimeText) && Time.ParseTime(startTimeText, false) is null)
            {
                timeRange = default;
                return false;
            }

            // 終了時間が空でなく、かつ書式が誤っている場合はエラーとする。
            if (!string.IsNullOrEmpty(endTimeText) && Time.ParseTime(endTimeText, false) is null)
            {
                timeRange = default;
                return false;
            }

            // 少なくとも開始時間と終了時間のどちらかに正しい指定がされている場合は正常復帰する。
            timeRange = new TimeRange(startTimeText, endTimeText);
            return true;
        }
    }
}
