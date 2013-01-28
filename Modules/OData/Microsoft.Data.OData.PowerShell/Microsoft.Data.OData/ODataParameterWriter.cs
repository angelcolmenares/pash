namespace Microsoft.Data.OData
{
    using System;
    using System.Threading.Tasks;

    internal abstract class ODataParameterWriter
    {
        protected ODataParameterWriter()
        {
        }

        public abstract ODataCollectionWriter CreateCollectionWriter(string parameterName);
        public abstract Task<ODataCollectionWriter> CreateCollectionWriterAsync(string parameterName);
        public abstract void Flush();
        public abstract Task FlushAsync();
        public abstract void WriteEnd();
        public abstract Task WriteEndAsync();
        public abstract void WriteStart();
        public abstract Task WriteStartAsync();
        public abstract void WriteValue(string parameterName, object parameterValue);
        public abstract Task WriteValueAsync(string parameterName, object parameterValue);
    }
}

