namespace System.Data.Services.Client.Materialization
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Data.Services.Client.Metadata;
    using System.Data.Services.Common;
    using System.Runtime.CompilerServices;

    internal class MaterializerEntry
    {
        private readonly System.Data.Services.Client.EntityDescriptor entityDescriptor;
        private readonly ODataEntry entry;
        private EntryFlags flags;
        private ICollection<ODataNavigationLink> navigationLinks;

        private MaterializerEntry()
        {
        }

        private MaterializerEntry(ODataEntry entry, DataServiceProtocolVersion maxProtocolVersion)
        {
            this.entry = entry;
            this.entityDescriptor = new System.Data.Services.Client.EntityDescriptor(maxProtocolVersion);
            SerializationTypeNameAnnotation annotation = entry.GetAnnotation<SerializationTypeNameAnnotation>();
            this.entityDescriptor.ServerTypeName = (annotation != null) ? annotation.TypeName : (this.entityDescriptor.ServerTypeName = this.Entry.TypeName);
        }

        public void AddNavigationLink(ODataNavigationLink link)
        {
            this.EntityDescriptor.AddNavigationLink(link.Name, link.Url);
            if (this.navigationLinks == null)
            {
                this.navigationLinks = new List<ODataNavigationLink>();
            }
            this.navigationLinks.Add(link);
        }

        public static MaterializerEntry CreateEmpty()
        {
            return new MaterializerEntry();
        }

        public static MaterializerEntry CreateEntry(ODataEntry entry, DataServiceProtocolVersion maxProtocolVersion)
        {
            MaterializerEntry annotation = new MaterializerEntry(entry, maxProtocolVersion);
            entry.SetAnnotation<MaterializerEntry>(annotation);
            if (entry.Id == null)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Deserialize_MissingIdElement);
            }
            annotation.EntityDescriptor.Identity = entry.Id;
            annotation.EntityDescriptor.EditLink = entry.EditLink;
            annotation.EntityDescriptor.SelfLink = entry.ReadLink;
            annotation.EntityDescriptor.ETag = entry.ETag;
            return annotation;
        }

        public static MaterializerEntry GetEntry(ODataEntry entry)
        {
            return entry.GetAnnotation<MaterializerEntry>();
        }

        private bool GetFlagValue(EntryFlags mask)
        {
            return ((this.flags & mask) != 0);
        }

        private void SetFlagValue(EntryFlags mask, bool value)
        {
            if (value)
            {
                this.flags |= mask;
            }
            else
            {
                this.flags &= ~mask;
            }
        }

        public void UpdateEntityDescriptor()
        {
            if (!this.EntityDescriptorUpdated)
            {
                if (this.entry.MediaResource != null)
                {
                    if (this.entry.MediaResource.ReadLink != null)
                    {
                        this.EntityDescriptor.ReadStreamUri = this.entry.MediaResource.ReadLink;
                    }
                    if (this.entry.MediaResource.EditLink != null)
                    {
                        this.EntityDescriptor.EditStreamUri = this.entry.MediaResource.EditLink;
                    }
                    if (this.entry.MediaResource.ETag != null)
                    {
                        this.EntityDescriptor.StreamETag = this.entry.MediaResource.ETag;
                    }
                }
                foreach (ODataProperty property in this.Properties)
                {
                    ODataStreamReferenceValue value2 = property.Value as ODataStreamReferenceValue;
                    if (value2 != null)
                    {
                        StreamDescriptor descriptor = this.EntityDescriptor.AddStreamInfoIfNotPresent(property.Name);
                        if (value2.ReadLink != null)
                        {
                            descriptor.SelfLink = value2.ReadLink;
                        }
                        if (value2.EditLink != null)
                        {
                            descriptor.EditLink = value2.EditLink;
                        }
                        descriptor.ETag = value2.ETag;
                        descriptor.ContentType = value2.ContentType;
                    }
                }
                foreach (ODataAssociationLink link in this.entry.AssociationLinks)
                {
                    this.EntityDescriptor.AddAssociationLink(link.Name, link.Url);
                }
                foreach (ODataFunction function in this.entry.Functions)
                {
                    FunctionDescriptor operationDescriptor = new FunctionDescriptor {
                        Title = function.Title,
                        Metadata = function.Metadata,
                        Target = function.Target
                    };
                    this.EntityDescriptor.AddOperationDescriptor(operationDescriptor);
                }
                foreach (ODataAction action in this.entry.Actions)
                {
                    ActionDescriptor descriptor3 = new ActionDescriptor {
                        Title = action.Title,
                        Metadata = action.Metadata,
                        Target = action.Target
                    };
                    this.EntityDescriptor.AddOperationDescriptor(descriptor3);
                }
                this.EntityDescriptorUpdated = true;
            }
        }

        public ClientTypeAnnotation ActualType { get; set; }

        public bool CreatedByMaterializer
        {
            get
            {
                return this.GetFlagValue(EntryFlags.CreatedByMaterializer);
            }
            set
            {
                this.SetFlagValue(EntryFlags.CreatedByMaterializer, value);
            }
        }

        public System.Data.Services.Client.EntityDescriptor EntityDescriptor
        {
            get
            {
                return this.entityDescriptor;
            }
        }

        private bool EntityDescriptorUpdated
        {
            get
            {
                return this.GetFlagValue(EntryFlags.EntityDescriptorUpdated);
            }
            set
            {
                this.SetFlagValue(EntryFlags.EntityDescriptorUpdated, value);
            }
        }

        public bool EntityHasBeenResolved
        {
            get
            {
                return this.GetFlagValue(EntryFlags.EntityHasBeenResolved);
            }
            set
            {
                this.SetFlagValue(EntryFlags.EntityHasBeenResolved, value);
            }
        }

        public ODataEntry Entry
        {
            get
            {
                return this.entry;
            }
        }

        public string Id
        {
            get
            {
                return this.entry.Id;
            }
        }

        public ICollection<ODataNavigationLink> NavigationLinks
        {
            get
            {
                return (this.navigationLinks ?? ((ICollection<ODataNavigationLink>) ODataMaterializer.EmptyLinks));
            }
        }

        public IEnumerable<ODataProperty> Properties
        {
            get
            {
                if (this.entry == null)
                {
                    return null;
                }
                return this.entry.Properties;
            }
        }

        public object ResolvedObject
        {
            get
            {
                if (this.entityDescriptor == null)
                {
                    return null;
                }
                return this.entityDescriptor.Entity;
            }
            set
            {
                this.entityDescriptor.Entity = value;
            }
        }

        public bool ShouldUpdateFromPayload
        {
            get
            {
                return this.GetFlagValue(EntryFlags.ShouldUpdateFromPayload);
            }
            set
            {
                this.SetFlagValue(EntryFlags.ShouldUpdateFromPayload, value);
            }
        }

        [Flags]
        private enum EntryFlags
        {
            CreatedByMaterializer = 2,
            EntityDescriptorUpdated = 8,
            EntityHasBeenResolved = 4,
            ShouldUpdateFromPayload = 1
        }
    }
}

