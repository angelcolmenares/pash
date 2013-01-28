namespace Microsoft.Data.OData
{
    using System;
    using System.Threading.Tasks;

    internal abstract class ODataCollectionWriter
    {
        protected ODataCollectionWriter()
        {
        }

        public abstract void Flush();
        public abstract Task FlushAsync();
        public abstract void WriteEnd();
        public abstract Task WriteEndAsync();
        public abstract void WriteItem(object item);
        public abstract Task WriteItemAsync(object item);
        public abstract void WriteStart(ODataCollectionStart collectionStart);
        public abstract Task WriteStartAsync(ODataCollectionStart collectionStart);
    }
}

