namespace System.Data.Services.Client
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client.Materialization;
    using System.Data.Services.Client.Metadata;
    using System.Data.Services.Common;
    using System.Linq;
    using System.Runtime.InteropServices;

    internal class AtomMaterializerLog
    {
        private readonly Dictionary<string, ODataEntry> appendOnlyEntries = new Dictionary<string, ODataEntry>(EqualityComparer<string>.Default);
        private readonly Dictionary<string, ODataEntry> identityStack;
        private object insertRefreshObject;
        private readonly List<LinkDescriptor> links;
        private readonly MergeOption mergeOption;
        private readonly ResponseInfo responseInfo;

        internal AtomMaterializerLog(ResponseInfo responseInfo)
        {
            this.responseInfo = responseInfo;
            this.mergeOption = responseInfo.MergeOption;
            this.identityStack = new Dictionary<string, ODataEntry>(EqualityComparer<string>.Default);
            this.links = new List<LinkDescriptor>();
        }

        internal void AddedLink(MaterializerEntry source, string propertyName, object target)
        {
            if (this.Tracking && (ShouldTrackWithContext(source) && ShouldTrackWithContext(target, this.responseInfo.MaxProtocolVersion)))
            {
                LinkDescriptor item = new LinkDescriptor(source.ResolvedObject, propertyName, target, EntityStates.Added);
                this.links.Add(item);
            }
        }

        internal void ApplyToContext()
        {
            if (this.Tracking)
            {
                foreach (KeyValuePair<string, ODataEntry> pair in this.identityStack)
                {
                    MaterializerEntry entry = MaterializerEntry.GetEntry(pair.Value);
                    bool mergeInfo = (entry.CreatedByMaterializer || (entry.ResolvedObject == this.insertRefreshObject)) || entry.ShouldUpdateFromPayload;
                    EntityDescriptor trackedEntityDescriptor = this.responseInfo.EntityTracker.InternalAttachEntityDescriptor(entry.EntityDescriptor, false);
                    MergeEntityDescriptorInfo(trackedEntityDescriptor, entry.EntityDescriptor, mergeInfo, this.mergeOption);
                    if (mergeInfo && ((this.responseInfo.MergeOption != MergeOption.PreserveChanges) || (trackedEntityDescriptor.State != EntityStates.Deleted)))
                    {
                        trackedEntityDescriptor.State = EntityStates.Unchanged;
                    }
                }
                foreach (LinkDescriptor descriptor2 in this.links)
                {
                    if (EntityStates.Added == descriptor2.State)
                    {
                        if ((EntityStates.Deleted == this.responseInfo.EntityTracker.GetEntityDescriptor(descriptor2.Target).State) || (EntityStates.Deleted == this.responseInfo.EntityTracker.GetEntityDescriptor(descriptor2.Source).State))
                        {
                            this.responseInfo.EntityTracker.DetachExistingLink(descriptor2, false);
                        }
                        else
                        {
                            this.responseInfo.EntityTracker.AttachLink(descriptor2.Source, descriptor2.SourceProperty, descriptor2.Target, this.mergeOption);
                        }
                    }
                    else
                    {
                        if (EntityStates.Modified == descriptor2.State)
                        {
                            object target = descriptor2.Target;
                            if (MergeOption.PreserveChanges == this.mergeOption)
                            {
                                LinkDescriptor descriptor3 = this.responseInfo.EntityTracker.GetLinks(descriptor2.Source, descriptor2.SourceProperty).SingleOrDefault<LinkDescriptor>();
                                if ((descriptor3 != null) && (descriptor3.Target == null))
                                {
                                    goto Label_0233;
                                }
                                if (((target != null) && (EntityStates.Deleted == this.responseInfo.EntityTracker.GetEntityDescriptor(target).State)) || (EntityStates.Deleted == this.responseInfo.EntityTracker.GetEntityDescriptor(descriptor2.Source).State))
                                {
                                    target = null;
                                }
                            }
                            this.responseInfo.EntityTracker.AttachLink(descriptor2.Source, descriptor2.SourceProperty, target, this.mergeOption);
                        }
                        else
                        {
                            this.responseInfo.EntityTracker.DetachExistingLink(descriptor2, false);
                        }
                    Label_0233:;
                    }
                }
            }
        }

        internal void Clear()
        {
            this.identityStack.Clear();
            this.links.Clear();
            this.insertRefreshObject = null;
        }

        internal void CreatedInstance(MaterializerEntry entry)
        {
            if (ShouldTrackWithContext(entry))
            {
                this.identityStack.Add(entry.Id, entry.Entry);
                if (this.mergeOption == MergeOption.AppendOnly)
                {
                    this.appendOnlyEntries.Add(entry.Id, entry.Entry);
                }
            }
        }

        internal void FoundExistingInstance(MaterializerEntry entry)
        {
            this.identityStack[entry.Id] = entry.Entry;
        }

        internal void FoundTargetInstance(MaterializerEntry entry)
        {
            if (ShouldTrackWithContext(entry))
            {
                this.responseInfo.EntityTracker.AttachIdentity(entry.EntityDescriptor, this.mergeOption);
                this.identityStack.Add(entry.Id, entry.Entry);
                this.insertRefreshObject = entry.ResolvedObject;
            }
        }

        internal static void MergeEntityDescriptorInfo(EntityDescriptor trackedEntityDescriptor, EntityDescriptor entityDescriptorFromMaterializer, bool mergeInfo, MergeOption mergeOption)
        {
            if (!object.ReferenceEquals(trackedEntityDescriptor, entityDescriptorFromMaterializer))
            {
                if ((entityDescriptorFromMaterializer.ETag != null) && (mergeOption != MergeOption.AppendOnly))
                {
                    trackedEntityDescriptor.ETag = entityDescriptorFromMaterializer.ETag;
                }
                if (mergeInfo)
                {
                    if (entityDescriptorFromMaterializer.SelfLink != null)
                    {
                        trackedEntityDescriptor.SelfLink = entityDescriptorFromMaterializer.SelfLink;
                    }
                    if (entityDescriptorFromMaterializer.EditLink != null)
                    {
                        trackedEntityDescriptor.EditLink = entityDescriptorFromMaterializer.EditLink;
                    }
                    foreach (LinkInfo info in entityDescriptorFromMaterializer.LinkInfos)
                    {
                        trackedEntityDescriptor.MergeLinkInfo(info);
                    }
                    foreach (StreamDescriptor descriptor in entityDescriptorFromMaterializer.StreamDescriptors)
                    {
                        trackedEntityDescriptor.MergeStreamDescriptor(descriptor);
                    }
                    trackedEntityDescriptor.ServerTypeName = entityDescriptorFromMaterializer.ServerTypeName;
                }
                if (entityDescriptorFromMaterializer.ReadStreamUri != null)
                {
                    trackedEntityDescriptor.ReadStreamUri = entityDescriptorFromMaterializer.ReadStreamUri;
                }
                if (entityDescriptorFromMaterializer.EditStreamUri != null)
                {
                    trackedEntityDescriptor.EditStreamUri = entityDescriptorFromMaterializer.EditStreamUri;
                }
                if ((entityDescriptorFromMaterializer.ReadStreamUri != null) || (entityDescriptorFromMaterializer.EditStreamUri != null))
                {
                    trackedEntityDescriptor.StreamETag = entityDescriptorFromMaterializer.StreamETag;
                }
            }
        }

        internal void RemovedLink(MaterializerEntry source, string propertyName, object target)
        {
            if (ShouldTrackWithContext(source) && ShouldTrackWithContext(target, this.responseInfo.MaxProtocolVersion))
            {
                LinkDescriptor item = new LinkDescriptor(source.ResolvedObject, propertyName, target, EntityStates.Detached);
                this.links.Add(item);
            }
        }

        internal void SetLink(MaterializerEntry source, string propertyName, object target)
        {
            if (this.Tracking && (ShouldTrackWithContext(source) && ShouldTrackWithContext(target, this.responseInfo.MaxProtocolVersion)))
            {
                LinkDescriptor item = new LinkDescriptor(source.ResolvedObject, propertyName, target, EntityStates.Modified);
                this.links.Add(item);
            }
        }

        private static bool ShouldTrackWithContext(MaterializerEntry entry)
        {
            return entry.ActualType.IsEntityType;
        }

        private static bool ShouldTrackWithContext(object entity, DataServiceProtocolVersion maxProtocolVersion)
        {
            return ((entity == null) || ClientTypeUtil.TypeIsEntity(entity.GetType(), maxProtocolVersion));
        }

        internal bool TryResolve(MaterializerEntry entry, out MaterializerEntry existingEntry)
        {
            ODataEntry entry2;
            if (this.identityStack.TryGetValue(entry.Id, out entry2))
            {
                existingEntry = MaterializerEntry.GetEntry(entry2);
                return true;
            }
            if (this.appendOnlyEntries.TryGetValue(entry.Id, out entry2))
            {
                EntityStates states;
                this.responseInfo.EntityTracker.TryGetEntity(entry.Id, out states);
                if (states == EntityStates.Unchanged)
                {
                    existingEntry = MaterializerEntry.GetEntry(entry2);
                    return true;
                }
                this.appendOnlyEntries.Remove(entry.Id);
            }
            existingEntry = null;
            return false;
        }

        internal bool Tracking
        {
            get
            {
                return (this.mergeOption != MergeOption.NoTracking);
            }
        }
    }
}

