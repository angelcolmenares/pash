namespace Microsoft.PowerShell.Cmdletization
{
    using System;
    using System.Runtime.CompilerServices;

    public sealed class MethodParameter
    {
        public MethodParameterBindings Bindings { get; set; }

        public bool IsValuePresent { get; set; }

        public string Name { get; set; }

        public Type ParameterType { get; set; }

        public string ParameterTypeName { get; set; }

        public object Value { get; set; }
    }
}

