namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.Generic;
	using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Spatial;
    using System.Threading;

    internal abstract class SpatialTreeBuilder<T> : TypeWashedPipeline where T: class, ISpatial
    {
        private List<T> currentFigure;
        private SpatialBuilderNode currentNode;
        private SpatialBuilderNode lastConstructedNode;

        public event Action<T> ProduceInstance;

        protected SpatialTreeBuilder()
        {
        }

        internal override void BeginFigure(double coordinate1, double coordinate2, double? coordinate3, double? coordinate4)
        {
            if (this.currentFigure == null)
            {
                this.currentFigure = new List<T>();
            }
            this.currentFigure.Add(this.CreatePoint(false, coordinate1, coordinate2, coordinate3, coordinate4));
        }

        internal override void BeginGeo(SpatialType type)
        {
            if (this.currentNode == null)
            {
                SpatialBuilderNode node = new SpatialBuilderNode {
                    Type = type
                };
                this.currentNode = node;
                this.lastConstructedNode = null;
            }
            else
            {
                this.currentNode = this.currentNode.CreateChildren(type);
            }
        }

        protected abstract T CreatePoint(bool isEmpty, double x, double y, double? z, double? m);
        protected abstract T CreateShapeInstance(SpatialType type, IEnumerable<T> spatialData);
        internal override void EndFigure()
        {
            if (this.currentFigure.Count == 1)
            {
                this.currentNode.CreateChildren(SpatialType.Point).Instance = this.currentFigure[0];
            }
            else
            {
                this.currentNode.CreateChildren(SpatialType.LineString).Instance = this.CreateShapeInstance(SpatialType.LineString, this.currentFigure);
            }
            this.currentFigure = null;
        }

        internal override void EndGeo()
        {
            switch (this.currentNode.Type)
            {
                case SpatialType.Point:
                    this.currentNode.Instance = (this.currentNode.Children.Count > 0) ? this.currentNode.Children[0].Instance : this.CreatePoint(true, double.NaN, double.NaN, null, null);
                    break;

                case SpatialType.LineString:
                    this.currentNode.Instance = (this.currentNode.Children.Count > 0) ? this.currentNode.Children[0].Instance : this.CreateShapeInstance(SpatialType.LineString, new T[0]);
                    break;

                case SpatialType.Polygon:
                case SpatialType.MultiPoint:
                case SpatialType.MultiLineString:
                case SpatialType.MultiPolygon:
                case SpatialType.Collection:
                    this.currentNode.Instance = this.CreateShapeInstance(this.currentNode.Type, from node in this.currentNode.Children select node.Instance);
                    break;

                case SpatialType.FullGlobe:
                    this.currentNode.Instance = this.CreateShapeInstance(SpatialType.FullGlobe, new T[0]);
                    break;
            }
            this.TraverseUpTheTree();
            this.NotifyIfWeJustFinishedBuildingSomething();
        }

        internal override void LineTo(double x, double y, double? z, double? m)
        {
            this.currentFigure.Add(this.CreatePoint(false, x, y, z, m));
        }

        private void NotifyIfWeJustFinishedBuildingSomething()
        {
            if ((this.currentNode == null) && (this.ProduceInstance != null))
            {
                this.ProduceInstance(this.lastConstructedNode.Instance);
            }
        }

        internal override void Reset()
        {
            this.currentNode = null;
            this.currentFigure = null;
        }

        private void TraverseUpTheTree()
        {
            this.lastConstructedNode = this.currentNode;
            this.currentNode = this.currentNode.Parent;
        }

        public T ConstructedInstance
        {
            get
            {
                if (((this.lastConstructedNode == null) || (this.lastConstructedNode.Instance == null)) || (this.lastConstructedNode.Parent != null))
                {
                    throw new InvalidOperationException(Strings.SpatialBuilder_CannotCreateBeforeDrawn);
                }
                return this.lastConstructedNode.Instance;
            }
        }

        public override bool IsGeography
        {
            get
            {
                return typeof(Geography).IsAssignableFrom(typeof(T));
            }
        }

        private class SpatialBuilderNode
        {
            public SpatialBuilderNode()
            {
                this.Children = new List<SpatialTreeBuilder<T>.SpatialBuilderNode>();
            }

            internal SpatialTreeBuilder<T>.SpatialBuilderNode CreateChildren(SpatialType type)
            {
                SpatialTreeBuilder<T>.SpatialBuilderNode item = new SpatialTreeBuilder<T>.SpatialBuilderNode {
                    Parent = (SpatialTreeBuilder<T>.SpatialBuilderNode) this,
                    Type = type
                };
                this.Children.Add(item);
                return item;
            }

            public List<SpatialTreeBuilder<T>.SpatialBuilderNode> Children { get; private set; }

            public T Instance { get; set; }

            public SpatialTreeBuilder<T>.SpatialBuilderNode Parent { get; private set; }

            public SpatialType Type { get; set; }
        }
    }
}

