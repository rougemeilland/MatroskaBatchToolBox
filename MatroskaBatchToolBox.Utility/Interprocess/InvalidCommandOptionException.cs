using System;

namespace MatroskaBatchToolBox.Utility.Interprocess
{
    public class InvalidCommandOptionException
        : Exception
    {
        public InvalidCommandOptionException(string message)
            : base(message)
        {
        }

        public InvalidCommandOptionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
