using System;
using System.Collections.Generic;
using Utility;

namespace ChapterConverter
{
    internal class ImmediateChapterFormatter
        : IChapterFormatter
    {
        private readonly ChapterFormatterParameter _parameter;

        public ImmediateChapterFormatter(ChapterFormatterParameter parameter)
        {
            _parameter = parameter;
        }

        IEnumerable<Chapter> IChapterFormatter.Parse(string rawText)
        {
            var startTimes = new LinkedList<(TimeSpan startTime, string startTimeSpec)>();
            foreach (var timeSpec in rawText.Split(','))
            {
                var time =
                    Time.ParseTime(timeSpec, false)
                    ?? throw new Exception($"The chapter start time is in an invalid format.: \"{timeSpec}\" in \"{rawText}\"");
                if (timeSpec.StartsWith('+'))
                {
                    var lastTime =
                        startTimes.Last
                        ?? throw new Exception($"Do not prefix the start time of the first chapter with a plus sign (+).: \"{timeSpec}\" in \"{rawText}\"");
                    time += lastTime.Value.startTime;
                }
                startTimes.AddLast(new LinkedListNode<(TimeSpan startTime, string startTimeSpec)>((time, timeSpec)));
            }

            if (startTimes.First is not null && startTimes.First.Value.startTime != TimeSpan.Zero)
                _parameter.ReportWarningMessage($"The time of the first chapter in the input data is not zero.");

            for (var currentChapterItem = startTimes.First; currentChapterItem is not null; currentChapterItem = currentChapterItem.Next)
            {
                var nextChapterItem = currentChapterItem.Next;
                var startTime = currentChapterItem.Value.startTime;
                var endTime = nextChapterItem is not null ? nextChapterItem.Value.startTime : _parameter.MaximumDuration;
                if (startTime > endTime)
                {
                    if (nextChapterItem is not null)
                        throw new Exception($"The chapters in the input data are not arranged in ascending chronological order.: \"{currentChapterItem.Value.startTimeSpec}\" and \"{nextChapterItem.Value.startTimeSpec}\" in \"{rawText}\"");
                    else
                        throw new Exception($"The chapter start time is too large in the input data. Check the chapter start time or change the maximum chapter duration with the \"--maximum_duration\" option.: \"{currentChapterItem.Value.startTimeSpec}\" in \"{rawText}\"");
                }
                yield return new Chapter(startTime, endTime, "");
            }
        }

        string IChapterFormatter.Render(IEnumerable<Chapter> chapters) => throw new NotSupportedException($"It is not possible to output in \"immediate\" format.");
    }
}
