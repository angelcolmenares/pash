namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Xml;

    internal sealed class EpmCustomWriter : EpmWriter
    {
        private EpmCustomWriter(ODataAtomOutputContext atomOutputContext) : base(atomOutputContext)
        {
        }

        private string GetEntryPropertyValueAsText(EpmTargetPathSegment targetSegment, EntryPropertiesValueCache epmValueCache, IEdmEntityTypeReference entityType)
        {
            object propertyValue = base.ReadEntryPropertyValue(targetSegment.EpmInfo, epmValueCache, entityType);
            if (propertyValue == null)
            {
                return string.Empty;
            }
            return EpmWriterUtils.GetPropertyValueAsText(propertyValue);
        }

        private void WriteAttributeEpm(XmlWriter writer, EpmTargetPathSegment targetSegment, EntryPropertiesValueCache epmValueCache, IEdmEntityTypeReference entityType, ref string alreadyDeclaredPrefix)
        {
            string str = this.GetEntryPropertyValueAsText(targetSegment, epmValueCache, entityType);
            string prefix = targetSegment.SegmentNamespacePrefix ?? string.Empty;
            writer.WriteAttributeString(prefix, targetSegment.AttributeName, targetSegment.SegmentNamespaceUri, str);
            if (prefix.Length > 0)
            {
                WriteNamespaceDeclaration(writer, targetSegment, ref alreadyDeclaredPrefix);
            }
        }

        private void WriteElementEpm(XmlWriter writer, EpmTargetPathSegment targetSegment, EntryPropertiesValueCache epmValueCache, IEdmEntityTypeReference entityType, ref string alreadyDeclaredPrefix)
        {
            string prefix = targetSegment.SegmentNamespacePrefix ?? string.Empty;
            writer.WriteStartElement(prefix, targetSegment.SegmentName, targetSegment.SegmentNamespaceUri);
            if (prefix.Length > 0)
            {
                WriteNamespaceDeclaration(writer, targetSegment, ref alreadyDeclaredPrefix);
            }
            foreach (EpmTargetPathSegment segment in targetSegment.SubSegments)
            {
                if (segment.IsAttribute)
                {
                    this.WriteAttributeEpm(writer, segment, epmValueCache, entityType, ref alreadyDeclaredPrefix);
                }
            }
            if (targetSegment.HasContent)
            {
                string str2 = this.GetEntryPropertyValueAsText(targetSegment, epmValueCache, entityType);
                ODataAtomWriterUtils.WriteString(writer, str2);
            }
            else
            {
                foreach (EpmTargetPathSegment segment2 in targetSegment.SubSegments)
                {
                    if (!segment2.IsAttribute)
                    {
                        this.WriteElementEpm(writer, segment2, epmValueCache, entityType, ref alreadyDeclaredPrefix);
                    }
                }
            }
            writer.WriteEndElement();
        }

        private void WriteEntryEpm(XmlWriter writer, EpmTargetTree epmTargetTree, EntryPropertiesValueCache epmValueCache, IEdmEntityTypeReference entityType)
        {
            EpmTargetPathSegment nonSyndicationRoot = epmTargetTree.NonSyndicationRoot;
            if (nonSyndicationRoot.SubSegments.Count != 0)
            {
                foreach (EpmTargetPathSegment segment2 in nonSyndicationRoot.SubSegments)
                {
                    string alreadyDeclaredPrefix = null;
                    this.WriteElementEpm(writer, segment2, epmValueCache, entityType, ref alreadyDeclaredPrefix);
                }
            }
        }

        internal static void WriteEntryEpm(XmlWriter writer, EpmTargetTree epmTargetTree, EntryPropertiesValueCache epmValueCache, IEdmEntityTypeReference entityType, ODataAtomOutputContext atomOutputContext)
        {
            new EpmCustomWriter(atomOutputContext).WriteEntryEpm(writer, epmTargetTree, epmValueCache, entityType);
        }

        private static void WriteNamespaceDeclaration(XmlWriter writer, EpmTargetPathSegment targetSegment, ref string alreadyDeclaredPrefix)
        {
            if (alreadyDeclaredPrefix == null)
            {
                writer.WriteAttributeString("xmlns", targetSegment.SegmentNamespacePrefix, "http://www.w3.org/2000/xmlns/", targetSegment.SegmentNamespaceUri);
                alreadyDeclaredPrefix = targetSegment.SegmentNamespacePrefix;
            }
        }
    }
}

