using System;
using Palmtree;

namespace Experiment
{
    public static partial class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<保留中>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:不要な抑制を削除します", Justification = "<保留中>")]
        public static int Main(string[] args)
        {
            Console.WriteLine("丈月".WindowsFileNameEncoding());
            return 0;
        }
    }
}
