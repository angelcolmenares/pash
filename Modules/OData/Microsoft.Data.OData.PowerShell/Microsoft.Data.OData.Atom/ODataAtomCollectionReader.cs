namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Xml;

    internal sealed class ODataAtomCollectionReader : ODataCollectionReaderCore
    {
        private readonly ODataAtomCollectionDeserializer atomCollectionDeserializer;
        private readonly ODataAtomInputContext atomInputContext;

        internal ODataAtomCollectionReader(ODataAtomInputContext atomInputContext, IEdmTypeReference expectedItemTypeReference) : base(atomInputContext, expectedItemTypeReference, null)
        {
            this.atomInputContext = atomInputContext;
            this.atomCollectionDeserializer = new ODataAtomCollectionDeserializer(atomInputContext);
        }

        protected override bool ReadAtCollectionEndImplementation()
        {
            this.atomCollectionDeserializer.ReadPayloadEnd();
            base.PopScope(ODataCollectionReaderState.CollectionEnd);
            base.ReplaceScope(ODataCollectionReaderState.Completed, null);
            return false;
        }

        protected override bool ReadAtCollectionStartImplementation()
        {
            this.atomCollectionDeserializer.SkipToElementInODataNamespace();
            if ((this.atomCollectionDeserializer.XmlReader.NodeType == XmlNodeType.EndElement) || base.IsCollectionElementEmpty)
            {
                this.atomCollectionDeserializer.ReadCollectionEnd();
                base.ReplaceScope(ODataCollectionReaderState.CollectionEnd, this.Item);
            }
            else
            {
                object item = this.atomCollectionDeserializer.ReadCollectionItem(base.ExpectedItemTypeReference, base.CollectionValidator);
                base.EnterScope(ODataCollectionReaderState.Value, item);
            }
            return true;
        }

        protected override bool ReadAtStartImplementation()
        {
            bool flag;
            this.atomCollectionDeserializer.ReadPayloadStart();
            ODataCollectionStart item = this.atomCollectionDeserializer.ReadCollectionStart(out flag);
            base.EnterScope(ODataCollectionReaderState.CollectionStart, item, flag);
            return true;
        }

        protected override bool ReadAtValueImplementation()
        {
            this.atomCollectionDeserializer.SkipToElementInODataNamespace();
            if (this.atomInputContext.XmlReader.NodeType == XmlNodeType.EndElement)
            {
                this.atomCollectionDeserializer.ReadCollectionEnd();
                base.PopScope(ODataCollectionReaderState.Value);
                base.ReplaceScope(ODataCollectionReaderState.CollectionEnd, this.Item);
            }
            else
            {
                object item = this.atomCollectionDeserializer.ReadCollectionItem(base.ExpectedItemTypeReference, base.CollectionValidator);
                base.ReplaceScope(ODataCollectionReaderState.Value, item);
            }
            return true;
        }
    }
}

