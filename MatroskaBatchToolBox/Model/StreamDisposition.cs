using MatroskaBatchToolBox.Model.Json;

namespace MatroskaBatchToolBox.Model
{
    internal class StreamDisposition
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
