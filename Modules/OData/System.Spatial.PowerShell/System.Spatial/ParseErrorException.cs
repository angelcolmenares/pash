namespace System.Spatial
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class ParseErrorException : Exception
    {
        public ParseErrorException()
        {
        }

        public ParseErrorException(string message) : base(message)
        {
        }

        protected ParseErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ParseErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

