using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MatroskaBatchToolBox.Utility.Interprocess;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;
using Palmtree.Serialization;

namespace DumpMusicMetadata
{
    internal class Program
    {
        private static readonly string _thisProgramName;

        static Program()
            => _thisProgramName = Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location);

        private static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                PrintErrorMessage("Invalid argument.");
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
                    throw new Exception($"The music file has no or multiple audio streams.: {sourceFilePathName}");
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
                            _ => throw new Exception($"Not supported tag name.: \"{arg}\""),
                        };
                    tagValues.Add(GetTagValue(musicInfo, tagType));
                }

                CsvSerializer.Serialize(TinyConsole.Out, new[] { tagValues }, new CsvSerializerOption { ColumnDelimiterChar = '\t', RowDelimiterString = "\n" });
                return 0;
            }
            catch (Exception ex)
            {
                PrintExceptionMessage(ex);
                return 1;
            }
        }

        private static FileInfo GetFileInfo(string sourceFilePathName)
        {
            try
            {
                var fileInfo = new FileInfo(sourceFilePathName);
                if (!fileInfo.Exists)
                    throw new Exception($"File does not exist.: \"{sourceFilePathName}\"");
                return fileInfo;
            }
            catch (IOException ex)
            {
                throw new Exception($"Unable to access file.: \"{sourceFilePathName}\"", ex);
            }
        }

        private static MovieInformation GetMovieInformation(string? inputFormat, FileInfo inputFile)
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
                            switch (level)
                            {
                                case "WARNING":
                                    PrintWarningMessage("ffprobe", message);
                                    break;
                                case "ERROR":
                                    PrintErrorMessage("ffprobe", message);
                                    break;
                                default:
                                    break;
                            }
                        });
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get movie information.", ex);
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
                    _ => throw new Exception($"Not supported {nameof(MusicTagType)} value.: {tagType}"),
                };
            return
                musicInfo.Format.FormatName switch
                {
                    "wav" or "mp3" or "flac" => musicInfo.Format.Tags[metadataName] ?? "",
                    "ogg" => musicInfo.AudioStreams.FirstOrDefault()?.Tags[metadataName] ?? "",
                    _ => throw new Exception($"Not supported music file format.: \"{musicInfo.Format.FormatName}\""),
                };
        }

        private static void PrintExceptionMessage(Exception ex)
        {
            for (var exception = ex; exception != null; exception = exception.InnerException)
                PrintErrorMessage(exception.Message);
        }

        private static void PrintInformationMessage(string message)
            => PrintInformationMessage(_thisProgramName, message);

        private static void PrintInformationMessage(string programName, string message)
        {
            TinyConsole.ForegroundColor = ConsoleColor.Cyan;
            TinyConsole.Write($"{programName}:INFORMATION:");
            TinyConsole.ResetColor();
            TinyConsole.Write($" {message}");
            try
            {
                TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
            }
            catch (InvalidOperationException)
            {
            }

            TinyConsole.WriteLine();
        }

        private static void PrintWarningMessage(string message)
            => PrintWarningMessage(_thisProgramName, message);

        private static void PrintWarningMessage(string programName, string message)
        {
            TinyConsole.ForegroundColor = ConsoleColor.Yellow;
            TinyConsole.Write($"{programName}:WARNING: {message}");
            TinyConsole.ResetColor();
            try
            {
                TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
            }
            catch (InvalidOperationException)
            {
            }

            TinyConsole.WriteLine();
        }

        private static void PrintErrorMessage(string message)
            => PrintErrorMessage(_thisProgramName, message);

        private static void PrintErrorMessage(string programName, string message)
        {
            TinyConsole.ForegroundColor = ConsoleColor.Red;
            TinyConsole.Write($"{programName}:ERROR: {message}");
            TinyConsole.ResetColor();
            try
            {
                TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
            }
            catch (InvalidOperationException)
            {
            }

            TinyConsole.WriteLine();
        }
    }
}
