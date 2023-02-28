using System;
using System.Collections.Generic;
using System.Linq;

namespace Utility
{
    public static class ChapterInfoExtensions
    {
        public static string ToMetadataString(this IEnumerable<ChapterInfo> chapters)
        {
            return
                string.Join(
                    "\n",
                    chapters
                    .Select(chapter => chapter.ToMetadataString())
                    .Prepend(";FFMETADATA1")
                    .Append(""));
        }

        public static IEnumerable<ChapterInfo> ChapterFilter(this IEnumerable<ChapterInfo> chapters, ChapterFilterParameter filterParameter)
        {
            if (filterParameter.From > filterParameter.To)
                throw new Exception("internal error (from > to)");

            var duration = filterParameter.To - filterParameter.From;

            var chapterList = chapters.ToList();
            var lastChapter = chapterList.LastOrDefault();
            if (lastChapter is null)
                return Array.Empty<ChapterInfo>();

            var trimmedChapters =
                new LinkedList<ChapterInfo>(
                    chapterList
                    .Select((chapter, index) =>
                    {
                        if (chapter.StartTime > chapter.EndTime)
                            throw new Exception("internal error (chapter.StartTime > chapter.EndTime)");
                        return new
                        {
                            startTime = chapter.StartTime - filterParameter.From,
                            endTime = chapter.EndTime - filterParameter.From,
                            title = chapter.Title,
                        };
                    })
                    .Where(chapter => chapter.endTime >= TimeSpan.Zero && chapter.startTime <= duration)
                    .Select(chapter =>
                        new ChapterInfo(
                            chapter.startTime > TimeSpan.Zero ? chapter.startTime : TimeSpan.Zero,
                            chapter.endTime < duration ? chapter.endTime : duration,
                            chapter.title))
                    .Where(chapter => filterParameter.KeepEmptyChapter || chapter.StartTime < chapter.EndTime));


            // チャプターが2つ以上あり、かつ最初のチャプターの時間が非常に短い間繰り返す
            while (trimmedChapters.First is not null && trimmedChapters.First.Next is not null)
            {
                var firstChapterNode = trimmedChapters.First;
                var secondChapterNode = trimmedChapters.First.Next;
                if (firstChapterNode.Value.Duration >= filterParameter.MinimumDuration)
                    break;
                var newChapter = MergeChapter(firstChapterNode.Value, secondChapterNode.Value, filterParameter);
                trimmedChapters.AddFirst(newChapter);
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
                    trimmedChapters.AddAfter(nextChapterNode, newChapter);
                    trimmedChapters.Remove(currentChapterNode);
                    trimmedChapters.Remove(nextChapterNode);
                    currentChapterNode = trimmedChapters.First;
                }
                else
                    currentChapterNode = currentChapterNode.Next;
            }

            var lastTrimmedChapterNode = trimmedChapters.Last;
            if (lastTrimmedChapterNode is not null)
            {
                // 元の最後のチャプターの終了時間を最後のチャプターの終了時間として設定する。
                // ※ エンコード時の誤差により、トリミングの終了時間が必ずしも動画の終了時間と一致しない可能性があるため、最後のチャプターの終了時間は「非常に大きい時間」のままにしておく。

                var newLastChapter =
                    new ChapterInfo(
                        lastTrimmedChapterNode.Value.StartTime,
                        lastChapter.EndTime,
                        lastTrimmedChapterNode.Value.Title);
                trimmedChapters.Remove(lastTrimmedChapterNode);
                trimmedChapters.AddLast(new LinkedListNode<ChapterInfo>(newLastChapter));
            }

            var invalidTitle =
                filterParameter.Titles
                .Where(item => item.Key >= trimmedChapters.Count)
                .Select(item => new { chapterNumber = item.Key, chapterTitle = item.Value })
                .FirstOrDefault();
            if (invalidTitle is not null)
                throw new Exception($"A chapter title was specified with the '--title:{invalidTitle.chapterNumber} \"{invalidTitle.chapterTitle}\"' option, but there is no corresponding chapter #{invalidTitle.chapterNumber}.");

            return
                trimmedChapters
                .Select((chapter, chapterNumber) =>
                    new ChapterInfo(
                        chapter.StartTime,
                        chapter.EndTime,
                        !filterParameter.Titles.ContainsKey(chapterNumber) ? chapter.Title : filterParameter.Titles[chapterNumber]));
        }

        private static ChapterInfo MergeChapter(ChapterInfo firstHalf, ChapterInfo secondHalf, ChapterFilterParameter filterParameter)
        {
            var title = firstHalf.Title;
            if (string.IsNullOrEmpty(title))
                title = secondHalf.Title;
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
            return new ChapterInfo(firstHalf.StartTime, secondHalf.EndTime, title);
        }

    }
}
