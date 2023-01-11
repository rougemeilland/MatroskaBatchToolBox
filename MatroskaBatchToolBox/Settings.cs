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
                AV1QualityFactor = null;
                AV1EncoderOptionOnResize = null;
                DegreeOfParallelism = null;
            }

            public string? FFmpegNormalizeCommandPath { get; set; }
            public int? AV1QualityFactor { get; set; }
            public string? AV1EncoderOptionOnResize { get; set; }
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

            var av1QualityFactor = settings.AV1QualityFactor ?? 23;
            var av1EncoderOptionOnResize = settings.AV1EncoderOptionOnResize;
            var degreeOfParallelism = settings.DegreeOfParallelism ?? 1;
            CurrentSettings =
                new Settings(
                    ffmpegNormalizeCommandFile,
                    ffmpegCommandFile,
                    av1QualityFactor,
                    av1EncoderOptionOnResize,
                    degreeOfParallelism: degreeOfParallelism);
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

        private Settings(FileInfo ffmpegNormalizeCommandFile, FileInfo ffmpegCommandFile, int av1QualityFactor, string? av1EncoderOptionOnResize, int degreeOfParallelism)
        {
            FFmpegNormalizeCommandFile = ffmpegNormalizeCommandFile;
            FFmpegCommandFile = ffmpegCommandFile;
            AV1QualityFactor = av1QualityFactor;
            AV1EncoderOptionOnResize = av1EncoderOptionOnResize;
            DegreeOfParallelism = degreeOfParallelism;
        }

        public FileInfo FFmpegNormalizeCommandFile { get;private set; }
        public FileInfo FFmpegCommandFile { get; private set; }
        public int AV1QualityFactor { get; private set; }
        public string? AV1EncoderOptionOnResize { get; private set; }
        public int DegreeOfParallelism { get; private set; }
        public static Settings CurrentSettings { get; private set; }
    }
}
