namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.OData;
    using System;

    internal class ODataJsonSerializer : ODataSerializer
    {
        private readonly ODataJsonOutputContext jsonOutputContext;

        internal ODataJsonSerializer(ODataJsonOutputContext jsonOutputContext) : base(jsonOutputContext)
        {
            this.jsonOutputContext = jsonOutputContext;
        }

        internal string UriToAbsoluteUriString(Uri uri)
        {
            return this.UriToUriString(uri, true);
        }

        internal string UriToUriString(Uri uri, bool makeAbsolute)
        {
            Uri uri2;
            if (base.UrlResolver != null)
            {
                uri2 = base.UrlResolver.ResolveUrl(base.MessageWriterSettings.BaseUri, uri);
                if (uri2 != null)
                {
                    return UriUtilsCommon.UriToString(uri2);
                }
            }
            uri2 = uri;
            if (!uri2.IsAbsoluteUri)
            {
                if (makeAbsolute)
                {
                    if (base.MessageWriterSettings.BaseUri == null)
                    {
                        throw new ODataException(Strings.ODataWriter_RelativeUriUsedWithoutBaseUriSpecified(UriUtilsCommon.UriToString(uri)));
                    }
                    uri2 = UriUtils.UriToAbsoluteUri(base.MessageWriterSettings.BaseUri, uri);
                }
                else
                {
                    uri2 = UriUtils.EnsureEscapedRelativeUri(uri2);
                }
            }
            return UriUtilsCommon.UriToString(uri2);
        }

        internal void WritePayloadEnd()
        {
            this.WritePayloadEnd(false);
        }

        internal void WritePayloadEnd(bool disableResponseWrapper)
        {
            if (base.WritingResponse && !disableResponseWrapper)
            {
                this.JsonWriter.EndObjectScope();
            }
        }

        internal void WritePayloadStart()
        {
            this.WritePayloadStart(false);
        }

        internal void WritePayloadStart(bool disableResponseWrapper)
        {
            if (base.WritingResponse && !disableResponseWrapper)
            {
                this.JsonWriter.StartObjectScope();
                this.JsonWriter.WriteDataWrapper();
            }
        }

        internal void WriteTopLevelError(ODataError error, bool includeDebugInformation)
        {
            this.WriteTopLevelPayload(() => ODataJsonWriterUtils.WriteError(this.JsonWriter, error, includeDebugInformation, this.MessageWriterSettings.MessageQuotas.MaxNestingDepth), true);
        }

        internal void WriteTopLevelPayload(Action payloadWriterAction)
        {
            this.WriteTopLevelPayload(payloadWriterAction, false);
        }

        internal void WriteTopLevelPayload(Action payloadWriterAction, bool disableResponseWrapper)
        {
            this.WritePayloadStart(disableResponseWrapper);
            payloadWriterAction();
            this.WritePayloadEnd(disableResponseWrapper);
        }

        internal Microsoft.Data.OData.Json.JsonWriter JsonWriter
        {
            get
            {
                return this.jsonOutputContext.JsonWriter;
            }
        }
    }
}

