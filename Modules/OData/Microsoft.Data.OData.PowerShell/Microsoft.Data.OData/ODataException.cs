namespace Microsoft.Data.OData
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    [Serializable, DebuggerDisplay("{Message}")]
    internal class ODataException : InvalidOperationException
    {
        public ODataException() : this(Strings.ODataException_GeneralError)
        {
        }

        public ODataException(string message) : this(message, null)
        {
        }

        protected ODataException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ODataException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

