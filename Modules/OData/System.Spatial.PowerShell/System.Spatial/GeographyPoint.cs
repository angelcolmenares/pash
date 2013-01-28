namespace System.Spatial
{
    using System;

    internal abstract class GeographyPoint : Geography
    {
        protected GeographyPoint(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
        }

        public static GeographyPoint Create(double latitude, double longitude)
        {
            return Create(CoordinateSystem.DefaultGeography, latitude, longitude, null, null);
        }

        public static GeographyPoint Create(double latitude, double longitude, double? z)
        {
            return Create(CoordinateSystem.DefaultGeography, latitude, longitude, z, null);
        }

        public static GeographyPoint Create(double latitude, double longitude, double? z, double? m)
        {
            return Create(CoordinateSystem.DefaultGeography, latitude, longitude, z, m);
        }

        public static GeographyPoint Create(CoordinateSystem coordinateSystem, double latitude, double longitude, double? z, double? m)
        {
            SpatialBuilder builder = SpatialBuilder.Create();
            GeographyPipeline geographyPipeline = builder.GeographyPipeline;
            geographyPipeline.SetCoordinateSystem(coordinateSystem);
            geographyPipeline.BeginGeography(SpatialType.Point);
            geographyPipeline.BeginFigure(new GeographyPosition(latitude, longitude, z, m));
            geographyPipeline.EndFigure();
            geographyPipeline.EndGeography();
            return (GeographyPoint) builder.ConstructedGeography;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as GeographyPoint);
        }

        public bool Equals(GeographyPoint other)
        {
            bool? nullable = base.BaseEquals(other);
            if (nullable.HasValue)
            {
                return nullable.GetValueOrDefault();
            }
            return ((((this.Latitude == other.Latitude) && (this.Longitude == other.Longitude)) && (this.Z == other.Z)) && (this.M == other.M));
        }

        public override int GetHashCode()
        {
            double[] fields = new double[4];
            fields[0] = this.IsEmpty ? 0.0 : this.Latitude;
            fields[1] = this.IsEmpty ? 0.0 : this.Longitude;
            double? z = this.Z;
            fields[2] = z.HasValue ? z.GetValueOrDefault() : 0.0;
            double? m = this.M;
            fields[3] = m.HasValue ? m.GetValueOrDefault() : 0.0;
            return Geography.ComputeHashCodeFor<double>(base.CoordinateSystem, fields);
        }

        public abstract double Latitude { get; }

        public abstract double Longitude { get; }

        public abstract double? M { get; }

        public abstract double? Z { get; }
    }
}

