using System;
using System.Collections.Generic;
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

        public bool? this[string dispositionName]
            => dispositionName.ToUpperInvariant() switch
            {
                "ATTACHED_PIC" or "ATTACHEDPIC" => AttachedPic,
                "CAPTIONS" => Captions,
                "CLEAN_EFFECTS" or "CLEANEFFECTS" => CleanEffects,
                "COMMENT" => Comment,
                "DEFAULT" => Default,
                "DEPENDENT" => Dependent,
                "DESCRIPTIONS" => Descriptions,
                "DUB" => Dub,
                "FORCED" => Forced,
                "HEARING_IMPAIRED" or "HEARINGIMPAIRED" => HearingImpaired,
                "KARAOKE" => Karaoke,
                "LYRICS" => Lyrics,
                "METADATA" => Metadata,
                "ORIGINAL" => Original,
                "STILL_IMAGE" => Still_image,
                "TIMED_THUMBNAILS" or "TIMEDTHUMBNAILS" => TimedThumbnails,
                "VISUAL_IMPAIRED" or "VISUALIMPAIRED" => VisualImpaired,
                _ => null,
            };

        public IEnumerable<(string dispositionName, bool dispositionValue)> EnumerateDispositions()
        {
            yield return ("attached_pic", AttachedPic);
            yield return ("captions", Captions);
            yield return ("clean_effects", CleanEffects);
            yield return ("comment", Comment);
            yield return ("default", Default);
            yield return ("dependent", Dependent);
            yield return ("descriptions", Descriptions);
            yield return ("dub", Dub);
            yield return ("forced", Forced);
            yield return ("hearing_impaired", HearingImpaired);
            yield return ("karaoke", Karaoke);
            yield return ("lyrics", Lyrics);
            yield return ("metadata", Metadata);
            yield return ("original", Original);
            yield return ("still_image", Still_image);
            yield return ("timed_thumbnails", TimedThumbnails);
            yield return ("visual_impaired", VisualImpaired);
        }
    }
}
