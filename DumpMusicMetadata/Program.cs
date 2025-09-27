using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MatroskaBatchToolBox.Utility.Interprocess;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;
using Palmtree.IO;
using Palmtree.IO.Console;
using Palmtree.IO.Serialization;

namespace DumpMusicMetadata
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            ProcessUtility.SetupCurrentProcessPriority();

            if (TinyConsole.InputEncoding.CodePage != Encoding.UTF8.CodePage || TinyConsole.OutputEncoding.CodePage != Encoding.UTF8.CodePage)
            {
                if (OperatingSystem.IsWindows())
                    TinyConsole.WriteLog(LogCategory.Warning, "The encoding of standard input or output is not UTF8. Consider running the command \"chcp 65001\".");
                else
                    TinyConsole.WriteLog(LogCategory.Warning, "The encoding of standard input or standard output is not UTF8.");
            }

            if (TinyConsole.InputEncoding.CodePage != Encoding.UTF8.CodePage || TinyConsole.OutputEncoding.CodePage != Encoding.UTF8.CodePage)
                TinyConsole.WriteLog(LogCategory.Warning, "The encoding of standard input or standard output is not UTF8.");

            TinyConsole.DefaultTextWriter = ConsoleTextWriterType.StandardError;

            if (args.Length != 2)
            {
                TinyConsole.WriteLog(LogCategory.Error, "Invalid argument.");
                return 1;
            }

            var sourceFilePathName = args[0];
            var tagNameSpecs =
                args[1].Split(',')
                .Select(arg => (arg, tagName: arg.ToUpperInvariant()))
                .ToList();
            try
            {
                var sourceFile = GetFileInfo(sourceFilePathName);
                var musicInfo = GetMovieInformation(null, sourceFile);
                if (musicInfo.AudioStreams.Count() != 1)
                    throw new ApplicationException($"The music file has no or multiple audio streams.: {sourceFilePathName}");
                var tagValues = new List<string>();
                foreach (var (arg, tagName) in tagNameSpecs)
                {
                    var tagType =
                        tagName switch
                        {
                            "ALBUM" => MusicTagType.Album,
                            "ALBUM_ARTIST" => MusicTagType.AlbumArtist,
                            "ARTIST" => MusicTagType.Artist,
                            "COMMENT" => MusicTagType.Comment,
                            "COMPOSER" => MusicTagType.Composer,
                            "COPYRIGHT" => MusicTagType.Copyright,
                            "DATE" => MusicTagType.Date,
                            "DISC" => MusicTagType.Disc,
                            "GENRE" => MusicTagType.Genre,
                            "LYRICIST" => MusicTagType.Lyricist,
                            "TITLE" => MusicTagType.Title,
                            "TRACK" => MusicTagType.Track,
                            "FILE_NAME" => MusicTagType.FileName,
                            _ => throw new ApplicationException($"Not supported tag name.: \"{arg}\""),
                        };
                    tagValues.Add(GetTagValue(musicInfo, tagType));
                }

                CsvSerializer.Serialize(TinyConsole.Out, new[] { tagValues }, new CsvSerializerOption { ColumnDelimiterChar = '\t', RowDelimiterString = "\n" });
                return 0;
            }
            catch (Exception ex)
            {
                TinyConsole.WriteLine(ex);
                return 1;
            }
        }

        private static FilePath GetFileInfo(string sourceFilePathName)
        {
            try
            {
                var fileInfo = new FilePath(sourceFilePathName);
                if (!fileInfo.Exists)
                    throw new ApplicationException($"File does not exist.: \"{sourceFilePathName}\"");
                return fileInfo;
            }
            catch (IOException ex)
            {
                throw new ApplicationException($"Unable to access file.: \"{sourceFilePathName}\"", ex);
            }
        }

        private static MovieInformation GetMovieInformation(string? inputFormat, FilePath inputFile)
        {
            try
            {
                return
                    Command.GetMovieInformation(
                        inputFormat,
                        inputFile,
                        MovieInformationType.Chapters | MovieInformationType.Streams | MovieInformationType.Format,
                        (level, message) =>
                        {
                            if (level != LogCategory.Information)
                                TinyConsole.WriteLog("ffprobe", level, message);
                        });
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to get movie information.", ex);
            }
        }

        private static string GetTagValue(MovieInformation musicInfo, MusicTagType tagType)
        {
            if (tagType == MusicTagType.FileName)
                return musicInfo.Format.File.FullName;
            var metadataName =
                tagType switch
                {
                    MusicTagType.Album => "album",
                    MusicTagType.AlbumArtist => "album_artist",
                    MusicTagType.Artist => "artist",
                    MusicTagType.Comment => "comment",
                    MusicTagType.Composer => "composer",
                    MusicTagType.Copyright => "copyright",
                    MusicTagType.Date => "date",
                    MusicTagType.Disc => "disc",
                    MusicTagType.Genre => "genre",
                    MusicTagType.Lyricist => musicInfo.Format.FormatName is "mp3" or "wav" ? "text" : "lyricist",
                    MusicTagType.Title => "title",
                    MusicTagType.Track => "track",
                    _ => throw new ApplicationException($"Not supported {nameof(MusicTagType)} value.: {tagType}"),
                };
            return
                musicInfo.Format.FormatName switch
                {
                    "wav" or "mp3" or "flac" => musicInfo.Format.Tags[metadataName] ?? "",
                    "ogg" => musicInfo.AudioStreams.FirstOrDefault()?.Tags[metadataName] ?? "",
                    _ => throw new ApplicationException($"Not supported music file format.: \"{musicInfo.Format.FormatName}\""),
                };
        }
    }
}
