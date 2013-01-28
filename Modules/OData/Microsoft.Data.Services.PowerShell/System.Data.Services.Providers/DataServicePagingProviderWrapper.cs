namespace System.Data.Services.Providers
{
    using System;
    using System.Data.Services;

    internal sealed class DataServicePagingProviderWrapper
    {
        private bool checkedForIDataServicePagingProvider;
        private IDataServicePagingProvider pagingProvider;
        private readonly IDataService service;

        public DataServicePagingProviderWrapper(IDataService serviceInstance)
        {
            this.service = serviceInstance;
        }

        internal void DisposeProvider()
        {
            if (this.pagingProvider != null)
            {
                WebUtil.Dispose(this.pagingProvider);
                this.pagingProvider = null;
            }
        }

        public bool IsCustomPagedForQuery
        {
            get
            {
                return (this.PagingProviderInterface != null);
            }
        }

        public bool IsCustomPagedForSerialization
        {
            get
            {
                return (this.checkedForIDataServicePagingProvider && (this.pagingProvider != null));
            }
        }

        public IDataServicePagingProvider PagingProviderInterface
        {
            get
            {
                if (!this.checkedForIDataServicePagingProvider)
                {
                    this.pagingProvider = this.service.Provider.GetService<IDataServicePagingProvider>();
                    this.checkedForIDataServicePagingProvider = true;
                }
                return this.pagingProvider;
            }
        }
    }
}

