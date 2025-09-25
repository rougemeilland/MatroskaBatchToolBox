using System;
using Palmtree.IO;

namespace ConcatMovie
{
    internal sealed class InputFileSpecification
    {
        public InputFileSpecification(FilePath inputFilePath, string? inputFileFormat, TimeSpan? inputFileDuration)
        {
            InputFilePath = inputFilePath;
            InputFileFormat = inputFileFormat;
            InputFileDuration = inputFileDuration;
        }

        public FilePath InputFilePath { get; }
        public string? InputFileFormat { get; }
        public TimeSpan? InputFileDuration { get; }
    }
}
