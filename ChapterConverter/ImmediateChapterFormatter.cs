using System;
using System.Collections.Generic;
using System.Linq;
using MatroskaBatchToolBox.Utility;

namespace ChapterConverter
{
    internal sealed class ImmediateChapterFormatter
        : ChapterFormatter
    {
        public ImmediateChapterFormatter(ChapterFormatterParameter parameter)
            : base(parameter)
        {
        }

        protected override IEnumerable<InternalChapterElement> Parse(string rawText)
            => rawText.ParseAsChapterStartTimes().ToSimpleChapterElements(Parameter.MaximumDuration, Parameter.ReportWarningMessage)
                .Select((chapter, index) => new InternalChapterElement($"#{index}", chapter.StartTime, chapter.EndTime, chapter.Title));

        protected override string Render(IEnumerable<InternalChapterElement> chapters)
            => throw new NotSupportedException($"It is not possible to output in \"immediate\" format.");
    }
}
