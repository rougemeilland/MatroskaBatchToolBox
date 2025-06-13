using System;
using System.Text;
using Palmtree;
using Palmtree.IO.Console;

namespace DumpTermInfo
{
    internal static class Program
    {
        private static void Main()
        {
            if (TinyConsole.InputEncoding.CodePage != Encoding.UTF8.CodePage || TinyConsole.OutputEncoding.CodePage != Encoding.UTF8.CodePage)
            {
                if (OperatingSystem.IsWindows())
                    TinyConsole.WriteLog(LogCategory.Warning, "The encoding of standard input or output is not UTF8. Consider running the command \"chcp 65001\".");
                else
                    TinyConsole.WriteLog(LogCategory.Warning, "The encoding of standard input or standard output is not UTF8.");
            }

            TinyConsole.DefaultTextWriter = ConsoleTextWriterType.StandardError;

            TerminalInfo.WriteAllTerminalInfos(Console.Out);
        }
    }
}
