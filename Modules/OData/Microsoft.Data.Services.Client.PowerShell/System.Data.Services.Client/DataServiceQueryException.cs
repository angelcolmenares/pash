namespace System.Data.Services.Client
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    [Serializable, DebuggerDisplay("{Message}")]
    internal sealed class DataServiceQueryException : InvalidOperationException
    {
        [NonSerialized]
        private readonly QueryOperationResponse response;

        public DataServiceQueryException() : base(Strings.DataServiceException_GeneralError)
        {
        }

        public DataServiceQueryException(string message) : base(message)
        {
        }

        protected DataServiceQueryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DataServiceQueryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public DataServiceQueryException(string message, Exception innerException, QueryOperationResponse response) : base(message, innerException)
        {
            this.response = response;
        }

        public QueryOperationResponse Response
        {
            get
            {
                return this.response;
            }
        }
    }
}

