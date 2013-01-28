namespace Microsoft.Data.OData
{
    using System;
    using System.Threading.Tasks;

    internal abstract class ODataParameterReader
    {
        protected ODataParameterReader()
        {
        }

        public abstract ODataCollectionReader CreateCollectionReader();
        public abstract bool Read();
        public abstract Task<bool> ReadAsync();

        public abstract string Name { get; }

        public abstract ODataParameterReaderState State { get; }

        public abstract object Value { get; }
    }
}

