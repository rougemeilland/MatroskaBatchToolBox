﻿using MatroskaBatchToolBox.Model;
using MatroskaBatchToolBox.Model.Json;
using System;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MatroskaBatchToolBox
{
    internal class Settings
    {
        private const string _localSettingFileName = $".{nameof(MatroskaBatchToolBox)}.setting.json";

        static Settings()
        {
            var baseDirectoryPath = Path.GetDirectoryName(typeof(Settings).Assembly.Location);
            if (baseDirectoryPath is null)
                throw new Exception("'settings.json' is not found.");
            var settingsFilePath = Path.Combine(baseDirectoryPath, "settings.json");
            var settingsText = File.ReadAllText(settingsFilePath);
            var settings = JsonSerializer.Deserialize<GlobalSettingsContainer>(settingsText, new JsonSerializerOptions { AllowTrailingCommas = true });
            if (settings is null)
                throw new Exception("Failed to parse 'settings.json'.");

            FileInfo? ffmpegCommandFile = null;
            foreach (var executableFile in new DirectoryInfo(Path.GetDirectoryName(typeof(Settings).Assembly.Location) ?? ".").EnumerateFiles())
            {
                if (Regex.IsMatch(executableFile.Name, @"^ffmpeg(\.exe)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                    ffmpegCommandFile = executableFile;
            }
            if (ffmpegCommandFile is null)
            {
                var message = $"'ffmpeg' is not installed.";
                PrintFatalMessage(message);
                throw new Exception(); // can't reach here
            }

            FileInfo? ffprobeCommandFile = null;
            foreach (var executableFile in new DirectoryInfo(Path.GetDirectoryName(typeof(Settings).Assembly.Location) ?? ".").EnumerateFiles())
            {
                if (Regex.IsMatch(executableFile.Name, @"^ffprobe(\.exe)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                    ffprobeCommandFile = executableFile;
            }
            if (ffprobeCommandFile is null)
            {
                var message = $"'ffprobe' is not installed.";
                PrintFatalMessage(message);
                throw new Exception(); // can't reach here
            }

            FileInfo? ffmpegNormalizeCommandFile;
            if (string.IsNullOrEmpty(settings.FFmpegNormalizeCommandFilePath))
            {
                var message = $"'{nameof(settings.FFmpegNormalizeCommandFilePath)}' is not set in 'settings.json'.";
                PrintFatalMessage(message);
                throw new Exception(); // can't reach here
            }
            try
            {
                ffmpegNormalizeCommandFile = new FileInfo(settings.FFmpegNormalizeCommandFilePath);
                if (!ffmpegNormalizeCommandFile.Exists)
                    ffmpegNormalizeCommandFile = null;
            }
            catch (Exception)
            {
                ffmpegNormalizeCommandFile = null;
            }
            if (ffmpegNormalizeCommandFile is null)
            {
                var message = $"The path name \"{settings.FFmpegNormalizeCommandFilePath}\" set in \"{nameof(settings.FFmpegNormalizeCommandFilePath)}\" does not exist.";
                PrintFatalMessage(message);
                throw new Exception(); // can't reach here
            }

            var videoEncoderOnComplexConversion = settings.FFmpegVideoEncoder.TryParseAsVideoEncoderType();
            if (videoEncoderOnComplexConversion is null)
            {
                // サポートしていないエンコーダーが設定されていた場合
                var message = $"Video encoders set to \"VideoEncoderOnComplexConversion\" are not supported.: {(settings.FFmpegVideoEncoder is null ? "null" : $"\"{settings.FFmpegVideoEncoder}\"")}";
                PrintFatalMessage(message);
                throw new Exception(); // can't reach here
            }

            // レートコントロールモードについては、「個人的な保存用の圧縮」が主用途であるため、品質の維持を最優先として、CRFを既定値とした。
            // ※ CRFの値は各エンコーダの「通常扱うであろう動画変換において視覚的に無損失と見なせる値」を選択した。
            //     参考: https://trac.ffmpeg.org/wiki/Encode/AV1 (AV1 ビデオ エンコーディング ガイド)
            //     参考: https://trac.ffmpeg.org/wiki/Encode/H.265 (H.265/HEVC ビデオ エンコーディング ガイド)
            var ffmpegLibx264EncoderOption = settings.FFmpegLibx264EncoderOption ?? "-crf 19";
            var ffmpegLibx265EncoderOption = settings.FFmpegLibx265EncoderOption ?? "-crf 19";
            var ffmpegLibaomAV1EncoderOption = settings.FFmpegLibaomAV1EncoderOption ?? "-crf 23";
            var ffmpegOption = settings.FFmpegOption ?? "";
            var deleteChapters = settings.DeleteChapters ?? false;
            var deleteMetadata = settings.DeleteMetadata ?? false;
            var deleteImageVideoStream = settings.DeleteImageVideoStream ?? false;
            var allowMultipleVideoStreams = settings.AllowMultipleVideoStreams ?? false;
            Rectangle cropping;
            if (settings.Cropping is null)
                cropping = Rectangle.DefaultValue;
            else
            {
                if (!Rectangle.TryParse(settings.Cropping, out Rectangle? cropping2))
                {
                    // クロッピングの指定に誤りがあった場合
                    var message = $"Incorrect format for cropping.: {(settings.Cropping is null ? "null" : $"\"{settings.Cropping}\"")}";
                    PrintFatalMessage(message);
                    throw new Exception(); // can't reach here
                }
                cropping = cropping2;
            }
            TimeRange trimming;
            if (settings.Trimming is null)
                trimming = TimeRange.DefaultValue;
            else
            {
                if (!TimeRange.TryParse(settings.Trimming, out TimeRange? trimming2))
                {
                    // トリミング範囲の指定に誤りがあった場合
                    var message = $"Incorrect format for trimming.: {(settings.Trimming is null ? "null" : $"\"{settings.Trimming}\"")}";
                    PrintFatalMessage(message);
                    throw new Exception(); // can't reach here
                }
                trimming = trimming2;
            }
            var calculateVMAFScore = settings.CalculateVMAFScore ?? false;
            var degreeOfParallelism = settings.DegreeOfParallelism ?? 1;
            GlobalSettings =
                new Settings(
                    ffmpegNormalizeCommandFile,
                    ffprobeCommandFile,
                    ffmpegCommandFile,
                    videoEncoderOnComplexConversion.Value,
                    ffmpegLibx264EncoderOption,
                    ffmpegLibx265EncoderOption,
                    ffmpegLibaomAV1EncoderOption,
                    ffmpegOption,
                    deleteChapters,
                    deleteMetadata,
                    deleteImageVideoStream,
                    allowMultipleVideoStreams,
                    cropping,
                    trimming,
                    calculateVMAFScore,
                    degreeOfParallelism);
        }

        private static void PrintFatalMessage(string message)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(message);
            Console.ForegroundColor = previousColor;
            Console.Beep();
            Console.WriteLine("Press ENTER key to exit.");
            Console.ReadLine();
            Environment.Exit(1);
        }

        private Settings(
            FileInfo ffmpegNormalizeCommandFile,
            FileInfo ffprobeCommandFile,
            FileInfo ffmpegCommandFile,
            VideoEncoderType ffmpegVideoEncoder,
            string ffmpegLibx264EncoderOption,
            string ffmpegLibx265EncoderOption,
            string ffmpegLibaomAV1EncoderOption,
            string ffmpegOption,
            bool deleteChapters,
            bool deleteMetadata,
            bool deleteImageVideoStream,
            bool allowMultipleVideoStreams,
            Rectangle cropping,
            TimeRange trimming,
            bool calculateVMAFScore,
            int degreeOfParallelism)
        {
            FFmpegNormalizeCommandFile = ffmpegNormalizeCommandFile;
            FFprobeCommandFile = ffprobeCommandFile;
            FFmpegCommandFile = ffmpegCommandFile;
            FFmpegVideoEncoder = ffmpegVideoEncoder;
            FFmpegLibx264EncoderOption = ffmpegLibx264EncoderOption;
            FFmpegLibx265EncoderOption = ffmpegLibx265EncoderOption;
            FFmpegLibaomAV1EncoderOption = ffmpegLibaomAV1EncoderOption;
            FFmpegOption = ffmpegOption;
            DeleteChapters = deleteChapters;
            DeleteMetadata = deleteMetadata;
            DeleteImageVideoStream = deleteImageVideoStream;
            AllowMultipleVideoStreams = allowMultipleVideoStreams;
            Cropping = cropping;
            Trimming = trimming;
            CalculateVMAFScore = calculateVMAFScore;
            DegreeOfParallelism = degreeOfParallelism;
        }

        public FileInfo FFmpegNormalizeCommandFile { get; }
        public FileInfo FFprobeCommandFile { get;  }
        public FileInfo FFmpegCommandFile { get; }
        public VideoEncoderType FFmpegVideoEncoder { get; }
        public string FFmpegLibx264EncoderOption { get; }
        public string FFmpegLibx265EncoderOption { get; }
        public string FFmpegLibaomAV1EncoderOption { get; }
        public string FFmpegOption { get; }
        public bool DeleteChapters { get; }
        public bool DeleteMetadata { get; }
        public bool DeleteImageVideoStream { get; }
        public bool AllowMultipleVideoStreams { get; }
        public Rectangle Cropping { get; set; }
        public TimeRange Trimming { get; set; }
        public bool CalculateVMAFScore { get; }
        public int DegreeOfParallelism { get; }
        public static Settings GlobalSettings { get;  }

        public Settings GetLocalSettings(DirectoryInfo movieFileDirectory)
        {
            try
            {
                var localSettingFilePath = Path.Combine(movieFileDirectory.FullName, _localSettingFileName);
                if (!File.Exists(localSettingFilePath))
                    return this;
                var settingsText = File.ReadAllText(localSettingFilePath);
                var localSettings = JsonSerializer.Deserialize<LocalSettingsContainer>(settingsText, new JsonSerializerOptions { AllowTrailingCommas= true });
                if (localSettings is null)
                    return this;
                
                return
                    new Settings(
                        FFmpegNormalizeCommandFile,
                        FFprobeCommandFile,
                        FFmpegCommandFile,
                        localSettings.FFmpegVideoEncoder.TryParseAsVideoEncoderType() ?? FFmpegVideoEncoder,
                        localSettings.FFmpegLibx264EncoderOption ?? FFmpegLibx264EncoderOption,
                        localSettings.FFmpegLibx265EncoderOption ?? FFmpegLibx265EncoderOption,
                        localSettings.FFmpegLibaomAV1EncoderOption ?? FFmpegLibaomAV1EncoderOption,
                        localSettings.FFmpegOption ?? FFmpegOption,
                        localSettings.DeleteChapters ?? DeleteChapters,
                        localSettings.DeleteMetadata ?? DeleteMetadata,
                        localSettings.DeleteImageVideoStream ?? DeleteImageVideoStream,
                        localSettings.AllowMultipleVideoStreams ?? AllowMultipleVideoStreams,
                        DeriveRectangle(Cropping, localSettings.Cropping),
                        DeriveTimeRange(Trimming, localSettings.Trimming),
                        localSettings.CalculateVMAFScore ?? CalculateVMAFScore,
                        DegreeOfParallelism);
            }
            catch (Exception)
            {
                return this;
            }
        }

        private static TimeRange DeriveTimeRange(TimeRange originalValue, string? newValueText)
        {
            if (newValueText is null)
                return originalValue;
            if (!TimeRange.TryParse(newValueText, out TimeRange? newValue))
                throw new Exception("Invalid time range format.");
            return newValue;
        }

        private static Rectangle DeriveRectangle(Rectangle originalValue, string? newValueText)
        {
            if (newValueText is null)
                return originalValue;
            if (!Rectangle.TryParse(newValueText, out Rectangle? newValue))
                throw new Exception("Invalid rectangle format.");
            return newValue;
        }
    }
}
