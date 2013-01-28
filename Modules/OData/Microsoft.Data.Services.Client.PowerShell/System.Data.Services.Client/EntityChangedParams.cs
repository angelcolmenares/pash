namespace System.Data.Services.Client
{
    using System;

    internal sealed class EntityChangedParams
    {
        private readonly DataServiceContext context;
        private readonly object entity;
        private readonly string propertyName;
        private readonly object propertyValue;
        private readonly string sourceEntitySet;
        private readonly string targetEntitySet;

        internal EntityChangedParams(DataServiceContext context, object entity, string propertyName, object propertyValue, string sourceEntitySet, string targetEntitySet)
        {
            this.context = context;
            this.entity = entity;
            this.propertyName = propertyName;
            this.propertyValue = propertyValue;
            this.sourceEntitySet = sourceEntitySet;
            this.targetEntitySet = targetEntitySet;
        }

        public DataServiceContext Context
        {
            get
            {
                return this.context;
            }
        }

        public object Entity
        {
            get
            {
                return this.entity;
            }
        }

        public string PropertyName
        {
            get
            {
                return this.propertyName;
            }
        }

        public object PropertyValue
        {
            get
            {
                return this.propertyValue;
            }
        }

        public string SourceEntitySet
        {
            get
            {
                return this.sourceEntitySet;
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

