namespace System.Management.Automation
{
    using System;
    using System.ComponentModel;
    using System.Threading;

    public class PSObjectTypeDescriptor : CustomTypeDescriptor
    {
        private PSObject _instance;
        internal static PSTraceSource typeDescriptor = PSTraceSource.GetTracer("TypeDescriptor", "Traces the behavior of PSObjectTypeDescriptor, PSObjectTypeDescriptionProvider and PSObjectPropertyDescriptor.", false);

        public event EventHandler<GettingValueExceptionEventArgs> GettingValueException;

        public event EventHandler<SettingValueExceptionEventArgs> SettingValueException;

        public PSObjectTypeDescriptor(PSObject instance)
        {
            this._instance = instance;
        }

        private void CheckAndAddProperty(PSPropertyInfo propertyInfo, Attribute[] attributes, ref PropertyDescriptorCollection returnValue)
        {
            using (typeDescriptor.TraceScope("Checking property \"{0}\".", new object[] { propertyInfo.Name }))
            {
                if (!propertyInfo.IsGettable)
                {
                    typeDescriptor.WriteLine("Property \"{0}\" is write-only so it has been skipped.", new object[] { propertyInfo.Name });
                }
                else
                {
                    AttributeCollection propertyAttributes = null;
                    Type propertyType = typeof(object);
                    if ((attributes != null) && (attributes.Length != 0))
                    {
                        PSProperty property = propertyInfo as PSProperty;
                        if (property != null)
                        {
                            DotNetAdapter.PropertyCacheEntry adapterData = property.adapterData as DotNetAdapter.PropertyCacheEntry;
                            if (adapterData == null)
                            {
                                typeDescriptor.WriteLine("Skipping attribute check for property \"{0}\" because it is an adapted property (not a .NET property).", new object[] { property.Name });
                            }
                            else if (property.isDeserialized)
                            {
                                typeDescriptor.WriteLine("Skipping attribute check for property \"{0}\" because it has been deserialized.", new object[] { property.Name });
                            }
                            else
                            {
                                propertyType = adapterData.propertyType;
                                propertyAttributes = adapterData.Attributes;
                                foreach (Attribute attribute in attributes)
                                {
                                    if (!propertyAttributes.Contains(attribute))
                                    {
                                        typeDescriptor.WriteLine("Property \"{0}\" does not contain attribute \"{1}\" so it has been skipped.", new object[] { property.Name, attribute });
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    if (propertyAttributes == null)
                    {
                        propertyAttributes = new AttributeCollection(new Attribute[0]);
                    }
                    typeDescriptor.WriteLine("Adding property \"{0}\".", new object[] { propertyInfo.Name });
                    PSObjectPropertyDescriptor descriptor = new PSObjectPropertyDescriptor(propertyInfo.Name, propertyType, !propertyInfo.IsSettable, propertyAttributes);
                    descriptor.SettingValueException += this.SettingValueException;
                    descriptor.GettingValueException += this.GettingValueException;
                    returnValue.Add(descriptor);
                }
            }
        }

        public override bool Equals(object obj)
        {
            PSObjectTypeDescriptor objB = obj as PSObjectTypeDescriptor;
            if (objB == null)
            {
                return false;
            }
            if ((this.Instance != null) && (objB.Instance != null))
            {
                return objB.Instance.Equals(this.Instance);
            }
            return object.ReferenceEquals(this, objB);
        }

        public override AttributeCollection GetAttributes()
        {
            if (this.Instance == null)
            {
                return new AttributeCollection(new Attribute[0]);
            }
            return TypeDescriptor.GetAttributes(this.Instance.BaseObject);
        }

        public override string GetClassName()
        {
            if (this.Instance == null)
            {
                return null;
            }
            return TypeDescriptor.GetClassName(this.Instance.BaseObject);
        }

        public override string GetComponentName()
        {
            if (this.Instance == null)
            {
                return null;
            }
            return TypeDescriptor.GetComponentName(this.Instance.BaseObject);
        }

        public override TypeConverter GetConverter()
        {
            if (this.Instance == null)
            {
                return new TypeConverter();
            }
            object baseObject = this.Instance.BaseObject;
            TypeConverter converter = LanguagePrimitives.GetConverter(baseObject.GetType(), null) as TypeConverter;
            if (converter == null)
            {
                converter = TypeDescriptor.GetConverter(baseObject);
            }
            return converter;
        }

        public override EventDescriptor GetDefaultEvent()
        {
            if (this.Instance == null)
            {
                return null;
            }
            return TypeDescriptor.GetDefaultEvent(this.Instance.BaseObject);
        }

        public override PropertyDescriptor GetDefaultProperty()
        {
            if (this.Instance != null)
            {
                string b = null;
                PSMemberSet pSStandardMembers = this.Instance.PSStandardMembers;
                if (pSStandardMembers != null)
                {
                    PSNoteProperty property = pSStandardMembers.Properties["DefaultDisplayProperty"] as PSNoteProperty;
                    if (property != null)
                    {
                        b = property.Value as string;
                    }
                }
                if (b == null)
                {
                    object[] customAttributes = this.Instance.BaseObject.GetType().GetCustomAttributes(typeof(DefaultPropertyAttribute), true);
                    if (customAttributes.Length == 1)
                    {
                        DefaultPropertyAttribute attribute = customAttributes[0] as DefaultPropertyAttribute;
                        if (attribute != null)
                        {
                            b = attribute.Name;
                        }
                    }
                }
                PropertyDescriptorCollection properties = this.GetProperties();
                if (b != null)
                {
                    foreach (PropertyDescriptor descriptor in properties)
                    {
                        if (string.Equals(descriptor.Name, b, StringComparison.OrdinalIgnoreCase))
                        {
                            return descriptor;
                        }
                    }
                }
            }
            return null;
        }

        public override object GetEditor(Type editorBaseType)
        {
            if (this.Instance == null)
            {
                return null;
            }
            return TypeDescriptor.GetEditor(this.Instance.BaseObject, editorBaseType);
        }

        public override EventDescriptorCollection GetEvents()
        {
            if (this.Instance == null)
            {
                return new EventDescriptorCollection(null);
            }
            return TypeDescriptor.GetEvents(this.Instance.BaseObject);
        }

        public override EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            if (this.Instance == null)
            {
                return null;
            }
            return TypeDescriptor.GetEvents(this.Instance.BaseObject, attributes);
        }

        public override int GetHashCode()
        {
            if (this.Instance == null)
            {
                return base.GetHashCode();
            }
            return this.Instance.GetHashCode();
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return this.GetProperties(null);
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            using (typeDescriptor.TraceScope("Getting properties.", new object[0]))
            {
                PropertyDescriptorCollection returnValue = new PropertyDescriptorCollection(null);
                if (this._instance != null)
                {
                    foreach (PSPropertyInfo info in this._instance.Properties)
                    {
                        this.CheckAndAddProperty(info, attributes, ref returnValue);
                    }
                }
                return returnValue;
            }
        }

        public override object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this.Instance;
        }

        public PSObject Instance
        {
            get
            {
                return this._instance;
            }
        }
    }
}

