namespace System.Data.Services.Client.Materialization
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Services.Client;
    using System.Data.Services.Client.Metadata;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal abstract class ODataEntityMaterializer : ODataMaterializer
    {
        protected object currentValue;
        private readonly AtomMaterializerLog log;
        private readonly ProjectionPlan materializeEntryPlan;
        private readonly MergeOption mergeOption;
        private object targetInstance;

        public ODataEntityMaterializer(ResponseInfo responseInfo, QueryComponents queryComponents, Type expectedType, ProjectionPlan materializeEntryPlan) : base(responseInfo, expectedType)
        {
            this.materializeEntryPlan = materializeEntryPlan ?? CreatePlan(queryComponents);
            this.mergeOption = base.ResponseInfo.MergeOption;
            this.log = new AtomMaterializerLog(base.ResponseInfo);
        }

        private void ApplyFeedToCollection(MaterializerEntry entry, ClientPropertyAnnotation property, ODataFeed feed, bool includeLinks)
        {
            ClientEdmModel model = ClientEdmModel.GetModel(base.ResponseInfo.MaxProtocolVersion);
            ClientTypeAnnotation clientTypeAnnotation = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(property.EntityCollectionItemType));
            IEnumerable<ODataEntry> entries = MaterializerFeed.GetFeed(feed).Entries;
            foreach (ODataEntry entry2 in entries)
            {
                this.Materialize(MaterializerEntry.GetEntry(entry2), clientTypeAnnotation.ElementType, includeLinks);
            }
            ProjectionPlan continuationPlan = includeLinks ? CreatePlanForDirectMaterialization(property.EntityCollectionItemType) : CreatePlanForShallowMaterialization(property.EntityCollectionItemType);
            this.ApplyItemsToCollection(entry, property, from e in entries select MaterializerEntry.GetEntry(e).ResolvedObject, feed.NextPageLink, continuationPlan);
        }

        private void ApplyItemsToCollection(MaterializerEntry entry, ClientPropertyAnnotation property, IEnumerable items, Uri nextLink, ProjectionPlan continuationPlan)
        {
            Func<LinkDescriptor, bool> predicate = null;
            object instance = entry.ShouldUpdateFromPayload ? GetOrCreateCollectionProperty(entry.ResolvedObject, property, null) : null;
            ClientEdmModel model = ClientEdmModel.GetModel(base.ResponseInfo.MaxProtocolVersion);
            ClientTypeAnnotation clientTypeAnnotation = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(property.EntityCollectionItemType));
            foreach (object obj3 in items)
            {
                if (!clientTypeAnnotation.ElementType.IsAssignableFrom(obj3.GetType()))
                {
                    throw new InvalidOperationException(System.Data.Services.Client.Strings.AtomMaterializer_EntryIntoCollectionMismatch(obj3.GetType().FullName, clientTypeAnnotation.ElementType.FullName));
                }
                if (entry.ShouldUpdateFromPayload)
                {
                    property.SetValue(instance, obj3, property.PropertyName, true);
                    this.log.AddedLink(entry, property.PropertyName, obj3);
                }
            }
            if (entry.ShouldUpdateFromPayload)
            {
                this.FoundNextLinkForCollection(instance as IEnumerable, nextLink, continuationPlan);
            }
            else
            {
                this.FoundNextLinkForUnmodifiedCollection(property.GetValue(entry.ResolvedObject) as IEnumerable);
            }
            if ((this.mergeOption == MergeOption.OverwriteChanges) || (this.mergeOption == MergeOption.PreserveChanges))
            {
                if (predicate == null)
                {
                    predicate = delegate (LinkDescriptor x) {
                        if (MergeOption.OverwriteChanges != this.mergeOption)
                        {
                            return EntityStates.Added != x.State;
                        }
                        return true;
                    };
                }
                foreach (object obj4 in (from x in base.ResponseInfo.EntityTracker.GetLinks(entry.ResolvedObject, property.PropertyName).Where<LinkDescriptor>(predicate) select x.Target).Except<object>(EnumerateAsElementType<object>(items)))
                {
                    if (instance != null)
                    {
                        property.RemoveValue(instance, obj4);
                    }
                    this.log.RemovedLink(entry, property.PropertyName, obj4);
                }
            }
        }

        private static void ApplyLinkProperties(ClientTypeAnnotation actualType, MaterializerEntry entry)
        {
            if (entry.ShouldUpdateFromPayload)
            {
                foreach (ClientPropertyAnnotation annotation in from p in actualType.Properties()
                    where p.PropertyType == typeof(DataServiceStreamLink)
                    select p)
                {
                    StreamDescriptor descriptor;
                    string propertyName = annotation.PropertyName;
                    if (entry.EntityDescriptor.TryGetNamedStreamInfo(propertyName, out descriptor))
                    {
                        annotation.SetValue(entry.ResolvedObject, descriptor.StreamLink, propertyName, true);
                    }
                }
            }
        }

        internal sealed override void ApplyLogToContext()
        {
            this.log.ApplyToContext();
        }

        private static void CheckEntryToAccessNotNull(MaterializerEntry entry, string name)
        {
            if (entry.Entry == null)
            {
                throw new NullReferenceException(System.Data.Services.Client.Strings.AtomMaterializer_EntryToAccessIsNull(name));
            }
        }

        internal sealed override void ClearLog()
        {
            this.log.Clear();
        }

        private static ProjectionPlan CreatePlan(QueryComponents queryComponents)
        {
            LambdaExpression projection = queryComponents.Projection;
            if (projection == null)
            {
                return CreatePlanForDirectMaterialization(queryComponents.LastSegmentType);
            }
            ProjectionPlan plan = ProjectionPlanCompiler.CompilePlan(projection, queryComponents.NormalizerRewrites);
            plan.LastSegmentType = queryComponents.LastSegmentType;
            return plan;
        }

        private static ProjectionPlan CreatePlanForDirectMaterialization(Type lastSegmentType)
        {
            return new ProjectionPlan { Plan = new Func<object, object, Type, object>(ODataEntityMaterializerInvoker.DirectMaterializePlan), ProjectedType = lastSegmentType, LastSegmentType = lastSegmentType };
        }

        private static ProjectionPlan CreatePlanForShallowMaterialization(Type lastSegmentType)
        {
            return new ProjectionPlan { Plan = new Func<object, object, Type, object>(ODataEntityMaterializerInvoker.ShallowMaterializePlan), ProjectedType = lastSegmentType, LastSegmentType = lastSegmentType };
        }

        internal static object DirectMaterializePlan(ODataEntityMaterializer materializer, MaterializerEntry entry, Type expectedEntryType)
        {
            materializer.Materialize(entry, expectedEntryType, true);
            return entry.ResolvedObject;
        }

        internal static IEnumerable<T> EnumerateAsElementType<T>(IEnumerable source)
        {
            IEnumerable<T> enumerable = source as IEnumerable<T>;
            if (enumerable != null)
            {
                return enumerable;
            }
            return EnumerateAsElementTypeInternal<T>(source);
        }

        internal static IEnumerable<T> EnumerateAsElementTypeInternal<T>(IEnumerable source)
        {
            IEnumerator enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                object current = enumerator.Current;
                yield return (T) current;
            }
        }

        private void FoundNextLinkForCollection(IEnumerable collection, Uri link, ProjectionPlan plan)
        {
            if ((collection != null) && !base.nextLinkTable.ContainsKey(collection))
            {
                DataServiceQueryContinuation continuation = DataServiceQueryContinuation.Create(link, plan);
                base.nextLinkTable.Add(collection, continuation);
                Util.SetNextLinkForCollection(collection, continuation);
            }
        }

        private void FoundNextLinkForUnmodifiedCollection(IEnumerable collection)
        {
            if ((collection != null) && !base.nextLinkTable.ContainsKey(collection))
            {
                base.nextLinkTable.Add(collection, null);
            }
        }

        private static object GetOrCreateCollectionProperty(object instance, ClientPropertyAnnotation property, Type collectionType)
        {
            object obj2 = property.GetValue(instance);
            if (obj2 == null)
            {
                if (collectionType == null)
                {
                    collectionType = property.PropertyType;
                    if (collectionType.IsInterfaceEx())
                    {
                        collectionType = typeof(Collection<>).MakeGenericType(new Type[] { property.EntityCollectionItemType });
                    }
                }
                obj2 = Activator.CreateInstance(collectionType);
                property.SetValue(instance, obj2, property.PropertyName, false);
            }
            return obj2;
        }

        private static MaterializerNavigationLink GetPropertyOrThrow(IEnumerable<ODataNavigationLink> links, string propertyName, string entryIdentity)
        {
            Func<ODataNavigationLink, bool> predicate = null;
            ODataNavigationLink link = null;
            if (links != null)
            {
                if (predicate == null)
                {
                    predicate = p => p.Name == propertyName;
                }
                link = links.Where<ODataNavigationLink>(predicate).FirstOrDefault<ODataNavigationLink>();
            }
            if (link == null)
            {
                throw new InvalidOperationException(System.Data.Services.Client.Strings.AtomMaterializer_PropertyMissing(propertyName, entryIdentity));
            }
            return MaterializerNavigationLink.GetLink(link);
        }

        internal static List<TTarget> ListAsElementType<T, TTarget>(ODataEntityMaterializer materializer, IEnumerable<T> source) where T: TTarget
        {
            List<TTarget> list2;
            DataServiceQueryContinuation continuation;
            List<TTarget> list = source as List<TTarget>;
            if (list != null)
            {
                return list;
            }
            IList list3 = source as IList;
            if (list3 != null)
            {
                list2 = new List<TTarget>(list3.Count);
            }
            else
            {
                list2 = new List<TTarget>();
            }
            foreach (T local in source)
            {
                list2.Add((TTarget) local);
            }
            if (materializer.nextLinkTable.TryGetValue(source, out continuation))
            {
                materializer.nextLinkTable[list2] = continuation;
            }
            return list2;
        }

        private void Materialize(MaterializerEntry entry, Type expectedEntryType, bool includeLinks)
        {
            this.ResolveOrCreateInstance(entry, expectedEntryType);
            this.MaterializeResolvedEntry(entry, includeLinks);
        }

        private void MaterializeResolvedEntry(MaterializerEntry entry, bool includeLinks)
        {
            ClientTypeAnnotation actualType = entry.ActualType;
            if (!actualType.IsEntityType)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.AtomMaterializer_InvalidNonEntityType(actualType.ElementTypeName));
            }
            ODataMaterializer.MaterializeDataValues(actualType, entry.Properties, base.ResponseInfo.IgnoreMissingProperties);
            if (entry.NavigationLinks != null)
            {
                foreach (ODataNavigationLink link in entry.NavigationLinks)
                {
                    ClientPropertyAnnotation annotation2 = actualType.GetProperty(link.Name, true);
                    if (annotation2 != null)
                    {
                        ValidatePropertyMatch(annotation2, link, base.ResponseInfo, true);
                    }
                }
            }
            if (includeLinks && (entry.NavigationLinks != null))
            {
                foreach (ODataNavigationLink link2 in entry.NavigationLinks)
                {
                    MaterializerNavigationLink link3 = MaterializerNavigationLink.GetLink(link2);
                    if (link3 != null)
                    {
                        ClientPropertyAnnotation annotation3 = actualType.GetProperty(link2.Name, base.ResponseInfo.IgnoreMissingProperties);
                        if (annotation3 != null)
                        {
                            if (link3.Feed != null)
                            {
                                this.ApplyFeedToCollection(entry, annotation3, link3.Feed, includeLinks);
                            }
                            else if (link3.Entry != null)
                            {
                                MaterializerEntry entry2 = link3.Entry;
                                if (entry2.Entry != null)
                                {
                                    this.Materialize(entry2, annotation3.PropertyType, includeLinks);
                                }
                                if (entry.ShouldUpdateFromPayload)
                                {
                                    annotation3.SetValue(entry.ResolvedObject, entry2.ResolvedObject, link2.Name, true);
                                    this.log.SetLink(entry, annotation3.PropertyName, entry2.ResolvedObject);
                                }
                            }
                        }
                    }
                }
            }
            foreach (ODataProperty property in entry.Properties)
            {
                if (!(property.Value is ODataStreamReferenceValue))
                {
                    ClientPropertyAnnotation annotation4 = actualType.GetProperty(property.Name, base.ResponseInfo.IgnoreMissingProperties);
                    if ((annotation4 != null) && entry.ShouldUpdateFromPayload)
                    {
                        ValidatePropertyMatch(annotation4, property, base.ResponseInfo, true);
                        ODataMaterializer.ApplyDataValue(actualType, property, base.ResponseInfo.IgnoreMissingProperties, base.ResponseInfo, entry.ResolvedObject);
                    }
                }
            }
            ApplyLinkProperties(actualType, entry);
            if (base.ResponseInfo.HasReadingEntityHandlers)
            {
                ODataMaterializer.ReadingEntityInfo annotation = entry.Entry.GetAnnotation<ODataMaterializer.ReadingEntityInfo>();
                base.ResponseInfo.FireReadingEntityEvent(entry.ResolvedObject, annotation.EntryPayload, annotation.BaseUri);
            }
        }

        private static void MaterializeToList(ODataEntityMaterializer materializer, IEnumerable list, Type nestedExpectedType, IEnumerable<ODataEntry> entries)
        {
            Action<object, object> addToCollectionDelegate = ODataMaterializer.GetAddToCollectionDelegate(list.GetType());
            foreach (ODataEntry entry in entries)
            {
                MaterializerEntry entry2 = MaterializerEntry.GetEntry(entry);
                if (!entry2.EntityHasBeenResolved)
                {
                    materializer.Materialize(entry2, nestedExpectedType, false);
                }
                addToCollectionDelegate(list, entry2.ResolvedObject);
            }
        }

        private void MergeLists(MaterializerEntry entry, ClientPropertyAnnotation property, IEnumerable list, Uri nextLink, ProjectionPlan plan)
        {
            if ((entry.ShouldUpdateFromPayload && (property.NullablePropertyType == list.GetType())) && (property.GetValue(entry.ResolvedObject) == null))
            {
                property.SetValue(entry.ResolvedObject, list, property.PropertyName, false);
                this.FoundNextLinkForCollection(list, nextLink, plan);
                foreach (object obj2 in list)
                {
                    this.log.AddedLink(entry, property.PropertyName, obj2);
                }
            }
            else
            {
                this.ApplyItemsToCollection(entry, property, list, nextLink, plan);
            }
        }

        internal static bool ProjectionCheckValueForPathIsNull(MaterializerEntry entry, Type expectedType, ProjectionPath path)
        {
            if ((path.Count == 0) || ((path.Count == 1) && (path[0].Member == null)))
            {
                return (entry.Entry == null);
            }
            bool flag = false;
            MaterializerNavigationLink link = null;
            IEnumerable<ODataNavigationLink> navigationLinks = entry.NavigationLinks;
            ClientEdmModel model = ClientEdmModel.GetModel(entry.EntityDescriptor.MaxProtocolVersion);
            for (int i = 0; i < path.Count; i++)
            {
                Func<ODataNavigationLink, bool> predicate = null;
                string propertyName;
                ProjectionPathSegment segment = path[i];
                if (segment.Member != null)
                {
                    bool flag2 = i == (path.Count - 1);
                    propertyName = segment.Member;
                    if (segment.SourceTypeAs != null)
                    {
                        expectedType = segment.SourceTypeAs;
                        if (predicate == null)
                        {
                            predicate = p => p.Name == propertyName;
                        }
                        if (!navigationLinks.Any<ODataNavigationLink>(predicate))
                        {
                            return true;
                        }
                    }
                    IEdmType orCreateEdmType = model.GetOrCreateEdmType(expectedType);
                    ClientPropertyAnnotation property = model.GetClientTypeAnnotation(orCreateEdmType).GetProperty(propertyName, false);
                    link = GetPropertyOrThrow(navigationLinks, propertyName, entry.Id);
                    ValidatePropertyMatch(property, link.Link);
                    if (link.Feed != null)
                    {
                        flag = false;
                    }
                    else
                    {
                        if (link.Entry == null)
                        {
                            return true;
                        }
                        if (flag2)
                        {
                            flag = link.Entry.Entry == null;
                        }
                        else
                        {
                            entry = link.Entry;
                            navigationLinks = entry.NavigationLinks;
                        }
                    }
                    expectedType = property.PropertyType;
                }
            }
            return flag;
        }

        internal static void ProjectionEnsureEntryAvailableOfType(ODataEntityMaterializer materializer, MaterializerEntry entry, Type requiredType)
        {
            if (entry.Id == null)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Deserialize_MissingIdElement);
            }
            if (!materializer.TryResolveAsCreated(entry) && !materializer.TryResolveFromContext(entry, requiredType))
            {
                materializer.ResolveByCreatingWithType(entry, requiredType);
            }
            else if (!requiredType.IsAssignableFrom(entry.ResolvedObject.GetType()))
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Deserialize_Current(requiredType, entry.ResolvedObject.GetType()));
            }
        }

        internal static ODataEntry ProjectionGetEntry(MaterializerEntry entry, string name)
        {
            MaterializerEntry entry2 = GetPropertyOrThrow(entry.NavigationLinks, name, entry.Id).Entry;
            if (entry2 == null)
            {
                throw new InvalidOperationException(System.Data.Services.Client.Strings.AtomMaterializer_PropertyNotExpectedEntry(name, entry.Id));
            }
            CheckEntryToAccessNotNull(entry2, name);
            return entry2.Entry;
        }

        internal static object ProjectionInitializeEntity(ODataEntityMaterializer materializer, MaterializerEntry entry, Type expectedType, Type resultType, string[] properties, Func<object, object, Type, object>[] propertyValues)
        {
            if (entry.Entry == null)
            {
                throw new NullReferenceException(System.Data.Services.Client.Strings.AtomMaterializer_EntryToInitializeIsNull(resultType.FullName));
            }
            if (!entry.EntityHasBeenResolved)
            {
                ProjectionEnsureEntryAvailableOfType(materializer, entry, resultType);
            }
            else if (!resultType.IsAssignableFrom(entry.ActualType.ElementType))
            {
                throw new InvalidOperationException(System.Data.Services.Client.Strings.AtomMaterializer_ProjectEntityTypeMismatch(resultType.FullName, entry.ActualType.ElementType.FullName, entry.Entry.Id));
            }
            object resolvedObject = entry.ResolvedObject;
            for (int i = 0; i < properties.Length; i++)
            {
                StreamDescriptor descriptor;
                string propertyName = properties[i];
                ClientPropertyAnnotation annotation = entry.ActualType.GetProperty(propertyName, materializer.ResponseInfo.IgnoreMissingProperties);
                object target = propertyValues[i](materializer, entry.Entry, expectedType);
                ODataProperty property = (from p in entry.Entry.Properties
                    where p.Name == propertyName
                    select p).FirstOrDefault<ODataProperty>();
                if ((((((property == null) && (entry.NavigationLinks != null)) ? (from l in entry.NavigationLinks
                    where l.Name == propertyName
                    select l).FirstOrDefault<ODataNavigationLink>() : null) != null) || (property != null)) || entry.EntityDescriptor.TryGetNamedStreamInfo(propertyName, out descriptor))
                {
                    if (entry.ShouldUpdateFromPayload && (annotation.EdmProperty.Type.TypeKind() == EdmTypeKind.Entity))
                    {
                        materializer.Log.SetLink(entry, annotation.PropertyName, target);
                    }
                    if (entry.ShouldUpdateFromPayload)
                    {
                        if (!annotation.IsEntityCollection)
                        {
                            if (!annotation.IsPrimitiveOrComplexCollection)
                            {
                                annotation.SetValue(resolvedObject, target, annotation.PropertyName, false);
                            }
                        }
                        else
                        {
                            IEnumerable list = (IEnumerable) target;
                            DataServiceQueryContinuation continuation = materializer.nextLinkTable[list];
                            Uri nextLink = (continuation == null) ? null : continuation.NextLinkUri;
                            ProjectionPlan plan = (continuation == null) ? null : continuation.Plan;
                            materializer.MergeLists(entry, annotation, list, nextLink, plan);
                        }
                    }
                    else if (annotation.IsEntityCollection)
                    {
                        materializer.FoundNextLinkForUnmodifiedCollection(annotation.GetValue(entry.ResolvedObject) as IEnumerable);
                    }
                }
            }
            return resolvedObject;
        }

        internal static IEnumerable ProjectionSelect(ODataEntityMaterializer materializer, MaterializerEntry entry, Type expectedType, Type resultType, ProjectionPath path, Func<object, object, Type, object> selector)
        {
            ClientEdmModel model = ClientEdmModel.GetModel(materializer.ResponseInfo.MaxProtocolVersion);
            ClientTypeAnnotation clientTypeAnnotation = entry.ActualType ?? model.GetClientTypeAnnotation(model.GetOrCreateEdmType(expectedType));
            IEnumerable enumerable = (IEnumerable) Util.ActivatorCreateInstance(typeof(List<>).MakeGenericType(new Type[] { resultType }), new object[0]);
            MaterializerNavigationLink link = null;
            ClientPropertyAnnotation property = null;
            for (int i = 0; i < path.Count; i++)
            {
                ProjectionPathSegment segment = path[i];
                if (segment.SourceTypeAs != null)
                {
                    clientTypeAnnotation = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(segment.SourceTypeAs));
                }
                if (segment.Member != null)
                {
                    string member = segment.Member;
                    property = clientTypeAnnotation.GetProperty(member, false);
                    link = GetPropertyOrThrow(entry.NavigationLinks, member, entry.Id);
                    if (link.Entry != null)
                    {
                        entry = link.Entry;
                        clientTypeAnnotation = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(property.PropertyType));
                    }
                }
            }
            ValidatePropertyMatch(property, link.Link);
            MaterializerFeed feed = MaterializerFeed.GetFeed(link.Feed);
            Action<object, object> addToCollectionDelegate = ODataMaterializer.GetAddToCollectionDelegate(enumerable.GetType());
            foreach (ODataEntry entry2 in feed.Entries)
            {
                object obj2 = selector(materializer, entry2, property.EntityCollectionItemType);
                addToCollectionDelegate(enumerable, obj2);
            }
            ProjectionPlan plan = new ProjectionPlan {
                LastSegmentType = property.EntityCollectionItemType,
                Plan = selector,
                ProjectedType = resultType
            };
            materializer.FoundNextLinkForCollection(enumerable, feed.NextPageLink, plan);
            return enumerable;
        }

        internal static object ProjectionValueForPath(ODataEntityMaterializer materializer, MaterializerEntry entry, Type expectedType, ProjectionPath path)
        {
            if ((path.Count == 0) || ((path.Count == 1) && (path[0].Member == null)))
            {
                if (!entry.EntityHasBeenResolved)
                {
                    materializer.Materialize(entry, expectedType, false);
                }
                return entry.ResolvedObject;
            }
            object streamLink = null;
            ODataNavigationLink link = null;
            ODataProperty atomProperty = null;
            ICollection<ODataNavigationLink> navigationLinks = entry.NavigationLinks;
            IEnumerable<ODataProperty> properties = entry.Entry.Properties;
            ClientEdmModel model = ClientEdmModel.GetModel(materializer.ResponseInfo.MaxProtocolVersion);
            for (int i = 0; i < path.Count; i++)
            {
                Func<StreamDescriptor, bool> predicate = null;
                Func<ODataNavigationLink, bool> func2 = null;
                Func<ODataProperty, bool> func3 = null;
                Func<ODataProperty, bool> func4 = null;
                Func<ODataNavigationLink, bool> func5 = null;
                string propertyName;
                ProjectionPathSegment segment = path[i];
                if (segment.Member != null)
                {
                    bool flag = i == (path.Count - 1);
                    propertyName = segment.Member;
                    expectedType = segment.SourceTypeAs ?? expectedType;
                    ClientPropertyAnnotation property = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(expectedType)).GetProperty(propertyName, false);
                    if (property.IsStreamLinkProperty)
                    {
                        if (predicate == null)
                        {
                            predicate = sd => sd.Name == propertyName;
                        }
                        StreamDescriptor descriptor = entry.EntityDescriptor.StreamDescriptors.Where<StreamDescriptor>(predicate).SingleOrDefault<StreamDescriptor>();
                        if (descriptor == null)
                        {
                            if (segment.SourceTypeAs == null)
                            {
                                throw new InvalidOperationException(System.Data.Services.Client.Strings.AtomMaterializer_PropertyMissing(propertyName, entry.Entry.Id));
                            }
                            return WebUtil.GetDefaultValue<DataServiceStreamLink>();
                        }
                        streamLink = descriptor.StreamLink;
                    }
                    else
                    {
                        if (segment.SourceTypeAs != null)
                        {
                            if (func2 == null)
                            {
                                func2 = p => p.Name == propertyName;
                            }
                            if (!navigationLinks.Any<ODataNavigationLink>(func2))
                            {
                                if (func3 == null)
                                {
                                    func3 = p => p.Name == propertyName;
                                }
                                if (!properties.Any<ODataProperty>(func3) && flag)
                                {
                                    return WebUtil.GetDefaultValue(property.PropertyType);
                                }
                            }
                        }
                        if (func4 == null)
                        {
                            func4 = p => p.Name == propertyName;
                        }
                        atomProperty = properties.Where<ODataProperty>(func4).FirstOrDefault<ODataProperty>();
                        if (func5 == null)
                        {
                            func5 = p => p.Name == propertyName;
                        }
                        link = ((atomProperty == null) && (navigationLinks != null)) ? navigationLinks.Where<ODataNavigationLink>(func5).FirstOrDefault<ODataNavigationLink>() : null;
                        if ((link == null) && (atomProperty == null))
                        {
                            throw new InvalidOperationException(System.Data.Services.Client.Strings.AtomMaterializer_PropertyMissing(propertyName, entry.Entry.Id));
                        }
                        if (link != null)
                        {
                            ValidatePropertyMatch(property, link);
                            MaterializerNavigationLink link2 = MaterializerNavigationLink.GetLink(link);
                            if (link2.Feed != null)
                            {
                                MaterializerFeed feed = MaterializerFeed.GetFeed(link2.Feed);
                                Type implementationType = ClientTypeUtil.GetImplementationType(segment.ProjectionType, typeof(ICollection<>));
                                if (implementationType == null)
                                {
                                    implementationType = ClientTypeUtil.GetImplementationType(segment.ProjectionType, typeof(IEnumerable<>));
                                }
                                Type nestedExpectedType = implementationType.GetGenericArguments()[0];
                                Type projectionType = segment.ProjectionType;
                                if (projectionType.IsInterfaceEx() || ODataMaterializer.IsDataServiceCollection(projectionType))
                                {
                                    projectionType = typeof(Collection<>).MakeGenericType(new Type[] { nestedExpectedType });
                                }
                                IEnumerable list = (IEnumerable) Util.ActivatorCreateInstance(projectionType, new object[0]);
                                MaterializeToList(materializer, list, nestedExpectedType, feed.Entries);
                                if (ODataMaterializer.IsDataServiceCollection(segment.ProjectionType))
                                {
                                    list = (IEnumerable) Util.ActivatorCreateInstance(WebUtil.GetDataServiceCollectionOfT(new Type[] { nestedExpectedType }), new object[] { list, TrackingMode.None });
                                }
                                ProjectionPlan plan = CreatePlanForShallowMaterialization(nestedExpectedType);
                                materializer.FoundNextLinkForCollection(list, feed.Feed.NextPageLink, plan);
                                streamLink = list;
                            }
                            else if (link2.Entry != null)
                            {
                                MaterializerEntry entry2 = link2.Entry;
                                if (flag)
                                {
                                    if ((entry2.Entry != null) && !entry2.EntityHasBeenResolved)
                                    {
                                        materializer.Materialize(entry2, property.PropertyType, false);
                                    }
                                }
                                else
                                {
                                    CheckEntryToAccessNotNull(entry2, propertyName);
                                }
                                properties = entry2.Properties;
                                navigationLinks = entry2.NavigationLinks;
                                streamLink = entry2.ResolvedObject;
                                entry = entry2;
                            }
                        }
                        else
                        {
                            if (atomProperty.Value is ODataStreamReferenceValue)
                            {
                                streamLink = null;
                                navigationLinks = ODataMaterializer.EmptyLinks;
                                properties = ODataMaterializer.EmptyProperties;
                                continue;
                            }
                            ValidatePropertyMatch(property, atomProperty);
                            if (ClientTypeUtil.TypeOrElementTypeIsEntity(property.PropertyType))
                            {
                                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.AtomMaterializer_InvalidEntityType(property.EntityCollectionItemType ?? property.PropertyType));
                            }
                            if (property.IsPrimitiveOrComplexCollection)
                            {
                                object instance = streamLink ?? (entry.ResolvedObject ?? Util.ActivatorCreateInstance(expectedType, new object[0]));
                                ODataMaterializer.ApplyDataValue(model.GetClientTypeAnnotation(model.GetOrCreateEdmType(instance.GetType())), atomProperty, materializer.ResponseInfo.IgnoreMissingProperties, materializer.ResponseInfo, instance);
                                navigationLinks = ODataMaterializer.EmptyLinks;
                                properties = ODataMaterializer.EmptyProperties;
                            }
                            else if (atomProperty.Value is ODataComplexValue)
                            {
                                ODataComplexValue complexValue = atomProperty.Value as ODataComplexValue;
                                ODataMaterializer.MaterializeComplexTypeProperty(property.PropertyType, complexValue, materializer.ResponseInfo.IgnoreMissingProperties, materializer.ResponseInfo);
                                properties = complexValue.Properties;
                                navigationLinks = ODataMaterializer.EmptyLinks;
                            }
                            else
                            {
                                if ((atomProperty.Value == null) && !ClientTypeUtil.CanAssignNull(property.NullablePropertyType))
                                {
                                    throw new InvalidOperationException(System.Data.Services.Client.Strings.AtomMaterializer_CannotAssignNull(atomProperty.Name, property.NullablePropertyType));
                                }
                                ODataMaterializer.MaterializePrimitiveDataValue(property.NullablePropertyType, atomProperty);
                                navigationLinks = ODataMaterializer.EmptyLinks;
                                properties = ODataMaterializer.EmptyProperties;
                            }
                            streamLink = atomProperty.GetMaterializedValue();
                        }
                    }
                    expectedType = property.PropertyType;
                }
            }
            return streamLink;
        }

        internal void PropagateContinuation<T>(IEnumerable<T> from, DataServiceCollection<T> to)
        {
            DataServiceQueryContinuation continuation;
            if (base.nextLinkTable.TryGetValue(from, out continuation))
            {
                base.nextLinkTable.Add(to, continuation);
                Util.SetNextLinkForCollection(to, continuation);
            }
        }

        protected sealed override bool ReadImplementation()
        {
            base.nextLinkTable.Clear();
            if (!this.ReadNextFeedOrEntry())
            {
                return false;
            }
            if (((this.CurrentEntry == null) && (this.CurrentFeed != null)) && !this.ReadNextFeedOrEntry())
            {
                return false;
            }
            MaterializerEntry.GetEntry(this.CurrentEntry).ResolvedObject = this.TargetInstance;
            this.currentValue = this.materializeEntryPlan.Run(this, this.CurrentEntry, base.ExpectedType);
            return true;
        }

        protected abstract bool ReadNextFeedOrEntry();
        private void ResolveByCreating(MaterializerEntry entry, Type expectedEntryType)
        {
            ClientTypeAnnotation annotation = base.ResponseInfo.TypeResolver.ResolveEdmTypeName(expectedEntryType, entry.Entry.TypeName);
            this.ResolveByCreatingWithType(entry, annotation.ElementType);
        }

        private void ResolveByCreatingWithType(MaterializerEntry entry, Type type)
        {
            ClientEdmModel model = ClientEdmModel.GetModel(base.ResponseInfo.MaxProtocolVersion);
            entry.ActualType = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(type));
            entry.ResolvedObject = Activator.CreateInstance(type);
            entry.CreatedByMaterializer = true;
            entry.ShouldUpdateFromPayload = true;
            entry.EntityHasBeenResolved = true;
            this.log.CreatedInstance(entry);
        }

        private void ResolveOrCreateInstance(MaterializerEntry entry, Type expectedEntryType)
        {
            if (!this.TryResolveAsTarget(entry))
            {
                if (entry.Id == null)
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Deserialize_MissingIdElement);
                }
                if (!this.TryResolveAsCreated(entry) && !this.TryResolveFromContext(entry, expectedEntryType))
                {
                    this.ResolveByCreating(entry, expectedEntryType);
                }
            }
        }

        internal static object ShallowMaterializePlan(ODataEntityMaterializer materializer, MaterializerEntry entry, Type expectedEntryType)
        {
            materializer.Materialize(entry, expectedEntryType, false);
            return entry.ResolvedObject;
        }

        private bool TryResolveAsCreated(MaterializerEntry entry)
        {
            MaterializerEntry entry2;
            if (!this.log.TryResolve(entry, out entry2))
            {
                return false;
            }
            entry.ActualType = entry2.ActualType;
            entry.ResolvedObject = entry2.ResolvedObject;
            entry.CreatedByMaterializer = entry2.CreatedByMaterializer;
            entry.ShouldUpdateFromPayload = entry2.ShouldUpdateFromPayload;
            entry.EntityHasBeenResolved = true;
            return true;
        }

        private bool TryResolveAsTarget(MaterializerEntry entry)
        {
            if (entry.ResolvedObject == null)
            {
                return false;
            }
            ClientEdmModel model = ClientEdmModel.GetModel(base.ResponseInfo.MaxProtocolVersion);
            entry.ActualType = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(entry.ResolvedObject.GetType()));
            this.log.FoundTargetInstance(entry);
            entry.ShouldUpdateFromPayload = this.mergeOption != MergeOption.PreserveChanges;
            entry.EntityHasBeenResolved = true;
            return true;
        }

        private bool TryResolveFromContext(MaterializerEntry entry, Type expectedEntryType)
        {
            if (this.mergeOption != MergeOption.NoTracking)
            {
                EntityStates states;
                entry.ResolvedObject = base.ResponseInfo.EntityTracker.TryGetEntity(entry.Id, out states);
                if (entry.ResolvedObject != null)
                {
                    if (!expectedEntryType.IsInstanceOfType(entry.ResolvedObject))
                    {
                        throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Deserialize_Current(expectedEntryType, entry.ResolvedObject.GetType()));
                    }
                    ClientEdmModel model = ClientEdmModel.GetModel(base.ResponseInfo.MaxProtocolVersion);
                    entry.ActualType = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(entry.ResolvedObject.GetType()));
                    entry.EntityHasBeenResolved = true;
                    entry.ShouldUpdateFromPayload = ((this.mergeOption == MergeOption.OverwriteChanges) || ((this.mergeOption == MergeOption.PreserveChanges) && (states == EntityStates.Unchanged))) || ((this.mergeOption == MergeOption.PreserveChanges) && (states == EntityStates.Deleted));
                    this.log.FoundExistingInstance(entry);
                    return true;
                }
            }
            return false;
        }

        internal static void ValidatePropertyMatch(ClientPropertyAnnotation property, ODataNavigationLink link)
        {
            ValidatePropertyMatch(property, link, null, false);
        }

        internal static void ValidatePropertyMatch(ClientPropertyAnnotation property, ODataProperty atomProperty)
        {
            ValidatePropertyMatch(property, atomProperty, null, false);
        }

        internal static Type ValidatePropertyMatch(ClientPropertyAnnotation property, ODataNavigationLink link, ResponseInfo responseInfo, bool performEntityCheck)
        {
            Type t = null;
            if (link.IsCollection.HasValue)
            {
                if (link.IsCollection.Value)
                {
                    if (!property.IsEntityCollection)
                    {
                        throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Deserialize_MismatchAtomLinkFeedPropertyNotCollection(property.PropertyName));
                    }
                    t = property.EntityCollectionItemType;
                }
                else
                {
                    if (property.IsEntityCollection)
                    {
                        throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Deserialize_MismatchAtomLinkEntryPropertyIsCollection(property.PropertyName));
                    }
                    t = property.PropertyType;
                }
            }
            if (((t != null) && performEntityCheck) && !ClientTypeUtil.TypeIsEntity(t, responseInfo.MaxProtocolVersion))
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.AtomMaterializer_InvalidNonEntityType(t.ToString()));
            }
            return t;
        }

        internal static void ValidatePropertyMatch(ClientPropertyAnnotation property, ODataProperty atomProperty, ResponseInfo responseInfo, bool performEntityCheck)
        {
            ODataFeed feed = atomProperty.Value as ODataFeed;
            ODataEntry entry = atomProperty.Value as ODataEntry;
            if (property.IsKnownType && ((feed != null) || (entry != null)))
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Deserialize_MismatchAtomLinkLocalSimple);
            }
            Type t = null;
            if (feed != null)
            {
                if (!property.IsEntityCollection)
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Deserialize_MismatchAtomLinkFeedPropertyNotCollection(property.PropertyName));
                }
                t = property.EntityCollectionItemType;
            }
            if (entry != null)
            {
                if (property.IsEntityCollection)
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Deserialize_MismatchAtomLinkEntryPropertyIsCollection(property.PropertyName));
                }
                t = property.PropertyType;
            }
            if (((t != null) && performEntityCheck) && !ClientTypeUtil.TypeIsEntity(t, responseInfo.MaxProtocolVersion))
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.AtomMaterializer_InvalidNonEntityType(t.ToString()));
            }
        }

        internal sealed override object CurrentValue
        {
            get
            {
                return this.currentValue;
            }
        }

        private AtomMaterializerLog Log
        {
            get
            {
                return this.log;
            }
        }

        internal sealed override ProjectionPlan MaterializeEntryPlan
        {
            get
            {
                return this.materializeEntryPlan;
            }
        }

        internal object TargetInstance
        {
            get
            {
                return this.targetInstance;
            }
            set
            {
                this.targetInstance = value;
            }
        }

        
    }
}

