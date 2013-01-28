namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;

    internal abstract class OperationResponse
    {
        private Dictionary<string, string> headers;
        private Exception innerException;
        private int statusCode;

        internal OperationResponse(Dictionary<string, string> headers)
        {
            this.headers = headers;
        }

        public Exception Error
        {
            get
            {
                return this.innerException;
            }
            set
            {
                this.innerException = value;
            }
        }

        public IDictionary<string, string> Headers
        {
            get
            {
                return this.headers;
            }
        }

        public int StatusCode
        {
            get
            {
                return this.statusCode;
            }
            internal set
            {
                this.statusCode = value;
            }
        }
    }
}

