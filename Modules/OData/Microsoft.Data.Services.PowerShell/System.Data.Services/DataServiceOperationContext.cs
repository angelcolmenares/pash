namespace System.Data.Services
{
    using System;
    using System.Data.Services.Providers;
    using System.Net;
    using System.Runtime.CompilerServices;

    internal sealed class DataServiceOperationContext : IServiceProvider
    {
        private readonly IDataServiceHost hostInterface;
        private DataServiceHostWrapper hostWrapper;
        private bool? isBatchRequest;

        internal DataServiceOperationContext(IDataServiceHost host)
        {
            this.hostInterface = host;
        }

        internal DataServiceOperationContext(bool isBatchRequest, IDataServiceHost2 host) : this(host)
        {
            this.isBatchRequest = new bool?(isBatchRequest);
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IDataServiceMetadataProvider))
            {
                return this.CurrentDataService.Provider.MetadataProvider;
            }
            if (serviceType == typeof(IDataServiceQueryProvider))
            {
                return this.CurrentDataService.Provider.QueryProvider;
            }
            if (serviceType == typeof(IUpdatable))
            {
                return this.CurrentDataService.Updatable.GetOrLoadUpdateProvider();
            }
            if (serviceType == typeof(IDataServiceUpdateProvider))
            {
                return (this.CurrentDataService.Updatable.GetOrLoadUpdateProvider() as IDataServiceUpdateProvider);
            }
            if (serviceType == typeof(IDataServiceUpdateProvider2))
            {
                return (this.CurrentDataService.Updatable.GetOrLoadUpdateProvider() as IDataServiceUpdateProvider2);
            }
            return null;
        }

        internal void InitializeAndCacheHeaders(IDataService dataService)
        {
            this.hostWrapper = new DataServiceHostWrapper(this.hostInterface);
            this.CurrentDataService = dataService;
            if (this.hostInterface is IDataServiceHost2)
            {
                this.ResponseHeaders.Add("X-Content-Type-Options", "nosniff");
            }
        }

        public Uri AbsoluteRequestUri
        {
            get
            {
                return this.hostWrapper.AbsoluteRequestUri;
            }
        }

        public Uri AbsoluteServiceUri
        {
            get
            {
                return this.hostWrapper.AbsoluteServiceUri;
            }
        }

        internal IDataService CurrentDataService { get; private set; }

        internal DataServiceHostWrapper Host
        {
            get
            {
                return this.hostWrapper;
            }
        }

        public bool IsBatchRequest
        {
            get
            {
                if (!this.isBatchRequest.HasValue)
                {
					string[] strArray = RequestUriProcessor.EnumerateSegments(this.AbsoluteRequestUri, this.AbsoluteServiceUri);
                    if ((strArray.Length > 0) && (strArray[0] == "$batch"))
                    {
                        this.isBatchRequest = true;
                    }
                    else
                    {
                        this.isBatchRequest = false;
                    }
                }
                return this.isBatchRequest.Value;
            }
        }

        public WebHeaderCollection RequestHeaders
        {
            get
            {
                return this.hostWrapper.RequestHeaders;
            }
        }

        public string RequestMethod
        {
            get
            {
                return this.hostWrapper.RequestHttpMethod;
            }
        }

        public WebHeaderCollection ResponseHeaders
        {
            get
            {
                return this.hostWrapper.ResponseHeaders;
            }
        }

        public int ResponseStatusCode
        {
            get
            {
                return this.hostWrapper.ResponseStatusCode;
            }
            set
            {
                this.hostWrapper.ResponseStatusCode = value;
            }
        }
    }
}

