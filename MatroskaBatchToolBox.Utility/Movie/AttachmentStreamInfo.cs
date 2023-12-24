using MatroskaBatchToolBox.Utility.Models.Json;

namespace MatroskaBatchToolBox.Utility.Movie
{
    public class AttachmentStreamInfo
        : StreamInfo
    {
        internal AttachmentStreamInfo(MovieStreamInfoContainer stream, int indexWithinAttachmentStream)
            : base(stream)
        {
            IndexWithinAttachmentStream = indexWithinAttachmentStream;
        }

        public int IndexWithinAttachmentStream { get; }
    }
}
