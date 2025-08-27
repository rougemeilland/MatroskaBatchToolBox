using System.Collections.Generic;
using MatroskaBatchToolBox.Utility.Movie;

namespace AudioNormalizer
{
    internal interface IMusicFileMetadataProvider
    {
        bool Supported { get; }
        string Format { get; }
        string DefaultExtension { get; }
        MusicFileMetadata GetMetadata(MovieInformation sourceMusicFileInfo);
        IEnumerable<(string metadataName, string metadataValue)> EnumerateFormatMetadata(MusicFileMetadata metadata);
        IEnumerable<(string metadataName, string metadataValue)> EnumerateStreamMetadata(MusicFileMetadata metadata);
        (string encoder, IEnumerable<string> encoderOptions) GuessDefaultEncoderSpec(AudioStreamInfo sourceAudioStreamInfo);
    }
}
