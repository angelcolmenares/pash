namespace System.Data.Services.Client
{
    using System;
	using System.IO;
    using System.Net;

    internal class GetReadStreamResult : BaseAsyncResult
    {
        private readonly ODataRequestMessageWrapper requestMessage;
        private HttpWebResponse response;
        private readonly StreamDescriptor streamDescriptor;

        internal GetReadStreamResult(DataServiceContext context, string method, ODataRequestMessageWrapper request, AsyncCallback callback, object state, StreamDescriptor streamDescriptor) : base(context, method, callback, state)
        {
            this.requestMessage = request;
            base.Abortable = request;
            this.streamDescriptor = streamDescriptor;
        }

        protected override void AsyncEndGetResponse(IAsyncResult asyncResult)
        {
            try
            {
                base.SetCompletedSynchronously(asyncResult.CompletedSynchronously);
                ODataRequestMessageWrapper request = Util.NullCheck<ODataRequestMessageWrapper>(this.requestMessage, InternalError.InvalidEndGetResponseRequest);
                HttpWebResponse webResponse = null;
                webResponse = WebUtil.EndGetResponse(request, asyncResult, (DataServiceContext) base.Source);
                this.SetHttpWebResponse(webResponse);
                base.SetCompleted();
            }
            catch (Exception exception)
            {
                if (base.HandleFailure(exception))
                {
                    throw;
                }
            }
            finally
            {
                base.HandleCompleted();
            }
        }

        internal void Begin()
        {
            try
            {
                IAsyncResult result = BaseAsyncResult.InvokeAsync(new Func<ODataRequestMessageWrapper, AsyncCallback, object, IAsyncResult>(WebUtil.BeginGetResponse), this.requestMessage, new AsyncCallback(this.AsyncEndGetResponse), null);
                base.SetCompletedSynchronously(result.CompletedSynchronously);
            }
            catch (Exception exception)
            {
                base.HandleFailure(exception);
                throw;
            }
            finally
            {
                base.HandleCompleted();
            }
        }

        protected override void CompletedRequest()
        {
            Func<Stream> getResponseStream = null;
            if (this.response != null)
            {
                InvalidOperationException e = null;
                if (!WebUtil.SuccessStatusCode(this.response.StatusCode))
                {
                    getResponseStream = () => WebUtil.GetResponseStream(this.response, (DataServiceContext) base.Source);
                    
                    e = BaseSaveResult.GetResponseText(getResponseStream, this.response.StatusCode);
                }
                if (e != null)
                {
                    this.response.Close();
                    base.HandleFailure(e);
                }
            }
        }

        internal DataServiceStreamResponse End()
        {
            if (this.response != null)
            {
                this.streamDescriptor.ETag = this.response.Headers["ETag"];
                this.streamDescriptor.ContentType = this.response.Headers["Content-Type"];
                return new DataServiceStreamResponse(this.response);
            }
            return null;
        }

        internal DataServiceStreamResponse Execute()
        {
            try
            {
                HttpWebResponse webResponse = null;
                webResponse = WebUtil.GetResponse(this.requestMessage, (DataServiceContext) base.Source, true);
                this.SetHttpWebResponse(webResponse);
            }
            catch (Exception exception)
            {
                base.HandleFailure(exception);
                throw;
            }
            finally
            {
                base.SetCompleted();
                this.CompletedRequest();
            }
            if (base.Failure != null)
            {
                throw base.Failure;
            }
            return this.End();
        }

        protected override void HandleCompleted(BaseAsyncResult.PerRequest pereq)
        {
            Error.ThrowInternalError(InternalError.InvalidHandleCompleted);
        }

        private void SetHttpWebResponse(HttpWebResponse webResponse)
        {
            this.response = webResponse;
        }
    }
}

