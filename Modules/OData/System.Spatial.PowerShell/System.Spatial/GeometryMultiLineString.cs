namespace System.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    internal abstract class GeometryMultiLineString : GeometryMultiCurve
    {
        protected GeometryMultiLineString(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as GeometryMultiLineString);
        }

        public bool Equals(GeometryMultiLineString other)
        {
            bool? nullable = base.BaseEquals(other);
            if (!nullable.HasValue)
            {
                return this.LineStrings.SequenceEqual<GeometryLineString>(other.LineStrings);
            }
            return nullable.GetValueOrDefault();
        }

        public override int GetHashCode()
        {
            return Geography.ComputeHashCodeFor<GeometryLineString>(base.CoordinateSystem, this.LineStrings);
        }

        public abstract ReadOnlyCollection<GeometryLineString> LineStrings { get; }
    }
}

