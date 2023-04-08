using System;
using System.Linq;

namespace Experiment
{
    public static partial class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<保留中>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:不要な抑制を削除します", Justification = "<保留中>")]
        public static void Main(string[] args)
        {
            foreach (var item in args.Select((arg, index) => new { arg, encodedArg = EncodeArgument(arg), index }))
            {
                Console.WriteLine($"{item.index}:{item.arg} => {item.encodedArg}");
            }
        }

        private static string EncodeArgument(string arg)
        {
            return
                OperatingSystem.IsWindows()
                ? arg.IndexOfAny(new[] { '\t', ' ', '"' }) >= 0
                    ? $"\"{EncodeForWindows(arg)}\""
                    : arg
                : arg.IndexOfAny(new[] { '\t', ' ' }) >= 0
                    ? $"\"{EncodeForUnix(arg)}\""
                    : EncodeForUnix(arg);

            static string EncodeForWindows(string arg)
            {
                return string.Concat(arg.Select(c => c == '"' ? "\"\"" : c.ToString()));
            }

            static string EncodeForUnix(string arg)
            {
                return string.Concat(arg.Select(c => c == '\\' ? "\\\\" : c == '"' ? "\\\"" : c.ToString()));
            }
        }
    }
}
