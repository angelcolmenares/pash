namespace System.Data.Services.Client
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Common;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Xml;

    internal abstract class DataServiceRequest
    {
        internal DataServiceRequest()
        {
            this.PayloadKind = ODataPayloadKind.Unsupported;
        }

        internal IAsyncResult BeginExecute(object source, DataServiceContext context, AsyncCallback callback, object state, string method)
        {
            QueryResult result = this.CreateResult(source, context, callback, state, method);
            result.BeginExecuteQuery(context);
            return result;
        }

        private QueryResult CreateResult(object source, DataServiceContext context, AsyncCallback callback, object state, string method)
        {
            System.Data.Services.Client.QueryComponents components = this.QueryComponents(context.MaxProtocolVersion);
            IEnumerable<KeyValuePair<string, string>> headers = null;
            IEnumerable<string> headersToReset = null;
            RequestInfo requestInfo = new RequestInfo(context);
            if (string.CompareOrdinal("POST", components.HttpMethod) == 0)
            {
                if (components.BodyOperationParameters == null)
                {
                    KeyValuePair<string, string>[] pairArray = new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("Content-Length", "0") };
                    headers = pairArray;
                }
                else
                {
                    KeyValuePair<string, string>[] pairArray2 = new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("Content-Type", "application/json;odata=verbose") };
                    headers = pairArray2;
                }
            }
            if (components.UriOperationParameters != null)
            {
                this.RequestUri = new Serializer(requestInfo).WriteUriOperationParametersToUri(this.RequestUri, components.UriOperationParameters);
            }
            ODataRequestMessageWrapper requestMessage = context.CreateODataRequestMessage(components.HttpMethod, this.RequestUri, false, components.Version, headers, headersToReset);
            if (components.BodyOperationParameters != null)
            {
                new Serializer(requestInfo).WriteBodyOperationParameters(components.BodyOperationParameters, requestMessage);
                return new QueryResult(source, method, this, requestMessage, requestInfo, callback, state, new BaseAsyncResult.ContentStream(requestMessage.CachedRequestStream, true));
            }
            return new QueryResult(source, method, this, requestMessage, requestInfo, callback, state);
        }

        internal static IEnumerable<TElement> EndExecute<TElement>(object source, DataServiceContext context, string method, IAsyncResult asyncResult)
        {
            QueryResult result = null;
            try
            {
                result = QueryResult.EndExecuteQuery<TElement>(source, method, asyncResult);
                return result.ProcessResult<TElement>(result.ServiceRequest.Plan);
            }
            catch (DataServiceQueryException exception)
            {
                Exception innerException = exception;
                while (innerException.InnerException != null)
                {
                    innerException = innerException.InnerException;
                }
                DataServiceClientException exception3 = innerException as DataServiceClientException;
                if ((!context.IgnoreResourceNotFoundException || (exception3 == null)) || (exception3.StatusCode != 0x194))
                {
                    throw;
                }
                QueryOperationResponse response = new QueryOperationResponse<TElement>(new Dictionary<string, string>(exception.Response.Headers), exception.Response.Query, MaterializeAtom.EmptyResults) {
                    StatusCode = 0x194
                };
                return (IEnumerable<TElement>) response;
            }
        }

        internal QueryOperationResponse<TElement> Execute<TElement>(DataServiceContext context, System.Data.Services.Client.QueryComponents queryComponents)
        {
            QueryResult result = null;
            QueryOperationResponse<TElement> response2;
            try
            {
                result = new DataServiceRequest<TElement>(queryComponents, this.Plan).CreateResult(this, context, null, null, "Execute");
                result.ExecuteQuery(context);
                response2 = result.ProcessResult<TElement>(this.Plan);
            }
            catch (InvalidOperationException exception)
            {
                if (result != null)
                {
                    QueryOperationResponse response = result.GetResponse<TElement>(MaterializeAtom.EmptyResults);
                    if (response != null)
                    {
                        if (context.IgnoreResourceNotFoundException)
                        {
                            DataServiceClientException exception2 = exception as DataServiceClientException;
                            if ((exception2 != null) && (exception2.StatusCode == 0x194))
                            {
                                return (QueryOperationResponse<TElement>) response;
                            }
                        }
                        response.Error = exception;
                        throw new DataServiceQueryException(System.Data.Services.Client.Strings.DataServiceException_GeneralError, exception, response);
                    }
                }
                throw;
            }
            return response2;
        }

        internal static DataServiceRequest GetInstance(Type elementType, Uri requestUri)
        {
            return (DataServiceRequest) Activator.CreateInstance(typeof(DataServiceRequest<>).MakeGenericType(new Type[] { elementType }), new object[] { requestUri });
        }

        internal long GetQuerySetCount(DataServiceContext context)
        {
            long num2;
            Version requestVersion = this.QueryComponents(context.MaxProtocolVersion).Version;
            if ((requestVersion == null) || (requestVersion.Major < 2))
            {
                requestVersion = Util.DataServiceVersion2;
            }
            QueryResult result = null;
            DataServiceRequest<long> serviceRequest = new DataServiceRequest<long>(this.QueryComponents(context.MaxProtocolVersion), null);
            KeyValuePair<string, string>[] pairArray = new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("Accept", "text/plain") };
            IEnumerable<KeyValuePair<string, string>> headers = pairArray;
            ODataRequestMessageWrapper request = context.CreateODataRequestMessage("GET", this.QueryComponents(context.MaxProtocolVersion).Uri, false, requestVersion, headers, new string[] { "Accept" });
            result = new QueryResult(this, "Execute", serviceRequest, request, new RequestInfo(context), null, null);
            try
            {
                result.ExecuteQuery(context);
                if (HttpStatusCode.NoContent == result.StatusCode)
                {
                    throw new DataServiceQueryException(System.Data.Services.Client.Strings.DataServiceRequest_FailGetCount, result.Failure);
                }
                StreamReader reader = new StreamReader(result.GetResponseStream());
                long num = -1L;
                try
                {
                    num = XmlConvert.ToInt64(reader.ReadToEnd());
                }
                finally
                {
                    reader.Close();
                }
                num2 = num;
            }
            catch (InvalidOperationException exception)
            {
                QueryOperationResponse response = null;
                response = result.GetResponse<long>(MaterializeAtom.EmptyResults);
                if (response != null)
                {
                    response.Error = exception;
                    throw new DataServiceQueryException(System.Data.Services.Client.Strings.DataServiceException_GeneralError, exception, response);
                }
                throw;
            }
            return num2;
        }

        internal static MaterializeAtom Materialize(ResponseInfo responseInfo, System.Data.Services.Client.QueryComponents queryComponents, ProjectionPlan plan, string contentType, IODataResponseMessage message, ODataPayloadKind expectedPayloadKind)
        {
            if ((message.StatusCode != 0xcc) && !string.IsNullOrEmpty(contentType))
            {
                return new MaterializeAtom(responseInfo, queryComponents, plan, message, expectedPayloadKind);
            }
            return MaterializeAtom.EmptyResults;
        }

        internal abstract System.Data.Services.Client.QueryComponents QueryComponents(DataServiceProtocolVersion maxProtocolVersion);

        public abstract Type ElementType { get; }

        internal ODataPayloadKind PayloadKind { get; set; }

        internal abstract ProjectionPlan Plan { get; }

        public abstract Uri RequestUri { get; internal set; }
    }
}

