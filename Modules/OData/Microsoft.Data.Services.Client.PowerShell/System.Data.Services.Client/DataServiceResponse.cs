namespace System.Data.Services.Client
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal sealed class DataServiceResponse : IEnumerable<OperationResponse>, IEnumerable
    {
        private bool batchResponse;
        private Dictionary<string, string> headers;
        private IEnumerable<OperationResponse> response;
        private int statusCode;

        internal DataServiceResponse(Dictionary<string, string> headers, int statusCode, IEnumerable<OperationResponse> response, bool batchResponse)
        {
            this.headers = headers ?? new Dictionary<string, string>(EqualityComparer<string>.Default);
            this.statusCode = statusCode;
            this.batchResponse = batchResponse;
            this.response = response;
        }

        public IEnumerator<OperationResponse> GetEnumerator()
        {
            return this.response.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IDictionary<string, string> BatchHeaders
        {
            get
            {
                return this.headers;
            }
        }

        public int BatchStatusCode
        {
            get
            {
                return this.statusCode;
            }
        }

        public bool IsBatchResponse
        {
            get
            {
                return this.batchResponse;
            }
        }
    }
}

