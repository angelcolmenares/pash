namespace System.Management.Automation.Language
{
    using System;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    public sealed class ArrayTypeName : ITypeName
    {
        private string _cachedFullName;
        private Type _cachedType;
        private readonly IScriptExtent _extent;

        public ArrayTypeName(IScriptExtent extent, ITypeName elementType, int rank)
        {
            if ((extent == null) || (elementType == null))
            {
                throw PSTraceSource.NewArgumentNullException((extent == null) ? "extent" : "name");
            }
            if (rank <= 0)
            {
                throw PSTraceSource.NewArgumentException("rank");
            }
            this._extent = extent;
            this.Rank = rank;
            this.ElementType = elementType;
        }

        private string GetName(bool includeAssemblyName)
        {
            StringBuilder builder = new StringBuilder();
            try
            {
                RuntimeHelpers.EnsureSufficientExecutionStack();
                builder.Append(this.ElementType.Name);
                builder.Append('[');
                if (this.Rank > 1)
                {
                    builder.Append(',', this.Rank - 1);
                }
                builder.Append(']');
                if (includeAssemblyName)
                {
                    string assemblyName = this.ElementType.AssemblyName;
                    if (assemblyName != null)
                    {
                        builder.Append(',');
                        builder.Append(assemblyName);
                    }
                }
            }
            catch (InsufficientExecutionStackException)
            {
                throw new ScriptCallDepthException();
            }
            return builder.ToString();
        }

        public Type GetReflectionAttributeType()
        {
            return null;
        }

        public Type GetReflectionType()
        {
            try
            {
                RuntimeHelpers.EnsureSufficientExecutionStack();
                if (this._cachedType == null)
                {
                    Type reflectionType = this.ElementType.GetReflectionType();
                    if (reflectionType != null)
                    {
                        Type type2 = (this.Rank == 1) ? reflectionType.MakeArrayType() : reflectionType.MakeArrayType(this.Rank);
                        Interlocked.CompareExchange<Type>(ref this._cachedType, type2, null);
                    }
                }
            }
            catch (InsufficientExecutionStackException)
            {
                throw new ScriptCallDepthException();
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
                return this.ElementType.AssemblyName;
            }
        }

        public ITypeName ElementType { get; private set; }

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
                    Interlocked.CompareExchange<string>(ref this._cachedFullName, this.GetName(true), null);
                }
                return this._cachedFullName;
            }
        }

        public bool IsArray
        {
            get
            {
                return true;
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
                return this.GetName(false);
            }
        }

        public int Rank { get; private set; }
    }
}

