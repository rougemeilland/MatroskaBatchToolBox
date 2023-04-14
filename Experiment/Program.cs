using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Palmtree;
using MatroskaBatchToolBox.Utility;

namespace Experiment
{
    public static partial class Program
    {
        private const string _ffmpegPathEnvironmentVariableName = "FFMPEG_PATH";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<保留中>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:不要な抑制を削除します", Justification = "<保留中>")]
        public static int Main(string[] args)
        {
            var ffmpegCommandFilePath = ProcessUtility.WhereIs("ffmpeg");
            if (ffmpegCommandFilePath is null)
                throw new Exception();
            Environment.SetEnvironmentVariable(_ffmpegPathEnvironmentVariableName, ffmpegCommandFilePath);
            var normalizeCommandFilePath = ProcessUtility.WhereIs("ffmpeg-normalize");
            if (normalizeCommandFilePath is null)
                throw new Exception();
            var inputFormat = "matroska";
            var inputFile = "";
            var outputFormat = "matroska";
            var outputFile = "";
            var param = new List<string>();

            if (outputFormat is not null)
                param.Add($"-ofmt {outputFormat}");
            param.Add($"-o {outputFile.CommandLineArgumentEncode()}");
            param.Add("-f");
            param.Add("-n");
            param.Add("-pr");
            param.Add("--keep-loudness-range-target");
            param.Add("-c:a libopus");
            if (inputFormat is not null)
                param.Add($"-e:i {$"[ \"-f\", \"{inputFormat}\" ]".CommandLineArgumentEncode()}");
            param.Add(inputFile.CommandLineArgumentEncode());
            return 0;
        }
    }
}
