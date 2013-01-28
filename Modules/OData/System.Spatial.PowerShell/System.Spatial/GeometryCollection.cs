namespace System.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    internal abstract class GeometryCollection : Geometry
    {
        protected GeometryCollection(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as GeometryCollection);
        }

        public bool Equals(GeometryCollection other)
        {
            bool? nullable = base.BaseEquals(other);
            if (!nullable.HasValue)
            {
                return this.Geometries.SequenceEqual<Geometry>(other.Geometries);
            }
            return nullable.GetValueOrDefault();
        }

        public override int GetHashCode()
        {
            return Geography.ComputeHashCodeFor<Geometry>(base.CoordinateSystem, this.Geometries);
        }

        public abstract ReadOnlyCollection<Geometry> Geometries { get; }
    }
}

