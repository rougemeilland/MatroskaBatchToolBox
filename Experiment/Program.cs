using System.IO;
using MatroskaBatchToolBox.Utility.Interprocess;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;
using Palmtree.IO;
using Palmtree.IO.Console;

namespace Experiment
{
    public static partial class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<保留中>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:不要な抑制を削除します", Justification = "<保留中>")]
        public static int Main(string[] args)
        {
            foreach (var dir in args)
            {
                foreach (var file in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
                {
                    if (Path.GetExtension(file).ToUpperInvariant().IsAnyOf(".MKV", ".MP4"))
                    {
                        var shortFile = Path.GetFileName(file);
                        if (shortFile.Length > 80)
                            shortFile = shortFile[..80];
                        TinyConsole.Write($"{shortFile}...");
                        TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
                        TinyConsole.Write("\r");
                        TinyConsole.Error.WriteLine(file);
                        _ = Command.GetMovieInformation(null, new FilePath(file), MovieInformationType.Streams, (level, message) => { });
                    }
                }
            }

            TinyConsole.Write("\r");
            TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);

            return 0;
        }
    }
}
