using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Palmtree;
using MatroskaBatchToolBox.Utility;

namespace Experiment
{
    public static partial class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<保留中>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:不要な抑制を削除します", Justification = "<保留中>")]
        public static int Main(string[] args)
        {
            foreach (var time in "0,+00:01:41.835000000,+00:01:49.309000000,+00:01:44.971000000".ParseAsChapterStartTimes())
                Console.WriteLine(time.FormatTime(3));
            return 0;
        }
    }
}
