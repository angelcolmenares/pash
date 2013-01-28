namespace System.Management.Automation
{
    using System;
    using System.Runtime.CompilerServices;

    internal class MergedCompiledCommandParameter
    {
        internal MergedCompiledCommandParameter(CompiledCommandParameter parameter, ParameterBinderAssociation binderAssociation)
        {
            this.Parameter = parameter;
            this.BinderAssociation = binderAssociation;
        }

        public override string ToString()
        {
            return this.Parameter.ToString();
        }

        internal ParameterBinderAssociation BinderAssociation { get; private set; }

        internal CompiledCommandParameter Parameter { get; private set; }
    }
}

