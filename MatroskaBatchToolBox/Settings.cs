using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MatroskaBatchToolBox
{
    internal class Settings
    {
        public class SettingsContainer
        {
            public SettingsContainer()
            {
                FFmpegNormalizeCommandPath = null;
                VideoEncoderOnComplexConversion = null;
                VideoEncoderOnComplexConversion = null;
                LibaomAV1EncoderOptionOnComplexConversion = null;
                CalculateVMAFScore = null;
                DegreeOfParallelism = null;
            }

            public string? FFmpegNormalizeCommandPath { get; set; }
            public string? VideoEncoderOnComplexConversion { get; set; }
            public string? Libx265EncoderOptionOnComplexConversion { get; set; }
            public string? LibaomAV1EncoderOptionOnComplexConversion { get; set; }
            public bool? CalculateVMAFScore { get; set; }
            public int? DegreeOfParallelism { get; set; }
        }

        static Settings()
        {
            var baseDirectoryPath = Path.GetDirectoryName(typeof(Settings).Assembly.Location);
            if (baseDirectoryPath is null)
                throw new Exception("'settings.json' is not found.");
            var settingsFilePath = Path.Combine(baseDirectoryPath, "settings.json");
            var settingsText = File.ReadAllText(settingsFilePath);
            var settings = JsonSerializer.Deserialize<SettingsContainer>(settingsText);
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

            FileInfo? ffmpegNormalizeCommandFile;
            if (string.IsNullOrEmpty(settings.FFmpegNormalizeCommandPath))
            {
                var message = $"'{nameof(settings.FFmpegNormalizeCommandPath)}' is not set in 'settings.json'.";
                PrintFatalMessage(message);
                throw new Exception(); // can't reach here
            }
            try
            {
                ffmpegNormalizeCommandFile = new FileInfo(settings.FFmpegNormalizeCommandPath);
                if (!ffmpegNormalizeCommandFile.Exists)
                    ffmpegNormalizeCommandFile = null;
            }
            catch (Exception)
            {
                ffmpegNormalizeCommandFile = null;
            }
            if (ffmpegNormalizeCommandFile is null)
            {
                var message = $"The path name \"{settings.FFmpegNormalizeCommandPath}\" set in \"{nameof(settings.FFmpegNormalizeCommandPath)}\" does not exist.";
                PrintFatalMessage(message);
                throw new Exception(); // can't reach here
            }

            // デフォルトのエンコーダについて：
            // 圧縮率と損失の少なさを考えると圧縮性能を考えると libaom-av1 一択であるが、あまりにも圧縮時間がかかりすぎる。
            // 多くのファイルのバッチ変換というこのアプリケーションの目的を達成するために、圧縮率は犠牲になっても所要時間の短縮が優先であると判断し、libx265 を既定値とした。
            //
            // (参考)いくつかのサンプル動画のエンコードに必要なCPU時間の実測の結果：
            //   libaom-av1(crf23): 動画の長さの6倍～400倍以上 (30分の動画なら3時間～200時間)
            //   libx265(crf18): 動画の長さの2倍～8倍以上 (30分の動画なら1時間～4時間)
            var videoEncoderOnComplexConversion = settings.VideoEncoderOnComplexConversion.TryParseAsVideoEncoderType();
            if (videoEncoderOnComplexConversion is null)
            {
                // サポートしていないエンコーダーが設定されていた場合
                var message = $"Video encoders set to \"VideoEncoderOnComplexConversion\" are not supported.: {(settings.VideoEncoderOnComplexConversion is null ? "null" : $"\"{settings.VideoEncoderOnComplexConversion}\"")}";
                PrintFatalMessage(message);
                throw new Exception(); // can't reach here
            }

            // レートコントロールモードについては、「個人的な保存用の圧縮」が主用途であるため、品質の維持を最優先として、CRFを既定値とした。
            // ※ CRFの値は各エンコーダの「通常扱うであろう動画変換において視覚的に無損失と見なせる値」を選択した。
            //     参考: https://trac.ffmpeg.org/wiki/Encode/AV1 (AV1 ビデオ エンコーディング ガイド)
            //     参考: https://trac.ffmpeg.org/wiki/Encode/H.265 (H.265/HEVC ビデオ エンコーディング ガイド)
            var libx265EncoderOptionOnComplexConversion = settings.Libx265EncoderOptionOnComplexConversion ?? "-crf 19 -tag:v hvc1";
            var libaomAV1EncoderOptionOnComplexConversion = settings.LibaomAV1EncoderOptionOnComplexConversion ?? "-crf 23";

            var calculateVMAFScore = settings.CalculateVMAFScore ?? false;
            var degreeOfParallelism = settings.DegreeOfParallelism ?? 1;
            CurrentSettings =
                new Settings(
                    ffmpegNormalizeCommandFile,
                    ffmpegCommandFile,
                    videoEncoderOnComplexConversion.Value,
                    libx265EncoderOptionOnComplexConversion,
                    libaomAV1EncoderOptionOnComplexConversion,
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

        private Settings(FileInfo ffmpegNormalizeCommandFile, FileInfo ffmpegCommandFile, VideoEncoderType videoEncoderOnComplexConversion, string libx265EncoderOptionOnComplexConversion, string libaomAV1EncoderOptionOnComplexConversion, bool calculateVMAFScore, int degreeOfParallelism)
        {
            FFmpegNormalizeCommandFile = ffmpegNormalizeCommandFile;
            FFmpegCommandFile = ffmpegCommandFile;
            VideoEncoderOnComplexConversion = videoEncoderOnComplexConversion;
            Libx265EncoderOptionOnComplexConversion = libx265EncoderOptionOnComplexConversion;
            LibaomAV1EncoderOptionOnComplexConversion = libaomAV1EncoderOptionOnComplexConversion;
            CalculateVMAFScore = calculateVMAFScore;
            DegreeOfParallelism = degreeOfParallelism;
        }

        public FileInfo FFmpegNormalizeCommandFile { get; private set; }
        public FileInfo FFmpegCommandFile { get; private set; }
        public VideoEncoderType VideoEncoderOnComplexConversion { get; set; }
        public string Libx265EncoderOptionOnComplexConversion { get; set; }
        public string LibaomAV1EncoderOptionOnComplexConversion { get; set; }
        public bool CalculateVMAFScore { get; private set; }
        public int DegreeOfParallelism { get; private set; }
        public static Settings CurrentSettings { get; private set; }
    }
}
