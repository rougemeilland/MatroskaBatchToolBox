using System.Text;
using Palmtree;
using Palmtree.IO.Console;

namespace Experiment
{
    public static partial class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:未使用のパラメーターを削除します", Justification = "<保留中>")]
        public static int Main(string[] args)
        {
            if (TinyConsole.InputEncoding.CodePage != Encoding.UTF8.CodePage || TinyConsole.OutputEncoding.CodePage != Encoding.UTF8.CodePage)
                TinyConsole.WriteLog(LogCategory.Warning, "The encoding of standard input or standard output is not UTF8.");

            TinyConsole.WriteLine("OK");
            TinyConsole.Beep();
            _ = TinyConsole.ReadLine();
            return 0;
        }
    }
}
