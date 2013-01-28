namespace System.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    internal abstract class GeographyCollection : Geography
    {
        protected GeographyCollection(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as GeographyCollection);
        }

        public bool Equals(GeographyCollection other)
        {
            bool? nullable = base.BaseEquals(other);
            if (!nullable.HasValue)
            {
                return this.Geographies.SequenceEqual<Geography>(other.Geographies);
            }
            return nullable.GetValueOrDefault();
        }

        public override int GetHashCode()
        {
            return Geography.ComputeHashCodeFor<Geography>(base.CoordinateSystem, this.Geographies);
        }

        public abstract ReadOnlyCollection<Geography> Geographies { get; }
    }
}

