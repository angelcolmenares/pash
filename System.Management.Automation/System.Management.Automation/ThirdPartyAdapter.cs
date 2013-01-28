namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal class ThirdPartyAdapter : PropertyOnlyAdapter
    {
        private Type adaptedType;
        private PSPropertyAdapter externalAdapter;

        internal ThirdPartyAdapter(Type adaptedType, PSPropertyAdapter externalAdapter)
        {
            this.adaptedType = adaptedType;
            this.externalAdapter = externalAdapter;
        }

        protected override void DoAddAllProperties<T>(object obj, PSMemberInfoInternalCollection<T> members)
        {
            Collection<PSAdaptedProperty> properties = null;
            try
            {
                properties = this.externalAdapter.GetProperties(obj);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw new ExtendedTypeSystemException("PSPropertyAdapter.GetProperties", exception, ExtendedTypeSystem.GetProperties, new object[] { obj.ToString() });
            }
            if (properties == null)
            {
                throw new ExtendedTypeSystemException("PSPropertyAdapter.NullReturnValueError", null, ExtendedTypeSystem.NullReturnValueError, new object[] { "PSPropertyAdapter.GetProperties" });
            }
            foreach (PSAdaptedProperty property in properties)
            {
                this.InitializeProperty(property, obj);
                members.Add(property as T);
            }
        }

        protected override PSProperty DoGetProperty(object obj, string propertyName)
        {
            PSAdaptedProperty property = null;
            try
            {
                property = this.externalAdapter.GetProperty(obj, propertyName);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw new ExtendedTypeSystemException("PSPropertyAdapter.GetProperty", exception, ExtendedTypeSystem.GetProperty, new object[] { propertyName, obj.ToString() });
            }
            if (property != null)
            {
                this.InitializeProperty(property, obj);
            }
            return property;
        }

        protected override IEnumerable<string> GetTypeNameHierarchy(object obj)
        {
            Collection<string> typeNameHierarchy = null;
            try
            {
                typeNameHierarchy = this.externalAdapter.GetTypeNameHierarchy(obj);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw new ExtendedTypeSystemException("PSPropertyAdapter.GetTypeNameHierarchyError", exception, ExtendedTypeSystem.GetTypeNameHierarchyError, new object[] { obj.ToString() });
            }
            if (typeNameHierarchy == null)
            {
                throw new ExtendedTypeSystemException("PSPropertyAdapter.NullReturnValueError", null, ExtendedTypeSystem.NullReturnValueError, new object[] { "PSPropertyAdapter.GetTypeNameHierarchy" });
            }
            return typeNameHierarchy;
        }

        private void InitializeProperty(PSAdaptedProperty property, object baseObject)
        {
            if (property.adapter == null)
            {
                property.adapter = this;
                property.baseObject = baseObject;
            }
        }

        protected override object PropertyGet(PSProperty property)
        {
            object propertyValue;
            PSAdaptedProperty adaptedProperty = property as PSAdaptedProperty;
            try
            {
                propertyValue = this.externalAdapter.GetPropertyValue(adaptedProperty);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw new ExtendedTypeSystemException("PSPropertyAdapter.PropertyGetError", exception, ExtendedTypeSystem.PropertyGetError, new object[] { property.Name });
            }
            return propertyValue;
        }

        protected override bool PropertyIsGettable(PSProperty property)
        {
            bool flag;
            PSAdaptedProperty adaptedProperty = property as PSAdaptedProperty;
            try
            {
                flag = this.externalAdapter.IsGettable(adaptedProperty);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw new ExtendedTypeSystemException("PSPropertyAdapter.PropertyIsGettableError", exception, ExtendedTypeSystem.PropertyIsGettableError, new object[] { property.Name });
            }
            return flag;
        }

        protected override bool PropertyIsSettable(PSProperty property)
        {
            bool flag;
            PSAdaptedProperty adaptedProperty = property as PSAdaptedProperty;
            try
            {
                flag = this.externalAdapter.IsSettable(adaptedProperty);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw new ExtendedTypeSystemException("PSPropertyAdapter.PropertyIsSettableError", exception, ExtendedTypeSystem.PropertyIsSettableError, new object[] { property.Name });
            }
            return flag;
        }

        protected override void PropertySet(PSProperty property, object setValue, bool convertIfPossible)
        {
            PSAdaptedProperty adaptedProperty = property as PSAdaptedProperty;
            try
            {
                this.externalAdapter.SetPropertyValue(adaptedProperty, setValue);
            }
            catch (SetValueException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw new ExtendedTypeSystemException("PSPropertyAdapter.PropertySetError", exception, ExtendedTypeSystem.PropertySetError, new object[] { property.Name });
            }
        }

        protected override string PropertyType(PSProperty property, bool forDisplay)
        {
            PSAdaptedProperty adaptedProperty = property as PSAdaptedProperty;
            string propertyTypeName = null;
            try
            {
                propertyTypeName = this.externalAdapter.GetPropertyTypeName(adaptedProperty);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw new ExtendedTypeSystemException("PSPropertyAdapter.PropertyTypeError", exception, ExtendedTypeSystem.PropertyTypeError, new object[] { property.Name });
            }
            return (propertyTypeName ?? "System.Object");
        }

        internal Type AdaptedType
        {
            get
            {
                return this.adaptedType;
            }
        }

        internal Type ExternalAdapterType
        {
            get
            {
                return this.externalAdapter.GetType();
            }
        }
    }
}

