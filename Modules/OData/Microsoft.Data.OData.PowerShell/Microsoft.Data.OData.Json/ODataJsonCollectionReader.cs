namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;

    internal sealed class ODataJsonCollectionReader : ODataCollectionReaderCore
    {
        private readonly ODataJsonCollectionDeserializer jsonCollectionDeserializer;
        private readonly ODataJsonInputContext jsonInputContext;

        internal ODataJsonCollectionReader(ODataJsonInputContext jsonInputContext, IEdmTypeReference expectedItemTypeReference, IODataReaderWriterListener listener) : base(jsonInputContext, expectedItemTypeReference, listener)
        {
            this.jsonInputContext = jsonInputContext;
            this.jsonCollectionDeserializer = new ODataJsonCollectionDeserializer(jsonInputContext);
            if (!jsonInputContext.Model.IsUserModel())
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonCollectionReader_ParsingWithoutMetadata);
            }
        }

        protected override bool ReadAtCollectionEndImplementation()
        {
            base.PopScope(ODataCollectionReaderState.CollectionEnd);
            this.jsonCollectionDeserializer.ReadCollectionEnd(this.IsResultsWrapperExpected);
            this.jsonCollectionDeserializer.ReadPayloadEnd(base.IsReadingNestedPayload);
            base.ReplaceScope(ODataCollectionReaderState.Completed, null);
            return false;
        }

        protected override bool ReadAtCollectionStartImplementation()
        {
            if (this.jsonCollectionDeserializer.JsonReader.NodeType == JsonNodeType.EndArray)
            {
                base.ReplaceScope(ODataCollectionReaderState.CollectionEnd, this.Item);
            }
            else
            {
                object item = this.jsonCollectionDeserializer.ReadCollectionItem(base.ExpectedItemTypeReference, base.CollectionValidator);
                base.EnterScope(ODataCollectionReaderState.Value, item);
            }
            return true;
        }

        protected override bool ReadAtStartImplementation()
        {
            this.jsonCollectionDeserializer.ReadPayloadStart(base.IsReadingNestedPayload);
            if (this.IsResultsWrapperExpected && (this.jsonCollectionDeserializer.JsonReader.NodeType != JsonNodeType.StartObject))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonCollectionReader_CannotReadWrappedCollectionStart(this.jsonCollectionDeserializer.JsonReader.NodeType));
            }
            if (!this.IsResultsWrapperExpected && (this.jsonCollectionDeserializer.JsonReader.NodeType != JsonNodeType.StartArray))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonCollectionReader_CannotReadCollectionStart(this.jsonCollectionDeserializer.JsonReader.NodeType));
            }
            ODataCollectionStart item = this.jsonCollectionDeserializer.ReadCollectionStart(this.IsResultsWrapperExpected);
            this.jsonCollectionDeserializer.JsonReader.ReadStartArray();
            base.EnterScope(ODataCollectionReaderState.CollectionStart, item);
            return true;
        }

        protected override bool ReadAtValueImplementation()
        {
            if (this.jsonCollectionDeserializer.JsonReader.NodeType == JsonNodeType.EndArray)
            {
                base.PopScope(ODataCollectionReaderState.Value);
                base.ReplaceScope(ODataCollectionReaderState.CollectionEnd, this.Item);
            }
            else
            {
                object item = this.jsonCollectionDeserializer.ReadCollectionItem(base.ExpectedItemTypeReference, base.CollectionValidator);
                base.ReplaceScope(ODataCollectionReaderState.Value, item);
            }
            return true;
        }

        private bool IsResultsWrapperExpected
        {
            get
            {
                return ((this.jsonInputContext.Version >= ODataVersion.V2) && this.jsonInputContext.ReadingResponse);
            }
        }
    }
}

