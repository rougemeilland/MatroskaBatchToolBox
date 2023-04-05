using System;
using System.IO;
using System.Linq;
using Palmtree.Terminal;

namespace Experiment
{
    public static partial class Program
    {
        public static void Main()
        {
            var names =
                TerminalInfo.EnumerateTerminalInfo()
                .Select(info => new { name = Path.GetFileName(info.TermInfoFilePath), info.E3 })
                .Where(item => item.E3 is not null && item.E3 == "\u001b[3J")
                .Select(item => item.name);
            foreach (var name in names.OrderBy(name => name))
                Console.WriteLine(name);
        }
    }
}
