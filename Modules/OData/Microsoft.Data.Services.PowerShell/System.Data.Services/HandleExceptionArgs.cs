namespace System.Data.Services
{
    using System;
    using System.Runtime.CompilerServices;

    internal class HandleExceptionArgs
    {
        public HandleExceptionArgs(System.Exception exception, bool responseWritten, string contentType, bool verboseResponse)
        {
            this.Exception = WebUtil.CheckArgumentNull<System.Exception>(exception, "exception");
            this.ResponseWritten = responseWritten;
            this.ResponseContentType = contentType;
            this.UseVerboseErrors = verboseResponse;
        }

        public System.Exception Exception { get; set; }

        internal string ResponseAllowHeader
        {
            get
            {
                if (this.Exception is DataServiceException)
                {
                    return ((DataServiceException) this.Exception).ResponseAllowHeader;
                }
                return null;
            }
        }

        public string ResponseContentType { get; private set; }

        public int ResponseStatusCode
        {
            get
            {
                if (this.Exception is DataServiceException)
                {
                    return ((DataServiceException) this.Exception).StatusCode;
                }
                return 500;
            }
        }

        public bool ResponseWritten { get; private set; }

        public bool UseVerboseErrors { get; set; }
    }
}

