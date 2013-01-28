namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Diagnostics;
    using System.Xml;

    internal abstract class ODataAtomDeserializer : ODataDeserializer
    {
        private readonly ODataAtomInputContext atomInputContext;

        protected ODataAtomDeserializer(ODataAtomInputContext atomInputContext) : base(atomInputContext)
        {
            this.atomInputContext = atomInputContext;
        }

        [Conditional("DEBUG")]
        internal void AssertXmlCondition(params XmlNodeType[] allowedNodeTypes)
        {
        }

        [Conditional("DEBUG")]
        internal void AssertXmlCondition(bool allowEmptyElement, params XmlNodeType[] allowedNodeTypes)
        {
        }

        internal Uri ProcessUriFromPayload(string uriFromPayload, Uri xmlBaseUri)
        {
            return this.ProcessUriFromPayload(uriFromPayload, xmlBaseUri, true);
        }

        internal Uri ProcessUriFromPayload(string uriFromPayload, Uri xmlBaseUri, bool makeAbsolute)
        {
            Uri baseUri = xmlBaseUri;
            if (baseUri == null)
            {
                baseUri = base.MessageReaderSettings.BaseUri;
                bool flag1 = baseUri != null;
            }
            Uri payloadUri = new Uri(uriFromPayload, UriKind.RelativeOrAbsolute);
            Uri uri3 = base.ResolveUri(baseUri, payloadUri);
            if (uri3 != null)
            {
                return uri3;
            }
            if (payloadUri.IsAbsoluteUri || !makeAbsolute)
            {
                return payloadUri;
            }
            if (baseUri == null)
            {
                throw new ODataException(Strings.ODataAtomDeserializer_RelativeUriUsedWithoutBaseUriSpecified(uriFromPayload));
            }
            return UriUtils.UriToAbsoluteUri(baseUri, payloadUri);
        }

        internal void ReadPayloadEnd()
        {
            this.XmlReader.ReadPayloadEnd();
        }

        internal void ReadPayloadStart()
        {
            this.XmlReader.ReadPayloadStart();
        }

        protected ODataAtomInputContext AtomInputContext
        {
            get
            {
                return this.atomInputContext;
            }
        }

        internal BufferingXmlReader XmlReader
        {
            get
            {
                return this.atomInputContext.XmlReader;
            }
        }
    }
}

