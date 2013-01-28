namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.OData;
    using System;
    using System.Diagnostics;

    internal abstract class ODataJsonDeserializer : ODataDeserializer
    {
        private readonly ODataJsonInputContext jsonInputContext;

        protected ODataJsonDeserializer(ODataJsonInputContext jsonInputContext) : base(jsonInputContext)
        {
            this.jsonInputContext = jsonInputContext;
        }

        [Conditional("DEBUG")]
        internal void AssertJsonCondition(params JsonNodeType[] allowedNodeTypes)
        {
        }

        internal Uri ProcessUriFromPayload(string uriFromPayload)
        {
            return this.ProcessUriFromPayload(uriFromPayload, true);
        }

        internal Uri ProcessUriFromPayload(string uriFromPayload, bool requireAbsoluteUri)
        {
            Uri payloadUri = new Uri(uriFromPayload, UriKind.RelativeOrAbsolute);
            Uri uri2 = base.ResolveUri(base.MessageReaderSettings.BaseUri, payloadUri);
            if (uri2 != null)
            {
                return uri2;
            }
            if (!payloadUri.IsAbsoluteUri)
            {
                if (base.MessageReaderSettings.BaseUri != null)
                {
                    return UriUtils.UriToAbsoluteUri(base.MessageReaderSettings.BaseUri, payloadUri);
                }
                if (requireAbsoluteUri)
                {
                    throw new ODataException(Strings.ODataJsonDeserializer_RelativeUriUsedWithoutBaseUriSpecified(uriFromPayload));
                }
            }
            return payloadUri;
        }

        internal void ReadPayloadEnd(bool isReadingNestedPayload)
        {
            this.ReadPayloadEnd(isReadingNestedPayload, true);
        }

        internal void ReadPayloadEnd(bool isReadingNestedPayload, bool expectResponseWrapper)
        {
            if (base.ReadingResponse && expectResponseWrapper)
            {
                while (this.JsonReader.NodeType == JsonNodeType.Property)
                {
                    string strB = this.JsonReader.ReadPropertyName();
                    if (string.CompareOrdinal("d", strB) == 0)
                    {
                        throw new ODataException(Strings.ODataJsonDeserializer_DataWrapperMultipleProperties);
                    }
                    this.JsonReader.SkipValue();
                }
                this.JsonReader.ReadEndObject();
            }
        }

        internal void ReadPayloadStart(bool isReadingNestedPayload)
        {
            this.ReadPayloadStart(isReadingNestedPayload, true);
        }

        internal void ReadPayloadStart(bool isReadingNestedPayload, bool expectResponseWrapper)
        {
            if (!isReadingNestedPayload)
            {
                this.JsonReader.Read();
            }
            if (base.ReadingResponse && expectResponseWrapper)
            {
                this.JsonReader.ReadStartObject();
                while (this.JsonReader.NodeType == JsonNodeType.Property)
                {
                    string strB = this.JsonReader.ReadPropertyName();
                    if (string.CompareOrdinal("d", strB) == 0)
                    {
                        break;
                    }
                    this.JsonReader.SkipValue();
                }
                if (this.JsonReader.NodeType == JsonNodeType.EndObject)
                {
                    throw new ODataException(Strings.ODataJsonDeserializer_DataWrapperPropertyNotFound);
                }
            }
        }

        internal Uri ResolveUri(string uriFromPayload)
        {
            Uri payloadUri = new Uri(uriFromPayload, UriKind.RelativeOrAbsolute);
            Uri uri2 = base.ResolveUri(base.MessageReaderSettings.BaseUri, payloadUri);
            if (uri2 != null)
            {
                return uri2;
            }
            return payloadUri;
        }

        internal BufferingJsonReader JsonReader
        {
            get
            {
                return this.jsonInputContext.JsonReader;
            }
        }
    }
}

