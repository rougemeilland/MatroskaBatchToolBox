using System;
using System.IO;

namespace MatroskaBatchToolBox.Utility.Interprocess
{
    public interface IChildProcessInputRedirectable
    {
        Action GetInputRedirector(StreamWriter writer);
    }
}
