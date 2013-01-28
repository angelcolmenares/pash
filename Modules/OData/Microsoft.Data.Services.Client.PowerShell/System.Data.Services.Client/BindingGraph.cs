namespace System.Data.Services.Client
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class BindingGraph
    {
        private Graph graph;
        private BindingObserver observer;

        public BindingGraph(BindingObserver observer)
        {
            this.observer = observer;
            this.graph = new Graph();
        }

        public void AddComplexObject(object source, string sourceProperty, object target)
        {
            if (this.graph.LookupVertex(target) != null)
            {
                throw new InvalidOperationException(System.Data.Services.Client.Strings.DataBinding_ComplexObjectAssociatedWithMultipleEntities(target.GetType()));
            }
            Vertex vertex2 = this.graph.LookupVertex(source);
            Vertex vertex = this.graph.AddVertex(target);
            vertex.Parent = vertex2;
            vertex.IsComplex = true;
            if (!this.AttachEntityOrComplexObjectNotification(target))
            {
                throw new InvalidOperationException(System.Data.Services.Client.Strings.DataBinding_NotifyPropertyChangedNotImpl(target.GetType()));
            }
            this.graph.AddEdge(source, target, sourceProperty);
            this.AddFromProperties(target);
        }

        public void AddComplexObjectsFromCollection(object collection, IEnumerable collectionItems)
        {
            foreach (object obj2 in collectionItems)
            {
                if (obj2 != null)
                {
                    this.AddComplexObject(collection, null, obj2);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public bool AddDataServiceCollection(object source, string sourceProperty, object collection, string collectionEntitySet)
        {
            if (this.graph.ExistsVertex(collection))
            {
                return false;
            }
            Vertex vertex = this.graph.AddVertex(collection);
            vertex.IsDataServiceCollection = true;
            vertex.EntitySet = collectionEntitySet;
            ICollection is2 = collection as ICollection;
            if (source != null)
            {
                vertex.Parent = this.graph.LookupVertex(source);
                vertex.ParentProperty = sourceProperty;
                this.graph.AddEdge(source, collection, sourceProperty);
                Type collectionEntityType = BindingUtils.GetCollectionEntityType(collection.GetType());
                if (!typeof(INotifyPropertyChanged).IsAssignableFrom(collectionEntityType))
                {
                    throw new InvalidOperationException(System.Data.Services.Client.Strings.DataBinding_NotifyPropertyChangedNotImpl(collectionEntityType));
                }
                typeof(BindingGraph).GetMethod("SetObserver", false, false).MakeGenericMethod(new Type[] { collectionEntityType }).Invoke(this, new object[] { is2 });
            }
            else
            {
                this.graph.Root = vertex;
            }
            this.AttachDataServiceCollectionNotification(collection);
            foreach (object obj2 in is2)
            {
                this.AddEntity(source, sourceProperty, obj2, collectionEntitySet, collection);
            }
            return true;
        }

        public bool AddEntity(object source, string sourceProperty, object target, string targetEntitySet, object edgeSource)
        {
            Vertex vertex = this.graph.LookupVertex(edgeSource);
            Vertex vertex2 = null;
            bool flag = false;
            if (target != null)
            {
                vertex2 = this.graph.LookupVertex(target);
                if (vertex2 == null)
                {
                    vertex2 = this.graph.AddVertex(target);
                    vertex2.EntitySet = BindingEntityInfo.GetEntitySet(target, targetEntitySet, this.observer.Context.MaxProtocolVersion);
                    if (!this.AttachEntityOrComplexObjectNotification(target))
                    {
                        throw new InvalidOperationException(System.Data.Services.Client.Strings.DataBinding_NotifyPropertyChangedNotImpl(target.GetType()));
                    }
                    flag = true;
                }
                if (this.graph.ExistsEdge(edgeSource, target, vertex.IsDataServiceCollection ? null : sourceProperty))
                {
                    throw new InvalidOperationException(System.Data.Services.Client.Strings.DataBinding_EntityAlreadyInCollection(target.GetType()));
                }
                this.graph.AddEdge(edgeSource, target, vertex.IsDataServiceCollection ? null : sourceProperty);
            }
            if (!vertex.IsDataServiceCollection)
            {
                this.observer.HandleUpdateEntityReference(source, sourceProperty, vertex.EntitySet, target, (vertex2 == null) ? null : vertex2.EntitySet);
            }
            else
            {
                this.observer.HandleAddEntity(source, sourceProperty, (vertex.Parent != null) ? vertex.Parent.EntitySet : null, edgeSource as ICollection, target, vertex2.EntitySet);
            }
            if (flag)
            {
                this.AddFromProperties(target);
            }
            return flag;
        }

        private void AddFromProperties(object entity)
        {
            foreach (BindingEntityInfo.BindingPropertyInfo info in BindingEntityInfo.GetObservableProperties(entity.GetType(), this.observer.Context.MaxProtocolVersion))
            {
                object target = info.PropertyInfo.GetValue(entity);
                if (target != null)
                {
                    switch (info.PropertyKind)
                    {
                        case BindingPropertyKind.BindingPropertyKindEntity:
                        {
                            this.AddEntity(entity, info.PropertyInfo.PropertyName, target, null, entity);
                            continue;
                        }
                        case BindingPropertyKind.BindingPropertyKindDataServiceCollection:
                        {
                            this.AddDataServiceCollection(entity, info.PropertyInfo.PropertyName, target, null);
                            continue;
                        }
                        case BindingPropertyKind.BindingPropertyKindPrimitiveOrComplexCollection:
                        {
                            this.AddPrimitiveOrComplexCollection(entity, info.PropertyInfo.PropertyName, target, info.PropertyInfo.PrimitiveOrComplexCollectionItemType);
                            continue;
                        }
                    }
                    this.AddComplexObject(entity, info.PropertyInfo.PropertyName, target);
                }
            }
        }

        public void AddPrimitiveOrComplexCollection(object source, string sourceProperty, object collection, Type collectionItemType)
        {
            Vertex vertex = this.graph.LookupVertex(source);
            if (this.graph.LookupVertex(collection) != null)
            {
                throw new InvalidOperationException(System.Data.Services.Client.Strings.DataBinding_CollectionAssociatedWithMultipleEntities(collection.GetType()));
            }
            Vertex vertex2 = this.graph.AddVertex(collection);
            vertex2.Parent = vertex;
            vertex2.ParentProperty = sourceProperty;
            vertex2.IsPrimitiveOrComplexCollection = true;
            vertex2.PrimitiveOrComplexCollectionItemType = collectionItemType;
            this.graph.AddEdge(source, collection, sourceProperty);
            if (!this.AttachPrimitiveOrComplexCollectionNotification(collection))
            {
                throw new InvalidOperationException(System.Data.Services.Client.Strings.DataBinding_NotifyCollectionChangedNotImpl(collection.GetType()));
            }
            if (!PrimitiveType.IsKnownNullableType(collectionItemType))
            {
                if (!typeof(INotifyPropertyChanged).IsAssignableFrom(collectionItemType))
                {
                    throw new InvalidOperationException(System.Data.Services.Client.Strings.DataBinding_NotifyPropertyChangedNotImpl(collectionItemType));
                }
                this.AddComplexObjectsFromCollection(collection, (IEnumerable) collection);
            }
        }

        private void AttachDataServiceCollectionNotification(object target)
        {
            INotifyCollectionChanged changed = target as INotifyCollectionChanged;
            changed.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.observer.OnDataServiceCollectionChanged);
            changed.CollectionChanged += new NotifyCollectionChangedEventHandler(this.observer.OnDataServiceCollectionChanged);
        }

        private bool AttachEntityOrComplexObjectNotification(object target)
        {
            INotifyPropertyChanged changed = target as INotifyPropertyChanged;
            if (changed != null)
            {
                changed.PropertyChanged -= new PropertyChangedEventHandler(this.observer.OnPropertyChanged);
                changed.PropertyChanged += new PropertyChangedEventHandler(this.observer.OnPropertyChanged);
                return true;
            }
            return false;
        }

        private bool AttachPrimitiveOrComplexCollectionNotification(object collection)
        {
            INotifyCollectionChanged changed = collection as INotifyCollectionChanged;
            if (changed != null)
            {
                changed.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.observer.OnPrimitiveOrComplexCollectionChanged);
                changed.CollectionChanged += new NotifyCollectionChangedEventHandler(this.observer.OnPrimitiveOrComplexCollectionChanged);
                return true;
            }
            return false;
        }

        private void DetachCollectionNotifications(object target)
        {
            INotifyCollectionChanged changed = target as INotifyCollectionChanged;
            if (changed != null)
            {
                changed.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.observer.OnDataServiceCollectionChanged);
                changed.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.observer.OnPrimitiveOrComplexCollectionChanged);
            }
        }

        private void DetachNotifications(object target)
        {
            this.DetachCollectionNotifications(target);
            INotifyPropertyChanged changed = target as INotifyPropertyChanged;
            if (changed != null)
            {
                changed.PropertyChanged -= new PropertyChangedEventHandler(this.observer.OnPropertyChanged);
            }
        }

        public void GetAncestorEntityForComplexProperty(ref object entity, ref string propertyName, ref object propertyValue)
        {
            for (Vertex vertex = this.graph.LookupVertex(entity); vertex.IsComplex || vertex.IsPrimitiveOrComplexCollection; vertex = vertex.Parent)
            {
                propertyName = vertex.IncomingEdges[0].Label;
                propertyValue = vertex.Item;
                entity = vertex.Parent.Item;
            }
        }

        public void GetDataServiceCollectionInfo(object collection, out object source, out string sourceProperty, out string sourceEntitySet, out string targetEntitySet)
        {
            this.graph.LookupVertex(collection).GetDataServiceCollectionInfo(out source, out sourceProperty, out sourceEntitySet, out targetEntitySet);
        }

        public IEnumerable<object> GetDataServiceCollectionItems(object collection)
        {
            Vertex vertex = this.graph.LookupVertex(collection);
            foreach (Edge iteratorVariable1 in vertex.OutgoingEdges.ToList<Edge>())
            {
                yield return iteratorVariable1.Target.Item;
            }
        }

        public void GetPrimitiveOrComplexCollectionInfo(object collection, out object source, out string sourceProperty, out Type collectionItemType)
        {
            this.graph.LookupVertex(collection).GetPrimitiveOrComplexCollectionInfo(out source, out sourceProperty, out collectionItemType);
        }

        public void RemoveCollection(object source)
        {
            foreach (Edge edge in this.graph.LookupVertex(source).OutgoingEdges.ToList<Edge>())
            {
                this.graph.RemoveEdge(source, edge.Target.Item, null);
            }
            this.RemoveUnreachableVertices();
        }

        public void RemoveComplexTypeCollectionItem(object item, object collection)
        {
            if ((item != null) && (this.graph.LookupVertex(item) != null))
            {
                this.graph.RemoveEdge(collection, item, null);
            }
        }

        public void RemoveDataServiceCollectionItem(object item, object parent, string parentProperty)
        {
            Func<BindingEntityInfo.BindingPropertyInfo, bool> predicate = null;
            if (this.graph.LookupVertex(item) != null)
            {
                if (parentProperty != null)
                {
                    if (predicate == null)
                    {
                        predicate = p => p.PropertyInfo.PropertyName == parentProperty;
                    }
                    parent = BindingEntityInfo.GetObservableProperties(parent.GetType(), this.observer.Context.MaxProtocolVersion).Single<BindingEntityInfo.BindingPropertyInfo>(predicate).PropertyInfo.GetValue(parent);
                }
                object source = null;
                string sourceProperty = null;
                string sourceEntitySet = null;
                string targetEntitySet = null;
                this.GetDataServiceCollectionInfo(parent, out source, out sourceProperty, out sourceEntitySet, out targetEntitySet);
                targetEntitySet = BindingEntityInfo.GetEntitySet(item, targetEntitySet, this.observer.Context.MaxProtocolVersion);
                this.observer.HandleDeleteEntity(source, sourceProperty, sourceEntitySet, parent as ICollection, item, targetEntitySet);
                this.graph.RemoveEdge(parent, item, null);
            }
        }

        public void RemoveNonTrackedEntities()
        {
            foreach (object obj2 in this.graph.Select(o => BindingEntityInfo.IsEntityType(o.GetType(), this.observer.Context.MaxProtocolVersion) && !this.observer.IsContextTrackingEntity(o)))
            {
                this.graph.ClearEdgesForVertex(this.graph.LookupVertex(obj2));
            }
            this.RemoveUnreachableVertices();
        }

        public void RemoveRelation(object source, string relation)
        {
            Edge edge = this.graph.LookupVertex(source).OutgoingEdges.SingleOrDefault<Edge>(e => (e.Source.Item == source) && (e.Label == relation));
            if (edge != null)
            {
                this.graph.RemoveEdge(edge.Source.Item, edge.Target.Item, edge.Label);
            }
            this.RemoveUnreachableVertices();
        }

        public void RemoveUnreachableVertices()
        {
            this.graph.RemoveUnreachableVertices(new Action<object>(this.DetachNotifications));
        }

        public void Reset()
        {
            this.graph.Reset(new Action<object>(this.DetachNotifications));
        }

        private void SetObserver<T>(ICollection collection)
        {
            DataServiceCollection<T> services = collection as DataServiceCollection<T>;
            services.Observer = this.observer;
        }

        

        internal sealed class Edge : IEquatable<BindingGraph.Edge>
        {
            public bool Equals(BindingGraph.Edge other)
            {
                return ((((other != null) && object.ReferenceEquals(this.Source, other.Source)) && object.ReferenceEquals(this.Target, other.Target)) && (this.Label == other.Label));
            }

            public string Label { get; set; }

            public BindingGraph.Vertex Source { get; set; }

            public BindingGraph.Vertex Target { get; set; }
        }

        internal sealed class Graph
        {
            private BindingGraph.Vertex root;
            private Dictionary<object, BindingGraph.Vertex> vertices = new Dictionary<object, BindingGraph.Vertex>(ReferenceEqualityComparer<object>.Instance);

            public BindingGraph.Edge AddEdge(object source, object target, string label)
            {
                BindingGraph.Vertex vertex = this.vertices[source];
                BindingGraph.Vertex vertex2 = this.vertices[target];
                BindingGraph.Edge item = new BindingGraph.Edge {
                    Source = vertex,
                    Target = vertex2,
                    Label = label
                };
                vertex.OutgoingEdges.Add(item);
                vertex2.IncomingEdges.Add(item);
                return item;
            }

            public BindingGraph.Vertex AddVertex(object item)
            {
                BindingGraph.Vertex vertex = new BindingGraph.Vertex(item);
                this.vertices.Add(item, vertex);
                return vertex;
            }

            public void ClearEdgesForVertex(BindingGraph.Vertex v)
            {
                foreach (BindingGraph.Edge edge in v.OutgoingEdges.Concat<BindingGraph.Edge>(v.IncomingEdges).ToList<BindingGraph.Edge>())
                {
                    this.RemoveEdge(edge.Source.Item, edge.Target.Item, edge.Label);
                }
            }

            public bool ExistsEdge(object source, object target, string label)
            {
                BindingGraph.Edge e = new BindingGraph.Edge {
                    Source = this.vertices[source],
                    Target = this.vertices[target],
                    Label = label
                };
                return this.vertices[source].OutgoingEdges.Any<BindingGraph.Edge>(r => r.Equals(e));
            }

            public bool ExistsVertex(object item)
            {
                BindingGraph.Vertex vertex;
                return this.vertices.TryGetValue(item, out vertex);
            }

            public BindingGraph.Vertex LookupVertex(object item)
            {
                BindingGraph.Vertex vertex;
                this.vertices.TryGetValue(item, out vertex);
                return vertex;
            }

            public void RemoveEdge(object source, object target, string label)
            {
                BindingGraph.Vertex vertex = this.vertices[source];
                BindingGraph.Vertex vertex2 = this.vertices[target];
                BindingGraph.Edge item = new BindingGraph.Edge {
                    Source = vertex,
                    Target = vertex2,
                    Label = label
                };
                vertex.OutgoingEdges.Remove(item);
                vertex2.IncomingEdges.Remove(item);
            }

            public void RemoveUnreachableVertices(Action<object> detachAction)
            {
                try
                {
                    foreach (BindingGraph.Vertex vertex in this.UnreachableVertices())
                    {
                        this.ClearEdgesForVertex(vertex);
                        detachAction(vertex.Item);
                        this.vertices.Remove(vertex.Item);
                    }
                }
                finally
                {
                    foreach (BindingGraph.Vertex vertex2 in this.vertices.Values)
                    {
                        vertex2.Color = VertexColor.White;
                    }
                }
            }

            public void Reset(Action<object> action)
            {
                foreach (object obj2 in this.vertices.Keys)
                {
                    action(obj2);
                }
                this.vertices.Clear();
            }

            public IList<object> Select(Func<object, bool> filter)
            {
                return this.vertices.Keys.Where<object>(filter).ToList<object>();
            }

            private IEnumerable<BindingGraph.Vertex> UnreachableVertices()
            {
                Queue<BindingGraph.Vertex> queue = new Queue<BindingGraph.Vertex>();
                this.Root.Color = VertexColor.Gray;
                queue.Enqueue(this.Root);
                while (queue.Count != 0)
                {
                    BindingGraph.Vertex vertex = queue.Dequeue();
                    foreach (BindingGraph.Edge edge in vertex.OutgoingEdges)
                    {
                        if (edge.Target.Color == VertexColor.White)
                        {
                            edge.Target.Color = VertexColor.Gray;
                            queue.Enqueue(edge.Target);
                        }
                    }
                    vertex.Color = VertexColor.Black;
                }
                return (from v in this.vertices.Values
                    where v.Color == VertexColor.White
                    select v).ToList<BindingGraph.Vertex>();
            }

            public BindingGraph.Vertex Root
            {
                get
                {
                    return this.root;
                }
                set
                {
                    this.root = value;
                }
            }
        }

        internal sealed class Vertex
        {
            private List<BindingGraph.Edge> incomingEdges;
            private List<BindingGraph.Edge> outgoingEdges;

            public Vertex(object item)
            {
                this.Item = item;
                this.Color = VertexColor.White;
            }

            public void GetDataServiceCollectionInfo(out object source, out string sourceProperty, out string sourceEntitySet, out string targetEntitySet)
            {
                if (!this.IsRootDataServiceCollection)
                {
                    source = this.Parent.Item;
                    sourceProperty = this.ParentProperty;
                    sourceEntitySet = this.Parent.EntitySet;
                }
                else
                {
                    source = null;
                    sourceProperty = null;
                    sourceEntitySet = null;
                }
                targetEntitySet = this.EntitySet;
            }

            public void GetPrimitiveOrComplexCollectionInfo(out object source, out string sourceProperty, out Type collectionItemType)
            {
                source = this.Parent.Item;
                sourceProperty = this.ParentProperty;
                collectionItemType = this.PrimitiveOrComplexCollectionItemType;
            }

            public VertexColor Color { get; set; }

            public string EntitySet { get; set; }

            public IList<BindingGraph.Edge> IncomingEdges
            {
                get
                {
                    if (this.incomingEdges == null)
                    {
                        this.incomingEdges = new List<BindingGraph.Edge>();
                    }
                    return this.incomingEdges;
                }
            }

            public bool IsComplex { get; set; }

            public bool IsDataServiceCollection { get; set; }

            public bool IsPrimitiveOrComplexCollection { get; set; }

            public bool IsRootDataServiceCollection
            {
                get
                {
                    return (this.IsDataServiceCollection && (this.Parent == null));
                }
            }

            public object Item { get; private set; }

            public IList<BindingGraph.Edge> OutgoingEdges
            {
                get
                {
                    if (this.outgoingEdges == null)
                    {
                        this.outgoingEdges = new List<BindingGraph.Edge>();
                    }
                    return this.outgoingEdges;
                }
            }

            public BindingGraph.Vertex Parent { get; set; }

            public string ParentProperty { get; set; }

            public Type PrimitiveOrComplexCollectionItemType { get; set; }
        }
    }
}

