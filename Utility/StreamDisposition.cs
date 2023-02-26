using Utility.Models.Json;

namespace Utility
{
    public class StreamDisposition
    {
        public StreamDisposition(MovieStreamDispositionContainer disposition)
        {
            Default = disposition.Default != 0;
            Forced = disposition.Forced != 0;
        }

        public bool Default { get; }

        public bool Forced { get; }
    }
}
