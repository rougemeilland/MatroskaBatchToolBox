using System.Collections.Generic;

namespace LyricsChecker
{
    internal class LyricsContainer
    {
        public LyricsContainer()
        {
            Tags = new Dictionary<string, string>();
            LyricsTexts = new List<string>();
        }

        public IDictionary<string, string> Tags { get; }
        public ICollection<string> LyricsTexts { get; }
    }
}
