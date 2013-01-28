namespace System.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    internal abstract class GeographyMultiPoint : GeographyCollection
    {
        protected GeographyMultiPoint(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as GeographyMultiPoint);
        }

        public bool Equals(GeographyMultiPoint other)
        {
            bool? nullable = base.BaseEquals(other);
            if (!nullable.HasValue)
            {
                return this.Points.SequenceEqual<GeographyPoint>(other.Points);
            }
            return nullable.GetValueOrDefault();
        }

        public override int GetHashCode()
        {
            return Geography.ComputeHashCodeFor<GeographyPoint>(base.CoordinateSystem, this.Points);
        }

        public abstract ReadOnlyCollection<GeographyPoint> Points { get; }
    }
}

