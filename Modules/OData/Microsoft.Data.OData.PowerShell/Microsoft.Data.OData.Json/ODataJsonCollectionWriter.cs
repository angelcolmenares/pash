namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Threading.Tasks;

    internal sealed class ODataJsonCollectionWriter : ODataCollectionWriterCore
    {
        private readonly ODataJsonCollectionSerializer jsonCollectionSerializer;
        private readonly ODataJsonOutputContext jsonOutputContext;

        internal ODataJsonCollectionWriter(ODataJsonOutputContext jsonOutputContext, IEdmTypeReference expectedItemType, IODataReaderWriterListener listener) : base(jsonOutputContext, expectedItemType, listener)
        {
            this.jsonOutputContext = jsonOutputContext;
            this.jsonCollectionSerializer = new ODataJsonCollectionSerializer(this.jsonOutputContext);
        }

        protected override void EndCollection()
        {
            this.jsonCollectionSerializer.WriteCollectionEnd();
        }

        protected override void EndPayload()
        {
            this.jsonCollectionSerializer.WritePayloadEnd();
        }

        protected override Task FlushAsynchronously()
        {
            return this.jsonOutputContext.FlushAsync();
        }

        protected override void FlushSynchronously()
        {
            this.jsonOutputContext.Flush();
        }

        protected override void StartCollection(ODataCollectionStart collectionStart)
        {
            this.jsonCollectionSerializer.WriteCollectionStart();
        }

        protected override void StartPayload()
        {
            this.jsonCollectionSerializer.WritePayloadStart();
        }

        protected override void VerifyNotDisposed()
        {
            this.jsonOutputContext.VerifyNotDisposed();
        }

        protected override void WriteCollectionItem(object item, IEdmTypeReference expectedItemType)
        {
            if (item == null)
            {
                ValidationUtils.ValidateNullCollectionItem(expectedItemType, this.jsonOutputContext.MessageWriterSettings.WriterBehavior);
                this.jsonOutputContext.JsonWriter.WriteValue((string) null);
            }
            else
            {
                ODataComplexValue complexValue = item as ODataComplexValue;
                if (complexValue != null)
                {
                    this.jsonCollectionSerializer.WriteComplexValue(complexValue, expectedItemType, false, base.DuplicatePropertyNamesChecker, base.CollectionValidator);
                    base.DuplicatePropertyNamesChecker.Clear();
                }
                else
                {
                    this.jsonCollectionSerializer.WritePrimitiveValue(item, base.CollectionValidator, expectedItemType);
                }
            }
        }
    }
}

