using System;
using System.Collections.Generic;
using System.Linq;
using Utility.Movie;

namespace Utility
{
    public static class SimpleChapterElementExtensions
    {
        private const long _defaultTimeBasenumerator = 1;
        private const long _defaultTimeBaseDenominator = 1000000000;

        public static string ToMetadataString(this ChapterInfo chapter)
        {
            var textLines = new[]
            {
                "[CHAPTER]",
                $"TIMEBASE={chapter.TimeBaseNumerator}/{chapter.TimeBaseDenominator}",
                $"START={chapter.Start}",
                $"END={chapter.End}",
            }.AsEnumerable();
            if (!string.IsNullOrEmpty(chapter.Title))
                textLines = textLines.Append($"title={chapter.Title}");
            return string.Join("\n", textLines);
        }

        public static string ToMetadataString(this SimpleChapterElement chapter)
        {
            var textLines = new[]
            {
                "[CHAPTER]",
                $"TIMEBASE={_defaultTimeBasenumerator}/{_defaultTimeBaseDenominator}",
                $"START={TimeSpanExtensions.FromTimeSpanToTimeCount(chapter.StartTime, _defaultTimeBasenumerator, _defaultTimeBaseDenominator)}",
                $"END={TimeSpanExtensions.FromTimeSpanToTimeCount(chapter.EndTime, _defaultTimeBasenumerator, _defaultTimeBaseDenominator)}",
            }.AsEnumerable();
            if (!string.IsNullOrEmpty(chapter.Title))
                textLines = textLines.Append($"title={chapter.Title}");
            return string.Join("\n", textLines);
        }

        public static string ToMetadataString(this IEnumerable<ChapterInfo> chapters)
            =>string.Join(
                "\n",
                chapters
                    .Select(chapter => chapter.ToMetadataString())
                    .Prepend(";FFMETADATA1")
                    .Append(""));

        public static string ToMetadataString(this IEnumerable<SimpleChapterElement> chapters)
            => string.Join(
                "\n",
                chapters
                    .Select(chapter => chapter.ToMetadataString())
                    .Prepend(";FFMETADATA1")
                    .Append(""));

        public static IEnumerable<SimpleChapterElement> ChapterFilter(this IEnumerable<ChapterInfo> chapters, ChapterFilterParameter filterParameter)
            => ChapterFilter(chapters.Select(chapter => new SimpleChapterElement(chapter.StartTime, chapter.EndTime, chapter.Title)), filterParameter);

        public static IEnumerable<SimpleChapterElement> ChapterFilter(this IEnumerable<SimpleChapterElement> chapters, ChapterFilterParameter filterParameter)
        {
            if (filterParameter.From > filterParameter.To)
                throw new Exception("internal error (from > to)");

            var duration = filterParameter.To - filterParameter.From;

            var chapterList = chapters.ToList();
            var lastChapter = chapterList.LastOrDefault();
            if (lastChapter is null)
                return Array.Empty<SimpleChapterElement>();

            var trimmedChapters =
                new LinkedList<SimpleChapterElement>(
                    chapterList
                    .Select((chapter, index) =>
                        chapter.StartTime > chapter.EndTime
                            ? throw new Exception("internal error (chapter.startTime > chapter.endTime)")
                            : new SimpleChapterElement(
                                chapter.StartTime - filterParameter.From,
                                chapter.EndTime - filterParameter.From,
                                chapter.Title))
                    .Where(chapter => chapter.EndTime >= TimeSpan.Zero && chapter.StartTime <= duration)
                    .Select(chapter =>
                        new SimpleChapterElement(
                            chapter.StartTime > TimeSpan.Zero ? chapter.StartTime : TimeSpan.Zero,
                            chapter.EndTime < duration ? chapter.EndTime : duration,
                            chapter.Title))
                    .Where(chapter => filterParameter.KeepEmptyChapter || chapter.StartTime < chapter.EndTime));

            // チャプターが2つ以上あり、かつ最初のチャプターの時間が非常に短い間繰り返す
            while (trimmedChapters.First is not null && trimmedChapters.First.Next is not null)
            {
                var firstChapterNode = trimmedChapters.First;
                var secondChapterNode = trimmedChapters.First.Next;
                if (firstChapterNode.Value.Duration >= filterParameter.MinimumDuration)
                    break;
                var newChapter = MergeChapter(firstChapterNode.Value, secondChapterNode.Value, filterParameter);
                _ = trimmedChapters.AddFirst(newChapter);
                trimmedChapters.Remove(firstChapterNode);
                trimmedChapters.Remove(secondChapterNode);
            }

            // チャプターが2つ以上あり、かつ2個目以降に時間が非常に短いチャプターが存在する間繰り返す
            for (var currentChapterNode = trimmedChapters.First; currentChapterNode is not null && currentChapterNode.Next is not null;)
            {
                var nextChapterNode = currentChapterNode.Next;
                if (nextChapterNode.Value.Duration < filterParameter.MinimumDuration)
                {
                    var newChapter = MergeChapter(currentChapterNode.Value, nextChapterNode.Value, filterParameter);
                    _ = trimmedChapters.AddAfter(nextChapterNode, newChapter);
                    trimmedChapters.Remove(currentChapterNode);
                    trimmedChapters.Remove(nextChapterNode);
                    currentChapterNode = trimmedChapters.First;
                }
                else
                {
                    currentChapterNode = currentChapterNode.Next;
                }
            }

            var lastTrimmedChapterNode = trimmedChapters.Last;
            if (lastTrimmedChapterNode is not null)
            {
                // 元の最後のチャプターの終了時間を最後のチャプターの終了時間として設定する。
                // ※ エンコード時の誤差により、トリミングの終了時間が必ずしも動画の終了時間と一致しない可能性があるため、最後のチャプターの終了時間は「非常に大きい時間」のままにしておく。

                var newLastChapter = new SimpleChapterElement(lastTrimmedChapterNode.Value.StartTime, lastChapter.EndTime, lastTrimmedChapterNode.Value.Title);
                trimmedChapters.Remove(lastTrimmedChapterNode);
                trimmedChapters.AddLast(new LinkedListNode<SimpleChapterElement>(newLastChapter));
            }

            var invalidTitle =
                filterParameter.Titles
                .Where(item => item.Key >= trimmedChapters.Count)
                .Select(item => new { chapterNumber = item.Key, chapterTitle = item.Value })
                .FirstOrDefault();
            return
                invalidTitle is null
                    ? trimmedChapters
                        .Select((chapter, chapterNumber) => new SimpleChapterElement(chapter.StartTime, chapter.EndTime, !filterParameter.Titles.ContainsKey(chapterNumber) ? chapter.Title : filterParameter.Titles[chapterNumber]))
                    : throw new Exception($"A chapter title was specified with the '--title:{invalidTitle.chapterNumber} \"{invalidTitle.chapterTitle}\"' option, but there is no corresponding chapter #{invalidTitle.chapterNumber}.");
        }

        private static SimpleChapterElement MergeChapter(SimpleChapterElement firstHalf, SimpleChapterElement secondHalf, ChapterFilterParameter filterParameter)
        {
            var title = firstHalf.Title;
            if (string.IsNullOrEmpty(title))
            {
                title = secondHalf.Title;
            }
            else if (!string.IsNullOrEmpty(secondHalf.Title))
            {
                var lostTitle = secondHalf.Title;
                if (firstHalf.Duration < secondHalf.Duration)
                {
                    title = secondHalf.Title;
                    lostTitle = firstHalf.Title;
                }

                filterParameter.WarningMessageReporter($"A very short chapter was merged with an adjacent chapter, resulting in the loss of the chapter name \"{lostTitle}\".");
            }
            else
            {
                // NOP
            }

            return new SimpleChapterElement(firstHalf.StartTime, secondHalf.EndTime, title);
        }
    }
}
