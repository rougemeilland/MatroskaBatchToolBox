using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MatroskaBatchToolBox.Utility;
using Palmtree.Numerics;

namespace ChapterConverter
{
    internal class FfmetadataChapterFormatter
        : ChapterFormatter
    {
        private static readonly Regex _ffMetadataHeaderPattern;
        private static readonly Regex _chapterPattern;

        static FfmetadataChapterFormatter()
        {
            _ffMetadataHeaderPattern = new Regex(@"^;FFMETADATA1\r?\nencoder=[^\r\n]+\r?\n", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            _chapterPattern = new Regex(@"\[CHAPTER\]\r?\nTIMEBASE=(?<timeBase>\d+/\d+)\r?\nSTART=(?<startTime>\d+)\r?\nEND=(?<endTime>\d+)\r?\n(title=(?<title>[^\r\n]*)\r?\n)?", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        public FfmetadataChapterFormatter(ChapterFormatterParameter parameter)
            : base(parameter)
        {
        }

        protected override IEnumerable<InternalChapterElement> Parse(string rawText)
            => _ffMetadataHeaderPattern.IsMatch(rawText)
                ? _chapterPattern.Matches(rawText)
                    .Select((match, index) =>
                    {
                        if (!match.Groups["timeBase"].Value.TryParse(out long timeBaseNumerator, out long timeBaseDenominator))
                            throw new Exception("internal error (illegal timeBase format)");
                        var startTime = match.Groups["startTime"].Value.ParseAsInt64().FromTimeCountToTimeSpan(timeBaseNumerator, timeBaseDenominator);
                        var endTime = match.Groups["endTime"].Value.ParseAsInt64().FromTimeCountToTimeSpan(timeBaseNumerator, timeBaseDenominator);
                        var title = match.Groups["title"].Success ? match.Groups["title"].Value : "";
                        return new InternalChapterElement($"[CHAPTER]#{index}", startTime, endTime, title);
                    })
                : throw new Exception("Input data is not in \"ffmetadata\" format.");

        protected override string Render(IEnumerable<InternalChapterElement> chapters)
            => chapters
                .Select(chapter => new SimpleChapterElement(chapter.StartTime, chapter.EndTime, chapter.Title))
                .ToMetadataString();
    }
}
