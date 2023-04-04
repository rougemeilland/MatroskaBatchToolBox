using System;
using System.Runtime.Serialization;

namespace Palmtree.Terminal.StringExpansion
{
    internal class ExpansionBadArgumentExceptionException
        : Exception
    {
        public ExpansionBadArgumentExceptionException(string message)
            : base(message)
        {
        }

        public ExpansionBadArgumentExceptionException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ExpansionBadArgumentExceptionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
