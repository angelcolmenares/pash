namespace System.Spatial
{
    using System;

    internal abstract class GeographyFullGlobe : GeographySurface
    {
        protected GeographyFullGlobe(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as GeographyFullGlobe);
        }

        public bool Equals(GeographyFullGlobe other)
        {
            bool? nullable = base.BaseEquals(other);
            return (!nullable.HasValue || nullable.GetValueOrDefault());
        }

        public override int GetHashCode()
        {
            return Geography.ComputeHashCodeFor<int>(base.CoordinateSystem, new int[1]);
        }
    }
}

