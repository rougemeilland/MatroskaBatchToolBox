using System.Collections.Generic;
using System.Linq;
using MatroskaBatchToolBox.Utility.Movie;

namespace AudioNormalizer
{
    internal abstract class MusicFileMetadataProvider
        : IMusicFileMetadataProvider
    {
        public abstract bool Supported { get; }
        public abstract MusicFileMetadata GetMetadata(MovieInformation sourceMusicFileInfo);
        public abstract string? FormatMetadataFile(MusicFileMetadata metadata);
        public abstract IEnumerable<(string metadataName, string metadatavalue)> GetStreamMetadata(MusicFileMetadata metadata);
        public abstract string GuessFileFormat();
        public abstract (string encoder, IEnumerable<string> encoderOptions) GuessDefaultEncoderSpec(AudioStreamInfo sourceAudioStreamInfo);

        protected static string EncodeFfmetadataValue(string metadataValue)
            => string.Concat(
                metadataValue.Replace("\r\n", "\n")
                .Select(c =>
                    c switch
                    {
                        '=' => @"\=",
                        ';' => @"\;",
                        '#' => @"\#",
                        '\\' => @"\\",
                        '\r' or '\n' => "\\\n",
                        _ => c.ToString(),
                    }));
    }
}
