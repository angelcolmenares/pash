namespace System.Data.Services.Client
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Data.Services.Client.Metadata;
	using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal sealed class BindingObserver
    {
        private BindingGraph bindingGraph;

        internal BindingObserver(DataServiceContext context, Func<EntityChangedParams, bool> entityChanged, Func<EntityCollectionChangedParams, bool> collectionChanged)
        {
            this.Context = context;
            this.Context.ChangesSaved += new EventHandler<SaveChangesEventArgs>(this.OnChangesSaved);
            this.EntityChanged = entityChanged;
            this.CollectionChanged = collectionChanged;
            this.bindingGraph = new BindingGraph(this);
        }

        private void CollectUnTrackingInfo(object currentEntity, object parentEntity, string parentProperty, IList<UnTrackingInfo> entitiesToUnTrack)
        {
            foreach (EntityDescriptor descriptor in from x in this.Context.Entities
                where (x.ParentEntity == currentEntity) && (x.State == EntityStates.Added)
                select x)
            {
                this.CollectUnTrackingInfo(descriptor.Entity, descriptor.ParentEntity, descriptor.ParentPropertyForInsert, entitiesToUnTrack);
            }
            UnTrackingInfo item = new UnTrackingInfo {
                Entity = currentEntity,
                Parent = parentEntity,
                ParentProperty = parentProperty
            };
            entitiesToUnTrack.Add(item);
        }

        private void DeepRemoveDataServiceCollection(IEnumerable collection, object source, string sourceProperty, Action<object> itemValidator)
        {
            foreach (object obj2 in collection)
            {
                if (itemValidator != null)
                {
                    itemValidator(obj2);
                }
                List<UnTrackingInfo> entitiesToUnTrack = new List<UnTrackingInfo>();
                this.CollectUnTrackingInfo(obj2, source, sourceProperty, entitiesToUnTrack);
                foreach (UnTrackingInfo info in entitiesToUnTrack)
                {
                    this.bindingGraph.RemoveDataServiceCollectionItem(info.Entity, info.Parent, info.ParentProperty);
                }
            }
            this.bindingGraph.RemoveUnreachableVertices();
        }

        internal void HandleAddEntity(object source, string sourceProperty, string sourceEntitySet, ICollection collection, object target, string targetEntitySet)
        {
            if (!this.Context.ApplyingChanges && ((source == null) || !this.IsDetachedOrDeletedFromContext(source)))
            {
                EntityDescriptor entityDescriptor = this.Context.GetEntityDescriptor(target);
                if ((!this.AttachBehavior && ((entityDescriptor == null) || (((source != null) && !this.IsContextTrackingLink(source, sourceProperty, target)) && (entityDescriptor.State != EntityStates.Deleted)))) && (this.CollectionChanged != null))
                {
                    EntityCollectionChangedParams arg = new EntityCollectionChangedParams(this.Context, source, sourceProperty, sourceEntitySet, collection, target, targetEntitySet, NotifyCollectionChangedAction.Add);
                    if (this.CollectionChanged(arg))
                    {
                        return;
                    }
                }
                if ((source != null) && this.IsDetachedOrDeletedFromContext(source))
                {
                    throw new InvalidOperationException(Strings.DataBinding_BindingOperation_DetachedSource);
                }
                entityDescriptor = this.Context.GetEntityDescriptor(target);
                if (source != null)
                {
                    if (!this.AttachBehavior)
                    {
                        if (entityDescriptor == null)
                        {
                            this.Context.AddRelatedObject(source, sourceProperty, target);
                        }
                        else if ((entityDescriptor.State != EntityStates.Deleted) && !this.IsContextTrackingLink(source, sourceProperty, target))
                        {
                            this.Context.AddLink(source, sourceProperty, target);
                        }
                    }
                    else if (entityDescriptor == null)
                    {
                        BindingUtils.ValidateEntitySetName(targetEntitySet, target);
                        this.Context.AttachTo(targetEntitySet, target);
                        this.Context.AttachLink(source, sourceProperty, target);
                    }
                    else if ((entityDescriptor.State != EntityStates.Deleted) && !this.IsContextTrackingLink(source, sourceProperty, target))
                    {
                        this.Context.AttachLink(source, sourceProperty, target);
                    }
                }
                else if (entityDescriptor == null)
                {
                    BindingUtils.ValidateEntitySetName(targetEntitySet, target);
                    if (this.AttachBehavior)
                    {
                        this.Context.AttachTo(targetEntitySet, target);
                    }
                    else
                    {
                        this.Context.AddObject(targetEntitySet, target);
                    }
                }
            }
        }

        internal void HandleDeleteEntity(object source, string sourceProperty, string sourceEntitySet, ICollection collection, object target, string targetEntitySet)
        {
            if (!this.Context.ApplyingChanges && ((source == null) || !this.IsDetachedOrDeletedFromContext(source)))
            {
                if ((this.IsContextTrackingEntity(target) && !this.DetachBehavior) && (this.CollectionChanged != null))
                {
                    EntityCollectionChangedParams arg = new EntityCollectionChangedParams(this.Context, source, sourceProperty, sourceEntitySet, collection, target, targetEntitySet, NotifyCollectionChangedAction.Remove);
                    if (this.CollectionChanged(arg))
                    {
                        return;
                    }
                }
                if ((source != null) && !this.IsContextTrackingEntity(source))
                {
                    throw new InvalidOperationException(Strings.DataBinding_BindingOperation_DetachedSource);
                }
                if (this.IsContextTrackingEntity(target))
                {
                    if (this.DetachBehavior)
                    {
                        this.Context.Detach(target);
                    }
                    else
                    {
                        this.Context.DeleteObject(target);
                    }
                }
            }
        }

        private void HandleUpdateEntity(object entity, string propertyName, object propertyValue)
        {
            if (!this.Context.ApplyingChanges)
            {
                if (!BindingEntityInfo.IsEntityType(entity.GetType(), this.Context.MaxProtocolVersion))
                {
                    this.bindingGraph.GetAncestorEntityForComplexProperty(ref entity, ref propertyName, ref propertyValue);
                }
                if (!this.IsDetachedOrDeletedFromContext(entity))
                {
                    if (this.EntityChanged != null)
                    {
                        EntityChangedParams arg = new EntityChangedParams(this.Context, entity, propertyName, propertyValue, null, null);
                        if (this.EntityChanged(arg))
                        {
                            return;
                        }
                    }
                    if (this.IsContextTrackingEntity(entity))
                    {
                        this.Context.UpdateObject(entity);
                    }
                }
            }
        }

        internal void HandleUpdateEntityReference(object source, string sourceProperty, string sourceEntitySet, object target, string targetEntitySet)
        {
            if (!this.Context.ApplyingChanges && !this.IsDetachedOrDeletedFromContext(source))
            {
                EntityDescriptor entityDescriptor = (target != null) ? this.Context.GetEntityDescriptor(target) : null;
                if ((!this.AttachBehavior && ((entityDescriptor == null) || !this.IsContextTrackingLink(source, sourceProperty, target))) && (this.EntityChanged != null))
                {
                    EntityChangedParams arg = new EntityChangedParams(this.Context, source, sourceProperty, target, sourceEntitySet, targetEntitySet);
                    if (this.EntityChanged(arg))
                    {
                        return;
                    }
                }
                if (this.IsDetachedOrDeletedFromContext(source))
                {
                    throw new InvalidOperationException(Strings.DataBinding_BindingOperation_DetachedSource);
                }
                entityDescriptor = (target != null) ? this.Context.GetEntityDescriptor(target) : null;
                if (target != null)
                {
                    if (entityDescriptor == null)
                    {
                        BindingUtils.ValidateEntitySetName(targetEntitySet, target);
                        if (this.AttachBehavior)
                        {
                            this.Context.AttachTo(targetEntitySet, target);
                        }
                        else
                        {
                            this.Context.AddObject(targetEntitySet, target);
                        }
                        entityDescriptor = this.Context.GetEntityDescriptor(target);
                    }
                    if (!this.IsContextTrackingLink(source, sourceProperty, target))
                    {
                        if (!this.AttachBehavior)
                        {
                            this.Context.SetLink(source, sourceProperty, target);
                        }
                        else if (entityDescriptor.State != EntityStates.Deleted)
                        {
                            this.Context.AttachLink(source, sourceProperty, target);
                        }
                    }
                }
                else
                {
                    this.Context.SetLink(source, sourceProperty, null);
                }
            }
        }

        internal bool IsContextTrackingEntity(object entity)
        {
            return (this.Context.GetEntityDescriptor(entity) != null);
        }

        private bool IsContextTrackingLink(object source, string sourceProperty, object target)
        {
            return (this.Context.GetLinkDescriptor(source, sourceProperty, target) != null);
        }

        private bool IsDetachedOrDeletedFromContext(object entity)
        {
            EntityDescriptor entityDescriptor = this.Context.GetEntityDescriptor(entity);
            if (entityDescriptor != null)
            {
                return (entityDescriptor.State == EntityStates.Deleted);
            }
            return true;
        }

        private void OnAddToComplexTypeCollection(object collection, IList newItems)
        {
            if (newItems != null)
            {
                this.bindingGraph.AddComplexObjectsFromCollection(collection, newItems);
            }
        }

        private void OnAddToDataServiceCollection(NotifyCollectionChangedEventArgs eventArgs, object source, string sourceProperty, string targetEntitySet, object collection)
        {
            if (eventArgs.NewItems != null)
            {
                foreach (object obj2 in eventArgs.NewItems)
                {
                    if (obj2 == null)
                    {
                        throw new InvalidOperationException(Strings.DataBinding_BindingOperation_ArrayItemNull("Add"));
                    }
                    if (!BindingEntityInfo.IsEntityType(obj2.GetType(), this.Context.MaxProtocolVersion))
                    {
                        throw new InvalidOperationException(Strings.DataBinding_BindingOperation_ArrayItemNotEntity("Add"));
                    }
                    this.bindingGraph.AddEntity(source, sourceProperty, obj2, targetEntitySet, collection);
                }
            }
        }

        private void OnChangesSaved(object sender, SaveChangesEventArgs eventArgs)
        {
            this.bindingGraph.RemoveNonTrackedEntities();
        }

        internal void OnDataServiceCollectionChanged(object collection, NotifyCollectionChangedEventArgs eventArgs)
        {
            object obj2;
            string str;
            string str2;
            string str3;
            Util.CheckArgumentNull<object>(collection, "collection");
            Util.CheckArgumentNull<NotifyCollectionChangedEventArgs>(eventArgs, "eventArgs");
            this.bindingGraph.GetDataServiceCollectionInfo(collection, out obj2, out str, out str2, out str3);
            switch (eventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    this.OnAddToDataServiceCollection(eventArgs, obj2, str, str3, collection);
                    return;

                case NotifyCollectionChangedAction.Remove:
                    this.OnRemoveFromDataServiceCollection(eventArgs, obj2, str, collection);
                    return;

                case NotifyCollectionChangedAction.Replace:
                    this.OnRemoveFromDataServiceCollection(eventArgs, obj2, str, collection);
                    this.OnAddToDataServiceCollection(eventArgs, obj2, str, str3, collection);
                    return;

                case NotifyCollectionChangedAction.Move:
                    return;

                case NotifyCollectionChangedAction.Reset:
                    if (!this.DetachBehavior)
                    {
                        this.bindingGraph.RemoveCollection(collection);
                        return;
                    }
                    this.RemoveWithDetachDataServiceCollection(collection);
                    return;
            }
            throw new InvalidOperationException(Strings.DataBinding_DataServiceCollectionChangedUnknownActionCollection(eventArgs.Action));
        }

        internal void OnPrimitiveOrComplexCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            object obj2;
            string str;
            Type type;
            Util.CheckArgumentNull<object>(sender, "sender");
            Util.CheckArgumentNull<NotifyCollectionChangedEventArgs>(e, "e");
            this.bindingGraph.GetPrimitiveOrComplexCollectionInfo(sender, out obj2, out str, out type);
            if (!PrimitiveType.IsKnownNullableType(type))
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        this.OnAddToComplexTypeCollection(sender, e.NewItems);
                        goto Label_00BC;

                    case NotifyCollectionChangedAction.Remove:
                        this.OnRemoveFromComplexTypeCollection(sender, e.OldItems);
                        goto Label_00BC;

                    case NotifyCollectionChangedAction.Replace:
                        this.OnRemoveFromComplexTypeCollection(sender, e.OldItems);
                        this.OnAddToComplexTypeCollection(sender, e.NewItems);
                        goto Label_00BC;

                    case NotifyCollectionChangedAction.Move:
                        goto Label_00BC;

                    case NotifyCollectionChangedAction.Reset:
                        this.bindingGraph.RemoveCollection(sender);
                        goto Label_00BC;
                }
                throw new InvalidOperationException(Strings.DataBinding_CollectionChangedUnknownActionCollection(e.Action, sender.GetType()));
            }
        Label_00BC:
            this.HandleUpdateEntity(obj2, str, sender);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        internal void OnPropertyChanged(object source, PropertyChangedEventArgs eventArgs)
        {
            Util.CheckArgumentNull<object>(source, "source");
            Util.CheckArgumentNull<PropertyChangedEventArgs>(eventArgs, "eventArgs");
            string propertyName = eventArgs.PropertyName;
            if (string.IsNullOrEmpty(propertyName))
            {
                this.HandleUpdateEntity(source, null, null);
            }
            else
            {
                BindingEntityInfo.BindingPropertyInfo info;
                ClientPropertyAnnotation annotation;
                object obj2;
                if (BindingEntityInfo.TryGetPropertyValue(source, propertyName, this.Context.MaxProtocolVersion, out info, out annotation, out obj2))
                {
                    if (info == null)
                    {
                        if (!annotation.IsStreamLinkProperty)
                        {
                            this.HandleUpdateEntity(source, propertyName, obj2);
                        }
                    }
                    else
                    {
                        this.bindingGraph.RemoveRelation(source, propertyName);
                        switch (info.PropertyKind)
                        {
                            case BindingPropertyKind.BindingPropertyKindEntity:
                                this.bindingGraph.AddEntity(source, propertyName, obj2, null, source);
                                return;

                            case BindingPropertyKind.BindingPropertyKindDataServiceCollection:
                                if (obj2 == null)
                                {
                                    return;
                                }
                                try
                                {
                                    typeof(BindingUtils).GetMethod("VerifyObserverNotPresent", false, true).MakeGenericMethod(new Type[] { info.PropertyInfo.EntityCollectionItemType }).Invoke(null, new object[] { obj2, propertyName, source.GetType() });
                                }
                                catch (TargetInvocationException exception)
                                {
                                    throw exception.InnerException;
                                }
                                try
                                {
                                    this.AttachBehavior = true;
                                    this.bindingGraph.AddDataServiceCollection(source, propertyName, obj2, null);
                                    return;
                                }
                                finally
                                {
                                    this.AttachBehavior = false;
                                }
                                break;

                            case BindingPropertyKind.BindingPropertyKindPrimitiveOrComplexCollection:
                                break;

                            default:
                                if (obj2 != null)
                                {
                                    this.bindingGraph.AddComplexObject(source, propertyName, obj2);
                                }
                                this.HandleUpdateEntity(source, propertyName, obj2);
                                return;
                        }
                        if (obj2 != null)
                        {
                            this.bindingGraph.AddPrimitiveOrComplexCollection(source, propertyName, obj2, info.PropertyInfo.PrimitiveOrComplexCollectionItemType);
                        }
                        this.HandleUpdateEntity(source, propertyName, obj2);
                    }
                }
            }
        }

        private void OnRemoveFromComplexTypeCollection(object collection, IList items)
        {
            if (items != null)
            {
                foreach (object obj2 in items)
                {
                    this.bindingGraph.RemoveComplexTypeCollectionItem(obj2, collection);
                }
                this.bindingGraph.RemoveUnreachableVertices();
            }
        }

        private void OnRemoveFromDataServiceCollection(NotifyCollectionChangedEventArgs eventArgs, object source, string sourceProperty, object collection)
        {
            if (eventArgs.OldItems != null)
            {
                this.DeepRemoveDataServiceCollection(eventArgs.OldItems, source ?? collection, sourceProperty, new Action<object>(this.ValidateDataServiceCollectionItem));
            }
        }

        private void RemoveWithDetachDataServiceCollection(object collection)
        {
            object source = null;
            string sourceProperty = null;
            string sourceEntitySet = null;
            string targetEntitySet = null;
            this.bindingGraph.GetDataServiceCollectionInfo(collection, out source, out sourceProperty, out sourceEntitySet, out targetEntitySet);
            this.DeepRemoveDataServiceCollection(this.bindingGraph.GetDataServiceCollectionItems(collection), source ?? collection, sourceProperty, null);
        }

        internal void StartTracking<T>(DataServiceCollection<T> collection, string collectionEntitySet)
        {
            try
            {
                this.AttachBehavior = true;
                this.bindingGraph.AddDataServiceCollection(null, null, collection, collectionEntitySet);
            }
            finally
            {
                this.AttachBehavior = false;
            }
        }

        internal void StopTracking()
        {
            this.bindingGraph.Reset();
            this.Context.ChangesSaved -= new EventHandler<SaveChangesEventArgs>(this.OnChangesSaved);
        }

        private void ValidateDataServiceCollectionItem(object target)
        {
            if (target == null)
            {
                throw new InvalidOperationException(Strings.DataBinding_BindingOperation_ArrayItemNull("Remove"));
            }
            if (!BindingEntityInfo.IsEntityType(target.GetType(), this.Context.MaxProtocolVersion))
            {
                throw new InvalidOperationException(Strings.DataBinding_BindingOperation_ArrayItemNotEntity("Remove"));
            }
        }

        internal bool AttachBehavior { get; set; }

        internal Func<EntityCollectionChangedParams, bool> CollectionChanged { get; private set; }

        internal DataServiceContext Context { get; private set; }

        internal bool DetachBehavior { get; set; }

        internal Func<EntityChangedParams, bool> EntityChanged { get; private set; }

        private class UnTrackingInfo
        {
            public object Entity { get; set; }

            public object Parent { get; set; }

            public string ParentProperty { get; set; }
        }
    }
}

