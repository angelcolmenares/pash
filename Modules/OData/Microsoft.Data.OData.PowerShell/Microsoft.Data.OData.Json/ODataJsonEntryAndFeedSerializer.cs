namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class ODataJsonEntryAndFeedSerializer : ODataJsonPropertyAndValueSerializer
    {
        internal ODataJsonEntryAndFeedSerializer(ODataJsonOutputContext jsonOutputContext) : base(jsonOutputContext)
        {
        }

        private void WriteAssociationLink(ODataAssociationLink associationLink, DuplicatePropertyNamesChecker duplicatePropertyNamesChecker)
        {
            duplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(associationLink);
            base.JsonWriter.WriteName(associationLink.Name);
            base.JsonWriter.StartObjectScope();
            base.JsonWriter.WriteName("associationuri");
            base.JsonWriter.WriteValue(base.UriToAbsoluteUriString(associationLink.Url));
            base.JsonWriter.EndObjectScope();
        }

        internal void WriteEntryMetadata(ODataEntry entry, ProjectedPropertiesAnnotation projectedProperties, IEdmEntityType entryEntityType, DuplicatePropertyNamesChecker duplicatePropertyNamesChecker)
        {
            base.JsonWriter.WriteName("__metadata");
            base.JsonWriter.StartObjectScope();
            string id = entry.Id;
            if (id != null)
            {
                base.JsonWriter.WriteName("id");
                base.JsonWriter.WriteValue(id);
            }
            Uri uri = entry.EditLink ?? entry.ReadLink;
            if (uri != null)
            {
                base.JsonWriter.WriteName("uri");
                base.JsonWriter.WriteValue(base.UriToAbsoluteUriString(uri));
            }
            string eTag = entry.ETag;
            if (eTag != null)
            {
                base.WriteETag("etag", eTag);
            }
            string typeName = entry.TypeName;
            SerializationTypeNameAnnotation annotation = entry.GetAnnotation<SerializationTypeNameAnnotation>();
            if (annotation != null)
            {
                typeName = annotation.TypeName;
            }
            if (typeName != null)
            {
                base.JsonWriter.WriteName("type");
                base.JsonWriter.WriteValue(typeName);
            }
            ODataStreamReferenceValue mediaResource = entry.MediaResource;
            if (mediaResource != null)
            {
                WriterValidationUtils.ValidateStreamReferenceValue(mediaResource, true);
                base.WriteStreamReferenceValueContent(mediaResource);
            }
            IEnumerable<ODataAction> actions = entry.Actions;
            if (actions != null)
            {
                this.WriteOperations(actions.Cast<ODataOperation>(), true);
            }
            IEnumerable<ODataFunction> functions = entry.Functions;
            if (functions != null)
            {
                this.WriteOperations(functions.Cast<ODataOperation>(), false);
            }
            IEnumerable<ODataAssociationLink> associationLinks = entry.AssociationLinks;
            if (associationLinks != null)
            {
                bool flag = true;
                foreach (ODataAssociationLink link in associationLinks)
                {
                    ValidationUtils.ValidateAssociationLinkNotNull(link);
                    if (!projectedProperties.ShouldSkipProperty(link.Name))
                    {
                        if (flag)
                        {
                            base.JsonWriter.WriteName("properties");
                            base.JsonWriter.StartObjectScope();
                            flag = false;
                        }
                        base.ValidateAssociationLink(link, entryEntityType);
                        this.WriteAssociationLink(link, duplicatePropertyNamesChecker);
                    }
                }
                if (!flag)
                {
                    base.JsonWriter.EndObjectScope();
                }
            }
            base.JsonWriter.EndObjectScope();
        }

        private void WriteOperation(ODataOperation operation)
        {
            base.JsonWriter.StartObjectScope();
            if (operation.Title != null)
            {
                base.JsonWriter.WriteName("title");
                base.JsonWriter.WriteValue(operation.Title);
            }
            string str = base.UriToAbsoluteUriString(operation.Target);
            base.JsonWriter.WriteName("target");
            base.JsonWriter.WriteValue(str);
            base.JsonWriter.EndObjectScope();
        }

        private void WriteOperationMetadataGroup(IGrouping<string, ODataOperation> operations)
        {
            bool flag = true;
            foreach (ODataOperation operation in operations)
            {
                if (flag)
                {
                    base.JsonWriter.WriteName(operations.Key);
                    base.JsonWriter.StartArrayScope();
                    flag = false;
                }
                this.WriteOperation(operation);
            }
            base.JsonWriter.EndArrayScope();
        }

        private void WriteOperations(IEnumerable<ODataOperation> operations, bool isAction)
        {
            bool flag = true;
            string name = isAction ? "actions" : "functions";
            foreach (IGrouping<string, ODataOperation> grouping in operations.GroupBy<ODataOperation, string>(delegate (ODataOperation o) {
				ValidationUtils.ValidateOperationNotNull(o, isAction);
				WriterValidationUtils.ValidateOperation(o, this.WritingResponse);
				return this.UriToUriString(o.Metadata, false);
            }))
            {
                if (flag)
                {
                    base.JsonWriter.WriteName(name);
                    base.JsonWriter.StartObjectScope();
                    flag = false;
                }
                this.WriteOperationMetadataGroup(grouping);
            }
            if (!flag)
            {
                base.JsonWriter.EndObjectScope();
            }
        }
    }
}

