namespace System.Data.Services.Client
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    [Serializable, DebuggerDisplay("{Message}")]
    internal sealed class DataServiceRequestException : InvalidOperationException
    {
        [NonSerialized]
        private readonly DataServiceResponse response;

        public DataServiceRequestException() : base(Strings.DataServiceException_GeneralError)
        {
        }

        public DataServiceRequestException(string message) : base(message)
        {
        }

        protected DataServiceRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DataServiceRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public DataServiceRequestException(string message, Exception innerException, DataServiceResponse response) : base(message, innerException)
        {
            this.response = response;
        }

        public DataServiceResponse Response
        {
            get
            {
                return this.response;
            }
        }
    }
}

