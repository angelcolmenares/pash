namespace Microsoft.PowerShell.Cim
{
    using Microsoft.Management.Infrastructure;
    using Microsoft.PowerShell;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;

    public sealed class CimInstanceAdapter : PSPropertyAdapter
    {
        private void AddTypeNameHierarchy(IList<string> typeNamesWithNamespace, IList<string> typeNamesWithoutNamespace, string namespaceName, string className)
        {
            if (!string.IsNullOrEmpty(namespaceName))
            {
                string item = string.Format(CultureInfo.InvariantCulture, "Microsoft.Management.Infrastructure.CimInstance#{0}/{1}", new object[] { namespaceName, className });
                typeNamesWithNamespace.Add(item);
            }
            typeNamesWithoutNamespace.Add(string.Format(CultureInfo.InvariantCulture, "Microsoft.Management.Infrastructure.CimInstance#{0}", new object[] { className }));
        }

        private static PSAdaptedProperty GetCimPropertyAdapter(CimProperty property, object baseObject)
        {
            try
            {
                string name = property.Name;
                return GetCimPropertyAdapter(property, baseObject, name);
            }
            catch (CimException)
            {
                return null;
            }
        }

        private static PSAdaptedProperty GetCimPropertyAdapter(CimProperty property, object baseObject, string propertyName)
        {
            return new PSAdaptedProperty(propertyName, property) { baseObject = baseObject };
        }

        private List<CimClass> GetInheritanceChain(CimInstance cimInstance)
        {
            List<CimClass> list = new List<CimClass>();
            CimClass cimClass = cimInstance.CimClass;
            while (cimClass != null)
            {
                list.Add(cimClass);
                try
                {
                    cimClass = cimClass.CimSuperClass;
					if (cimClass == null) break;
                    continue;
                }
                catch (CimException)
                {
                    return list;
                }
            }
            return list;
        }

        public override Collection<PSAdaptedProperty> GetProperties(object baseObject)
        {
            CimInstance cimInstance = baseObject as CimInstance;
            if (cimInstance == null)
            {
                throw new PSInvalidOperationException(string.Format(CultureInfo.InvariantCulture, CimInstanceTypeAdapterResources.BaseObjectNotCimInstance, new object[] { "baseObject", typeof(CimInstance).ToString() }));
            }
            Collection<PSAdaptedProperty> collection = new Collection<PSAdaptedProperty>();
            if (cimInstance.CimInstanceProperties != null)
            {
                foreach (CimProperty property in cimInstance.CimInstanceProperties)
                {
                    PSAdaptedProperty cimPropertyAdapter = GetCimPropertyAdapter(property, baseObject);
                    if (cimPropertyAdapter != null)
                    {
                        collection.Add(cimPropertyAdapter);
                    }
                }
            }
            PSAdaptedProperty pSComputerNameAdapter = GetPSComputerNameAdapter(cimInstance);
            if (pSComputerNameAdapter != null)
            {
                collection.Add(pSComputerNameAdapter);
            }
            return collection;
        }

        public override PSAdaptedProperty GetProperty(object baseObject, string propertyName)
        {
            if (propertyName == null)
            {
                throw new PSArgumentNullException("propertyName");
            }
            CimInstance cimInstance = baseObject as CimInstance;
            if (cimInstance == null)
            {
                throw new PSInvalidOperationException(string.Format(CultureInfo.InvariantCulture, CimInstanceTypeAdapterResources.BaseObjectNotCimInstance, new object[] { "baseObject", typeof(CimInstance).ToString() }));
            }
            CimProperty property = cimInstance.CimInstanceProperties[propertyName];
            if (property != null)
            {
                return GetCimPropertyAdapter(property, baseObject, propertyName);
            }
            if (propertyName.Equals(RemotingConstants.ComputerNameNoteProperty, StringComparison.OrdinalIgnoreCase))
            {
                return GetPSComputerNameAdapter(cimInstance);
            }
            return null;
        }

        public override string GetPropertyTypeName(PSAdaptedProperty adaptedProperty)
        {
            if (adaptedProperty == null)
            {
                throw new ArgumentNullException("adaptedProperty");
            }
            CimProperty tag = adaptedProperty.Tag as CimProperty;
            if (tag == null)
            {
                if (!adaptedProperty.Name.Equals(RemotingConstants.ComputerNameNoteProperty, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentNullException("adaptedProperty");
                }
                return ToStringCodeMethods.Type(typeof(string), false);
            }
            switch (tag.CimType)
            {
                case CimType.DateTime:
                case CimType.Reference:
                case CimType.Instance:
                case CimType.DateTimeArray:
                case CimType.ReferenceArray:
                case CimType.InstanceArray:
                    return ("CimInstance#" + tag.CimType.ToString());
            }
            return ToStringCodeMethods.Type(CimConverter.GetDotNetType(tag.CimType), false);
        }

        public override object GetPropertyValue(PSAdaptedProperty adaptedProperty)
        {
            if (adaptedProperty == null)
            {
                throw new ArgumentNullException("adaptedProperty");
            }
            CimProperty tag = adaptedProperty.Tag as CimProperty;
            if (tag != null)
            {
                return tag.Value;
            }
            if (!adaptedProperty.Name.Equals(RemotingConstants.ComputerNameNoteProperty, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentNullException("adaptedProperty");
            }
            CimInstance instance = (CimInstance) adaptedProperty.Tag;
            return instance.GetCimSessionComputerName();
        }

        private static PSAdaptedProperty GetPSComputerNameAdapter(CimInstance cimInstance)
        {
            return new PSAdaptedProperty(RemotingConstants.ComputerNameNoteProperty, cimInstance) { baseObject = cimInstance };
        }

        public override Collection<string> GetTypeNameHierarchy(object baseObject)
        {
            CimInstance cimInstance = baseObject as CimInstance;
            if (cimInstance == null)
            {
                throw new ArgumentNullException("baseObject");
            }
            List<string> typeNamesWithNamespace = new List<string>();
            List<string> typeNamesWithoutNamespace = new List<string>();
            IList<CimClass> inheritanceChain = this.GetInheritanceChain(cimInstance);
            if ((inheritanceChain == null) || (inheritanceChain.Count == 0))
            {
                this.AddTypeNameHierarchy(typeNamesWithNamespace, typeNamesWithoutNamespace, cimInstance.CimSystemProperties.Namespace, cimInstance.CimSystemProperties.ClassName);
            }
            else
            {
                foreach (CimClass class2 in inheritanceChain)
                {
                    this.AddTypeNameHierarchy(typeNamesWithNamespace, typeNamesWithoutNamespace, class2.CimSystemProperties.Namespace, class2.CimSystemProperties.ClassName);
                    class2.Dispose();
                }
            }
            List<string> list = new List<string>();
            list.AddRange(typeNamesWithNamespace);
            list.AddRange(typeNamesWithoutNamespace);
            if (baseObject != null)
            {
                for (Type type = baseObject.GetType(); type != null; type = type.BaseType)
                {
                    list.Add(type.FullName);
                }
            }
            return new Collection<string>(list);
        }

        public override bool IsGettable(PSAdaptedProperty adaptedProperty)
        {
            return true;
        }

        public override bool IsSettable(PSAdaptedProperty adaptedProperty)
        {
            if (adaptedProperty == null)
            {
                return false;
            }
            CimProperty tag = adaptedProperty.Tag as CimProperty;
            if (tag == null)
            {
                return false;
            }
            bool flag = CimFlags.ReadOnly == (tag.Flags & CimFlags.ReadOnly);
            return !flag;
        }

        public override void SetPropertyValue(PSAdaptedProperty adaptedProperty, object value)
        {
            if (adaptedProperty == null)
            {
                throw new ArgumentNullException("adaptedProperty");
            }
            if (!this.IsSettable(adaptedProperty))
            {
                throw new SetValueException("ReadOnlyCIMProperty", null, CimInstanceTypeAdapterResources.ReadOnlyCIMProperty, new object[] { adaptedProperty.Name });
            }
            CimProperty tag = adaptedProperty.Tag as CimProperty;
            object obj2 = value;
            if (obj2 != null)
            {
                Type dotNetType;
                CimType cimType = tag.CimType;
                if (cimType == CimType.DateTime)
                {
                    dotNetType = typeof(object);
                }
                else if (cimType == CimType.DateTimeArray)
                {
                    dotNetType = typeof(object[]);
                }
                else
                {
                    dotNetType = CimConverter.GetDotNetType(tag.CimType);
                }
                obj2 = Adapter.PropertySetAndMethodArgumentConvertTo(value, dotNetType, CultureInfo.InvariantCulture);
            }
            tag.Value = obj2;
        }
    }
}

