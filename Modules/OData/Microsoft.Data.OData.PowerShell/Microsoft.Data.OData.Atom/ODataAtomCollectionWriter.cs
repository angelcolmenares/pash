namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Threading.Tasks;

    internal sealed class ODataAtomCollectionWriter : ODataCollectionWriterCore
    {
        private readonly ODataAtomCollectionSerializer atomCollectionSerializer;
        private readonly ODataAtomOutputContext atomOutputContext;

        internal ODataAtomCollectionWriter(ODataAtomOutputContext atomOutputContext, IEdmTypeReference expectedItemType, IODataReaderWriterListener listener) : base(atomOutputContext, expectedItemType, listener)
        {
            this.atomOutputContext = atomOutputContext;
            this.atomCollectionSerializer = new ODataAtomCollectionSerializer(atomOutputContext);
        }

        protected override void EndCollection()
        {
            this.atomOutputContext.XmlWriter.WriteEndElement();
        }

        protected override void EndPayload()
        {
            this.atomCollectionSerializer.WritePayloadEnd();
        }

        protected override Task FlushAsynchronously()
        {
            return this.atomOutputContext.FlushAsync();
        }

        protected override void FlushSynchronously()
        {
            this.atomOutputContext.Flush();
        }

        protected override void StartCollection(ODataCollectionStart collectionStart)
        {
            string name = collectionStart.Name;
            if (name == null)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomCollectionWriter_CollectionNameMustNotBeNull);
            }
            this.atomOutputContext.XmlWriter.WriteStartElement(name, this.atomCollectionSerializer.MessageWriterSettings.WriterBehavior.ODataNamespace);
            this.atomOutputContext.XmlWriter.WriteAttributeString("xmlns", "http://www.w3.org/2000/xmlns/", this.atomCollectionSerializer.MessageWriterSettings.WriterBehavior.ODataNamespace);
            this.atomCollectionSerializer.WriteDefaultNamespaceAttributes(ODataAtomSerializer.DefaultNamespaceFlags.Gml | ODataAtomSerializer.DefaultNamespaceFlags.GeoRss | ODataAtomSerializer.DefaultNamespaceFlags.ODataMetadata);
        }

        protected override void StartPayload()
        {
            this.atomCollectionSerializer.WritePayloadStart();
        }

        protected override void VerifyNotDisposed()
        {
            this.atomOutputContext.VerifyNotDisposed();
        }

        protected override void WriteCollectionItem(object item, IEdmTypeReference expectedItemType)
        {
            this.atomOutputContext.XmlWriter.WriteStartElement("element", this.atomCollectionSerializer.MessageWriterSettings.WriterBehavior.ODataNamespace);
            if (item == null)
            {
                ValidationUtils.ValidateNullCollectionItem(expectedItemType, this.atomOutputContext.MessageWriterSettings.WriterBehavior);
                this.atomOutputContext.XmlWriter.WriteAttributeString("null", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", "true");
            }
            else
            {
                ODataComplexValue complexValue = item as ODataComplexValue;
                if (complexValue != null)
                {
                    this.atomCollectionSerializer.WriteComplexValue(complexValue, expectedItemType, false, true, null, null, base.DuplicatePropertyNamesChecker, base.CollectionValidator, null, null, null);
                    base.DuplicatePropertyNamesChecker.Clear();
                }
                else
                {
                    this.atomCollectionSerializer.WritePrimitiveValue(item, base.CollectionValidator, expectedItemType);
                }
            }
            this.atomOutputContext.XmlWriter.WriteEndElement();
        }
    }
}

