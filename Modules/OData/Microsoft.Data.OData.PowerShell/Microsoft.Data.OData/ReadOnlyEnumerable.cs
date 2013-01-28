namespace Microsoft.Data.OData
{
    using System;
    using System.Collections;

    internal class ReadOnlyEnumerable : IEnumerable
    {
        private readonly IEnumerable sourceEnumerable;

        internal ReadOnlyEnumerable(IEnumerable sourceEnumerable)
        {
            this.sourceEnumerable = sourceEnumerable;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.sourceEnumerable.GetEnumerator();
        }
    }
}

