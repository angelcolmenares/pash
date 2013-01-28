namespace System.Data.Services.Client
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;

    internal sealed class EntityCollectionChangedParams
    {
        private readonly NotifyCollectionChangedAction action;
        private readonly ICollection collection;
        private readonly DataServiceContext context;
        private readonly string propertyName;
        private readonly object sourceEntity;
        private readonly string sourceEntitySet;
        private readonly object targetEntity;
        private readonly string targetEntitySet;

        internal EntityCollectionChangedParams(DataServiceContext context, object sourceEntity, string propertyName, string sourceEntitySet, ICollection collection, object targetEntity, string targetEntitySet, NotifyCollectionChangedAction action)
        {
            this.context = context;
            this.sourceEntity = sourceEntity;
            this.propertyName = propertyName;
            this.sourceEntitySet = sourceEntitySet;
            this.collection = collection;
            this.targetEntity = targetEntity;
            this.targetEntitySet = targetEntitySet;
            this.action = action;
        }

        public NotifyCollectionChangedAction Action
        {
            get
            {
                return this.action;
            }
        }

        public ICollection Collection
        {
            get
            {
                return this.collection;
            }
        }

        public DataServiceContext Context
        {
            get
            {
                return this.context;
            }
        }

        public string PropertyName
        {
            get
            {
                return this.propertyName;
            }
        }

        public object SourceEntity
        {
            get
            {
                return this.sourceEntity;
            }
        }

        public string SourceEntitySet
        {
            get
            {
                return this.sourceEntitySet;
            }
        }

        public object TargetEntity
        {
            get
            {
                return this.targetEntity;
            }
        }

        public string TargetEntitySet
        {
            get
            {
                return this.targetEntitySet;
            }
        }
    }
}

