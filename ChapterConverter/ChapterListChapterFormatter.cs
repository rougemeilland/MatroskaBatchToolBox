using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MatroskaBatchToolBox.Utility;
using Palmtree;

namespace ChapterConverter
{
    internal class ChapterListChapterFormatter
        : ChapterFormatter
    {
        private static readonly Regex _chapterPattern;

        static ChapterListChapterFormatter()
            => _chapterPattern = new Regex(@"^(CHAPTER(?<timeIndex>\d+)=(?<time>[\d\.:]+))|(CHAPTER(?<nameIndex>\d+)NAME=(?<name>.*))$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public ChapterListChapterFormatter(ChapterFormatterParameter parameter)
            : base(parameter)
        {
        }

        protected override IEnumerable<InternalChapterElement> Parse(string rawText)
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
                    var index = match.Groups["timeIndex"].Value.ParseAsInt32();
                    if (times.TryGetValue(index, out (TimeSpan time, string lineText) value))
                        throw new Exception($"There are duplicate rows in the input data.: \"{currentLineText}\", \"{value.lineText}\"");

                    if (!match.Groups["time"].Value.TryParse(true, out TimeSpan currentTime))
                        throw new Exception($"There is an error in the format of the input line.: \"{currentLineText}\"");

                    times.Add(index, (currentTime, currentLineText));
                }
                else if (match.Groups["nameIndex"].Success)
                {
                    var index = match.Groups["nameIndex"].Value.ParseAsInt32();
                    if (names.TryGetValue(index, out (string name, string lineText) value))
                        throw new Exception($"There are duplicate rows in the input data.: \"{currentLineText}\", \"{value.lineText}\"");

                    var name = match.Groups["name"].Value.Trim();

                    names.Add(index, (name, currentLineText));
                }
                else
                {
                    throw new Exception($"There is an error in the format of the input line.: \"{currentLineText}\"");
                }
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
                    name = names.TryGetValue(item.Key, out (string name, string lineText) value) ? value.name : "",
                    item.Value.lineText,
                })
                .ToArray();

            var chapterIndexFormat = $"D{chapters.Select(chapter => chapter.index).Append(0).Max().ToString().Length}";
            for (var index = 0; index < chapters.Length; ++index)
            {
                var currentChapter = chapters[index];
                var nextStartTime = index + 1 < chapters.Length ? chapters[index + 1].startTime : Parameter.MaximumDuration;
                yield return new InternalChapterElement($"CHAPTER{currentChapter.index.ToString(chapterIndexFormat)}", currentChapter.startTime, nextStartTime, currentChapter.name);
            }
        }

        protected override string Render(IEnumerable<InternalChapterElement> chapters)
        {
            var chapterSummaries =
                chapters
                .Select((chapter, index) => new { index, startTime = chapter.StartTime, name = chapter.Title })
                .ToList();

            var indexFormat = $"D{(chapterSummaries.Count - 1).ToString().Length}";

            var lineText =
                chapterSummaries
                .SelectMany(summary =>
                {
                    var indexText = summary.index.ToString(indexFormat);
                    return
                        new[]
                        {
                            $"CHAPTER{indexText}={summary.startTime.FormatTime(3)}",
                            $"CHAPTER{indexText}NAME={summary.name}",
                        };
                });

            return string.Join("\r\n", lineText.Append(""));
        }
    }
}
