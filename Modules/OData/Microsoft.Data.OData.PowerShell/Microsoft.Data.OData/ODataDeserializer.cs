namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using System;

    internal abstract class ODataDeserializer
    {
        private readonly ODataInputContext inputContext;

        protected ODataDeserializer(ODataInputContext inputContext)
        {
            this.inputContext = inputContext;
        }

        internal DuplicatePropertyNamesChecker CreateDuplicatePropertyNamesChecker()
        {
            return this.inputContext.CreateDuplicatePropertyNamesChecker();
        }

        internal Uri ResolveUri(Uri baseUri, Uri payloadUri)
        {
            IODataUrlResolver urlResolver = this.inputContext.UrlResolver;
            if (urlResolver != null)
            {
                return urlResolver.ResolveUrl(baseUri, payloadUri);
            }
            return null;
        }

        internal ODataMessageReaderSettings MessageReaderSettings
        {
            get
            {
                return this.inputContext.MessageReaderSettings;
            }
        }

        internal IEdmModel Model
        {
            get
            {
                return this.inputContext.Model;
            }
        }

        internal bool ReadingResponse
        {
            get
            {
                return this.inputContext.ReadingResponse;
            }
        }

        internal bool UseClientFormatBehavior
        {
            get
            {
                return this.inputContext.UseClientFormatBehavior;
            }
        }

        internal bool UseDefaultFormatBehavior
        {
            get
            {
                return this.inputContext.UseDefaultFormatBehavior;
            }
        }

        internal bool UseServerFormatBehavior
        {
            get
            {
                return this.inputContext.UseServerFormatBehavior;
            }
        }

        internal ODataVersion Version
        {
            get
            {
                return this.inputContext.Version;
            }
        }
    }
}

