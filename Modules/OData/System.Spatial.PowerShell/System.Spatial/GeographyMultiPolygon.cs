namespace System.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    internal abstract class GeographyMultiPolygon : GeographyMultiSurface
    {
        protected GeographyMultiPolygon(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as GeographyMultiPolygon);
        }

        public bool Equals(GeographyMultiPolygon other)
        {
            bool? nullable = base.BaseEquals(other);
            if (!nullable.HasValue)
            {
                return this.Polygons.SequenceEqual<GeographyPolygon>(other.Polygons);
            }
            return nullable.GetValueOrDefault();
        }

        public override int GetHashCode()
        {
            return Geography.ComputeHashCodeFor<GeographyPolygon>(base.CoordinateSystem, this.Polygons);
        }

        public abstract ReadOnlyCollection<GeographyPolygon> Polygons { get; }
    }
}

