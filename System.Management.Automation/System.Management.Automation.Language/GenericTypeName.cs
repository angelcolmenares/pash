namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    public sealed class GenericTypeName : ITypeName
    {
        private string _cachedFullName;
        private Type _cachedType;
        private readonly IScriptExtent _extent;

        public GenericTypeName(IScriptExtent extent, ITypeName genericTypeName, IEnumerable<ITypeName> genericArguments)
        {
            if ((genericTypeName == null) || (extent == null))
            {
                throw PSTraceSource.NewArgumentNullException((extent == null) ? "extent" : "genericTypeName");
            }
            if ((genericArguments == null) || !genericArguments.Any<ITypeName>())
            {
                throw PSTraceSource.NewArgumentException("genericArguments");
            }
            this._extent = extent;
            this.TypeName = genericTypeName;
            this.GenericArguments = new ReadOnlyCollection<ITypeName>(genericArguments.ToArray<ITypeName>());
        }

        internal Type GetGenericType(Type generic)
        {
            if (((generic == null) || !generic.ContainsGenericParameters) && (this.TypeName.FullName.IndexOf("`", StringComparison.OrdinalIgnoreCase) == -1))
            {
                generic = System.Management.Automation.Language.TypeName.ResolveSimpleType(string.Format(CultureInfo.InvariantCulture, "{0}`{1}", new object[] { this.TypeName.FullName, this.GenericArguments.Count }));
            }
            return generic;
        }

        public Type GetReflectionAttributeType()
        {
            Type reflectionType = this.GetReflectionType();
            if (reflectionType == null)
            {
                Type reflectionAttributeType = this.TypeName.GetReflectionAttributeType();
                if (((reflectionAttributeType == null) || !reflectionAttributeType.ContainsGenericParameters) && (this.TypeName.FullName.IndexOf("`", StringComparison.OrdinalIgnoreCase) == -1))
                {
                    reflectionAttributeType = System.Management.Automation.Language.TypeName.ResolveSimpleType(string.Format(CultureInfo.InvariantCulture, "{0}Attribute`{1}", new object[] { this.TypeName.FullName, this.GenericArguments.Count }));
                }
                if ((reflectionAttributeType == null) || !reflectionAttributeType.ContainsGenericParameters)
                {
                    return reflectionType;
                }
                reflectionType = reflectionAttributeType.MakeGenericType((from arg in this.GenericArguments select arg.GetReflectionType()).ToArray<Type>());
                Interlocked.CompareExchange<Type>(ref this._cachedType, reflectionType, null);
            }
            return reflectionType;
        }

        public Type GetReflectionType()
        {
            if (this._cachedType == null)
            {
                Type genericType = this.GetGenericType(this.TypeName.GetReflectionType());
                if ((genericType != null) && genericType.ContainsGenericParameters)
                {
                    Type[] source = (from arg in this.GenericArguments select arg.GetReflectionType()).ToArray<Type>();
                    if (source.Any<Type>(t => t == null))
                    {
                        return null;
                    }
                    try
                    {
                        Type type2 = genericType.MakeGenericType(source);
                        Interlocked.CompareExchange<Type>(ref this._cachedType, type2, null);
                    }
                    catch (Exception exception)
                    {
                        CommandProcessorBase.CheckForSevereException(exception);
                    }
                }
            }
            return this._cachedType;
        }

        public override string ToString()
        {
            return this.FullName;
        }

        public string AssemblyName
        {
            get
            {
                return this.TypeName.AssemblyName;
            }
        }

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
                if (this._cachedFullName == null)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append(this.TypeName.Name);
                    builder.Append('[');
                    bool flag = true;
                    foreach (ITypeName name in this.GenericArguments)
                    {
                        if (!flag)
                        {
                            builder.Append(',');
                        }
                        flag = false;
                        builder.Append(name.FullName);
                    }
                    builder.Append(']');
                    string assemblyName = this.TypeName.AssemblyName;
                    if (assemblyName != null)
                    {
                        builder.Append(',');
                        builder.Append(assemblyName);
                    }
                    Interlocked.CompareExchange<string>(ref this._cachedFullName, builder.ToString(), null);
                }
                return this._cachedFullName;
            }
        }

        public ReadOnlyCollection<ITypeName> GenericArguments { get; private set; }

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
                return true;
            }
        }

        public string Name
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(this.TypeName.Name);
                builder.Append('[');
                bool flag = true;
                foreach (ITypeName name in this.GenericArguments)
                {
                    if (!flag)
                    {
                        builder.Append(',');
                    }
                    flag = false;
                    builder.Append(name.Name);
                }
                builder.Append(']');
                return builder.ToString();
            }
        }

        public ITypeName TypeName { get; private set; }
    }
}

