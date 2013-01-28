namespace Microsoft.PowerShell.Cmdletization
{
    using System;
    using System.Collections.ObjectModel;

    internal sealed class MethodParametersCollection : KeyedCollection<string, MethodParameter>
    {
        public MethodParametersCollection() : base(StringComparer.Ordinal, 5)
        {
        }

        protected override string GetKeyForItem(MethodParameter item)
        {
            return item.Name;
        }
    }
}

