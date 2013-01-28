namespace System.Data.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Providers;
    using System.Linq;
    using System.Linq.Expressions;

    internal interface IDataService
    {
        void DisposeDataSource();
        object GetResource(RequestDescription description, int segmentIndex, string typeFullName);
        SegmentInfo GetSegmentForContentId(string contentId);
        void InternalApplyingExpansions(Expression queryExpression, ICollection<ExpandSegmentCollection> expandPaths);
        void InternalHandleException(HandleExceptionArgs args);
        void InternalOnRequestQueryConstructed(IQueryable query);
        void InternalOnStartProcessingRequest(ProcessRequestArgs args);

        DataServiceActionProviderWrapper ActionProvider { get; }

        DataServiceConfiguration Configuration { get; }

        DataServiceExecutionProviderWrapper ExecutionProvider { get; }

        object Instance { get; }

        DataServiceOperationContext OperationContext { get; }

        DataServicePagingProviderWrapper PagingProvider { get; }

        DataServiceProcessingPipeline ProcessingPipeline { get; }

        DataServiceProviderWrapper Provider { get; }

        DataServiceStreamProviderWrapper StreamProvider { get; }

        UpdatableWrapper Updatable { get; }
    }
}

