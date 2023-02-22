using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChapterConverter
{
    internal class FFMetadataChapterFormatter
        : IChapterFormatter
    {
        private const long _defaultTimeBaseDenominator = 1000000000;
        private static readonly Regex _ffMetadataHeaderPattern;
        private static readonly Regex _chapterPattern;
        private readonly ChapterFormatterParameter _parameter;

        static FFMetadataChapterFormatter()
        {
            _ffMetadataHeaderPattern = new Regex(@"^;FFMETADATA1\r?\nencoder=[^\r\n]+\r?\n", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            _chapterPattern = new Regex(@"\[CHAPTER\]\r?\nTIMEBASE=(?<timeBaseNumerator>\d+)/(?<timeBaseDenominator>\d+)\r?\nSTART=(?<startTime>\d+)\r?\nEND=(?<endTime>\d+)\r?\n(title=(?<title>[^\r\n]*)\r?\n)?", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        public FFMetadataChapterFormatter(ChapterFormatterParameter parameter)
        {
            _parameter = parameter;
        }

        IEnumerable<Chapter> IChapterFormatter.Parse(string rawText)
        {
            if (!_ffMetadataHeaderPattern.IsMatch(rawText))
                throw new Exception("Input data is not in \"ffmetadata\" format.");

            var chapters =
                _chapterPattern.Matches(rawText)
                .Select(match =>
                {
                    var timeBaseNumerator = long.Parse(match.Groups["timeBaseNumerator"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
                    var timeBaseDenominator = long.Parse(match.Groups["timeBaseDenominator"].Value, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
                    var timeBase = (double)timeBaseNumerator / timeBaseDenominator;
                    var startTimeText = match.Groups["startTime"].Value;
                    var startTime = TimeSpan.FromSeconds(long.Parse(startTimeText, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat) * timeBase);
                    var endTimeText = match.Groups["endTime"].Value;
                    var endTime = TimeSpan.FromSeconds(long.Parse(endTimeText, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat) * timeBase);
                    if (startTime > endTime)
                        throw new Exception($"In a chapter with input data, the end time is earlier than the start time.: START={startTimeText}, END={endTimeText}");
                    if (startTime >= _parameter.MaximumDuration)
                        throw new Exception($"The chapter start time is too large in the input data. Check the chapter start time or change the maximum chapter duration with the \"--maximum_duration\" option.: START={startTimeText}");
                    var title = match.Groups["title"].Success ? match.Groups["title"].Value : "";
                    return new { startTime, endTime, title, startTimeText, endTimeText };
                })
                .ToArray();

            if (chapters.Length > 0 && chapters[0].startTime != TimeSpan.Zero)
                throw new Exception($"The time of the first chapter in the input data is not zero.");

            for (var index = 0; index < chapters.Length - 1; ++index)
            {
                var chapter = chapters[index];
                var nextChapter = chapters[index + 1];
                if (chapter.startTime > nextChapter.startTime)
                    throw new Exception($"The chapters in the input data are not arranged in ascending chronological order.: (START={chapter.startTimeText}, END={chapter.endTimeText}), (START={nextChapter.startTimeText}, END={nextChapter.endTimeText})");
                if (chapter.endTime != nextChapter.startTime)
                    throw new Exception($"In the input data, the start time of one chapter does not match the end time of the next chapter.: (START={chapter.startTimeText}, END={chapter.endTimeText}), (START={nextChapter.startTimeText}, END={nextChapter.endTimeText})");
            }

            foreach (var chapter in chapters)
                yield return new Chapter(chapter.startTime, chapter.endTime, chapter.title);
        }

        string IChapterFormatter.Render(IEnumerable<Chapter> chapters)
        {
            return
                string.Join(
                    "\n",
                    chapters
                    .SelectMany(chapter =>
                    {
                        var chapterTextLines = new[]
                        {
                            "[CHAPTER]",
                            $"TIMEBASE=1/{_defaultTimeBaseDenominator}",
                            $"START={Convert.ToInt64(chapter.StartTime.TotalSeconds * _defaultTimeBaseDenominator)}",
                            $"END={Convert.ToInt64(chapter.EndTime.TotalSeconds * _defaultTimeBaseDenominator)}",
                        };
                        if (string.IsNullOrEmpty(chapter.Title))
                            return chapterTextLines;
                        else
                            return chapterTextLines.Append($"title={chapter.Title}");
                    })
                    .Prepend(";FFMETADATA1")
                    .Append(""));
        }
    }
}
