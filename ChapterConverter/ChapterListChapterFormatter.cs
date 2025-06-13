using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using MatroskaBatchToolBox.Utility;
using Palmtree.Numerics;

namespace ChapterConverter
{
    internal sealed partial class ChapterListChapterFormatter
        : ChapterFormatter
    {
        internal static readonly char[] _carriageReturnOrNewLine = ['\r', '\n'];

        public ChapterListChapterFormatter(ChapterFormatterParameter parameter)
            : base(parameter)
        {
        }

        protected override IEnumerable<InternalChapterElement> Parse(string rawText)
        {
            var times = new Dictionary<int, (TimeSpan time, string lineText)>();
            var names = new Dictionary<int, (string name, string lineText)>();
            foreach (var currentLineText in rawText.Split(_carriageReturnOrNewLine, StringSplitOptions.RemoveEmptyEntries))
            {
                var match = GetChapterPattern().Match(currentLineText);
                if (!match.Success)
                    throw new ApplicationException($"There is an error in the format of the input line.: \"{currentLineText}\"");
                if (match.Groups["timeIndex"].Success)
                {
                    var index = match.Groups["timeIndex"].Value.ParseAsInt32();
                    if (times.TryGetValue(index, out var value))
                        throw new ApplicationException($"There are duplicate rows in the input data.: \"{currentLineText}\", \"{value.lineText}\"");

                    if (!match.Groups["time"].Value.TryParse(TimeParsingMode.StrictForLongTimeFormat, out var currentTime))
                        throw new ApplicationException($"There is an error in the format of the input line.: \"{currentLineText}\"");

                    times.Add(index, (currentTime, currentLineText));
                }
                else if (match.Groups["nameIndex"].Success)
                {
                    var index = match.Groups["nameIndex"].Value.ParseAsInt32();
                    if (names.TryGetValue(index, out var value))
                        throw new ApplicationException($"There are duplicate rows in the input data.: \"{currentLineText}\", \"{value.lineText}\"");

                    var name = match.Groups["name"].Value.Trim();

                    names.Add(index, (name, currentLineText));
                }
                else
                {
                    throw new ApplicationException($"There is an error in the format of the input line.: \"{currentLineText}\"");
                }
            }

            var indexFormat = $"D{(times.Count > 0 ? times.Keys.Max() : 0).ToString(CultureInfo.InvariantCulture).Length}";

            foreach (var index in names.Keys)
            {
                if (!times.ContainsKey(index))
                    throw new ApplicationException($"Row not found in input data. : \"CHAPTER{index.ToString(indexFormat, CultureInfo.InvariantCulture)}=...\"");
            }

            var chapters =
                times
                .OrderBy(item => item.Key)
                .Select(item => new
                {
                    index = item.Key,
                    startTime = item.Value.time,
                    name = names.TryGetValue(item.Key, out var value) ? value.name : "",
                    item.Value.lineText,
                })
                .ToArray();

            var chapterIndexFormat = $"D{chapters.Select(chapter => chapter.index).Append(0).Max().ToString(CultureInfo.InvariantCulture).Length}";
            for (var index = 0; index < chapters.Length; ++index)
            {
                var currentChapter = chapters[index];
                var nextStartTime = index + 1 < chapters.Length ? chapters[index + 1].startTime : Parameter.MaximumDuration;
                yield return new InternalChapterElement($"CHAPTER{currentChapter.index.ToString(chapterIndexFormat, CultureInfo.InvariantCulture)}", currentChapter.startTime, nextStartTime, currentChapter.name);
            }
        }

        protected override string Render(IEnumerable<InternalChapterElement> chapters)
        {
            var chapterSummaries =
                chapters
                .Select((chapter, index) => new { index, startTime = chapter.StartTime, name = chapter.Title })
                .ToList();

            var indexFormat = $"D{(chapterSummaries.Count - 1).ToString(CultureInfo.InvariantCulture).Length}";

            var lineText =
                chapterSummaries
                .SelectMany(summary =>
                {
                    var indexText = summary.index.ToString(indexFormat, CultureInfo.InvariantCulture);
                    return
                        new[]
                        {
                            $"CHAPTER{indexText}={summary.startTime.FormatTime(TimeFormatType.LongFormat, 3)}",
                            $"CHAPTER{indexText}NAME={summary.name}",
                        };
                });

            return string.Join("\r\n", lineText.Append(""));
        }

        [GeneratedRegex(@"^(CHAPTER(?<timeIndex>\d+)=(?<time>[\d\.:]+))|(CHAPTER(?<nameIndex>\d+)NAME=(?<name>.*))$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex GetChapterPattern();
    }
}
