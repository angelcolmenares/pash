namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Linq;
    using System.Xml;

    internal abstract class ODataAtomEpmDeserializer : ODataAtomMetadataDeserializer
    {
        internal ODataAtomEpmDeserializer(ODataAtomInputContext atomInputContext) : base(atomInputContext)
        {
        }

        private void ReadCustomEpmAttribute(IODataAtomReaderEntryState entryState, EpmTargetPathSegment epmTargetPathSegmentForElement)
        {
            string localName = base.XmlReader.LocalName;
            string namespaceUri = base.XmlReader.NamespaceURI;
            EpmTargetPathSegment segment = epmTargetPathSegmentForElement.SubSegments.FirstOrDefault<EpmTargetPathSegment>(x => (x.IsAttribute && (string.CompareOrdinal(x.AttributeName, localName) == 0)) && (string.CompareOrdinal(x.SegmentNamespaceUri, namespaceUri) == 0));
            if ((segment != null) && !entryState.EpmCustomReaderValueCache.Contains(segment.EpmInfo))
            {
                entryState.EpmCustomReaderValueCache.Add(segment.EpmInfo, base.XmlReader.Value);
            }
        }

        private bool TryReadCustomEpmElement(IODataAtomReaderEntryState entryState, EpmTargetPathSegment epmTargetPathSegment)
        {
            string localName = base.XmlReader.LocalName;
            string namespaceUri = base.XmlReader.NamespaceURI;
            EpmTargetPathSegment epmTargetPathSegmentForElement = epmTargetPathSegment.SubSegments.FirstOrDefault<EpmTargetPathSegment>(segment => (!segment.IsAttribute && (string.CompareOrdinal(segment.SegmentName, localName) == 0)) && (string.CompareOrdinal(segment.SegmentNamespaceUri, namespaceUri) == 0));
            if ((epmTargetPathSegmentForElement != null) && (!epmTargetPathSegmentForElement.HasContent || !entryState.EpmCustomReaderValueCache.Contains(epmTargetPathSegmentForElement.EpmInfo)))
            {
                while (base.XmlReader.MoveToNextAttribute())
                {
                    this.ReadCustomEpmAttribute(entryState, epmTargetPathSegmentForElement);
                }
                base.XmlReader.MoveToElement();
                if (epmTargetPathSegmentForElement.HasContent)
                {
                    string str = base.ReadElementStringValue();
                    entryState.EpmCustomReaderValueCache.Add(epmTargetPathSegmentForElement.EpmInfo, str);
                    goto Label_0115;
                }
                if (!base.XmlReader.IsEmptyElement)
                {
                    base.XmlReader.Read();
                    while (base.XmlReader.NodeType != XmlNodeType.EndElement)
                    {
                        switch (base.XmlReader.NodeType)
                        {
                            case XmlNodeType.Element:
                            {
                                if (!this.TryReadCustomEpmElement(entryState, epmTargetPathSegmentForElement))
                                {
                                    base.XmlReader.Skip();
                                }
                                continue;
                            }
                            case XmlNodeType.EndElement:
                            {
                                continue;
                            }
                        }
                        base.XmlReader.Skip();
                    }
                }
            }
            else
            {
                return false;
            }
            base.XmlReader.Read();
        Label_0115:
            return true;
        }

        internal bool TryReadExtensionElementInEntryContent(IODataAtomReaderEntryState entryState)
        {
            ODataEntityPropertyMappingCache cachedEpm = entryState.CachedEpm;
            if (cachedEpm == null)
            {
                return false;
            }
            EpmTargetPathSegment nonSyndicationRoot = cachedEpm.EpmTargetTree.NonSyndicationRoot;
            return this.TryReadCustomEpmElement(entryState, nonSyndicationRoot);
        }
    }
}

