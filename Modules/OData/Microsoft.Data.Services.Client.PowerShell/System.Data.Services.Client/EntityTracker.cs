namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client.Metadata;
    using System.Data.Services.Common;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class EntityTracker
    {
        private Dictionary<LinkDescriptor, LinkDescriptor> bindings = new Dictionary<LinkDescriptor, LinkDescriptor>(LinkDescriptor.EquivalenceComparer);
        private Dictionary<object, EntityDescriptor> entityDescriptors = new Dictionary<object, EntityDescriptor>(EqualityComparer<object>.Default);
        private Dictionary<string, EntityDescriptor> identityToDescriptor;
        private readonly DataServiceProtocolVersion maxProtocolVersion;
        private uint nextChange;

        public EntityTracker(DataServiceProtocolVersion maxProtocolVersion)
        {
            this.maxProtocolVersion = maxProtocolVersion;
        }

        internal void AddEntityDescriptor(EntityDescriptor descriptor)
        {
            try
            {
                this.entityDescriptors.Add(descriptor.Entity, descriptor);
            }
            catch (ArgumentException)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_EntityAlreadyContained);
            }
        }

        internal void AddLink(LinkDescriptor linkDescriptor)
        {
            try
            {
                this.bindings.Add(linkDescriptor, linkDescriptor);
            }
            catch (ArgumentException)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_RelationAlreadyContained);
            }
        }

        internal void AttachIdentity(EntityDescriptor entityDescriptorFromMaterializer, MergeOption metadataMergeOption)
        {
            this.EnsureIdentityToResource();
            EntityDescriptor descriptor = this.entityDescriptors[entityDescriptorFromMaterializer.Entity];
            this.ValidateDuplicateIdentity(entityDescriptorFromMaterializer.Identity, descriptor);
            this.DetachResourceIdentity(descriptor);
            if (descriptor.IsDeepInsert)
            {
                LinkDescriptor descriptor2 = this.bindings[descriptor.GetRelatedEnd(this.maxProtocolVersion)];
                descriptor2.State = EntityStates.Unchanged;
            }
            descriptor.Identity = entityDescriptorFromMaterializer.Identity;
            AtomMaterializerLog.MergeEntityDescriptorInfo(descriptor, entityDescriptorFromMaterializer, true, metadataMergeOption);
            descriptor.State = EntityStates.Unchanged;
            this.identityToDescriptor[entityDescriptorFromMaterializer.Identity] = descriptor;
        }

        internal void AttachLink(object source, string sourceProperty, object target, MergeOption linkMerge)
        {
            LinkDescriptor linkDescriptor = new LinkDescriptor(source, sourceProperty, target, this.maxProtocolVersion);
            LinkDescriptor descriptor2 = this.TryGetLinkDescriptor(source, sourceProperty, target);
            if (descriptor2 != null)
            {
                switch (linkMerge)
                {
                    case MergeOption.OverwriteChanges:
                        linkDescriptor = descriptor2;
                        break;

                    case MergeOption.PreserveChanges:
                        if (((EntityStates.Added == descriptor2.State) || (EntityStates.Unchanged == descriptor2.State)) || ((EntityStates.Modified == descriptor2.State) && (descriptor2.Target != null)))
                        {
                            linkDescriptor = descriptor2;
                        }
                        break;

                    case MergeOption.NoTracking:
                        throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_RelationAlreadyContained);
                }
            }
            else
            {
                ClientEdmModel model = ClientEdmModel.GetModel(this.maxProtocolVersion);
                if (model.GetClientTypeAnnotation(model.GetOrCreateEdmType(source.GetType())).GetProperty(sourceProperty, false).IsEntityCollection || ((descriptor2 = this.DetachReferenceLink(source, sourceProperty, target, linkMerge)) == null))
                {
                    this.AddLink(linkDescriptor);
                    this.IncrementChange(linkDescriptor);
                }
                else if ((linkMerge != MergeOption.AppendOnly) && ((MergeOption.PreserveChanges != linkMerge) || (EntityStates.Modified != descriptor2.State)))
                {
                    linkDescriptor = descriptor2;
                }
            }
            linkDescriptor.State = EntityStates.Unchanged;
        }

        internal void AttachLocation(object entity, string identity, Uri editLink)
        {
            this.EnsureIdentityToResource();
            EntityDescriptor descriptor = this.entityDescriptors[entity];
            this.ValidateDuplicateIdentity(identity, descriptor);
            this.DetachResourceIdentity(descriptor);
            if (descriptor.IsDeepInsert)
            {
                LinkDescriptor descriptor2 = this.bindings[descriptor.GetRelatedEnd(this.maxProtocolVersion)];
                descriptor2.State = EntityStates.Unchanged;
            }
            descriptor.Identity = identity;
            descriptor.EditLink = editLink;
            this.identityToDescriptor[identity] = descriptor;
        }

        internal void DetachExistingLink(LinkDescriptor existingLink, bool targetDelete)
        {
            if (existingLink.Target != null)
            {
                EntityDescriptor entityDescriptor = this.GetEntityDescriptor(existingLink.Target);
                if (entityDescriptor.IsDeepInsert && !targetDelete)
                {
                    EntityDescriptor parentForInsert = entityDescriptor.ParentForInsert;
                    if (object.ReferenceEquals(entityDescriptor.ParentEntity, existingLink.Source) && ((parentForInsert.State != EntityStates.Deleted) || (parentForInsert.State != EntityStates.Detached)))
                    {
                        throw new InvalidOperationException(System.Data.Services.Client.Strings.Context_ChildResourceExists);
                    }
                }
            }
            if (this.TryRemoveLinkDescriptor(existingLink))
            {
                existingLink.State = EntityStates.Detached;
            }
        }

        internal LinkDescriptor DetachReferenceLink(object source, string sourceProperty, object target, MergeOption linkMerge)
        {
            LinkDescriptor existingLink = this.GetLinks(source, sourceProperty).FirstOrDefault<LinkDescriptor>();
            if (existingLink != null)
            {
                if (((target == existingLink.Target) || (linkMerge == MergeOption.AppendOnly)) || ((MergeOption.PreserveChanges == linkMerge) && (EntityStates.Modified == existingLink.State)))
                {
                    return existingLink;
                }
                this.DetachExistingLink(existingLink, false);
            }
            return null;
        }

        internal bool DetachResource(EntityDescriptor resource)
        {
            foreach (LinkDescriptor descriptor in this.bindings.Values.Where<LinkDescriptor>(new Func<LinkDescriptor, bool>(resource.IsRelatedEntity)).ToList<LinkDescriptor>())
            {
                this.DetachExistingLink(descriptor, (descriptor.Target == resource.Entity) && (resource.State == EntityStates.Added));
            }
            resource.ChangeOrder = uint.MaxValue;
            resource.State = EntityStates.Detached;
            this.entityDescriptors.Remove(resource.Entity);
            this.DetachResourceIdentity(resource);
            return true;
        }

        internal void DetachResourceIdentity(EntityDescriptor resource)
        {
            EntityDescriptor descriptor = null;
            if (((resource.Identity != null) && this.identityToDescriptor.TryGetValue(resource.Identity, out descriptor)) && object.ReferenceEquals(descriptor, resource))
            {
                this.identityToDescriptor.Remove(resource.Identity);
            }
        }

        private void EnsureIdentityToResource()
        {
            if (this.identityToDescriptor == null)
            {
                Interlocked.CompareExchange<Dictionary<string, EntityDescriptor>>(ref this.identityToDescriptor, new Dictionary<string, EntityDescriptor>(EqualityComparer<string>.Default), null);
            }
        }

        internal EntityDescriptor GetEntityDescriptor(object resource)
        {
            EntityDescriptor descriptor = this.TryGetEntityDescriptor(resource);
            if (descriptor == null)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_EntityNotContained);
            }
            return descriptor;
        }

        internal IEnumerable<LinkDescriptor> GetLinks(object source, string sourceProperty)
        {
            return (from o in this.bindings.Values
                where (o.Source == source) && (o.SourceProperty == sourceProperty)
                select o);
        }

        internal void IncrementChange(Descriptor descriptor)
        {
            descriptor.ChangeOrder = ++this.nextChange;
        }

        internal EntityDescriptor InternalAttachEntityDescriptor(EntityDescriptor entityDescriptorFromMaterializer, bool failIfDuplicated)
        {
            EntityDescriptor descriptor;
            EntityDescriptor descriptor2;
            this.EnsureIdentityToResource();
            this.entityDescriptors.TryGetValue(entityDescriptorFromMaterializer.Entity, out descriptor);
            this.identityToDescriptor.TryGetValue(entityDescriptorFromMaterializer.Identity, out descriptor2);
            if (failIfDuplicated && (descriptor != null))
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_EntityAlreadyContained);
            }
            if (descriptor != descriptor2)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_DifferentEntityAlreadyContained);
            }
            if (descriptor == null)
            {
                descriptor = entityDescriptorFromMaterializer;
                this.IncrementChange(entityDescriptorFromMaterializer);
                this.entityDescriptors.Add(entityDescriptorFromMaterializer.Entity, entityDescriptorFromMaterializer);
                this.identityToDescriptor.Add(entityDescriptorFromMaterializer.Identity, entityDescriptorFromMaterializer);
            }
            return descriptor;
        }

        internal object TryGetEntity(string resourceUri, out EntityStates state)
        {
            state = EntityStates.Detached;
            EntityDescriptor descriptor = null;
            if ((this.identityToDescriptor != null) && this.identityToDescriptor.TryGetValue(resourceUri, out descriptor))
            {
                state = descriptor.State;
                return descriptor.Entity;
            }
            return null;
        }

        internal EntityDescriptor TryGetEntityDescriptor(object entity)
        {
            EntityDescriptor descriptor = null;
            this.entityDescriptors.TryGetValue(entity, out descriptor);
            return descriptor;
        }

        internal EntityDescriptor TryGetEntityDescriptor(string identity)
        {
            EntityDescriptor descriptor;
            if ((this.identityToDescriptor != null) && this.identityToDescriptor.TryGetValue(identity, out descriptor))
            {
                return descriptor;
            }
            return null;
        }

        internal LinkDescriptor TryGetLinkDescriptor(object source, string sourceProperty, object target)
        {
            LinkDescriptor descriptor;
            this.bindings.TryGetValue(new LinkDescriptor(source, sourceProperty, target, this.maxProtocolVersion), out descriptor);
            return descriptor;
        }

        internal bool TryRemoveLinkDescriptor(LinkDescriptor linkDescriptor)
        {
            return this.bindings.Remove(linkDescriptor);
        }

        private void ValidateDuplicateIdentity(string identity, EntityDescriptor descriptor)
        {
            EntityDescriptor descriptor2;
            if ((this.identityToDescriptor.TryGetValue(identity, out descriptor2) && (descriptor != descriptor2)) && ((descriptor2.State != EntityStates.Deleted) && (descriptor2.State != EntityStates.Detached)))
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_DifferentEntityAlreadyContained);
            }
        }

        public IEnumerable<EntityDescriptor> Entities
        {
            get
            {
                return this.entityDescriptors.Values;
            }
        }

        public IEnumerable<LinkDescriptor> Links
        {
            get
            {
                return this.bindings.Values;
            }
        }
    }
}

