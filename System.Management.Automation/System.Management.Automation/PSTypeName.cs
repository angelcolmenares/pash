namespace System.Management.Automation
{
    using System;

    public class PSTypeName
    {
        private readonly string _name;
        private System.Type _type;
        private bool _typeWasCalculated;

        public PSTypeName(string name)
        {
            this._name = name;
            this._type = null;
        }

        public PSTypeName(System.Type type)
        {
            this._type = type;
            if (this._type != null)
            {
                this._name = this._type.FullName;
            }
        }

        public override string ToString()
        {
            return (this._name ?? string.Empty);
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public System.Type Type
        {
            get
            {
                if (!this._typeWasCalculated)
                {
                    if (this._type == null)
                    {
                        Exception exception;
                        this._type = LanguagePrimitives.ConvertStringToType(this._name, out exception);
                    }
                    if (((this._type == null) && (this._name != null)) && (this._name.StartsWith("[", StringComparison.OrdinalIgnoreCase) && this._name.EndsWith("]", StringComparison.OrdinalIgnoreCase)))
                    {
                        Exception exception2;
                        string typeName = this._name.Substring(1, this._name.Length - 2);
                        this._type = LanguagePrimitives.ConvertStringToType(typeName, out exception2);
                    }
                    this._typeWasCalculated = true;
                }
                return this._type;
            }
        }
    }
}

