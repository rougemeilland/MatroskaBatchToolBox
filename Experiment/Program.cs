using System;
using System.Diagnostics;

namespace Experiment
{
    public static partial class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<保留中>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:不要な抑制を削除します", Justification = "<保留中>")]
        public static int Main(string[] args)
        {
            if (args.Length <= 0)
            {
                var process = Process.Start(new ProcessStartInfo { FileName = "experiment.exe", Arguments = "comment \"1行目\n2行目\"" });
                process?.WaitForExit();
                return 0;
            }
            else
            {
                for (var index = 0; index < args.Length; ++index)
                    Console.WriteLine($"args[{index}]: \"{args[index]}\"");
                return 0;
            }
        }
    }
}
