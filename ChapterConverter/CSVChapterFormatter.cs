using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChapterConverter
{
    internal class CSVChapterFormatter
        : IChapterFormatter
    {
        private static readonly Regex _rawPattern;
        private readonly ChapterFormatterParameter _parameter;

        static CSVChapterFormatter()
        {
            _rawPattern = new Regex(@"^(?<time>[\d\.:]+)\t(?<name>[^\t]*)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        public CSVChapterFormatter(ChapterFormatterParameter parameter)
        {
            _parameter = parameter;
        }

        IEnumerable<Chapter> IChapterFormatter.Parse(string rawText)
        {
            var chapterSummaries =
                rawText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(lineText =>
                {
                    var match = _rawPattern.Match(lineText);
                    if (!match.Success)
                        throw new Exception($"Invalid row format in input data.: {lineText}");

                    var time = Utility.ParseTime(match.Groups["time"].Value, false);
                    if (time is null)
                        throw new Exception($"Invalid row format in input data.: {lineText}");

                    if (time >= _parameter.MaximumDuration)
                        throw new Exception($"The chapter start time is too large in the input data. Check the chapter start time or change the maximum chapter duration with the \"--maximum_duration\" option.: {lineText}");

                    var name = match.Groups["name"].Value.Trim();

                    return new { time = time.Value, name, lineText };
                })
                .ToArray();

            if (chapterSummaries[0].time != TimeSpan.Zero)
                _parameter.ReportWarningMessage($"The time of the first chapter in the input data is not zero.");

            for (var index = 0; index < chapterSummaries.Length - 1; ++index)
            {
                var currentChapter = chapterSummaries[index];
                var nextChapter = chapterSummaries[index + 1];
                if (currentChapter.time > nextChapter.time)
                    throw new Exception($"The chapters in the input data are not arranged in ascending chronological order.: \"{currentChapter.lineText}\", \"{nextChapter.lineText}\"");
            }

            for (var index = 0; index < chapterSummaries.Length; ++index)
            {
                var summary = chapterSummaries[index];
                var endTime = index + 1 < chapterSummaries.Length ? chapterSummaries[index + 1].time : _parameter.MaximumDuration;
                yield return new Chapter(summary.time, endTime, summary.name);
            }
        }

        string IChapterFormatter.Render(IEnumerable<Chapter> chapters)
        {
            return
                string.Join(
                    "\r\n",
                    chapters
                    .Select(chapter => FormatRaw(chapter.StartTime, chapter.Title))
                    .Append(""));
        }

        private string FormatRaw(TimeSpan time, string name)
        {
            var timeText = Utility.FormatTime(time, 6);
            var modifiedName = name.Replace('\t', ' ');
            if (name.Contains('\t'))
                _parameter.ReportWarningMessage($"In the output data, the TAB code is included in the chapter name, so replace the TAB code with white space.: time={timeText} ({time.TotalSeconds:F6}), name=\"{modifiedName}\"");

            return $"{timeText}\t{modifiedName}";
        }
    }
}
