using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChapterConverter
{
    internal class FFLogChapterFormatter
        : IChapterFormatter
    {
        private static readonly Regex _firstPattern;
        private static readonly Regex _chapterPattern;
        private readonly ChapterFormatterParameter _parameter;

        static FFLogChapterFormatter()
        {
            _firstPattern = new Regex(@"\s*Chapters\s*:", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            _chapterPattern = new Regex(@"^\s*(?<key>Chapter\s+#\d+:\d+):\s+start\s+(?<startTime>\d+(\.\d+)?)\s*,\s*end\s+(?<endTime>\d+(\.\d+)?)\s+(Metadata\s*:\s*title\s*:\s*(?<title>[^\r\n]*))?", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        public FFLogChapterFormatter(ChapterFormatterParameter parameter)
        {
            _parameter = parameter;
        }

        IEnumerable<Chapter> IChapterFormatter.Parse(string rawText)
        {
            var firstMatch = _firstPattern.Match(rawText);
            if (!firstMatch.Success)
                throw new Exception("The string \"Chapters:\" indicating the beginning of a chapter was not found in the input data.");

            var chapters = FFLogChapterFormatter.EnumerateChapters(rawText[(firstMatch.Index + firstMatch.Length)..]).ToArray();
            if (chapters.Length > 0 && chapters[0].chapter.StartTime != TimeSpan.Zero)
                throw new Exception($"The time of the first chapter in the input data is not zero.");

            for (var index = 0; index < chapters.Length - 1; ++index)
            {
                var (chapter, key) = chapters[index];
                var nextChapter = chapters[index + 1];
                if (chapter.StartTime > nextChapter.chapter.StartTime)
                    throw new Exception($"The chapters in the input data are not arranged in ascending chronological order.: \"{key}\", \"{nextChapter.key}\"");
                if (chapter.EndTime != nextChapter.chapter.StartTime)
                    throw new Exception($"In the input data, the start time of one chapter does not match the end time of the next chapter.: \"{key}\", \"{nextChapter.key}\"");
            }

            foreach (var chapter in chapters)
                yield return chapter.chapter;
        }

        string IChapterFormatter.Render(IEnumerable<Chapter> chapters) => throw new NotSupportedException($"It is not possible to output in \"fflog\" format.");

        private static IEnumerable<(Chapter chapter, string key)> EnumerateChapters(string rawText)
        {
            while (rawText.Length > 0)
            {
                var match = _chapterPattern.Match(rawText);
                if (!match.Success)
                    break;
                var key = match.Groups["key"].Value;
                var startTime = double.Parse(match.Groups["startTime"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat);
                var endTime = double.Parse(match.Groups["endTime"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat);
                if (startTime > endTime)
                    throw new Exception($"In a chapter with input data, the end time is earlier than the start time.: \"{key}\"");
                var title = match.Groups["title"].Success ? match.Groups["title"].Value : "";
                yield return (new Chapter(TimeSpan.FromSeconds(startTime), TimeSpan.FromSeconds(endTime), title), key);
                rawText = rawText[(match.Index + match.Length)..];
            }
        }

    }
}
