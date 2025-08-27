using System;
using System.Collections.Generic;
using System.Linq;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;
using Palmtree.Linq;

namespace AudioNormalizer
{
    internal sealed class OggMusicFileMetadataProvider
        : MusicFileMetadataProvider
    {
        private const int _bitratePerChannelForLibVorbis = 96000;
        private readonly TransferDirection _direction;
        private readonly string? estimatedCodec;
        private readonly string? _fileFormat;

        public OggMusicFileMetadataProvider(TransferDirection direction, string? fileFormat, string? fileExtension)
        {
            _direction = direction;
            if (fileFormat is not null)
            {
                if (fileExtension is not null)
                {
                    if (fileFormat == "ogg" && fileExtension == ".ogg")
                    {
                        estimatedCodec = "vorbis";
                        _fileFormat = "ogg";
                    }
                    else if (direction == TransferDirection.Output && fileFormat == "opus" && fileExtension == ".opus")
                    {
                        estimatedCodec = "opus";
                        _fileFormat = "opus";
                    }
                    else if (direction == TransferDirection.Input && fileFormat == "ogg" && fileExtension == ".opus")
                    {
                        estimatedCodec = "opus";
                        _fileFormat = "ogg";
                    }
                    else
                    {
                        estimatedCodec = null;
                        _fileFormat = null;
                    }
                }
                else
                {
                    if (fileFormat == "ogg")
                    {
                        estimatedCodec = "vorbis";
                        _fileFormat = "ogg";
                    }
                    else if (direction == TransferDirection.Output && fileFormat == "opus")
                    {
                        estimatedCodec = "opus";
                        _fileFormat = "opus";
                    }
                    else
                    {
                        estimatedCodec = null;
                        _fileFormat = null;
                    }
                }
            }
            else
            {
                if (fileExtension is not null)
                {
                    if (fileExtension == ".ogg")
                    {
                        estimatedCodec = "vorbis";
                        _fileFormat = "ogg";
                    }
                    else if (direction == TransferDirection.Output && fileExtension == ".opus")
                    {
                        estimatedCodec = "opus";
                        _fileFormat = "opus";
                    }
                    else if (direction == TransferDirection.Input && fileExtension == ".opus")
                    {
                        estimatedCodec = "opus";
                        _fileFormat = "ogg";
                    }
                    else
                    {
                        estimatedCodec = null;
                        _fileFormat = null;
                    }
                }
                else
                {
                    estimatedCodec = null;
                    _fileFormat = null;
                }
            }
        }

        public override bool Supported => estimatedCodec is not null && _fileFormat is not null;

        public override string DefaultExtension
            => _fileFormat is null
                ? throw new InvalidOperationException()
                : _fileFormat == "opus"
                ? ".opus"
                : ".ogg";

        public override string Format => _fileFormat ?? throw new InvalidOperationException();

        public override MusicFileMetadata GetMetadata(MovieInformation musicFileInfo)
        {
            if (_direction != TransferDirection.Input)
                throw new InvalidOperationException();

            Validation.Assert(musicFileInfo.AudioStreams.Any());
            Validation.Assert(musicFileInfo.AudioStreams.Skip(1).None());

            var sourceStream = musicFileInfo.AudioStreams.First();

            return
                new MusicFileMetadata
                {
                    Album = sourceStream.Tags["album"],
                    AlbumArtist = sourceStream.Tags["album_artist"],
                    Artist = sourceStream.Tags["artist"],
                    Comment = sourceStream.Tags["comment"],
                    Composer = sourceStream.Tags["composer"],
                    Copyright = sourceStream.Tags["copyright"],
                    Date = sourceStream.Tags["date"],
                    Disc = sourceStream.Tags["disc"],
                    Genre = sourceStream.Tags["genre"],
                    Lyricist = sourceStream.Tags["lyricist"],
                    Title = sourceStream.Tags["title"],
                    Track = sourceStream.Tags["track"],
                };
        }

        public override IEnumerable<(string metadataName, string metadataValue)> EnumerateFormatMetadata(MusicFileMetadata metadata)
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            yield break;
        }

        public override IEnumerable<(string metadataName, string metadataValue)> EnumerateStreamMetadata(MusicFileMetadata metadata)
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            if (metadata.Album is not null)
                yield return ("album", metadata.Album);
            if (metadata.AlbumArtist is not null)
                yield return ("album_artist", metadata.AlbumArtist);
            if (metadata.Artist is not null)
                yield return ("artist", metadata.Artist);
            if (metadata.Comment is not null)
                yield return ("comment", metadata.Comment);
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
            if (metadata.Lyricist is not null)
                yield return ("lyricist", metadata.Lyricist);
            if (metadata.Title is not null)
                yield return ("title", metadata.Title);
            if (metadata.Track is not null)
                yield return ("track", metadata.Track);
        }

        public override (string encoder, IEnumerable<string> encoderOptions) GuessDefaultEncoderSpec(AudioStreamInfo sourceAudioStream)
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            return
                estimatedCodec switch
                {
                    null => throw new InvalidOperationException(),
                    "opus" => GetDefaultOpusEncoderSpec(sourceAudioStream),
                    "vorbis" => GetDefaultVorbisEncoderSpec(sourceAudioStream),
                    _ => throw Validation.GetFailErrorException(),
                };

            static (string encoder, IEnumerable<string> encoderOptions) GetDefaultOpusEncoderSpec(AudioStreamInfo sourceStreamInfo)
            {
                return
                    sourceStreamInfo.ChannelLayout is null || sourceStreamInfo.ChannelLayout.IsNoneOf("5.0(side)", "5.1(side)")
                    ? ("libopus", new[] { $"-b:a:{sourceStreamInfo.IndexWithinAudioStream} {CalculateBitRateForOpus(sourceStreamInfo.Channels) / 1000:F0}k" }.Append(MapLibopusSampleFormatOptions(sourceStreamInfo.IndexWithinAudioStream, sourceStreamInfo.SampleFormat)))
                    : throw new NotSupportedException($"The input music file cannot be converted to opus format. Because opus does not support channel layout \"{sourceStreamInfo.ChannelLayout}\".");
            }

            static (string encoder, IEnumerable<string> encoderOptions) GetDefaultVorbisEncoderSpec(AudioStreamInfo sourceStreamInfo)
            {
                return ("libvorbis", new[] { $"-q:a:{sourceStreamInfo.IndexWithinAudioStream} {CalculateLibVorbisQualityByBitRate(sourceStreamInfo.Channels * _bitratePerChannelForLibVorbis):F1}" }.Append(MapLibvorbisSampleFormatOptions(sourceStreamInfo.IndexWithinAudioStream, sourceStreamInfo.SampleFormat)));
            }
        }

        private static string MapLibopusSampleFormatOptions(int index, AudioSampleFormat sampleFormat)
            => sampleFormat switch
            {
                AudioSampleFormat.U8 or AudioSampleFormat.U8P or AudioSampleFormat.S16 or AudioSampleFormat.S16P => $"-sample_fmt:a:{index} s16",
                _ => $"-sample_fmt:a:{index} flt",
            };

        private static string MapLibvorbisSampleFormatOptions(int index, AudioSampleFormat sampleFormat)
            => sampleFormat switch
            {
                _ => $"-sample_fmt:a:{index} fltp",
            };

        private static double CalculateLibVorbisQualityByBitRate(int bitRate)
        {
            // -1 <= q <= 4 : 16k * (q + 4) bps  (48k <= bps <= 128k)
            // 4 < q < 8    : 32k * q bps        (128k < bps < 256k)    
            // 8 <= q <= 10 : 64k * (q - 4) bps  (256k <= bps <= 384k)

            if (bitRate <= 48000)
                return -1;
            else if (bitRate <= 128000)
                return (double)bitRate / 16000 - 4;
            else if (bitRate <= 256000)
                return (double)bitRate / 32000;
            else if (bitRate <= 384000)
                return (double)bitRate / 64000 + 4;
            else
                return 10.0;
        }

        private static double CalculateBitRateForOpus(int channels)
        {
            if (channels <= 1)
                return 96000;
            else if (channels <= 2)
                return 128000;
            else if (channels <= 5)
                return 192000;
            else if (channels <= 7)
                return 256000;
            else
                return 450000;
        }
    }
}
