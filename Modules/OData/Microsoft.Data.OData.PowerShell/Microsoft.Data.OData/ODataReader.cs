namespace Microsoft.Data.OData
{
    using System;
    using System.Threading.Tasks;

    internal abstract class ODataReader
    {
        protected ODataReader()
        {
        }

        public abstract bool Read();
        public abstract Task<bool> ReadAsync();

        public abstract ODataItem Item { get; }

        public abstract ODataReaderState State { get; }
    }
}

