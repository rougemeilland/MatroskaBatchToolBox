using System;
using System.Runtime.Serialization;

namespace Palmtree.Terminal.StringExpansion
{
    internal class ExpansionStringSyntaxErrorExceptionException
        : Exception
    {
        public ExpansionStringSyntaxErrorExceptionException(string message)
            : base(message)
        {
        }

        public ExpansionStringSyntaxErrorExceptionException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ExpansionStringSyntaxErrorExceptionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
