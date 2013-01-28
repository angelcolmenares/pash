namespace System.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    internal abstract class GeographyPolygon : GeographySurface
    {
        protected GeographyPolygon(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as GeographyPolygon);
        }

        public bool Equals(GeographyPolygon other)
        {
            bool? nullable = base.BaseEquals(other);
            if (!nullable.HasValue)
            {
                return this.Rings.SequenceEqual<GeographyLineString>(other.Rings);
            }
            return nullable.GetValueOrDefault();
        }

        public override int GetHashCode()
        {
            return Geography.ComputeHashCodeFor<GeographyLineString>(base.CoordinateSystem, this.Rings);
        }

        public abstract ReadOnlyCollection<GeographyLineString> Rings { get; }
    }
}

