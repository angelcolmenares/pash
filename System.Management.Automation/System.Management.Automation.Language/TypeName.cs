namespace System.Management.Automation.Language
{
    using System;
    using System.Linq;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public sealed class TypeName : ITypeName
    {
        internal readonly IScriptExtent _extent;
        internal readonly string _name;
        internal Type _type;

        public TypeName(IScriptExtent extent, string name)
        {
            if ((extent == null) || string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentNullException((extent == null) ? "extent" : "name");
            }
            if (name.FirstOrDefault<char>(delegate (char c) {
                if ((c != '[') && (c != ']'))
                {
                    return (c == ',');
                }
                return true;
            }) != '\0')
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            if (name.IndexOf('`') != -1)
            {
                name = name.Replace("``", "`");
            }
            this._extent = extent;
            this._name = name;
        }

        public TypeName(IScriptExtent extent, string name, string assembly) : this(extent, name)
        {
            if (string.IsNullOrEmpty(assembly))
            {
                throw PSTraceSource.NewArgumentNullException("assembly");
            }
            this.AssemblyName = assembly;
        }

        public Type GetReflectionAttributeType()
        {
            Type reflectionType = this.GetReflectionType();
            if ((reflectionType == null) || !typeof(Attribute).IsAssignableFrom(reflectionType))
            {
                reflectionType = ResolveSimpleType(this.FullName + "Attribute");
                if ((reflectionType != null) && !typeof(Attribute).IsAssignableFrom(reflectionType))
                {
                    reflectionType = null;
                }
            }
            return reflectionType;
        }

        public Type GetReflectionType()
        {
            if (this._type == null)
            {
                Type type = ResolveSimpleType(this.FullName);
                Interlocked.CompareExchange<Type>(ref this._type, type, null);
            }
            return this._type;
        }

        internal static Type ResolveSimpleType(string name)
        {
            Type type;
            LanguagePrimitives.TryConvertTo<Type>(name, out type);
            return type;
        }

        public override string ToString()
        {
            return this.FullName;
        }

        public string AssemblyName { get; internal set; }

        public IScriptExtent Extent
        {
            get
            {
                return this._extent;
            }
        }

        public string FullName
        {
            get
            {
                if (this.AssemblyName == null)
                {
                    return this._name;
                }
                return (this._name + "," + this.AssemblyName);
            }
        }

        public bool IsArray
        {
            get
            {
                return false;
            }
        }

        public bool IsGeneric
        {
            get
            {
                return false;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }
    }
}

