using System.Collections.Generic;
using MatroskaBatchToolBox.Utility.Movie;

namespace AudioNormalizer
{
    internal interface IMusicFileMetadataProvider
    {
        bool Supported { get; }
        MusicFileMetadata GetMetadata(MovieInformation sourceMusicFileInfo);
        string? FormatMetadataFile(MusicFileMetadata metadata);
        IEnumerable<(string metadataName, string metadatavalue)> GetStreamMetadata(MusicFileMetadata metadata);
        (string encoder, IEnumerable<string> encoderOptions) GuessDefaultEncoderSpec(AudioStreamInfo sourceAudioStreamInfo);
        string GuessFileFormat();
    }
}
