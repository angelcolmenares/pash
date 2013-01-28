namespace Microsoft.Data.OData
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    [Serializable, DebuggerDisplay("{Message}")]
    internal class ODataContentTypeException : ODataException
    {
        public ODataContentTypeException() : this(Strings.ODataException_GeneralError)
        {
        }

        public ODataContentTypeException(string message) : this(message, null)
        {
        }

        protected ODataContentTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ODataContentTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

