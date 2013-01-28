namespace System.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    internal abstract class GeographyMultiLineString : GeographyMultiCurve
    {
        protected GeographyMultiLineString(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as GeographyMultiLineString);
        }

        public bool Equals(GeographyMultiLineString other)
        {
            bool? nullable = base.BaseEquals(other);
            if (!nullable.HasValue)
            {
                return this.LineStrings.SequenceEqual<GeographyLineString>(other.LineStrings);
            }
            return nullable.GetValueOrDefault();
        }

        public override int GetHashCode()
        {
            return Geography.ComputeHashCodeFor<GeographyLineString>(base.CoordinateSystem, this.LineStrings);
        }

        public abstract ReadOnlyCollection<GeographyLineString> LineStrings { get; }
    }
}

