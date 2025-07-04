﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MatroskaBatchToolBox.Utility;
using MatroskaBatchToolBox.Utility.Interprocess;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;
using Palmtree.IO;
using Palmtree.IO.Console;
using Palmtree.Numerics;

namespace LyricsChecker
{
    internal static partial class Program
    {
        private const string _metadataNameAlbum = "Album";
        private const string _metadataMameAlbumArtist = "album_artist";
        private const string _metadataNameArtist = "artist";
        private const string _metadataNameDate = "date";
        private const string _metadataNameLyricist = "lyricist";
        private const string _metadataNameText = "text";
        private const string _metadataNameTitle = "title";
        private const string _metadataNameTrack = "track";

        private static int Main(string[] args)
        {
            if (TinyConsole.InputEncoding.CodePage != Encoding.UTF8.CodePage || TinyConsole.OutputEncoding.CodePage != Encoding.UTF8.CodePage)
            {
                if (OperatingSystem.IsWindows())
                    TinyConsole.WriteLog(LogCategory.Warning, "The encoding of standard input or output is not UTF8. Consider running the command \"chcp 65001\".");
                else
                    TinyConsole.WriteLog(LogCategory.Warning, "The encoding of standard input or standard output is not UTF8.");
            }

            // このプロセスでは Ctrl+C を無視する。
            TinyConsole.CancelKeyPress += (sender, e) => e.Cancel = true;

            // コマンドの入出力エンコーディングを UTF8 にする
            TinyConsole.InputEncoding = Encoding.UTF8;
            TinyConsole.OutputEncoding = Encoding.UTF8;
            TinyConsole.DefaultTextWriter = ConsoleTextWriterType.StandardError;

            var options = new List<string>();
            var lyricsFilePath = (string?)null;
            var musicFilePath = (string?)null;
            var doHelp = (bool?)null;
            var doModify = (bool?)null;
            var index = 0;
            for (; index < args.Length; ++index)
            {
                if (args[index].StartsWith('-'))
                    options.Add(args[index]);
                else
                    break;
            }

            if (index < args.Length)
                musicFilePath = args[index++];
            if (index < args.Length)
                lyricsFilePath = args[index++];
            if (index < args.Length)
            {
                TinyConsole.WriteLog(LogCategory.Error, "Only one lyrics file and one music file can be specified.");
                return 1;
            }

            foreach (var option in options)
            {
                if (option == "-help")
                {
                    if (doHelp is not null)
                    {
                        TinyConsole.WriteLog(LogCategory.Error, "Duplicate \"-help\" option specified.");
                        return 1;
                    }

                    doHelp = true;
                }
                else if (option is "-m" or "--modify_lyrics")
                {
                    if (doModify is not null)
                    {
                        TinyConsole.WriteLog(LogCategory.Error, "Duplicate \"--modify_lyrics\" or \"-m\" option specified.");
                        return 1;
                    }

                    doModify = true;
                }
                else
                {
                    TinyConsole.WriteLog(LogCategory.Error, $"An invalid option is specified.: \"{option}\"");
                    return 1;
                }
            }

            if (doHelp == true && doModify == true)
            {
                TinyConsole.WriteLog(LogCategory.Error, "\"-help\" option and \"--modify_lyrics\" option cannot be specified at the same time.");
                return 1;
            }

            if (musicFilePath is null)
            {
                TinyConsole.WriteLog(LogCategory.Error, "No music file specified.");
                return 1;
            }

            if (doHelp == true)
            {
                PrintHelpMessage();
                return 0;
            }

            try
            {
                if (lyricsFilePath is null)
                {
                    var result = CheckMusicFile(musicFilePath);
                    return result ? 0 : 1;
                }
                else if (doModify == true)
                {
                    _ = ModifyLyricsFile(musicFilePath, lyricsFilePath);
                    return 0;
                }
                else
                {
                    var result = CheckLyricsFile(musicFilePath, lyricsFilePath);
                    return result ? 0 : 1;
                }
            }
            catch (Exception ex)
            {
                TinyConsole.WriteLog(ex);
                return 1;
            }
        }

        private static bool CheckMusicFile(string musicFilePath)
        {
            var musicFile = new FilePath(musicFilePath);
            if (!musicFile.Exists)
            {
                TinyConsole.Out.WriteLine($"The specified music file does not exist.: \"{musicFile.FullName}\"");
                return false;
            }

            var musicFileInfo =
                Command.GetMovieInformation(
                    null,
                    musicFile,
                    MovieInformationType.Format | MovieInformationType.Streams,
                    (level, message) =>
                    {
                        if (level != LogCategory.Information)
                            TinyConsole.WriteLog(level, message);
                    });
            var ok = true;
            ok = CheckFileNameStrictly(musicFileInfo, musicFile) && ok;
            ok = CheckTag("artist name", "ar", () => GetTagValue(musicFileInfo, _metadataNameArtist), musicFile) && ok;
            ok = CheckTag("album name", "al", () => GetTagValue(musicFileInfo, _metadataNameAlbum), musicFile) && ok;
            ok = CheckTag("song title", "ti", () => GetTagValue(musicFileInfo, _metadataNameTitle), musicFile) && ok;
            ok = CheckTag("lyricist name", "au", () => GetLyricistFromMusicFile(musicFileInfo), musicFile) && ok;
            ok = CheckTag("song length", "length", () => musicFileInfo.Format.Duration, musicFile) && ok;

            return ok;
        }

        private static bool CheckLyricsFile(string musicFilePath, string lyricsFilePath)
        {
            var lyricsFile = new FilePath(lyricsFilePath);
            if (!lyricsFile.Exists)
            {
                TinyConsole.Out.WriteLine($"The specified lyrics file does not exist.: \"{lyricsFile.FullName}\"");
                return false;
            }

            var musicFile = new FilePath(musicFilePath);
            if (!musicFile.Exists)
            {
                TinyConsole.Out.WriteLine($"The specified music file does not exist.: \"{musicFile.FullName}\"");
                return false;
            }

            var musicFileInfo =
                Command.GetMovieInformation(
                    null,
                    musicFile,
                    MovieInformationType.Format | MovieInformationType.Streams,
                    (level, message) =>
                    {
                        if (level != LogCategory.Information)
                            TinyConsole.WriteLog(level, message);
                    });
            try
            {
                var lyricsData = ReadLyricsFile(lyricsFile);
                var ok = true;
                ok = CheckFileNameStrictly(musicFileInfo, musicFile, lyricsFile) && ok;
                ok = CheclLyricsFile(lyricsFile) && ok;
                ok = CheckTag("artist name", "ar", () => GetTagValue(musicFileInfo, _metadataNameArtist), lyricsData.Tags, lyricsFile) && ok;
                ok = CheckTag("album name", "al", () => GetTagValue(musicFileInfo, _metadataNameAlbum), lyricsData.Tags, lyricsFile) && ok;
                ok = CheckTag("song title", "ti", () => GetTagValue(musicFileInfo, _metadataNameTitle), lyricsData.Tags, lyricsFile) && ok;
                ok = CheckTag("lyricist name", "au", () => GetLyricistFromMusicFile(musicFileInfo), lyricsData.Tags, lyricsFile) && ok;
                ok = CheckTag("song length", "length", () => musicFileInfo.Format.Duration, lyricsData.Tags, lyricsFile) && ok;

                var previousLyricsText = "<dummy>";
                foreach (var lyricsText in lyricsData.LyricsTexts)
                {
                    if (string.IsNullOrEmpty(lyricsText))
                    {
                        TinyConsole.Out.WriteLine($"Contains empty lines.: \"{lyricsFile.FullName}\"");
                        if (string.IsNullOrEmpty(previousLyricsText))
                            TinyConsole.Out.WriteLine($"Consecutive lines with no lyrics found.: \"{lyricsFile.FullName}\"");
                        previousLyricsText = "";
                        ok = false;
                    }
                    else
                    {
                        var (timeStamp, text) = NormalizeLyricsText(lyricsText);
                        var normalizedLyricsText = $"{timeStamp}{text}";
                        if (normalizedLyricsText != lyricsText)
                        {
                            TinyConsole.Out.WriteLine($"Inappropriate lyrics timeline.: Current: \"{lyricsText}\", Desired: \"{normalizedLyricsText}\", \"{lyricsFile.FullName}\"");
                            ok = false;
                        }

                        if (string.IsNullOrEmpty(previousLyricsText) && string.IsNullOrEmpty(text.Trim()))
                        {
                            TinyConsole.Out.WriteLine($"Consecutive lines with no lyrics found.: \"{lyricsFile.FullName}\"");
                            ok = false;
                        }

                        previousLyricsText = text.Trim();
                    }
                }

                return ok;
            }
            catch (DecoderFallbackException)
            {
                TinyConsole.Out.WriteLine($"The lyrics file contains characters that cannot be decoded by UTF-8.: \"{lyricsFile.FullName}\"");
                return false;
            }
        }

        private static bool ModifyLyricsFile(string musicFilePath, string lyricsFilePath)
        {
            var lyricsFile = new FilePath(lyricsFilePath);
            var musicFile = new FilePath(musicFilePath);
            if (!musicFile.Exists)
            {
                TinyConsole.Out.WriteLine($"The specified music file does not exist.: \"{musicFile.FullName}\"");
                return false;
            }

            var musicFileInfo =
                Command.GetMovieInformation(
                    null,
                    musicFile,
                    MovieInformationType.Format | MovieInformationType.Streams,
                    (level, message) =>
                    {
                        if (level != LogCategory.Information)
                            TinyConsole.WriteLog(level, message);
                    });
            _ = CheckFileNameStrictly(musicFileInfo, musicFile, lyricsFile);
            var modified = false;
            modified = ModifyLyricsFile(lyricsFile) || modified;
            var lyricsData = lyricsFile.Exists ? ReadLyricsFile(lyricsFile) : new LyricsContainer();
            modified = ModifyTag("artist name", "ar", () => GetTagValue(musicFileInfo, _metadataNameArtist), lyricsData.Tags, lyricsFile) || modified;
            modified = ModifyTag("album name", "al", () => GetTagValue(musicFileInfo, _metadataNameAlbum), lyricsData.Tags, lyricsFile) || modified;
            modified = ModifyTag("song title", "ti", () => GetTagValue(musicFileInfo, _metadataNameTitle), lyricsData.Tags, lyricsFile) || modified;
            modified = ModifyTag("lyricist name", "au", () => GetLyricistFromMusicFile(musicFileInfo), lyricsData.Tags, lyricsFile) || modified;
            modified = ModifyTag("song length", "length", () => musicFileInfo.Format.Duration, lyricsData.Tags, lyricsFile) || modified;
            var newLyricsTexts = new List<string>();
            var modifiedLyricsTimeStamp = false;
            foreach (var lyricsText in lyricsData.LyricsTexts)
            {
                if (string.IsNullOrEmpty(lyricsText))
                {
                    newLyricsTexts.Add("[00:00.00] ");
                    modified = true;
                    modifiedLyricsTimeStamp = true;
                }
                else
                {
                    var (timeStamp, text) = NormalizeLyricsText(lyricsText);
                    if (string.IsNullOrEmpty(timeStamp) && !text.StartsWith('['))
                    {
                        timeStamp = "[00:00.00]";
                        modified = true;
                        modifiedLyricsTimeStamp = true;
                    }

                    var normalizedLyricsText = $"{timeStamp}{text}";
                    newLyricsTexts.Add(normalizedLyricsText);
                    if (normalizedLyricsText != lyricsText)
                    {
                        TinyConsole.Out.WriteLine($"Replace inappropriate lyric text. : Current: \"{lyricsText}\", New: \"{normalizedLyricsText}\", \"{lyricsFile.FullName}\"");
                        modified = true;
                    }
                }
            }

            if (newLyricsTexts.Count > 0 && !newLyricsTexts.First().StartsWith("[00:00.00]", StringComparison.Ordinal))
            {
                // 最初の歌詞行がタイムスタンプ [00:00.00] で始まっていない場合

                // ダミーのタイムスタンプ [00:00.00] を先頭に挿入する。
                newLyricsTexts.Insert(0, "[00:00.00] ");
                TinyConsole.Out.WriteLine($"Inserts \"[00:00.00] \" because the timestamp of the first lyric is not \"[00:00.00]\".: \"{lyricsFile.FullName}\"");
                modified = true;
                modifiedLyricsTimeStamp = true;
            }

            if (modifiedLyricsTimeStamp && !newLyricsTexts.Any(newLyricsText => newLyricsText.Contains("#要タイミング調整")))
            {
                newLyricsTexts.Add("[00:00.00]#要タイミング調整");
                modified = true;
            }

            lyricsData.LyricsTexts.Clear();
            foreach (var lyricsText in newLyricsTexts)
                lyricsData.LyricsTexts.Add(lyricsText);
            if (modified)
                SaveNewLyricsFile(lyricsFile, lyricsData);
            return modified;
        }

        private static void SaveNewLyricsFile(FilePath lyricsFile, LyricsContainer lyricsData)
        {
            var tempFilePath = Path.GetTempFileName();
            try
            {
                WriteLyricsFile(lyricsData, tempFilePath);
                var backupFilePath = "";
                if (File.Exists(lyricsFile.FullName))
                {
                    for (var index = 1; ; ++index)
                    {
                        backupFilePath = index == 1 ? $"{lyricsFile.FullName}.backup" : $"{lyricsFile.FullName}.{index}.backup";
                        if (!File.Exists(backupFilePath))
                        {
                            var SuccessfulBackup = false;
                            try
                            {
                                File.Move(lyricsFile.FullName, backupFilePath);
                                SuccessfulBackup = true;
                            }
                            catch (Exception)
                            {
                            }

                            if (SuccessfulBackup)
                                break;
                        }
                    }
                }

                Validation.Assert(!File.Exists(lyricsFile.FullName));
                var successfullRenaming = false;
                try
                {
                    File.Move(tempFilePath, lyricsFile.FullName);
                    successfullRenaming = true;
                }
                finally
                {
                    if (!successfullRenaming)
                        File.Move(backupFilePath, lyricsFile.FullName);
                }
            }
            finally
            {
                try
                {
                    if (File.Exists(tempFilePath))
                        File.Delete(tempFilePath);
                }
                catch (Exception)
                {
                }
            }
        }

        private static bool CheckFileNameStrictly(MovieInformation musicFileInfo, FilePath musicFile)
        {
            var ok = true;
            var musicTracktext = GetTagValue(musicFileInfo, _metadataNameTrack);
            var musicTitleText = GetTagValue(musicFileInfo, _metadataNameTitle);
            if (musicTracktext is not null && musicTitleText is not null && musicTracktext.TryParse(out int track))
            {
                var extension = GetMusicFileExtension(musicFileInfo);
                var expectedMusicFileName = $"{track:D2} {Mp3TagEncode(musicTitleText.Trim())}{extension}";
                ok = CheckMusicFilePath(musicFileInfo, musicFile) && ok;
                ok = CheckFileName(musicFile, expectedMusicFileName) && ok;
            }

            return ok;
        }

        private static bool CheckFileNameStrictly(MovieInformation musicFileInfo, FilePath musicFile, FilePath lyricsFile)
        {
            var ok = true;
            var musicTracktext = GetTagValue(musicFileInfo, _metadataNameTrack);
            var musicTitleText = GetTagValue(musicFileInfo, _metadataNameTitle);
            if (musicTracktext is not null && musicTitleText is not null && musicTracktext.TryParse(out int track))
            {
                var extension = GetMusicFileExtension(musicFileInfo);
                var expectedMusicFileName = $"{track:D2} {Mp3TagEncode(musicTitleText.Trim())}{extension}";
                ok = CheckMusicFilePath(musicFileInfo, musicFile) && ok;
                ok = CheckFileName(musicFile, expectedMusicFileName) && ok;
                ok = CheckFileName(lyricsFile, expectedMusicFileName) && ok;
            }

            return ok;
        }

        private static bool CheckMusicFilePath(MovieInformation musicFileInfo, FilePath musicFile)
        {
            var albumArtist = GetTagValue(musicFileInfo, _metadataMameAlbumArtist);
            var album = GetTagValue(musicFileInfo, _metadataNameAlbum);
            var date = GetTagValue(musicFileInfo, _metadataNameDate);
            if (albumArtist is null || album is null || date is null)
                return true;

            var albumDirectory = musicFile.Directory;
            if (albumDirectory is null)
                return true;
            var albumArtistDirectory = albumDirectory.Parent;
            if (albumArtistDirectory is null)
                return true;
            var baseDirectory = albumArtistDirectory.Parent;
            if (baseDirectory is null)
                return true;

            var actualAlbumArtistDirectory = baseDirectory.GetSubDirectory(albumArtistDirectory.Name);
            Validation.Assert(actualAlbumArtistDirectory.Exists);

            var actualAlbumDirectory = albumArtistDirectory.GetSubDirectory(albumDirectory.Name);
            Validation.Assert(actualAlbumDirectory.Exists);

            var desiredAlbumArtistDirectoryName = albumArtist.WindowsFileNameEncoding();

            // いくつかのプレイヤーソフトでは "." で始まるディレクトリ/ファイル名を無視するため、置き換える。
            if (desiredAlbumArtistDirectoryName.StartsWith('.'))
                desiredAlbumArtistDirectoryName = $"．{desiredAlbumArtistDirectoryName[1..]}";

            var desiredAlbumDirectoryName = $"{album} - {date}年".WindowsFileNameEncoding();

            // いくつかのプレイヤーソフトでは "." で始まるディレクトリ/ファイル名を無視するため、置き換える。
            if (desiredAlbumDirectoryName.StartsWith('.'))
                desiredAlbumDirectoryName = $"．{desiredAlbumDirectoryName[1..]}";

            if (actualAlbumArtistDirectory.Name != desiredAlbumArtistDirectoryName ||
                actualAlbumDirectory.Name != desiredAlbumDirectoryName)
            {
                TinyConsole.Out.WriteLine($"Place the music file under the directory \"{Path.Combine(baseDirectory.FullName, desiredAlbumArtistDirectoryName, desiredAlbumDirectoryName)}\".: \"{musicFile.FullName}\"");
                return false;
            }

            return true;
        }

        private static bool CheclLyricsFile(FilePath lyricsFile)
        {
            var ok = true;
            var text = File.ReadAllText(lyricsFile.FullName);
            if (text.Contains('\r'))
            {
                var currentNewLineType = text.Contains('\n') ? "CR+LF" : "CR";
                TinyConsole.Out.WriteLine($"Line breaks in lyrics files should be \"LF\" for compatibility. The current line feed code is \"{currentNewLineType}\".: \"{lyricsFile.FullName}\"");
                ok = false;
            }

            return ok;
        }

        private static bool ModifyLyricsFile(FilePath lyricsFile)
        {
            var modified = false;
            var text = File.ReadAllText(lyricsFile.FullName);
            if (text.Contains('\r'))
            {
                var currentNewLineType = text.Contains('\n') ? "CR+LF" : "CR";
                TinyConsole.Out.WriteLine($"The newline code of the current lyrics file is \"{currentNewLineType}\", so it will be corrected to \"LF\".: \"{lyricsFile.FullName}\"");
                modified = true;
            }

            return modified;
        }

        private static LyricsContainer ReadLyricsFile(FilePath lyricsFile)
        {
            var lyricsData = new LyricsContainer();
            using var lyricsReader = new StreamReader(lyricsFile.FullName, new UTF8Encoding(false, true));
            while (true)
            {
                var lineText = lyricsReader.ReadLine();
                if (lineText is null)
                    break;
                var tagMatch = GetLyricsFileTagPattern().Match(lineText);
                if (tagMatch.Success)
                {
                    var tagName = tagMatch.Groups["name"].Value;
                    var tagValue = tagMatch.Groups["value"].Value.Trim();
                    if (!lyricsData.Tags.TryAdd(tagName, tagValue))
                        TinyConsole.Out.WriteLine($"Duplicate \"{tagName}\" tag. : \"{lyricsFile.FullName}\"");
                }
                else
                {
                    lyricsData.LyricsTexts.Add(lineText);
                }
            }

            return lyricsData;
        }

        private static (string timeStamp, string text) NormalizeLyricsText(string lyricsLineText)
        {
            var lyricsTextMatch = GetLyricsTextPattern().Match(lyricsLineText);
            if (!lyricsTextMatch.Success)
                return ("", lyricsLineText.Trim());

            var lyricsTime = lyricsTextMatch.Groups["lyricsTime"].Value;

            // 歌詞テキストの前後に誤って空白を挿入してしまうことがよくあるので、前後の空白を除去する
            var lyricsText = lyricsTextMatch.Groups["lyricsText"].Value.Trim();

            // 一部の lrc ファイルエディタが空の歌詞を受け入れないため、タイムライン上の歌詞が空であっても1文字の空白文字に置換する
            if (lyricsText.Length <= 0)
                lyricsText = " ";

            // タイムラインの時刻表記を正規化する。
            var reformattedTime =
                lyricsTime.ParseAsTimeSpan(TimeParsingMode.LazyMode)
                .FormatTime(TimeFormatType.ShortFormat, 2);

            return ($"[{reformattedTime}]", lyricsText);
        }

        private static void WriteLyricsFile(LyricsContainer lyricsData, string tempFilePath)
        {
            using var lyricsWriter = new StreamWriter(tempFilePath, false, new UTF8Encoding(false));
            var tags = new Dictionary<string, string>(lyricsData.Tags);
            lyricsWriter.NewLine = "\n";
            var isFirstLine = true;
            if (tags.TryGetValue("ti", out var songTitle))
            {
                if (!isFirstLine)
                    lyricsWriter.WriteLine();
                lyricsWriter.Write($"[ti:{songTitle}]");
                isFirstLine = false;
                _ = tags.Remove("ti");
            }

            if (tags.TryGetValue("ar", out var artistName))
            {
                if (!isFirstLine)
                    lyricsWriter.WriteLine();
                lyricsWriter.Write($"[ar:{artistName}]");
                isFirstLine = false;
                _ = tags.Remove("ar");
            }

            if (tags.TryGetValue("al", out var albumName))
            {
                if (!isFirstLine)
                    lyricsWriter.WriteLine();
                lyricsWriter.Write($"[al:{albumName}]");
                isFirstLine = false;
                _ = tags.Remove("al");
            }

            if (tags.TryGetValue("au", out var lyricistName))
            {
                if (!isFirstLine)
                    lyricsWriter.WriteLine();
                lyricsWriter.Write($"[au:{lyricistName}]");
                isFirstLine = false;
                _ = tags.Remove("au");
            }

            if (tags.TryGetValue("length", out var musicLength))
            {
                if (!isFirstLine)
                    lyricsWriter.WriteLine();
                lyricsWriter.Write($"[length:{musicLength}]");
                isFirstLine = false;
                _ = tags.Remove("length");
            }

            foreach (var tagEntry in tags)
            {
                if (!isFirstLine)
                    lyricsWriter.WriteLine();
                lyricsWriter.Write($"[{tagEntry.Key}:{tagEntry.Value}]");
                isFirstLine = false;
            }

            foreach (var lyricsText in lyricsData.LyricsTexts)
            {
                if (!isFirstLine)
                    lyricsWriter.WriteLine();
                lyricsWriter.Write(lyricsText);
                isFirstLine = false;
            }
        }

        // 将来の拡張性のために残してある未使用のパラメタへのコンパイラの警告を抑止する。
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:未使用のパラメーターを削除します", Justification = "<保留中>")]
        private static bool CheckTag(string lyricsFileFriendlyTagName, string lyricsFileTagName, Func<string?> musicTagValueGetter, FilePath musicFile)
        {
            var ok = true;
            var musicTagValue = musicTagValueGetter();
            if (musicTagValue is not null && musicTagValue.Trim() != musicTagValue)
            {
                TinyConsole.Out.WriteLine($"The music file contains blank characters at the beginning or end of the {lyricsFileFriendlyTagName} tag.: \"{musicTagValue}\", \"{musicFile.FullName}\"");
                ok = false;
            }

            return ok;
        }

        private static bool CheckTag(string lyricsFileFriendlyTagName, string lyricsFileTagName, Func<string?> musicTagValueGetter, IDictionary<string, string> lyricsTags, FilePath lyricsFile)
        {
            var ok = true;
            var musicTagValue = musicTagValueGetter();
            if (musicTagValue is not null && musicTagValue.Trim() != musicTagValue)
            {
                TinyConsole.Out.WriteLine($"The music file contains blank characters at the beginning or end of the {lyricsFileFriendlyTagName} tag.: \"{musicTagValue}\", \"{lyricsFile.FullName}\"");
                ok = false;
            }

            var lyricsTagValue = lyricsTags.TryGetValue(lyricsFileTagName, out var tagValue) ? tagValue : null;
            if (musicTagValue != lyricsTagValue)
            {
                TinyConsole.Out.WriteLine($"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lyricsFileFriendlyTagName)} does not match.: {(lyricsTagValue is not null ? $"\"{lyricsTagValue}\"" : "(none)")} for lyrics, {(musicTagValue is not null ? $"\"{musicTagValue}\"" : "(none)")} for music, \"{lyricsFile.FullName}\"");
                ok = false;
            }

            return ok;
        }

        // 将来の拡張性のために残してある未使用のパラメタへのコンパイラの警告を抑止する。
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:未使用のパラメーターを削除します", Justification = "<保留中>")]
        private static bool CheckTag(string lyricsFileFriendlyTagName, string lyricsFileTagName, Func<TimeSpan?> musicTagValueGetter, FilePath musicFile) => true;

        private static bool CheckTag(string lyricsFileFriendlyTagName, string lyricsFileTagName, Func<TimeSpan?> musicTagValueGetter, IDictionary<string, string> lyricsTags, FilePath lyricsFile)
        {
            var musicTagValue = musicTagValueGetter();
            var lyricsTagValue = lyricsTags.TryGetValue(lyricsFileTagName, out var tagValue) ? tagValue : null;
            if (musicTagValue is null)
            {

                if (lyricsTagValue is null)
                {
                    return true;
                }
                else
                {
                    TinyConsole.Out.WriteLine($"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lyricsFileFriendlyTagName)} does not match.: \"{lyricsTagValue}\" for lyrics, (none) for music, \"{lyricsFile.FullName}\"");
                    return false;
                }
            }
            else
            {
                if (lyricsTagValue is null)
                {
                    TinyConsole.Out.WriteLine($"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lyricsFileFriendlyTagName)} does not match.: (none) for lyrics, \"{musicTagValue.Value.FormatTime(TimeFormatType.ShortFormat, 2)}\" for music, \"{lyricsFile.FullName}\"");
                    return false;
                }
                else
                {
                    if (!lyricsTagValue.TryParse(TimeParsingMode.StrictForShortTimeFormat, out var durationForLyrics))
                    {
                        TinyConsole.Out.WriteLine($"Invalid length tag format in lyrics file.: \"{lyricsTagValue}\", \"{lyricsFile.FullName}\"");
                        return false;
                    }

                    var difference = musicTagValue.Value - durationForLyrics;
                    if (difference < TimeSpan.Zero)
                        difference = -difference;
                    if (difference >= TimeSpan.FromMilliseconds(10))
                    {
                        TinyConsole.Out.WriteLine($"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lyricsFileFriendlyTagName)} does not match.: \"{lyricsTagValue}\" for lyrics, \"{musicTagValue.Value.FormatTime(TimeFormatType.ShortFormat, 2)}\" for music, \"{lyricsFile.FullName}\"");
                        return false;
                    }

                    return true;
                }
            }
        }

        private static bool ModifyTag(string lyricsFileFriendlyTagName, string lyricsFileTagName, Func<string?> musicTagValueGetter, IDictionary<string, string> lyricsTags, FilePath lyricsFile)
        {
            var musicTagValue = musicTagValueGetter();
            if (lyricsTags.TryGetValue(lyricsFileTagName, out var lyricsTagValue))
            {
                if (musicTagValue is not null)
                {
                    if (lyricsTagValue == musicTagValue)
                    {
                        return false;
                    }
                    else
                    {
                        TinyConsole.Out.WriteLine($"Replace the {lyricsFileFriendlyTagName} in the lyrics file as it does not match the music file.: \"{lyricsTagValue}\" for lyrics, \"{musicTagValue}\" for music, \"{lyricsFile.FullName}\"");
                        lyricsTags[lyricsFileTagName] = musicTagValue;
                        return true;
                    }
                }
                else
                {
                    TinyConsole.Out.WriteLine($"Replace the {lyricsFileFriendlyTagName} in the lyrics file as it does not match the music file.: \"{lyricsTagValue}\" for lyrics, (none) for music, \"{lyricsFile.FullName}\"");
                    _ = lyricsTags.Remove(lyricsFileTagName);
                    return true;
                }
            }
            else
            {
                if (musicTagValue is not null)
                {
                    TinyConsole.Out.WriteLine($"Replace the {lyricsFileFriendlyTagName} in the lyrics file as it does not match the music file.: (none) for lyrics, \"{musicTagValue}\" for music, \"{lyricsFile.FullName}\"");
                    lyricsTags.Add(lyricsFileTagName, musicTagValue);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private static bool ModifyTag(string lyricsFileFriendlyTagName, string lyricsFileTagName, Func<TimeSpan?> musicTagValueGetter, IDictionary<string, string> lyricsTags, FilePath lyricsFile)
        {
            var musicTagValue = musicTagValueGetter();
            if (lyricsTags.TryGetValue(lyricsFileTagName, out var lyricsTagValue))
            {
                if (musicTagValue is not null)
                {
                    if (!lyricsTagValue.TryParse(TimeParsingMode.StrictForShortTimeFormat, out var durationForLyrics))
                    {
                        TinyConsole.Out.WriteLine($"Invalid length tag format in lyrics file.: \"{lyricsTagValue}\", \"{lyricsFile.FullName}\"");
                        return false;
                    }

                    var difference = musicTagValue.Value - durationForLyrics;
                    if (difference < TimeSpan.Zero)
                        difference = -difference;
                    if (difference >= TimeSpan.FromMilliseconds(10))
                    {
                        lyricsTags[lyricsFileTagName] = musicTagValue.Value.FormatTime(TimeFormatType.ShortFormat, 2);
                        TinyConsole.Out.WriteLine($"Replace the {lyricsFileFriendlyTagName} in the lyrics file as it does not match the music file.: \"{lyricsTagValue}\" for lyrics, \"{musicTagValue.Value.FormatTime(TimeFormatType.ShortFormat, 2)}\" for music, \"{lyricsFile.FullName}\"");
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    _ = lyricsTags.Remove(lyricsFileTagName);
                    TinyConsole.Out.WriteLine($"Replace the {lyricsFileFriendlyTagName} in the lyrics file as it does not match the music file.: \"{lyricsTagValue}\" for lyrics, (none) for music, \"{lyricsFile.FullName}\"");
                    return true;
                }
            }
            else
            {
                if (musicTagValue is not null)
                {
                    var musicTagValueText = musicTagValue.Value.FormatTime(TimeFormatType.ShortFormat, 2);
                    lyricsTags.Add(lyricsFileTagName, musicTagValueText);
                    TinyConsole.Out.WriteLine($"Replace the {lyricsFileFriendlyTagName} in the lyrics file as it does not match the music file.: (none) for lyrics, \"{musicTagValueText}\" for music, \"{lyricsFile.FullName}\"");
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private static bool CheckFileName(FilePath file, string fileName)
        {
            var ok = true;
            var parentDirectory = file.Directory;
            Validation.Assert(parentDirectory is not null);
            var actualFiles = parentDirectory.EnumerateFiles().Where(f => string.Equals(f.Name, file.Name, StringComparison.OrdinalIgnoreCase)).ToList();
            Validation.Assert(actualFiles.Count > 0);
            if (actualFiles.Count > 1)
            {
                TinyConsole.Out.WriteLine($"The specified file name is ambiguous. There is probably a case difference.: \"{file.FullName}\"");
                ok = false;
            }

            var actualFile = actualFiles.First();
            if (Path.GetFileNameWithoutExtension(actualFile.Name) != Path.GetFileNameWithoutExtension(fileName))
            {
                TinyConsole.Out.WriteLine($"The file name is different from what is expected by the tags in the music file. (expected: \"{Path.GetFileNameWithoutExtension(fileName)}{file.Extension}\", actual: \"{file.Name}\", full-path: \"{file.FullName}\")");
                ok = false;
            }

            return ok;
        }

        private static string? GetLyricistFromMusicFile(MovieInformation musicFileInfo)
            => musicFileInfo.Format.FormatName switch
            {
                "mp3" or "wav" => GetTagValue(musicFileInfo, _metadataNameText),
                "flac" or "ogg" => GetTagValue(musicFileInfo, _metadataNameLyricist),
                _ => throw new ApplicationException($"Not supported music file format.: \"{musicFileInfo.Format.File.FullName}\""),
            };

        private static string GetTagValue(MovieInformation musicFileInfo, string tagName)
            => musicFileInfo.Format.FormatName switch
            {
                "wav" or "mp3" or "flac" => musicFileInfo.Format.Tags[tagName] ?? "",
                "ogg" => musicFileInfo.AudioStreams.FirstOrDefault()?.Tags[tagName] ?? "",
                _ => throw new ApplicationException($"Not supported music file format.: \"{musicFileInfo.Format.File.FullName}\""),
            };

        private static string GetMusicFileExtension(MovieInformation musicFileInfo)
        {
            switch (musicFileInfo.Format.FormatName)
            {
                case "wav":
                    return ".wav";
                case "mp3":
                    return ".mp3";
                case "flac":
                    return ".flac";
                case "ogg":
                {
                    if (musicFileInfo.AudioStreams.Any(stream => stream.CodecName == "opus"))
                        return ".opus";
                    else if (musicFileInfo.AudioStreams.Any(stream => stream.CodecName == "vorbis"))
                        return ".ogg";
                    else
                        throw new ApplicationException($"Not supported music file format.: \"{musicFileInfo.Format.File.FullName}\"");
                }
                default:
                    throw new ApplicationException($"Not supported music file format.: \"{musicFileInfo.Format.File.FullName}\"");
            }
        }

        // mp3tag のファイル名自動生成時のエンコードに準拠
        private static string Mp3TagEncode(string s)
            => string.Concat(
                s.Select(c =>
                    c switch
                    {
                        // Mp3tag は '\' にだけは正常に対応できていないようだが一応含める
                        '\\' or '/' or ':' or '*' or '?' or '"' or '<' or '>' or '|' => "",
                        _ => c.ToString(),
                    }));

        private static void PrintHelpMessage()
        {
            var textLines =
                new[]
                {
                    "[Usage]",
                    $"{Validation.DefaultApplicationName} <option list> <audio file path> [<lyrics file path>]",
                    "",
                    "[Options]",
                    "--modify_lyrics  or  -m",
                    "  Automatically modify or create the lyrics file based on the tag information of the music file.",
                };
            foreach (var lineText in textLines)
                TinyConsole.Out.WriteLine(lineText);
        }

        [GeneratedRegex(@"^\[(?<name>[a-z]+):\s*(?<value>.*)\]\s*$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)]
        private static partial Regex GetLyricsFileTagPattern();

        [GeneratedRegex(@"^\[(?<lyricsTime>(\d+:)?\d+(\.\d+)?)\](?<lyricsText>.*)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)]
        private static partial Regex GetLyricsTextPattern();
    }
}
