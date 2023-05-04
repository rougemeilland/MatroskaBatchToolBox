using MatroskaBatchToolBox.Utility.Models.Json;

namespace MatroskaBatchToolBox.Utility.Movie
{
    public class MovieFormatTags
    {
        internal MovieFormatTags(MovieFormatTagsContainer? tags)
        {
            Title = tags?.Title;
            Artist = tags?.Artist;
            WmMcdi = tags?.WmMcdi;
            WmCollectionId = tags?.WmCollectionId;
            Album = tags?.Album;
            AlbumArtist = tags?.AlbumArtist;
            Genre = tags?.Genre;
            Lyricist = tags?.Lyricist;
            Composer = tags?.Composer;
            Comment = tags?.Comment;
            Date = tags?.Date;
            Track = tags?.Track;
            Text = tags?.Text;
        }

        public string? Title { get; }
        public string? Artist { get; }
        public string? WmMcdi { get; }
        public string? WmCollectionId { get; }
        public string? Album { get; }
        public string? AlbumArtist { get; }
        public string? Genre { get; }
        public string? Lyricist { get; }
        public string? Composer { get; }
        public string? Comment { get; }
        public string? Date { get; }
        public string? Track { get; }
        public string? Text { get; }
    }
}
