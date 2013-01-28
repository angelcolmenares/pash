namespace System.Management.Automation.Language
{
    using Microsoft.PowerShell;
    using System;
    using System.Management.Automation;

    public sealed class ReflectionTypeName : ITypeName
    {
        private readonly Type _type;

        public ReflectionTypeName(Type type)
        {
            if (type == null)
            {
                throw PSTraceSource.NewArgumentNullException("type");
            }
            this._type = type;
        }

        public Type GetReflectionAttributeType()
        {
            return this._type;
        }

        public Type GetReflectionType()
        {
            return this._type;
        }

        public override string ToString()
        {
            return this.FullName;
        }

        public string AssemblyName
        {
            get
            {
                return this._type.Assembly.FullName;
            }
        }

        public IScriptExtent Extent
        {
            get
            {
                return PositionUtilities.EmptyExtent;
            }
        }

        public string FullName
        {
            get
            {
                return ToStringCodeMethods.Type(this._type, false);
            }
        }

        public bool IsArray
        {
            get
            {
                return this._type.IsArray;
            }
        }

        public bool IsGeneric
        {
            get
            {
                return this._type.IsGenericType;
            }
        }

        public string Name
        {
            get
            {
                return this.FullName;
            }
        }
    }
}

