namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;

    public class PSParameterizedProperty : PSMethodInfo
    {
        internal Adapter adapter;
        internal object adapterData;
        internal object baseObject;

        internal PSParameterizedProperty(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            base.name = name;
        }

        internal PSParameterizedProperty(string name, Adapter adapter, object baseObject, object adapterData)
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
            PSParameterizedProperty destiny = new PSParameterizedProperty(base.name, this.adapter, this.baseObject, this.adapterData);
            base.CloneBaseProperties(destiny);
            return destiny;
        }

        public override object Invoke(params object[] arguments)
        {
            if (arguments == null)
            {
                throw PSTraceSource.NewArgumentNullException("arguments");
            }
            return this.adapter.BaseParameterizedPropertyGet(this, arguments);
        }

        public void InvokeSet(object valueToSet, params object[] arguments)
        {
            if (arguments == null)
            {
                throw PSTraceSource.NewArgumentNullException("arguments");
            }
            this.adapter.BaseParameterizedPropertySet(this, valueToSet, arguments);
        }

        public override string ToString()
        {
            return this.adapter.BaseParameterizedPropertyToString(this);
        }

        public bool IsGettable
        {
            get
            {
                return this.adapter.BaseParameterizedPropertyIsGettable(this);
            }
        }

        public bool IsSettable
        {
            get
            {
                return this.adapter.BaseParameterizedPropertyIsSettable(this);
            }
        }

        public override PSMemberTypes MemberType
        {
            get
            {
                return PSMemberTypes.ParameterizedProperty;
            }
        }

        public override Collection<string> OverloadDefinitions
        {
            get
            {
                return this.adapter.BaseParameterizedPropertyDefinitions(this);
            }
        }

        public override string TypeNameOfValue
        {
            get
            {
                return this.adapter.BaseParameterizedPropertyType(this);
            }
        }
    }
}

