﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Palmtree.Numerics;

namespace ChapterConverter
{
    internal sealed partial class FflogChapterFormatter
        : ChapterFormatter
    {
        public FflogChapterFormatter(ChapterFormatterParameter parameter)
            : base(parameter)
        {
        }

        protected override IEnumerable<InternalChapterElement> Parse(string rawText)
        {
            var firstMatch = GetFirstLinePattern().Match(rawText);
            return
                firstMatch.Success
                ? EnumerateChapters(rawText[(firstMatch.Index + firstMatch.Length)..])
                    .Select(chapter => new InternalChapterElement(chapter.key, chapter.startTime, chapter.endTime, chapter.title))
                : throw new ApplicationException("The string \"Chapters:\" indicating the beginning of a chapter was not found in the input data.");
        }

        protected override string Render(IEnumerable<InternalChapterElement> chapters)
            => throw new NotSupportedException($"It is not possible to output in \"fflog\" format.");

        private static IEnumerable<(string key, TimeSpan startTime, TimeSpan endTime, string title)> EnumerateChapters(string rawText)
        {
            while (rawText.Length > 0)
            {
                var match = GetChapterPattern().Match(rawText);
                if (!match.Success)
                    break;
                var key = match.Groups["key"].Value;
                var startTime = match.Groups["startTime"].Value.ParseAsDouble();
                var endTime = match.Groups["endTime"].Value.ParseAsDouble();
                var title = match.Groups["title"].Success ? match.Groups["title"].Value : "";
                yield return (key, TimeSpan.FromSeconds(startTime), TimeSpan.FromSeconds(endTime), title);
                rawText = rawText[(match.Index + match.Length)..];
            }
        }

        [GeneratedRegex(@"\s*Chapters\s*:", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)]
        private static partial Regex GetFirstLinePattern();

        [GeneratedRegex(@"^\s*(?<key>Chapter\s+#\d+:\d+):\s+start\s+(?<startTime>\d+(\.\d+)?)\s*,\s*end\s+(?<endTime>\d+(\.\d+)?)\s+(Metadata\s*:\s*title\s*:\s*(?<title>[^\r\n]*))?", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)]
        private static partial Regex GetChapterPattern();
    }
}
