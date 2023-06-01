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
            for (var index = 0; index < args.Length; ++index)
                Console.WriteLine($"args[{index}]=\"{args[index]}\"");
            return 0;
        }
    }
}
