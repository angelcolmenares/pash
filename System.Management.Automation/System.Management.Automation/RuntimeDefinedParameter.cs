namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    public class RuntimeDefinedParameter
    {
        private readonly Collection<Attribute> _attributes;
        private string _name;
        private Type _parameterType;
        private object _value;

        public RuntimeDefinedParameter()
        {
            this._name = string.Empty;
            this._attributes = new Collection<Attribute>();
        }

        public RuntimeDefinedParameter(string name, Type parameterType, Collection<Attribute> attributes)
        {
            this._name = string.Empty;
            this._attributes = new Collection<Attribute>();
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            if (parameterType == null)
            {
                throw PSTraceSource.NewArgumentNullException("parameterType");
            }
            this._name = name;
            this._parameterType = parameterType;
            if (attributes != null)
            {
                this._attributes = attributes;
            }
        }

        public Collection<Attribute> Attributes
        {
            get
            {
                return this._attributes;
            }
        }

        public bool IsSet { get; set; }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw PSTraceSource.NewArgumentException("name");
                }
                this._name = value;
            }
        }

        public Type ParameterType
        {
            get
            {
                return this._parameterType;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("value");
                }
                this._parameterType = value;
            }
        }

        public object Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this.IsSet = true;
                this._value = value;
            }
        }
    }
}

