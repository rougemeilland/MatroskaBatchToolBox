using System;
using System.IO;
using System.Text.Json;
using MatroskaBatchToolBox.Model;
using MatroskaBatchToolBox.Model.Json;
using Palmtree;

namespace MatroskaBatchToolBox
{
    internal class Settings
    {
        private const string _localSettingFileName = $".{nameof(MatroskaBatchToolBox)}.setting.json";

        static Settings()
        {
            var baseDirectoryPath =
                Path.GetDirectoryName(typeof(Settings).Assembly.Location)
                ?? throw new Exception("'settings.json' is not found.");
            var settingsFilePath = Path.Combine(baseDirectoryPath, "settings.json");
            var settingsText = File.ReadAllText(settingsFilePath);
            var settings =
                JsonSerializer.Deserialize<GlobalSettingsContainer>(
                    settingsText,
                    new JsonSerializerOptions { AllowTrailingCommas = true, PropertyNameCaseInsensitive = true })
                ?? throw new Exception("Failed to parse 'settings.json'.");

            FileInfo? ffmpegNormalizeCommandFile;
            if (string.IsNullOrEmpty(settings.FfmpegNormalizeCommandFilePath))
            {
                var message = $"'{nameof(settings.FfmpegNormalizeCommandFilePath)}' is not set in 'settings.json'.";
                PrintFatalMessage(message);
                throw new Exception(); // can't reach here
            }

            try
            {
                ffmpegNormalizeCommandFile = new FileInfo(settings.FfmpegNormalizeCommandFilePath);
                if (!ffmpegNormalizeCommandFile.Exists)
                    ffmpegNormalizeCommandFile = null;
            }
            catch (Exception)
            {
                ffmpegNormalizeCommandFile = null;
            }

            if (ffmpegNormalizeCommandFile is null)
            {
                var message = $"The path name \"{settings.FfmpegNormalizeCommandFilePath}\" set in \"{nameof(settings.FfmpegNormalizeCommandFilePath)}\" does not exist.";
                PrintFatalMessage(message);
                throw new Exception(); // can't reach here
            }

            var videoEncoderOnComplexConversion = settings.FfmpegVideoEncoder.TryParseAsVideoEncoderType();
            if (videoEncoderOnComplexConversion is null)
            {
                // サポートしていないエンコーダーが設定されていた場合
                var message = $"Video encoders set to \"VideoEncoderOnComplexConversion\" are not supported.: {(settings.FfmpegVideoEncoder is null ? "null" : $"\"{settings.FfmpegVideoEncoder}\"")}";
                PrintFatalMessage(message);
                throw new Exception(); // can't reach here
            }

            // レートコントロールモードについては、「個人的な保存用の圧縮」が主用途であるため、品質の維持を最優先として、CRFを既定値とした。
            // ※ CRFの値は各エンコーダの「通常扱うであろう動画変換において視覚的に無損失と見なせる値」を選択した。
            //     参考: https://trac.ffmpeg.org/wiki/Encode/AV1 (AV1 ビデオ エンコーディング ガイド)
            //     参考: https://trac.ffmpeg.org/wiki/Encode/H.265 (H.265/HEVC ビデオ エンコーディング ガイド)
            var ffmpegLibx264EncoderOption = settings.FfmpegLibx264EncoderOption ?? "-crf 19";
            var ffmpegLibx265EncoderOption = settings.FfmpegLibx265EncoderOption ?? "-crf 19";
            var ffmpegLibaomAv1EncoderOption = settings.FfmpegLibaomAv1EncoderOption ?? "-crf 23";
            var ffmpegOption = settings.FfmpegOption ?? "";
            var deleteChapters = settings.DeleteChapters ?? false;
            var keepChapterTitles = settings.KeepChapterTitles ?? true;
            var deleteMetadata = settings.DeleteMetadata ?? false;
            var deleteImageVideoStream = settings.DeleteImageVideoStream ?? false;
            var allowMultipleVideoStreams = settings.AllowMultipleVideoStreams ?? false;
            var behaviorForDataStreams = ParseStreamOperationType(settings.BehaviorForDataStreams, "behavior_for_data_streams");
            var behaviorForAttachmentStreams = ParseStreamOperationType(settings.BehaviorForAttachmentStreams, "behavior_for_attachment_streams");
            var defaultVideoLanguage = (string?)null;
            var defaultAudioLanguage = (string?)null;
            var resetForceStream = false;
            var resetDefaultStream = false;
            var calculateVmafScore = settings.CalculateVmafScore ?? false;
            var doNotConvert = false;
            var degreeOfParallelism = settings.DegreeOfParallelism ?? 1;
            GlobalSettings =
                new Settings(
                    ffmpegNormalizeCommandFile,
                    videoEncoderOnComplexConversion.Value,
                    ffmpegLibx264EncoderOption,
                    ffmpegLibx265EncoderOption,
                    ffmpegLibaomAv1EncoderOption,
                    ffmpegOption,
                    deleteChapters,
                    keepChapterTitles,
                    deleteMetadata,
                    deleteImageVideoStream,
                    allowMultipleVideoStreams,
                    behaviorForDataStreams,
                    behaviorForAttachmentStreams,
                    Rectangle.DefaultValue,
                    TimeRange.DefaultValue,
                    defaultVideoLanguage,
                    defaultAudioLanguage,
                    resetForceStream,
                    resetDefaultStream,
                    calculateVmafScore,
                    doNotConvert,
                    degreeOfParallelism);
        }

        private static void PrintFatalMessage(string message)
        {
            var previousColor = TinyConsole.ForegroundColor;
            TinyConsole.ForegroundColor = ConsoleColor.Red;
            TinyConsole.WriteLine();
            TinyConsole.WriteLine();
            TinyConsole.WriteLine(message);
            TinyConsole.ForegroundColor = previousColor;
            TinyConsole.Beep();
            TinyConsole.WriteLine("Press ENTER key to exit.");
            _ = TinyConsole.ReadLine();
            Environment.Exit(1);
        }

        private Settings(
            FileInfo ffmpegNormalizeCommandFile,
            VideoEncoderType ffmpegVideoEncoder,
            string ffmpegLibx264EncoderOption,
            string ffmpegLibx265EncoderOption,
            string ffmpegLibaomAv1EncoderOption,
            string ffmpegOption,
            bool deleteChapters,
            bool keepChapterTitles,
            bool deleteMetadata,
            bool deleteImageVideoStream,
            bool allowMultipleVideoStreams,
            StreamOperationType behaviorForDataStreams,
            StreamOperationType behaviorForAttachmentStreams,
            Rectangle cropping,
            TimeRange trimming,
            string? defaultVideoLanguage,
            string? defaultAudioLanguage,
            bool resetForceStream,
            bool resetDefaultStream,
            bool calculateVmafScore,
            bool doNotConvert,
            int degreeOfParallelism)
        {
            FfmpegNormalizeCommandFile = ffmpegNormalizeCommandFile;
            FfmpegVideoEncoder = ffmpegVideoEncoder;
            FfmpegLibx264EncoderOption = ffmpegLibx264EncoderOption;
            FfmpegLibx265EncoderOption = ffmpegLibx265EncoderOption;
            FfmpegLibaomAv1EncoderOption = ffmpegLibaomAv1EncoderOption;
            FfmpegOption = ffmpegOption;
            DeleteChapters = deleteChapters;
            KeepChapterTitles = keepChapterTitles;
            DeleteMetadata = deleteMetadata;
            DeleteImageVideoStream = deleteImageVideoStream;
            AllowMultipleVideoStreams = allowMultipleVideoStreams;
            BehaviorForDataStreams = behaviorForDataStreams;
            BehaviorForAttachmentStreams = behaviorForAttachmentStreams;
            Cropping = cropping;
            Trimming = trimming;
            CalculateVmafScore = calculateVmafScore;
            DoNotConvert = doNotConvert;
            DegreeOfParallelism = degreeOfParallelism;
            DefaultVideoLanguage = defaultVideoLanguage;
            DefaultAudioLanguage = defaultAudioLanguage;
            ResetForcedStream = resetForceStream;
            ResetDefaultStream = resetDefaultStream;
        }

        public FileInfo FfmpegNormalizeCommandFile { get; }
        public VideoEncoderType FfmpegVideoEncoder { get; }
        public string FfmpegLibx264EncoderOption { get; }
        public string FfmpegLibx265EncoderOption { get; }
        public string FfmpegLibaomAv1EncoderOption { get; }
        public string FfmpegOption { get; }
        public bool DeleteChapters { get; }
        public bool KeepChapterTitles { get; }
        public bool DeleteMetadata { get; }
        public bool DeleteImageVideoStream { get; }
        public bool AllowMultipleVideoStreams { get; }
        public StreamOperationType BehaviorForDataStreams { get; set; }
        public StreamOperationType BehaviorForAttachmentStreams { get; set; }
        public Rectangle Cropping { get; set; }
        public TimeRange Trimming { get; set; }
        public string? DefaultVideoLanguage { get; set; }
        public string? DefaultAudioLanguage { get; set; }
        public bool ResetForcedStream { get; set; }
        public bool ResetDefaultStream { get; set; }
        public bool CalculateVmafScore { get; }
        public bool DoNotConvert { get; set; }
        public int DegreeOfParallelism { get; }
        public static Settings GlobalSettings { get; }

        public Settings GetLocalSettings(DirectoryInfo movieFileDirectory)
        {
            try
            {
                var localSettingFilePath = Path.Combine(movieFileDirectory.FullName, _localSettingFileName);
                if (!File.Exists(localSettingFilePath))
                    return this;
                var settingsText = File.ReadAllText(localSettingFilePath);
                var localSettings =
                    JsonSerializer.Deserialize<LocalSettingsContainer>(
                        settingsText,
                        new JsonSerializerOptions { AllowTrailingCommas = true, PropertyNameCaseInsensitive = true });
                if (localSettings is null)
                    return this;
                return
                    new Settings(
                        FfmpegNormalizeCommandFile,
                        localSettings.FfmpegVideoEncoder.TryParseAsVideoEncoderType() ?? FfmpegVideoEncoder,
                        localSettings.FfmpegLibx264EncoderOption ?? FfmpegLibx264EncoderOption,
                        localSettings.FfmpegLibx265EncoderOption ?? FfmpegLibx265EncoderOption,
                        localSettings.FfmpegLibaomAv1EncoderOption ?? FfmpegLibaomAv1EncoderOption,
                        localSettings.FfmpegOption ?? FfmpegOption,
                        localSettings.DeleteChapters ?? DeleteChapters,
                        localSettings.KeepChapterTitles ?? KeepChapterTitles,
                        localSettings.DeleteMetadata ?? DeleteMetadata,
                        localSettings.DeleteImageVideoStream ?? DeleteImageVideoStream,
                        localSettings.AllowMultipleVideoStreams ?? AllowMultipleVideoStreams,
                        DeriveStreamOperation(BehaviorForDataStreams, localSettings.BehaviorForDataStreams, "behavior_for_data_streams"),
                        DeriveStreamOperation(BehaviorForAttachmentStreams, localSettings.BehaviorForAttachmentStreams, "behavior_for_attachment_streams"),
                        DeriveRectangle(Cropping, localSettings.Cropping),
                        DeriveTimeRange(Trimming, localSettings.Trimming),
                        localSettings.DefaultVideoLanguage ?? DefaultVideoLanguage,
                        localSettings.DefaultAudioLanguage ?? DefaultAudioLanguage,
                        localSettings.ResetForcedStream ?? ResetForcedStream,
                        localSettings.ResetDefaultStream ?? ResetDefaultStream,
                        localSettings.CalculateVmafScore ?? CalculateVmafScore,
                        localSettings.DoNotConvert ?? DoNotConvert,
                        DegreeOfParallelism);
            }
            catch (Exception)
            {
                return this;
            }
        }

        private static StreamOperationType ParseStreamOperationType(string? valueText, string propertyName)
            => valueText switch
            {
                null => StreamOperationType.Keep,
                "keep" => StreamOperationType.Keep,
                "delete" => StreamOperationType.Delete,
                "error" => StreamOperationType.Error,
                _ => throw new Exception($"The value of the \"{propertyName}\" property is invalid. Set the value of this property to \"keep\", \"delete\" or \"error\". : \"{valueText}\""),
            };

        private static StreamOperationType DeriveStreamOperation(StreamOperationType originalValue, string? newValueText, string propertyName)
            => newValueText switch
            {
                null => originalValue,
                "keep" => StreamOperationType.Keep,
                "delete" => StreamOperationType.Delete,
                "error" => StreamOperationType.Error,
                _ => throw new Exception($"The value of the \"{propertyName}\" property is invalid. Set the value of this property to \"keep\", \"delete\" or \"error\". : \"{newValueText}\""),
            };

        private static TimeRange DeriveTimeRange(TimeRange originalValue, string? newValueText)
            => newValueText is null
                ? originalValue
                : TimeRange.TryParse(newValueText, out TimeRange? newValue)
                ? newValue
                : throw new Exception($"Invalid time range format.: \"{newValueText}\"");

        private static Rectangle DeriveRectangle(Rectangle originalValue, string? newValueText)
            => newValueText is null
                ? originalValue
                : Rectangle.TryParse(newValueText, out Rectangle? newValue)
                ? newValue
                : throw new Exception($"Invalid rectangle format.: {newValueText}");
    }
}
