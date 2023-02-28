using System;
using System.Linq;
using System.Text.RegularExpressions;
using Utility.Models.Json;

namespace Utility
{
    public class ChapterInfo
    {
        public static readonly TimeSpan DefaultMaximumDuration;
        public static readonly TimeSpan DefaultMinimumDuration;

        private const long _timeBaseDenominator = 1000000000;
        private static readonly Regex _uselessChapterTitlePattern;

        static ChapterInfo()
        {
            DefaultMaximumDuration = TimeSpan.FromDays(7);
            DefaultMinimumDuration = TimeSpan.FromMilliseconds(10);
            _uselessChapterTitlePattern = new Regex(@"^\s*((\d+)|(\d+:\d+:\d+\.\d+)|(chapter\.?\s*\d+)|(チャプター\s*\d+)|(Р“Р»Р°РІР°\s*\d+))\s*$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        }

        internal ChapterInfo(MovieChapterContainer chapter)
        {
            if (!Numerics.TryParseRationalNumber(chapter.TimeBase, out long timeBaseNumerator, out long timeBaseDenominator))
                throw new Exception($"Invalid time-base format: \"{chapter.TimeBase}\"");
            StartTime = Time.FromTimeCountToTimeSpan(chapter.Start, timeBaseNumerator, timeBaseDenominator);
            EndTime = Time.FromTimeCountToTimeSpan(chapter.End, timeBaseNumerator, timeBaseDenominator);
            Duration = EndTime - StartTime;
            Title = chapter.Tags?.Title ?? "";
            HasUniqueChapterTitle = !string.IsNullOrEmpty(Title) && !_uselessChapterTitlePattern.IsMatch(Title);
        }

        public ChapterInfo(TimeSpan startTime, TimeSpan endTime, string title)
        {
            StartTime = startTime;
            EndTime = endTime;
            Title = title;
            Duration = endTime - startTime;
            HasUniqueChapterTitle = !string.IsNullOrEmpty(Title) && !_uselessChapterTitlePattern.IsMatch(Title);
        }

        public TimeSpan StartTime { get; }
        public TimeSpan EndTime { get; }
        public TimeSpan Duration { get; }
        public string Title { get; }
        public bool HasUniqueChapterTitle { get; }

        public string ToMetadataString()
        {
            var textLines = new[]
            {
                "[CHAPTER]",
                $"TIMEBASE=1/{_timeBaseDenominator}",
                $"START={Time.FromTimeSpanToTimeCount(StartTime, 1, _timeBaseDenominator)}",
                $"END={Time.FromTimeSpanToTimeCount(EndTime, 1, _timeBaseDenominator)}",
            }.AsEnumerable();
            if (!string.IsNullOrEmpty(Title))
                textLines = textLines.Append($"title={Title}");
            return string.Join("\n", textLines);
        }
    }
}
