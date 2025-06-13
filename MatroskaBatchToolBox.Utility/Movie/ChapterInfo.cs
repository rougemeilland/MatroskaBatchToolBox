using System;
using System.Text.RegularExpressions;
using MatroskaBatchToolBox.Utility.Models.Json;
using Palmtree.Numerics;

namespace MatroskaBatchToolBox.Utility.Movie
{
    public partial class ChapterInfo
    {
        private static readonly Regex _uselessChapterTitlePattern;

        static ChapterInfo()
        {
            _uselessChapterTitlePattern = GetUselessChapterTitlePattern();
        }

        public ChapterInfo(MovieChapterContainer chapter)
        {
            if (!chapter.TimeBase.TryParse(out var timeBaseNumerator, out long timeBaseDenominator))
                throw new ApplicationException($"Invalid time-base format: \"{chapter.TimeBase}\"");
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

        public long Id { get; }
        public long TimeBaseNumerator { get; }
        public long TimeBaseDenominator { get; }
        public long Start { get; }
        public TimeSpan StartTime { get; }
        public long End { get; }
        public TimeSpan EndTime { get; }
        public TimeSpan Duration { get; }
        public string Title { get; }
        public bool HasUniqueChapterTitle { get; }

        [GeneratedRegex(@"^\s*((\d+)|(\d+:\d+:\d+\.\d+)|(chapter\.?\s*\d+)|(チャプター\s*\d+)|(Р“Р»Р°РІР°\s*\d+))\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)]
        private static partial Regex GetUselessChapterTitlePattern();
    }
}
