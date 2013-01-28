namespace Microsoft.Management.Infrastructure.Generic
{
    using System;

    public abstract class CimKeyedCollection<T> : CimReadOnlyKeyedCollection<T>
    {
        internal CimKeyedCollection()
        {
        }

        public abstract void Add(T newItem);
    }
}

