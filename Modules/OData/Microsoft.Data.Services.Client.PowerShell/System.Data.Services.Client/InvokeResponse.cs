namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;

    internal class InvokeResponse : OperationResponse
    {
        public InvokeResponse(Dictionary<string, string> headers) : base(headers)
        {
        }
    }
}

