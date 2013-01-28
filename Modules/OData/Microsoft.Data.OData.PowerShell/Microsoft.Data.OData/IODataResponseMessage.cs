namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal interface IODataResponseMessage
    {
        string GetHeader(string headerName);
        Stream GetStream();
        void SetHeader(string headerName, string headerValue);

        IEnumerable<KeyValuePair<string, string>> Headers { get; }

        int StatusCode { get; set; }
    }
}

