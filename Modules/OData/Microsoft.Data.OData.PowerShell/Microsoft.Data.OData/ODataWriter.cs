namespace Microsoft.Data.OData
{
    using System;
    using System.Threading.Tasks;

    internal abstract class ODataWriter
    {
        protected ODataWriter()
        {
        }

        public abstract void Flush();
        public abstract Task FlushAsync();
        public abstract void WriteEnd();
        public abstract Task WriteEndAsync();
        public abstract void WriteEntityReferenceLink(ODataEntityReferenceLink entityReferenceLink);
        public abstract Task WriteEntityReferenceLinkAsync(ODataEntityReferenceLink entityReferenceLink);
        public abstract void WriteStart(ODataEntry entry);
        public abstract void WriteStart(ODataFeed feed);
        public abstract void WriteStart(ODataNavigationLink navigationLink);
        public abstract Task WriteStartAsync(ODataEntry entry);
        public abstract Task WriteStartAsync(ODataFeed feed);
        public abstract Task WriteStartAsync(ODataNavigationLink navigationLink);
    }
}

