using System.Diagnostics;

namespace MatroskaBatchToolBox.Utility.Interprocess
{
    public interface IChildProcessCancellable
    {
        void CancelChildProcess(Process childProcess);
    }
}
