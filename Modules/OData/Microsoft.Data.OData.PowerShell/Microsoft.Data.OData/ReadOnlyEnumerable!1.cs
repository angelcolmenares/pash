namespace Microsoft.Data.OData
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal sealed class ReadOnlyEnumerable<T> : ReadOnlyEnumerable, IEnumerable<T>, IEnumerable
    {
        private readonly List<T> sourceList;

        internal ReadOnlyEnumerable() : this(new List<T>())
        {
        }

        internal ReadOnlyEnumerable(List<T> sourceList) : base(sourceList)
        {
            this.sourceList = sourceList;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.sourceList.GetEnumerator();
        }

        internal List<T> SourceList
        {
            get
            {
                return this.sourceList;
            }
        }
    }
}

