namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;

    internal sealed class ODataJsonServiceDocumentDeserializer : ODataJsonDeserializer
    {
        internal ODataJsonServiceDocumentDeserializer(ODataJsonInputContext jsonInputContext) : base(jsonInputContext)
        {
        }

        internal ODataWorkspace ReadServiceDocument()
        {
            List<ODataResourceCollectionInfo> sourceList = null;
            base.ReadPayloadStart(false);
            base.JsonReader.ReadStartObject();
            while (base.JsonReader.NodeType == JsonNodeType.Property)
            {
                string strB = base.JsonReader.ReadPropertyName();
                if (string.CompareOrdinal("EntitySets", strB) == 0)
                {
                    if (sourceList != null)
                    {
                        throw new ODataException(Strings.ODataJsonServiceDocumentDeserializer_MultipleEntitySetsPropertiesForServiceDocument);
                    }
                    sourceList = new List<ODataResourceCollectionInfo>();
                    base.JsonReader.ReadStartArray();
                    while (base.JsonReader.NodeType != JsonNodeType.EndArray)
                    {
                        string collectionInfoUrl = base.JsonReader.ReadStringValue();
                        ValidationUtils.ValidateResourceCollectionInfoUrl(collectionInfoUrl);
                        ODataResourceCollectionInfo item = new ODataResourceCollectionInfo {
                            Url = base.ProcessUriFromPayload(collectionInfoUrl, false)
                        };
                        sourceList.Add(item);
                    }
                    base.JsonReader.ReadEndArray();
                }
                else
                {
                    base.JsonReader.SkipValue();
                }
            }
            if (sourceList == null)
            {
                throw new ODataException(Strings.ODataJsonServiceDocumentDeserializer_NoEntitySetsPropertyForServiceDocument);
            }
            base.JsonReader.ReadEndObject();
            base.ReadPayloadEnd(false);
            return new ODataWorkspace { Collections = new ReadOnlyEnumerable<ODataResourceCollectionInfo>(sourceList) };
        }
    }
}

