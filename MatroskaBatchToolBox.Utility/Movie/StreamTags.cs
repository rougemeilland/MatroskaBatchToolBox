using System;
using MatroskaBatchToolBox.Utility.Models.Json;

namespace MatroskaBatchToolBox.Utility.Movie
{
    public class StreamTags
    {
        internal StreamTags(MovieStreamTagsContainer? tags)
        {
            var durationText = tags?.Duration;
            Encoder = tags?.Encoder;
            Duration =
                durationText is null || !durationText.TryParse(TimeParsingMode.LazyMode, out TimeSpan duration)
                ? null
                : duration;
            Title = tags?.Title;
            Language = NormalizeLanguageCode(tags?.Language);
            Comment = tags?.Comment;
        }

        /// <summary>
        /// ストリームのエンコーダーを表すメタデータ。
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>ffmpeg 固有のメタデータなので、必ず存在するとは限らない。</item>
        /// </list>
        /// </remarks>
        public string? Encoder { get; }

        /// <summary>
        /// ストリームの長さを表す時間を表すメタデータ。
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>ffmpeg 固有のメタデータなので、必ず存在するとは限らない。</item>
        /// </list>
        /// </remarks>
        public TimeSpan? Duration { get; }

        public string? Title { get; }

        public string? Language { get; }

        /// <summary>
        /// ストリームのコメントを表すメタデータ。
        /// </summary>
        /// <remarks>
        /// 以下の条件の場合、このタグの値は "Cover (front)" となる。
        /// <list type="bullet">
        /// <item>当該ファイルが楽曲であり、かつ</item>
        /// <item>当該ファイルにカバー画像のストリームが含まれており、かつ</item>
        /// <item>当該ストリームがカバー画像のストリームである場合</item>
        /// </list>
        /// </remarks>
        public string? Comment { get; }

        private static string? NormalizeLanguageCode(string? language)
            // 言語コード "und" は未定義 (null) とみなす。
            => language == "und" ? null : language;

    }
}
