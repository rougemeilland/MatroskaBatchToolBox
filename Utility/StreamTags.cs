using System;
using Utility.Models.Json;

namespace Utility
{
    public class StreamTags
    {
        public StreamTags(MovieStreamTagsContainer? tags)
        {
            Title = tags?.Title;
            Language = NormalizeLanguageCode(tags?.Language);
        }

        public string? Title { get; }

        public string? Language { get; }

        private static string? NormalizeLanguageCode(string? language)
        {
            // 言語コード "und" は未定義 (null) とみなす。
            return
                string.Equals(language, "und", StringComparison.InvariantCulture)
                ? null
                : language;
        }
    }
}
