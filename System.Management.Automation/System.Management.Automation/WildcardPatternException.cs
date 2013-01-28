namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class WildcardPatternException : RuntimeException
    {
        [NonSerialized]
        private ErrorRecord _errorRecord;

        public WildcardPatternException()
        {
        }

        internal WildcardPatternException(ErrorRecord errorRecord) : base(RuntimeException.RetrieveMessage(errorRecord))
        {
            if (errorRecord == null)
            {
                throw new ArgumentNullException("errorRecord");
            }
            this._errorRecord = errorRecord;
        }

        public WildcardPatternException(string message) : base(message)
        {
        }

        protected WildcardPatternException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public WildcardPatternException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

