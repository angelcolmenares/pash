namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;

    public abstract class PSPropertyAdapter
    {
        protected PSPropertyAdapter()
        {
        }

        public abstract Collection<PSAdaptedProperty> GetProperties(object baseObject);
        public abstract PSAdaptedProperty GetProperty(object baseObject, string propertyName);
        public abstract string GetPropertyTypeName(PSAdaptedProperty adaptedProperty);
        public abstract object GetPropertyValue(PSAdaptedProperty adaptedProperty);
        public virtual Collection<string> GetTypeNameHierarchy(object baseObject)
        {
            if (baseObject == null)
            {
                throw new ArgumentNullException("baseObject");
            }
            Collection<string> collection = new Collection<string>();
            for (Type type = baseObject.GetType(); type != null; type = type.BaseType)
            {
                collection.Add(type.FullName);
            }
            return collection;
        }

        public abstract bool IsGettable(PSAdaptedProperty adaptedProperty);
        public abstract bool IsSettable(PSAdaptedProperty adaptedProperty);
        public abstract void SetPropertyValue(PSAdaptedProperty adaptedProperty, object value);
    }
}

