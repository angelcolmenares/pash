namespace System.Data.Services.Client
{
    using System;
    using System.Net;

    internal class SendingRequestEventArgs : EventArgs
    {
        private WebRequest request;
        private WebHeaderCollection requestHeaders;

        internal SendingRequestEventArgs(WebRequest request, WebHeaderCollection requestHeaders)
        {
            this.request = request;
            this.requestHeaders = requestHeaders;
        }

        public WebRequest Request
        {
            get
            {
                return this.request;
            }
            set
            {
                Util.CheckArgumentNull<WebRequest>(value, "value");
                if (!(value is HttpWebRequest))
                {
                    throw Error.Argument(Strings.Context_SendingRequestEventArgsNotHttp, "value");
                }
                this.request = value;
                this.requestHeaders = value.Headers;
            }
        }

        public WebHeaderCollection RequestHeaders
        {
            get
            {
                return this.requestHeaders;
            }
        }
    }
}

