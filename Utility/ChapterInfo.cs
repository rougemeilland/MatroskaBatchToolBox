using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Utility.Models.Json;

namespace Utility
{
    public class ChapterInfo
    {
        public ChapterInfo(MovieChapterContainer chapter)
        {
            if (!Numerics.TryParseRationalNumber(chapter.TimeBase, out long timeBaseNumerator, out long timeBaseDenominator))
                throw new Exception($"Invalid time-base format: \"{chapter.TimeBase}\"");
            StartTime = Time.FromTimeBaseToTimeSpan(chapter.Start, timeBaseNumerator, timeBaseDenominator);
            EndTime = Time.FromTimeBaseToTimeSpan(chapter.End, timeBaseNumerator, timeBaseDenominator);
            Title = chapter.Tags?.Title ?? "";
        }

        public TimeSpan StartTime { get; }
        public TimeSpan EndTime { get; }
        public string Title { get; }
    }
}
