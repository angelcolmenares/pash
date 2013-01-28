namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;

    internal sealed class ODataJsonEntityReferenceLinkDeserializer : ODataJsonDeserializer
    {
        internal ODataJsonEntityReferenceLinkDeserializer(ODataJsonInputContext jsonInputContext) : base(jsonInputContext)
        {
        }

        internal ODataEntityReferenceLink ReadEntityReferenceLink()
        {
            base.ReadPayloadStart(false);
            ODataEntityReferenceLink link = this.ReadSingleEntityReferenceLink();
            base.ReadPayloadEnd(false);
            return link;
        }

        private bool ReadEntityReferenceLinkProperties(ODataEntityReferenceLinks entityReferenceLinks, ref ODataJsonReaderUtils.EntityReferenceLinksWrapperPropertyBitMask propertiesFoundBitField)
        {
            while (base.JsonReader.NodeType == JsonNodeType.Property)
            {
                string str3 = base.JsonReader.ReadPropertyName();
                if (str3 == null)
                {
                    goto Label_00D9;
                }
                if (!(str3 == "results"))
                {
                    if (str3 == "__count")
                    {
                        goto Label_0057;
                    }
                    if (str3 == "__next")
                    {
                        goto Label_00A2;
                    }
                    goto Label_00D9;
                }
                ODataJsonReaderUtils.VerifyEntityReferenceLinksWrapperPropertyNotFound(ref propertiesFoundBitField, ODataJsonReaderUtils.EntityReferenceLinksWrapperPropertyBitMask.Results, "results");
                return true;
            Label_0057:
                ODataJsonReaderUtils.VerifyEntityReferenceLinksWrapperPropertyNotFound(ref propertiesFoundBitField, ODataJsonReaderUtils.EntityReferenceLinksWrapperPropertyBitMask.Count, "__count");
                long? propertyValue = (long?) ODataJsonReaderUtils.ConvertValue(base.JsonReader.ReadPrimitiveValue(), EdmCoreModel.Instance.GetInt64(true), base.MessageReaderSettings, base.Version, true);
                ODataJsonReaderUtils.ValidateCountPropertyInEntityReferenceLinks(propertyValue);
                entityReferenceLinks.Count = propertyValue;
                continue;
            Label_00A2:
                ODataJsonReaderUtils.VerifyEntityReferenceLinksWrapperPropertyNotFound(ref propertiesFoundBitField, ODataJsonReaderUtils.EntityReferenceLinksWrapperPropertyBitMask.NextPageLink, "__next");
                string str2 = base.JsonReader.ReadStringValue("__next");
                ODataJsonReaderUtils.ValidateEntityReferenceLinksStringProperty(str2, "__next");
                entityReferenceLinks.NextPageLink = base.ProcessUriFromPayload(str2);
                continue;
            Label_00D9:
                base.JsonReader.SkipValue();
            }
            return false;
        }

        internal ODataEntityReferenceLinks ReadEntityReferenceLinks()
        {
            bool flag = (base.Version >= ODataVersion.V2) && base.ReadingResponse;
            ODataJsonReaderUtils.EntityReferenceLinksWrapperPropertyBitMask none = ODataJsonReaderUtils.EntityReferenceLinksWrapperPropertyBitMask.None;
            ODataEntityReferenceLinks entityReferenceLinks = new ODataEntityReferenceLinks();
            base.ReadPayloadStart(false);
            if (flag)
            {
                base.JsonReader.ReadStartObject();
                if (!this.ReadEntityReferenceLinkProperties(entityReferenceLinks, ref none))
                {
                    throw new ODataException(Strings.ODataJsonEntityReferenceLinkDeserializer_ExpectedEntityReferenceLinksResultsPropertyNotFound);
                }
            }
            base.JsonReader.ReadStartArray();
            List<ODataEntityReferenceLink> sourceList = new List<ODataEntityReferenceLink>();
            while (base.JsonReader.NodeType != JsonNodeType.EndArray)
            {
                ODataEntityReferenceLink item = this.ReadSingleEntityReferenceLink();
                sourceList.Add(item);
            }
            base.JsonReader.ReadEndArray();
            if (flag)
            {
                this.ReadEntityReferenceLinkProperties(entityReferenceLinks, ref none);
                base.JsonReader.ReadEndObject();
            }
            entityReferenceLinks.Links = new ReadOnlyEnumerable<ODataEntityReferenceLink>(sourceList);
            base.ReadPayloadEnd(false);
            return entityReferenceLinks;
        }

        private ODataEntityReferenceLink ReadSingleEntityReferenceLink()
        {
            if (base.JsonReader.NodeType != JsonNodeType.StartObject)
            {
                throw new ODataException(Strings.ODataJsonEntityReferenceLinkDeserializer_EntityReferenceLinkMustBeObjectValue(base.JsonReader.NodeType));
            }
            base.JsonReader.ReadStartObject();
            ODataEntityReferenceLink link = new ODataEntityReferenceLink();
            while (base.JsonReader.NodeType == JsonNodeType.Property)
            {
                string strB = base.JsonReader.ReadPropertyName();
                if (string.CompareOrdinal("uri", strB) == 0)
                {
                    if (link.Url != null)
                    {
                        throw new ODataException(Strings.ODataJsonEntityReferenceLinkDeserializer_MultipleUriPropertiesInEntityReferenceLink);
                    }
                    string uriFromPayload = base.JsonReader.ReadStringValue("uri");
                    if (uriFromPayload == null)
                    {
                        throw new ODataException(Strings.ODataJsonEntityReferenceLinkDeserializer_EntityReferenceLinkUriCannotBeNull);
                    }
                    link.Url = base.ProcessUriFromPayload(uriFromPayload);
                }
                else
                {
                    base.JsonReader.SkipValue();
                }
            }
            ReaderValidationUtils.ValidateEntityReferenceLink(link);
            base.JsonReader.ReadEndObject();
            return link;
        }
    }
}

