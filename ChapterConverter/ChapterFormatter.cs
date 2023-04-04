using System;
using System.Collections.Generic;
using System.Linq;
using MatroskaBatchToolBox.Utility;
using Palmtree;

namespace ChapterConverter
{
    internal abstract class ChapterFormatter
        : IChapterFormatter
    {
        protected class InternalChapterElement
        {
            protected static readonly TimeSpan DefaultMaximumDuration;
            protected static readonly TimeSpan DefaultMinimumDuration;

            protected const long _timeBaseDenominator = 1000000000;

            static InternalChapterElement()
            {
                DefaultMaximumDuration = SimpleChapterElement.DefaultMaximumDuration;
                DefaultMinimumDuration = SimpleChapterElement.DefaultMinimumDuration;
            }

            public InternalChapterElement(string friendlyId, TimeSpan startTime, TimeSpan endTime, string title)
            {
                FriendlyId = friendlyId;
                StartTime = startTime;
                EndTime = endTime;
                Title = title;
            }

            public string FriendlyId { get; }

            public TimeSpan StartTime { get; }
            public TimeSpan EndTime { get; }
            public string Title { get; }
        }

        protected const long DefaultTimeBaseNumerator = 1;
        protected const long DefaultTimeBaseDenominator = 1000000000;
        protected readonly ChapterFormatterParameter Parameter;

        protected ChapterFormatter(ChapterFormatterParameter parameter)
            => Parameter = parameter;

        protected abstract IEnumerable<InternalChapterElement> Parse(string rawText);
        protected abstract string Render(IEnumerable<InternalChapterElement> chapters);

        IEnumerable<SimpleChapterElement> IChapterFormatter.Parse(string rawText)
        {
            var chapters = Parse(rawText).ToReadOnlyArray();

            if (chapters.Length > 0 && chapters[0].StartTime != TimeSpan.Zero)
                Parameter.ReportWarningMessage($"The time of the first chapter in the input data is not zero.: start-time=\"{chapters[0].StartTime.TotalSeconds:F6}\"");

            for (var index = 0; index < chapters.Length - 1; ++index)
            {
                var currentChapter = chapters[index];
                var nextChapter = chapters[index + 1];
                if (currentChapter.StartTime > nextChapter.StartTime)
                    throw new Exception($"The chapters in the input data are not arranged in ascending chronological order.: \"{currentChapter.FriendlyId}\".start-time=\"{currentChapter.StartTime.TotalSeconds:F6}\", \"{nextChapter.FriendlyId}\".start-time=\"{nextChapter.StartTime.TotalSeconds:F6}\"");
                if (currentChapter.EndTime != nextChapter.StartTime)
                    throw new Exception($"In the input data, the start time of one chapter does not match the end time of the next chapter.: \"{currentChapter.FriendlyId}\".start-time=\"{currentChapter.StartTime.TotalSeconds:F6}\", \"{nextChapter.FriendlyId}\".start-time=\"{nextChapter.StartTime.TotalSeconds:F6}\"");
            }

            foreach (var chapter in chapters)
            {
                if (chapter.StartTime > chapter.EndTime)
                    throw new Exception($"The value of \"start-time\" is greater than the value of \"end-time\" in the input data. : start-time={chapter.StartTime.TotalSeconds:F6}, end-time={chapter.EndTime.TotalSeconds:F6}");
                if (chapter.StartTime >= Parameter.MaximumDuration)
                    throw new Exception($"The chapter start time is too large in the input data. Change the maximum chapter duration with the \"--maximum_duration\" option.: start-time=\"{chapter.StartTime.TotalSeconds:F6}\" at \"{chapter.FriendlyId}\"");
            }

            return
                chapters
                .Select(chapter => new SimpleChapterElement(chapter.StartTime, chapter.EndTime, chapter.Title));
        }

        string IChapterFormatter.Render(IEnumerable<SimpleChapterElement> chapters)
            => Render(
                chapters
                .Select((chapter, index) =>
                    new InternalChapterElement($"#{index}", chapter.StartTime, chapter.EndTime, chapter.Title)));
    }
}
