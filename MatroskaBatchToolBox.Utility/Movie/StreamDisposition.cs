using MatroskaBatchToolBox.Utility.Models.Json;

namespace MatroskaBatchToolBox.Utility.Movie
{
    public class StreamDisposition
    {
        internal StreamDisposition(MovieStreamDispositionContainer disposition)
        {
            AttachedPic = disposition.AttachedPic != 0;
            Captions = disposition.Captions != 0;
            CleanEffects = disposition.CleanEffects != 0;
            Comment = disposition.Comment != 0;
            Default = disposition.Default != 0;
            Dependent = disposition.Dependent != 0;
            Descriptions = disposition.Descriptions != 0;
            Dub = disposition.Dub != 0;
            Forced = disposition.Forced != 0;
            HearingImpaired = disposition.HearingImpaired != 0;
            Karaoke = disposition.Karaoke != 0;
            Lyrics = disposition.Lyrics != 0;
            Metadata = disposition.Metadata != 0;
            Original = disposition.Original != 0;
            Still_image = disposition.Still_image != 0;
            TimedThumbnails = disposition.TimedThumbnails != 0;
            VisualImpaired = disposition.VisualImpaired != 0;
        }

        public bool AttachedPic { get; }
        public bool Captions { get; }
        public bool CleanEffects { get; }
        public bool Comment { get; }
        public bool Default { get; }
        public bool Dependent { get; }
        public bool Descriptions { get; }
        public bool Dub { get; }
        public bool Forced { get; }
        public bool HearingImpaired { get; }
        public bool Karaoke { get; }
        public bool Lyrics { get; }
        public bool Metadata { get; }
        public bool Original { get; }
        public bool Still_image { get; }
        public bool TimedThumbnails { get; }
        public bool VisualImpaired { get; }
    }
}
