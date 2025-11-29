using System;
using System.Collections.Generic;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;

namespace AudioNormalizer
{
    internal sealed class ID3MusicFileMetadataProvider
        : MusicFileMetadataProvider
    {
        private readonly TransferDirection _direction;
        private readonly string? _fileFormat;

        public ID3MusicFileMetadataProvider(TransferDirection direction, string? fileFormat, string? fileExtension)
        {
            _direction = direction;
            _fileFormat =
                fileFormat is null && fileExtension is null
                ? null
                : (fileFormat is null || fileFormat == "wav") && (fileExtension is null || fileExtension == ".wav")
                ? "wav"
                : (fileFormat is null || fileFormat == "mp3") && (fileExtension is null || fileExtension == ".mp3")
                ? "mp3"
                : null;
        }

        public override bool Supported => _fileFormat is not null;

        public override string DefaultExtension
        {
            get
            {
                if (_fileFormat is null)
                    throw Validation.GetFailErrorException();
                return
                    _fileFormat switch
                    {
                        "wav" => ".wav",
                        "mp3" => ".mp3",
                        _ => throw Validation.GetFailErrorException(),
                    };
            }
        }

        public override string Format => _fileFormat ?? throw new InvalidOperationException();

        public override MusicFileMetadata GetMetadata(MovieInformation sourceMusicFileInfo)
        {
            if (_direction != TransferDirection.Input)
                throw new InvalidOperationException();

            return new()
            {
                Album = sourceMusicFileInfo.Format.Tags["album"],
                AlbumArtist = sourceMusicFileInfo.Format.Tags["album_artist"],
                Artist = sourceMusicFileInfo.Format.Tags["artist"],
                Comment = sourceMusicFileInfo.Format.Tags["comment"],
                Composer = sourceMusicFileInfo.Format.Tags["composer"],
                Copyright = sourceMusicFileInfo.Format.Tags["copyright"],
                Date = sourceMusicFileInfo.Format.Tags["date"],
                Disc = sourceMusicFileInfo.Format.Tags["disc"],
                Genre = sourceMusicFileInfo.Format.Tags["genre"],
                Lyricist = sourceMusicFileInfo.Format.Tags["TEXT"],
                Title = sourceMusicFileInfo.Format.Tags["title"],
                Track = sourceMusicFileInfo.Format.Tags["track"],
            };
        }

        public override IEnumerable<(string metadataName, string metadataValue)> EnumerateFormatMetadata(MusicFileMetadata metadata)
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            if (metadata.Album is not null)
                yield return ("album", metadata.Album);
            if (metadata.AlbumArtist is not null)
                yield return ("album_artist", metadata.AlbumArtist);
            if (metadata.Artist is not null)
                yield return ("artist", metadata.Artist);
#if false // mp3 では ffmpeg によって comment タグを正しく設定できないため、この行は無効化
            if (metadata.Comment is not null)
                yield return ("comment", metadata.Comment);
#endif
            if (metadata.Composer is not null)
                yield return ("composer", metadata.Composer);
            if (metadata.Copyright is not null)
                yield return ("copyright", metadata.Copyright);
            if (metadata.Date is not null)
                yield return ("date", metadata.Date);
            if (metadata.Disc is not null)
                yield return ("disc", metadata.Disc);
            if (metadata.Genre is not null)
                yield return ("genre", metadata.Genre);
#if false // mp3 では ffmpeg によって lyricist タグを正しく設定できないため、この行は無効化
            if (metadata.Lyricist is not null)
                yield return ("TEXT", metadata.Lyricist);
#endif
            if (metadata.Title is not null)
                yield return ("title", metadata.Title);
            if (metadata.Track is not null)
                yield return ("track", metadata.Track);
        }

        public override IEnumerable<(string metadataName, string metadataValue)> EnumerateStreamMetadata(MusicFileMetadata metadata)
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            yield break;
        }

        public override IEnumerable<(string metadataName, string metadataValue)> EnumerateKid3Metadata(MusicFileMetadata metadata)
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            if (metadata.Comment is not null)
                yield return ("comment", metadata.Comment);
            if (metadata.Lyricist is not null)
                yield return ("lyricist", metadata.Lyricist);
        }

        public override (string encoder, IEnumerable<string> encoderOptions) GuessDefaultEncoderSpec(AudioStreamInfo sourceAudioStream)
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            return _fileFormat switch
            {
                null => throw new InvalidOperationException(),
                "wav" => (MapPcmEncoder(sourceAudioStream.SampleFormat, sourceAudioStream.BitsPerRawSample), MapPcmEncoderOptions(sourceAudioStream.IndexWithinAudioStream, sourceAudioStream.BitsPerRawSample)),
                "mp3" => ("libmp3lame", [$"-compression_level:a 0 -q:a 0 -b:a {(128 * sourceAudioStream.Channels).Maximum(320)}k"]),
                _ => throw Validation.GetFailErrorException(),
            };
        }

        private static string MapPcmEncoder(AudioSampleFormat sampleFormat, int? bitsPerRawSample)
            => sampleFormat switch
            {
                AudioSampleFormat.U8 or AudioSampleFormat.U8P => "pcm_u8",
                AudioSampleFormat.S16 or AudioSampleFormat.S16P => "pcm_s16le",
                AudioSampleFormat.S32 or AudioSampleFormat.S32P => bitsPerRawSample is null or <= 24 ? "pcm_s24le" : "pcm_s32le",
                AudioSampleFormat.S64 or AudioSampleFormat.S64P => "pcm_s64le",
                AudioSampleFormat.FLT or AudioSampleFormat.FLTP => "pcm_f32le",
                AudioSampleFormat.DBL or AudioSampleFormat.DBLP => "pcm_f64le",
                _ => "pcm_f32le",
            };

        private static string[] MapPcmEncoderOptions(int index, int? bitsPerRawSample)
        {
            if (bitsPerRawSample is null)
                return [];
            else
                return [$"-bits_per_raw_sample:a:{index} {bitsPerRawSample.Value}"];
        }
    }
}
