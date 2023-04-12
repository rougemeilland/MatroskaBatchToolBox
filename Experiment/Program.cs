using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Palmtree;

namespace Experiment
{
    public static partial class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<保留中>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:不要な抑制を削除します", Justification = "<保留中>")]
        public static int Main(string[] args)
        {
            var thisCommandName = typeof(Program).Assembly.GetName().Name;
            try
            {
                var location = ProcessUtility.WhereIs(args[0]);
                TinyConsole.WriteLine(location ?? "(null)");
                return 0;
            }
            catch (Exception ex)
            {
                TinyConsole.ForegroundColor = ConsoleColor.Red;
                TinyConsole.Error.WriteLine($"{thisCommandName}:ERROR: {ex.Message}");
                return 1;
            }
            finally
            {
                TinyConsole.ResetColor();
            }
        }
    }
}
