namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;

    internal sealed class ODataJsonCollectionDeserializer : ODataJsonPropertyAndValueDeserializer
    {
        private readonly DuplicatePropertyNamesChecker duplicatePropertyNamesChecker;

        internal ODataJsonCollectionDeserializer(ODataJsonInputContext jsonInputContext) : base(jsonInputContext)
        {
            this.duplicatePropertyNamesChecker = base.CreateDuplicatePropertyNamesChecker();
        }

        internal void ReadCollectionEnd(bool isResultsWrapperExpected)
        {
            base.JsonReader.ReadEndArray();
            if (isResultsWrapperExpected)
            {
                while (base.JsonReader.NodeType == JsonNodeType.Property)
                {
                    string strB = base.JsonReader.ReadPropertyName();
                    if (string.CompareOrdinal("results", strB) == 0)
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonCollectionDeserializer_MultipleResultsPropertiesForCollection);
                    }
                    base.JsonReader.SkipValue();
                }
                base.JsonReader.ReadEndObject();
            }
        }

        internal object ReadCollectionItem(IEdmTypeReference expectedItemTypeReference, CollectionWithoutExpectedTypeValidator collectionValidator)
        {
            return base.ReadNonEntityValue(expectedItemTypeReference, this.duplicatePropertyNamesChecker, collectionValidator, true);
        }

        internal ODataCollectionStart ReadCollectionStart(bool isResultsWrapperExpected)
        {
            if (isResultsWrapperExpected)
            {
                base.JsonReader.ReadStartObject();
                bool flag = false;
                while (base.JsonReader.NodeType == JsonNodeType.Property)
                {
                    string strB = base.JsonReader.ReadPropertyName();
                    if (string.CompareOrdinal("results", strB) == 0)
                    {
                        flag = true;
                        break;
                    }
                    base.JsonReader.SkipValue();
                }
                if (!flag)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonCollectionDeserializer_MissingResultsPropertyForCollection);
                }
            }
            if (base.JsonReader.NodeType != JsonNodeType.StartArray)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonCollectionDeserializer_CannotReadCollectionContentStart(base.JsonReader.NodeType));
            }
            return new ODataCollectionStart { Name = null };
        }
    }
}

