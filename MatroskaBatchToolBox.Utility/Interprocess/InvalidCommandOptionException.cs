using System;
using System.Runtime.Serialization;

namespace MatroskaBatchToolBox.Utility.Interprocess
{
    [Serializable]
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

        protected InvalidCommandOptionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
