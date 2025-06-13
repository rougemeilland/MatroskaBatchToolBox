using System.Collections.Generic;

namespace LyricsChecker
{
    internal sealed class LyricsContainer
    {
        public LyricsContainer()
        {
            Tags = new Dictionary<string, string>();
            LyricsTexts = [];
        }

        public IDictionary<string, string> Tags { get; }
        public ICollection<string> LyricsTexts { get; }
    }
}
