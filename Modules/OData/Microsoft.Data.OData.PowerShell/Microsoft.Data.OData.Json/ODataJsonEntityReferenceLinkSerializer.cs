namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;

    internal sealed class ODataJsonEntityReferenceLinkSerializer : ODataJsonSerializer
    {
        internal ODataJsonEntityReferenceLinkSerializer(ODataJsonOutputContext jsonOutputContext) : base(jsonOutputContext)
        {
        }

        internal void WriteEntityReferenceLink(ODataEntityReferenceLink link)
        {
            base.WriteTopLevelPayload(() => this.WriteEntityReferenceLinkImplementation(link));
        }

        private void WriteEntityReferenceLinkImplementation(ODataEntityReferenceLink entityReferenceLink)
        {
            WriterValidationUtils.ValidateEntityReferenceLink(entityReferenceLink);
            base.JsonWriter.StartObjectScope();
            base.JsonWriter.WriteName("uri");
            base.JsonWriter.WriteValue(base.UriToAbsoluteUriString(entityReferenceLink.Url));
            base.JsonWriter.EndObjectScope();
        }

        internal void WriteEntityReferenceLinks(ODataEntityReferenceLinks entityReferenceLinks)
        {
            base.WriteTopLevelPayload(() => this.WriteEntityReferenceLinksImplementation(entityReferenceLinks, (this.Version >= ODataVersion.V2) && this.WritingResponse));
        }

        private void WriteEntityReferenceLinksImplementation(ODataEntityReferenceLinks entityReferenceLinks, bool includeResultsWrapper)
        {
            if (includeResultsWrapper)
            {
                base.JsonWriter.StartObjectScope();
            }
            if (entityReferenceLinks.Count.HasValue)
            {
                base.JsonWriter.WriteName("__count");
                base.JsonWriter.WriteValue(entityReferenceLinks.Count.Value);
            }
            if (includeResultsWrapper)
            {
                base.JsonWriter.WriteDataArrayName();
            }
            base.JsonWriter.StartArrayScope();
            IEnumerable<ODataEntityReferenceLink> links = entityReferenceLinks.Links;
            if (links != null)
            {
                foreach (ODataEntityReferenceLink link in links)
                {
                    WriterValidationUtils.ValidateEntityReferenceLinkNotNull(link);
                    this.WriteEntityReferenceLinkImplementation(link);
                }
            }
            base.JsonWriter.EndArrayScope();
            if (entityReferenceLinks.NextPageLink != null)
            {
                base.JsonWriter.WriteName("__next");
                base.JsonWriter.WriteValue(base.UriToAbsoluteUriString(entityReferenceLinks.NextPageLink));
            }
            if (includeResultsWrapper)
            {
                base.JsonWriter.EndObjectScope();
            }
        }
    }
}

