namespace System.Data.Services
{
    using System;
    using System.IO;

    internal interface IDataServiceHost
    {
        string GetQueryStringItem(string item);
        void ProcessException(HandleExceptionArgs args);

        Uri AbsoluteRequestUri { get; }

        Uri AbsoluteServiceUri { get; }

        string RequestAccept { get; }

        string RequestAcceptCharSet { get; }

        string RequestContentType { get; }

        string RequestHttpMethod { get; }

        string RequestIfMatch { get; }

        string RequestIfNoneMatch { get; }

        string RequestMaxVersion { get; }

        Stream RequestStream { get; }

        string RequestVersion { get; }

        string ResponseCacheControl { get; set; }

        string ResponseContentType { get; set; }

        string ResponseETag { get; set; }

        string ResponseLocation { get; set; }

        int ResponseStatusCode { get; set; }

        Stream ResponseStream { get; }

        string ResponseVersion { get; set; }
    }
}

