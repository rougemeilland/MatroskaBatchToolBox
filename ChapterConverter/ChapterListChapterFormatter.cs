using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChapterConverter
{
    internal class ChapterListChapterFormatter
        : IChapterFormatter
    {
        private static readonly Regex _chapterPattern;
        private readonly ChapterFormatterParameter _parameter;

        static ChapterListChapterFormatter()
        {
            _chapterPattern = new Regex(@"^(CHAPTER(?<timeIndex>\d+)=(?<time>[\d\.:]+))|(CHAPTER(?<nameIndex>\d+)NAME=(?<name>.*))$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        public ChapterListChapterFormatter(ChapterFormatterParameter parameter)
        {
            _parameter = parameter;
        }

        IEnumerable<Chapter> IChapterFormatter.Parse(string rawText)
        {
            var times = new Dictionary<int, (TimeSpan time, string lineText)>();
            var names = new Dictionary<int, (string name, string lineText)>();
            foreach (var currentLineText in rawText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var match = _chapterPattern.Match(currentLineText);
                if (!match.Success)
                    throw new Exception($"There is an error in the format of the input line.: \"{currentLineText}\"");
                if (match.Groups["timeIndex"].Success)
                {
                    var indexText = match.Groups["timeIndex"].Value;
                    if (!int.TryParse(indexText, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out int index) || index < 0)
                        throw new Exception($"There is an error in the format of the input line.: \"{currentLineText}\"");
                    if (times.ContainsKey(index))
                        throw new Exception($"There are duplicate rows in the input data.: \"{currentLineText}\", \"{times[index].lineText}\"");

                    var timeText = match.Groups["time"].Value;
                    var currentTime = Utility.ParseTime(timeText, true);
                    if (currentTime is null)
                        throw new Exception($"There is an error in the format of the input line.: \"{currentLineText}\"");

                    if (currentTime >= _parameter.MaximumDuration)
                        throw new Exception($"The chapter start time is too large in the input data. Check the chapter start time or change the maximum chapter duration with the \"--maximum_duration\" option.: {currentLineText}");

                    var misorderedLineText1 =
                        times
                        .Where(timeItem => timeItem.Key < index && timeItem.Value.time > currentTime)
                        .Select(item => item.Value.lineText)
                        .FirstOrDefault();
                    if (misorderedLineText1 is not null)
                        throw new Exception($"The chapters in the input data are not arranged in ascending chronological order.: \"{currentLineText}\", \"{misorderedLineText1}\"");

                    var misorderedLineText2 =
                        times
                        .Where(timeItem => timeItem.Key > index && timeItem.Value.time < currentTime)
                        .Select(item => item.Value.lineText)
                        .FirstOrDefault();
                    if (misorderedLineText2 is not null)
                        throw new Exception($"The chapters in the input data are not arranged in ascending chronological order.: \"{currentLineText}\", \"{misorderedLineText2}\"");

                    times.Add(index, (currentTime.Value, currentLineText));
                }
                else if (match.Groups["nameIndex"].Success)
                {
                    var indexText = match.Groups["nameIndex"].Value;
                    if (!int.TryParse(indexText, NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat, out int index) || index < 0)
                        throw new Exception($"There is an error in the format of the input line.: \"{currentLineText}\"");
                    if (names.ContainsKey(index))
                        throw new Exception($"There are duplicate rows in the input data.: \"{currentLineText}\", \"{names[index].lineText}\"");

                    var name = match.Groups["name"].Value.Trim();

                    names.Add(index, (name, currentLineText));
                }
                else
                    throw new Exception($"There is an error in the format of the input line.: \"{currentLineText}\"");
            }

            var indexFormat = $"D{(times.Any() ? times.Keys.Max() : 0).ToString().Length}";

            foreach (var index in names.Keys)
            {
                if (!times.ContainsKey(index))
                    throw new Exception($"Row not found in input data. : \"CHAPTER{index.ToString(indexFormat)}=...\"");
            }

            var chapters =
                times
                .OrderBy(item => item.Key)
                .Select(item => new
                {
                    index = item.Key,
                    startTime = item.Value.time,
                    name = names.ContainsKey(item.Key) ? names[item.Key].name : "",
                    item.Value.lineText,
                })
                .ToArray();

            if (chapters[0].startTime != TimeSpan.Zero)
                _parameter.ReportWarningMessage($"The time of the first chapter in the input data is not zero.");

            for (var index = 0; index < chapters.Length; ++index)
            {
                var currentChapter = chapters[index];
                var nextStartTime = index + 1 < chapters.Length ? chapters[index + 1].startTime : _parameter.MaximumDuration;
                yield return new Chapter(currentChapter.startTime, nextStartTime, currentChapter.name);
            }
        }

        string IChapterFormatter.Render(IEnumerable<Chapter> chapters)
        {
            var chapterSummaries =
                chapters
                .Select((chapter, index) => new { index, startTime = chapter.StartTime, name = chapter.Title })
                .ToList();

            var indexFormat = $"D{chapterSummaries.Count.ToString().Length}";

            var textLines =
                chapterSummaries
                .SelectMany(summary =>
                {
                    var indexText = summary.index.ToString(indexFormat);
                    return
                        new[]
                        {
                            $"CHAPTER{indexText}={Utility.FormatTime(summary.startTime, 3)}",
                            $"CHAPTER{indexText}NAME={summary.name}",
                        };
                });

            return string.Join("\r\n", textLines.Append(""));
        }
    }
}
