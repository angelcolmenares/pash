namespace Microsoft.Data.OData
{
    using System;
    using System.Threading.Tasks;

    internal abstract class ODataCollectionReader
    {
        protected ODataCollectionReader()
        {
        }

        public abstract bool Read();
        public abstract Task<bool> ReadAsync();

        public abstract object Item { get; }

        public abstract ODataCollectionReaderState State { get; }
    }
}

