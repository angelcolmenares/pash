namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using System;

    internal abstract class ODataSerializer
    {
        private readonly ODataOutputContext outputContext;

        protected ODataSerializer(ODataOutputContext outputContext)
        {
            this.outputContext = outputContext;
        }

        internal DuplicatePropertyNamesChecker CreateDuplicatePropertyNamesChecker()
        {
            return new DuplicatePropertyNamesChecker(this.MessageWriterSettings.WriterBehavior.AllowDuplicatePropertyNames, this.WritingResponse);
        }

        protected void ValidateAssociationLink(ODataAssociationLink associationLink, IEdmEntityType entryEntityType)
        {
            WriterValidationUtils.ValidateAssociationLink(associationLink, this.Version, this.WritingResponse);
            WriterValidationUtils.ValidateNavigationPropertyDefined(associationLink.Name, entryEntityType);
        }

        internal ODataMessageWriterSettings MessageWriterSettings
        {
            get
            {
                return this.outputContext.MessageWriterSettings;
            }
        }

        internal IEdmModel Model
        {
            get
            {
                return this.outputContext.Model;
            }
        }

        internal IODataUrlResolver UrlResolver
        {
            get
            {
                return this.outputContext.UrlResolver;
            }
        }

        internal bool UseClientFormatBehavior
        {
            get
            {
                return this.outputContext.UseClientFormatBehavior;
            }
        }

        internal bool UseDefaultFormatBehavior
        {
            get
            {
                return this.outputContext.UseDefaultFormatBehavior;
            }
        }

        internal bool UseServerFormatBehavior
        {
            get
            {
                return this.outputContext.UseServerFormatBehavior;
            }
        }

        internal ODataVersion Version
        {
            get
            {
                return this.outputContext.Version;
            }
        }

        internal bool WritingResponse
        {
            get
            {
                return this.outputContext.WritingResponse;
            }
        }
    }
}

