namespace System.Data.Services.Providers
{
    using System;
    using System.Collections;
    using System.Data.Services;
    using System.Linq;
    using System.Linq.Expressions;

    internal class DataServiceExecutionProviderWrapper
    {
        private readonly IDataService dataService;
        private IDataServiceExecutionProvider executionProvider;

        public DataServiceExecutionProviderWrapper(IDataService dataService)
        {
            this.dataService = dataService;
        }

        internal object Execute(Expression requestExpression)
        {
            return this.ExecutionProvider.Execute(requestExpression, this.dataService.OperationContext);
        }

        internal IEnumerable GetResultEnumerableFromRequest(SegmentInfo segmentInfo)
        {
            object obj2 = this.Execute(segmentInfo.RequestExpression);
            IQueryable query = obj2 as IQueryable;
            if (query != null)
            {
                this.dataService.InternalOnRequestQueryConstructed(query);
            }
            if ((segmentInfo.SingleResult && (query == null)) || (obj2 is IDataServiceInvokable))
            {
                obj2 = new object[] { obj2 };
            }
            return (IEnumerable) obj2;
        }

        internal static IEnumerator GetSingleResultFromRequest(SegmentInfo segmentInfo)
        {
            IEnumerator enumerator2;
            IEnumerator requestEnumerator = WebUtil.GetRequestEnumerator(segmentInfo.RequestEnumerable);
            bool flag = true;
            try
            {
                WebUtil.CheckResourceExists(requestEnumerator.MoveNext(), segmentInfo.Identifier);
                RequestDescription.CheckNullDirectReference(requestEnumerator.Current, segmentInfo);
                flag = false;
                enumerator2 = requestEnumerator;
            }
            finally
            {
                if (flag)
                {
                    WebUtil.Dispose(requestEnumerator);
                }
            }
            return enumerator2;
        }

        private IDataServiceExecutionProvider ExecutionProvider
        {
            get
            {
                if (this.executionProvider == null)
                {
                    this.executionProvider = new DataServiceExecutionProvider();
                }
                return this.executionProvider;
            }
        }
    }
}

