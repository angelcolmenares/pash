namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Runspaces;
    using System.Text;

    public class PSProperty : PSPropertyInfo
    {
        internal Adapter adapter;
        internal object adapterData;
        internal object baseObject;
        internal bool isDeserialized;
        internal object serializedValue;
        internal string typeOfValue;

        internal PSProperty(string name, object serializedValue)
        {
            this.isDeserialized = true;
            this.serializedValue = serializedValue;
            base.name = name;
        }

        internal PSProperty(string name, Adapter adapter, object baseObject, object adapterData)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            base.name = name;
            this.adapter = adapter;
            this.adapterData = adapterData;
            this.baseObject = baseObject;
        }

        public override PSMemberInfo Copy()
        {
            PSProperty destiny = new PSProperty(base.name, this.adapter, this.baseObject, this.adapterData);
            base.CloneBaseProperties(destiny);
            destiny.typeOfValue = this.typeOfValue;
            destiny.serializedValue = this.serializedValue;
            destiny.isDeserialized = this.isDeserialized;
            return destiny;
        }

        private object GetAdaptedValue()
        {
            if (this.isDeserialized)
            {
                return this.serializedValue;
            }
            return this.adapter.BasePropertyGet(this);
        }

        internal void SetAdaptedValue(object setValue, bool shouldConvert)
        {
            if (this.isDeserialized)
            {
                this.serializedValue = setValue;
            }
            else
            {
                this.adapter.BasePropertySet(this, setValue, shouldConvert);
            }
        }

        public override string ToString()
        {
            if (this.isDeserialized)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(this.TypeNameOfValue);
                builder.Append(" {get;set;}");
                return builder.ToString();
            }
            return this.adapter.BasePropertyToString(this);
        }

        public override bool IsGettable
        {
            get
            {
                return (this.isDeserialized || this.adapter.BasePropertyIsGettable(this));
            }
        }

        public override bool IsSettable
        {
            get
            {
                return (this.isDeserialized || this.adapter.BasePropertyIsSettable(this));
            }
        }

        public override PSMemberTypes MemberType
        {
            get
            {
                return PSMemberTypes.Property;
            }
        }

        public override string TypeNameOfValue
        {
            get
            {
                if (!this.isDeserialized)
                {
                    return this.adapter.BasePropertyType(this);
                }
                if (this.serializedValue == null)
                {
                    return string.Empty;
                }
                PSObject serializedValue = this.serializedValue as PSObject;
                if (serializedValue != null)
                {
                    ConsolidatedString internalTypeNames = serializedValue.InternalTypeNames;
                    if ((internalTypeNames != null) && (internalTypeNames.Count >= 1))
                    {
                        return internalTypeNames[0];
                    }
                }
                return this.serializedValue.GetType().FullName;
            }
        }

        public override object Value
        {
            get
            {
                return this.GetAdaptedValue();
            }
            set
            {
                this.SetAdaptedValue(value, true);
            }
        }
    }
}

