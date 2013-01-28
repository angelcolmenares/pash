namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data.Services.Client.Materialization;

    internal class DataServiceCollection<T> : ObservableCollection<T>
    {
        private Func<EntityCollectionChangedParams, bool> collectionChangedCallback;
        private DataServiceQueryContinuation<T> continuation;
        private Func<EntityChangedParams, bool> entityChangedCallback;
        private string entitySetName;
        private BindingObserver observer;
        private bool rootCollection;
        private bool trackingOnLoad;

        public DataServiceCollection() : this(null, null, TrackingMode.AutoChangeTracking, null, null, null)
        {
        }

        public DataServiceCollection(IEnumerable<T> items) : this(null, items, TrackingMode.AutoChangeTracking, null, null, null)
        {
        }

        public DataServiceCollection(DataServiceContext context) : this(context, null, TrackingMode.AutoChangeTracking, null, null, null)
        {
        }

        public DataServiceCollection(IEnumerable<T> items, TrackingMode trackingMode) : this(null, items, trackingMode, null, null, null)
        {
        }

        public DataServiceCollection(DataServiceContext context, string entitySetName, Func<EntityChangedParams, bool> entityChangedCallback, Func<EntityCollectionChangedParams, bool> collectionChangedCallback) : this(context, null, TrackingMode.AutoChangeTracking, entitySetName, entityChangedCallback, collectionChangedCallback)
        {
        }

        public DataServiceCollection(IEnumerable<T> items, TrackingMode trackingMode, string entitySetName, Func<EntityChangedParams, bool> entityChangedCallback, Func<EntityCollectionChangedParams, bool> collectionChangedCallback) : this(null, items, trackingMode, entitySetName, entityChangedCallback, collectionChangedCallback)
        {
        }

        public DataServiceCollection(DataServiceContext context, IEnumerable<T> items, TrackingMode trackingMode, string entitySetName, Func<EntityChangedParams, bool> entityChangedCallback, Func<EntityCollectionChangedParams, bool> collectionChangedCallback)
        {
            if (trackingMode == TrackingMode.AutoChangeTracking)
            {
                if (context == null)
                {
                    if (items == null)
                    {
                        this.trackingOnLoad = true;
                        this.entitySetName = entitySetName;
                        this.entityChangedCallback = entityChangedCallback;
                        this.collectionChangedCallback = collectionChangedCallback;
                    }
                    else
                    {
                        context = DataServiceCollection<T>.GetContextFromItems(items);
                    }
                }
                if (!this.trackingOnLoad)
                {
                    if (items != null)
                    {
                        DataServiceCollection<T>.ValidateIteratorParameter(items);
                    }
                    this.StartTracking(context, items, entitySetName, entityChangedCallback, collectionChangedCallback);
                }
            }
            else if (items != null)
            {
                this.Load(items);
            }
        }

        internal DataServiceCollection(object entityMaterializer, DataServiceContext context, IEnumerable<T> items, TrackingMode trackingMode, string entitySetName, Func<EntityChangedParams, bool> entityChangedCallback, Func<EntityCollectionChangedParams, bool> collectionChangedCallback) : this((context != null) ? context : ((ODataEntityMaterializer) entityMaterializer).ResponseInfo.Context, items, trackingMode, entitySetName, entityChangedCallback, collectionChangedCallback)
        {
            if (items != null)
            {
                ((ODataEntityMaterializer) entityMaterializer).PropagateContinuation<T>(items, (DataServiceCollection<T>) this);
            }
        }

        public void Clear(bool stopTracking)
        {
            if (!this.IsTracking)
            {
                throw new InvalidOperationException(Strings.DataServiceCollection_OperationForTrackedOnly);
            }
            if (!stopTracking)
            {
                base.Clear();
            }
            else
            {
                try
                {
                    this.observer.DetachBehavior = true;
                    base.Clear();
                }
                finally
                {
                    this.observer.DetachBehavior = false;
                }
            }
        }

        public void Detach()
        {
            if (!this.IsTracking)
            {
                throw new InvalidOperationException(Strings.DataServiceCollection_OperationForTrackedOnly);
            }
            if (!this.rootCollection)
            {
                throw new InvalidOperationException(Strings.DataServiceCollection_CannotStopTrackingChildCollection);
            }
            this.observer.StopTracking();
            this.observer = null;
            this.rootCollection = false;
        }

        private void FinishLoading()
        {
            if (this.IsTracking)
            {
                this.observer.AttachBehavior = false;
            }
        }

        private static DataServiceContext GetContextFromItems(IEnumerable<T> items)
        {
            DataServiceQuery<T> query = items as DataServiceQuery<T>;
            if (query != null)
            {
                DataServiceQueryProvider provider = query.Provider as DataServiceQueryProvider;
                return provider.Context;
            }
            QueryOperationResponse response = items as QueryOperationResponse;
            if (response == null)
            {
                throw new ArgumentException(Strings.DataServiceCollection_CannotDetermineContextFromItems);
            }
            return response.Results.Context;
        }

        protected override void InsertItem(int index, T item)
        {
            if (this.trackingOnLoad)
            {
                throw new InvalidOperationException(Strings.DataServiceCollection_InsertIntoTrackedButNotLoadedCollection);
            }
            if ((this.IsTracking && (item != null)) && !(item is INotifyPropertyChanged))
            {
                throw new InvalidOperationException(Strings.DataBinding_NotifyPropertyChangedNotImpl(item.GetType()));
            }
            base.InsertItem(index, item);
        }

        private void InternalLoadCollection(IEnumerable<T> items)
        {
            DataServiceQuery<T> query = items as DataServiceQuery<T>;
            if (query != null)
            {
                items = query.Execute() as QueryOperationResponse<T>;
            }
            foreach (T local in items)
            {
                if (!base.Contains(local))
                {
                    base.Add(local);
                }
            }
            QueryOperationResponse<T> response = items as QueryOperationResponse<T>;
            if (response != null)
            {
                this.continuation = response.GetContinuation();
            }
            else
            {
                this.continuation = null;
            }
        }

        public void Load(IEnumerable<T> items)
        {
            DataServiceCollection<T>.ValidateIteratorParameter(items);
            if (this.trackingOnLoad)
            {
                DataServiceContext contextFromItems = DataServiceCollection<T>.GetContextFromItems(items);
                this.trackingOnLoad = false;
                this.StartTracking(contextFromItems, items, this.entitySetName, this.entityChangedCallback, this.collectionChangedCallback);
            }
            else
            {
                this.StartLoading();
                try
                {
                    this.InternalLoadCollection(items);
                }
                finally
                {
                    this.FinishLoading();
                }
            }
        }

        public void Load(T item)
        {
            if (item == null)
            {
                throw Error.ArgumentNull("item");
            }
            this.StartLoading();
            try
            {
                if (!base.Contains(item))
                {
                    base.Add(item);
                }
            }
            finally
            {
                this.FinishLoading();
            }
        }

        private void StartLoading()
        {
            if (this.IsTracking)
            {
                if (this.observer.Context == null)
                {
                    throw new InvalidOperationException(Strings.DataServiceCollection_LoadRequiresTargetCollectionObserved);
                }
                this.observer.AttachBehavior = true;
            }
        }

        private void StartTracking(DataServiceContext context, IEnumerable<T> items, string entitySet, Func<EntityChangedParams, bool> entityChanged, Func<EntityCollectionChangedParams, bool> collectionChanged)
        {
            if (!BindingEntityInfo.IsEntityType(typeof(T), context.MaxProtocolVersion))
            {
                throw new ArgumentException(Strings.DataBinding_DataServiceCollectionArgumentMustHaveEntityType(typeof(T)));
            }
            this.observer = new BindingObserver(context, entityChanged, collectionChanged);
            if (items != null)
            {
                try
                {
                    this.InternalLoadCollection(items);
                }
                catch
                {
                    this.observer = null;
                    throw;
                }
            }
            this.observer.StartTracking<T>((DataServiceCollection<T>) this, entitySet);
            this.rootCollection = true;
        }

        private static void ValidateIteratorParameter(IEnumerable<T> items)
        {
            Util.CheckArgumentNull<IEnumerable<T>>(items, "items");
        }

        public DataServiceQueryContinuation<T> Continuation
        {
            get
            {
                return this.continuation;
            }
            set
            {
                this.continuation = value;
            }
        }

        internal bool IsTracking
        {
            get
            {
                return (this.observer != null);
            }
        }

        internal BindingObserver Observer
        {
            get
            {
                return this.observer;
            }
            set
            {
                this.observer = value;
            }
        }
    }
}

