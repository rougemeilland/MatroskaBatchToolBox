using System;
using System.Text.RegularExpressions;
using MatroskaBatchToolBox.Utility.Models.Json;
using Palmtree;

namespace MatroskaBatchToolBox.Utility.Movie
{
    public class ChapterInfo
    {
        private static readonly Regex _uselessChapterTitlePattern;

        static ChapterInfo()
            => _uselessChapterTitlePattern = new Regex(@"^\s*((\d+)|(\d+:\d+:\d+\.\d+)|(chapter\.?\s*\d+)|(チャプター\s*\d+)|(Р“Р»Р°РІР°\s*\d+))\s*$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public ChapterInfo(MovieChapterContainer chapter)
        {
            if (!chapter.TimeBase.TryParse(out long timeBaseNumerator, out long timeBaseDenominator))
                throw new Exception($"Invalid time-base format: \"{chapter.TimeBase}\"");
            Id = chapter.Id;
            TimeBaseNumerator = timeBaseNumerator;
            TimeBaseDenominator = timeBaseDenominator;
            Start = chapter.Start;
            StartTime = chapter.Start.FromTimeCountToTimeSpan(timeBaseNumerator, timeBaseDenominator);
            End = chapter.End;
            EndTime = chapter.End.FromTimeCountToTimeSpan(timeBaseNumerator, timeBaseDenominator);
            Duration = EndTime - StartTime;
            Title = chapter.Tags?.Title ?? "";
            HasUniqueChapterTitle = !string.IsNullOrEmpty(Title) && !_uselessChapterTitlePattern.IsMatch(Title);
        }

        public int Id { get; }
        public long TimeBaseNumerator { get; }
        public long TimeBaseDenominator { get; }
        public long Start { get; }
        public TimeSpan StartTime { get; }
        public long End { get; }
        public TimeSpan EndTime { get; }
        public TimeSpan Duration { get; }
        public string Title { get; }
        public bool HasUniqueChapterTitle { get; }
    }
}
