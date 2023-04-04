using System;
using System.IO;

namespace MatroskaBatchToolBox.Utility.Interprocess
{
    public interface IChildProcessOutputRedirectable
    {
        Action GetOutputRedirector(StreamReader reader);
    }
}
