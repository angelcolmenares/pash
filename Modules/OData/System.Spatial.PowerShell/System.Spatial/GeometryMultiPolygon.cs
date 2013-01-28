namespace System.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    internal abstract class GeometryMultiPolygon : GeometryMultiSurface
    {
        protected GeometryMultiPolygon(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as GeometryMultiPolygon);
        }

        public bool Equals(GeometryMultiPolygon other)
        {
            bool? nullable = base.BaseEquals(other);
            if (!nullable.HasValue)
            {
                return this.Polygons.SequenceEqual<GeometryPolygon>(other.Polygons);
            }
            return nullable.GetValueOrDefault();
        }

        public override int GetHashCode()
        {
            return Geography.ComputeHashCodeFor<GeometryPolygon>(base.CoordinateSystem, this.Polygons);
        }

        public abstract ReadOnlyCollection<GeometryPolygon> Polygons { get; }
    }
}

