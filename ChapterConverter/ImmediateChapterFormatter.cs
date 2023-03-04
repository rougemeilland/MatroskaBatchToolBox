using System;
using System.Collections.Generic;
using Utility;

namespace ChapterConverter
{
    internal class ImmediateChapterFormatter
        : ChapterFormatter
    {
        public ImmediateChapterFormatter(ChapterFormatterParameter parameter)
            : base(parameter)
        {
        }

        protected override IEnumerable<InternalChapterElement> Parse(string rawText)
        {
            var startTimes = new LinkedList<(TimeSpan startTime, string startTimeSpec)>();
            foreach (var timeSpec in rawText.Split(','))
            {
                if (!timeSpec.TryParse(false, out TimeSpan time))
                    throw new Exception($"The chapter start time is in an invalid format.: \"{timeSpec}\" in \"{rawText}\"");
                if (timeSpec.StartsWith('+'))
                {
                    var lastTime =
                        startTimes.Last
                        ?? throw new Exception($"Do not prefix the start time of the first chapter with a plus sign (+).: \"{timeSpec}\" in \"{rawText}\"");
                    time += lastTime.Value.startTime;
                }

                startTimes.AddLast(new LinkedListNode<(TimeSpan startTime, string startTimeSpec)>((time, timeSpec)));
            }

            var index = 0;
            for (var currentChapterItem = startTimes.First; currentChapterItem is not null; currentChapterItem = currentChapterItem.Next)
            {
                var nextChapterItem = currentChapterItem.Next;
                var startTime = currentChapterItem.Value.startTime;
                var endTime = nextChapterItem is not null ? nextChapterItem.Value.startTime : Parameter.MaximumDuration;
                yield return new InternalChapterElement($"#{index}", startTime, endTime, "");
                ++index;
            }
        }

        protected override string Render(IEnumerable<InternalChapterElement> chapters)
            => throw new NotSupportedException($"It is not possible to output in \"immediate\" format.");
    }
}
