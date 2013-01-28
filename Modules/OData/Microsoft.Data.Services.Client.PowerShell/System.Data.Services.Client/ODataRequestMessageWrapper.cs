namespace System.Data.Services.Client
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;

    internal class ODataRequestMessageWrapper
    {
        private bool allowAnyAcceptType;
        private readonly RequestInfo requestInfo;
        private readonly IODataRequestMessage requestMessage;
        private bool sendChunked;

        private ODataRequestMessageWrapper(IODataRequestMessage message, RequestInfo requestInfo)
        {
            this.requestMessage = message;
            this.requestInfo = requestInfo;
        }

        internal ODataRequestMessageWrapper(string httpMethod, Uri requestUrl, RequestInfo requestInfo)
        {
            this.requestMessage = new HttpWebRequestMessage(requestUrl, httpMethod, requestInfo);
            this.requestInfo = requestInfo;
            this.IsBatchPartRequest = false;
        }

        internal void Abort()
        {
            ((HttpWebRequestMessage) this.requestMessage).Abort();
        }

        internal void AddHeadersToReset(IEnumerable<string> headerNames)
        {
            if (this.requestInfo.HasSendingRequestEventHandlers)
            {
                ((HttpWebRequestMessage) this.requestMessage).AddHeadersToReset(headerNames);
            }
        }

        internal void AddHeadersToReset(string headerName)
        {
            if (this.requestInfo.HasSendingRequestEventHandlers)
            {
                ((HttpWebRequestMessage) this.requestMessage).AddHeadersToReset(headerName);
            }
        }

        internal IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            return ((HttpWebRequestMessage) this.requestMessage).BeginGetRequestStream(callback, state);
        }

        internal IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            return ((HttpWebRequestMessage) this.requestMessage).BeginGetResponse(callback, state);
        }

        internal static ODataRequestMessageWrapper CreateBatchPartRequestMessage(ODataBatchWriter batchWriter, string method, Uri requestUrl, RequestInfo requestInfo)
        {
            return new ODataRequestMessageWrapper(batchWriter.CreateOperationRequestMessage(method, requestUrl), requestInfo) { IsBatchPartRequest = true };
        }

        internal ODataMessageWriter CreateWriter(ODataMessageWriterSettings writerSettings)
        {
            return new ODataMessageWriter(this.requestMessage, writerSettings, ClientEdmModel.GetModel(this.requestInfo.MaxProtocolVersion));
        }

        internal Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            return ((HttpWebRequestMessage) this.requestMessage).EndGetRequestStream(asyncResult);
        }

        internal HttpWebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            return ((HttpWebRequestMessage) this.requestMessage).EndGetResponse(asyncResult);
        }

        internal void FireSendingRequest2(Descriptor descriptor)
        {
            HttpWebRequestMessage requestMessage = this.requestMessage as HttpWebRequestMessage;
            if (!this.IsBatchPartRequest)
            {
                if (this.GetHeader("Accept") == null)
                {
                    string headerValue = this.AllowAnyAcceptType ? "*/*" : "application/atom+xml,application/xml";
                    this.SetHeader("Accept", headerValue);
                }
                requestMessage.HttpWebRequest.SendChunked = this.SendChunked;
            }
            if (this.requestInfo.HasSendingRequest2EventHandlers)
            {
                HttpWebRequest request = (requestMessage == null) ? null : requestMessage.HttpWebRequest;
                IODataRequestMessage message2 = this.requestMessage;
                if ((requestMessage == null) || (request == null))
                {
                    message2 = new InternalODataRequestMessage(this.requestMessage);
                }
                if (requestMessage != null)
                {
                    requestMessage.BeforeSendingRequest2Event();
                }
                this.requestInfo.FireSendingRequest2(new SendingRequest2EventArgs(message2, descriptor, this.IsBatchPartRequest));
                if (requestMessage != null)
                {
                    requestMessage.AfterSendingRequest2Event();
                }
            }
        }

        public string GetHeader(string headerName)
        {
            return this.requestMessage.GetHeader(headerName);
        }

        internal Stream GetRequestStream()
        {
            return ((HttpWebRequestMessage) this.requestMessage).GetRequestStream();
        }

        internal HttpWebResponse GetResponse()
        {
            return ((HttpWebRequestMessage) this.requestMessage).GetResponse();
        }

        internal void SetContentLengthHeader()
        {
            if (this.requestInfo.HasSendingRequestEventHandlers)
            {
                ((HttpWebRequestMessage) this.requestMessage).SetContentLengthHeader();
            }
        }

        public void SetHeader(string headerName, string headerValue)
        {
            this.requestMessage.SetHeader(headerName, headerValue);
        }

        internal bool AllowAnyAcceptType
        {
            get
            {
                return this.allowAnyAcceptType;
            }
            set
            {
                this.allowAnyAcceptType = value;
            }
        }

        internal MemoryStream CachedRequestStream
        {
            get
            {
                return ((HttpWebRequestMessage) this.requestMessage).CachedRequestStream;
            }
        }

        internal bool IsBatchPartRequest { get; private set; }

        internal bool SendChunked
        {
            get
            {
                return this.sendChunked;
            }
            set
            {
                this.sendChunked = value;
            }
        }
    }
}

