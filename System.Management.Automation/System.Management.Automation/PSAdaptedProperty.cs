namespace System.Management.Automation
{
    using System;

    public class PSAdaptedProperty : PSProperty
    {
        public PSAdaptedProperty(string name, object tag) : base(name, null, null, tag)
        {
        }

        internal PSAdaptedProperty(string name, Adapter adapter, object baseObject, object adapterData) : base(name, adapter, baseObject, adapterData)
        {
        }

        public override PSMemberInfo Copy()
        {
            PSAdaptedProperty destiny = new PSAdaptedProperty(base.name, base.adapter, base.baseObject, base.adapterData);
            base.CloneBaseProperties(destiny);
            destiny.typeOfValue = base.typeOfValue;
            destiny.serializedValue = base.serializedValue;
            destiny.isDeserialized = base.isDeserialized;
            return destiny;
        }

        public object BaseObject
        {
            get
            {
                return base.baseObject;
            }
        }

        public object Tag
        {
            get
            {
                return base.adapterData;
            }
        }
    }
}

