namespace System.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    internal abstract class GeometryPolygon : GeometrySurface
    {
        protected GeometryPolygon(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as GeometryPolygon);
        }

        public bool Equals(GeometryPolygon other)
        {
            bool? nullable = base.BaseEquals(other);
            if (!nullable.HasValue)
            {
                return this.Rings.SequenceEqual<GeometryLineString>(other.Rings);
            }
            return nullable.GetValueOrDefault();
        }

        public override int GetHashCode()
        {
            return Geography.ComputeHashCodeFor<GeometryLineString>(base.CoordinateSystem, this.Rings);
        }

        public abstract ReadOnlyCollection<GeometryLineString> Rings { get; }
    }
}

