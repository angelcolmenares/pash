namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class IncompleteParseException : ParseException
    {
        private const string errorIdString = "IncompleteParse";

        public IncompleteParseException()
        {
            base.SetErrorId("IncompleteParse");
        }

        public IncompleteParseException(string message) : base(message)
        {
            base.SetErrorId("IncompleteParse");
        }

        protected IncompleteParseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IncompleteParseException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorId("IncompleteParse");
        }

        internal IncompleteParseException(string message, string errorId) : base(message, errorId)
        {
        }

        internal IncompleteParseException(string message, string errorId, Exception innerException) : base(message, errorId, innerException)
        {
        }
    }
}

