using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace MatroskaBatchToolBox.Model
{
    // わかりやすいトリミングの指定方法
    // https://ffmpeg146.rssing.com/chan-52944944/article183.html

    internal class TimeRange
    {
        private static Regex _timeRangePattern1;
        private static Regex _timeRangePattern2;

        static TimeRange()
        {
            _timeRangePattern1 = new Regex(@"^(?<start>((\d+:)?\d+:)?\d+(\.\d+))\-(?<end>((\d+:)?\d+:)?\d+(\.\d+))?$", RegexOptions.Compiled);
            _timeRangePattern2 = new Regex(@"^\-(?<end>((\d+:)?\d+:)?\d+(\.\d+))$", RegexOptions.Compiled);
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
            var match1 = _timeRangePattern1.Match(text);
            if (match1.Success)
            {
                var startTimeText = match1.Groups["start"].Value;
                if (match1.Groups["end"].Success)
                {
                    var endTimeText = match1.Groups["end"].Value;
                    timeRange = new TimeRange(startTimeText, endTimeText);
                    return true;
                }
                else
                {
                    timeRange = new TimeRange(startTimeText, "");
                    return true;
                }
            }
            else
            {
                var match2 = _timeRangePattern2.Match(text);
                if (match2.Success)
                {
                    var endTimeText = match2.Groups["end"].Value;
                    timeRange = new TimeRange("", endTimeText);
                    return true;
                }
                else
                {
                    timeRange = null;
                    return false;
                }
            }
        }
    }
}
