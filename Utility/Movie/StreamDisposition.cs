using Utility.Models.Json;

namespace Utility.Movie
{
    public class StreamDisposition
    {
        internal StreamDisposition(MovieStreamDispositionContainer disposition)
        {
            Default = disposition.Default != 0;
            Forced = disposition.Forced != 0;
            AttachedPicture = disposition.AttachedPicture != 0;
        }

        public bool Default { get; }

        public bool Forced { get; }
        public bool AttachedPicture { get; set; }
    }
}
