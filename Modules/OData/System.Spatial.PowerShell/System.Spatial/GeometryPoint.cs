namespace System.Spatial
{
    using System;

    internal abstract class GeometryPoint : Geometry
    {
        protected GeometryPoint(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
        }

        public static GeometryPoint Create(double x, double y)
        {
            return Create(CoordinateSystem.DefaultGeometry, x, y, null, null);
        }

        public static GeometryPoint Create(double x, double y, double? z)
        {
            return Create(CoordinateSystem.DefaultGeometry, x, y, z, null);
        }

        public static GeometryPoint Create(double x, double y, double? z, double? m)
        {
            return Create(CoordinateSystem.DefaultGeometry, x, y, z, m);
        }

        public static GeometryPoint Create(CoordinateSystem coordinateSystem, double x, double y, double? z, double? m)
        {
            SpatialBuilder builder = SpatialBuilder.Create();
            GeometryPipeline geometryPipeline = builder.GeometryPipeline;
            geometryPipeline.SetCoordinateSystem(coordinateSystem);
            geometryPipeline.BeginGeometry(SpatialType.Point);
            geometryPipeline.BeginFigure(new GeometryPosition(x, y, z, m));
            geometryPipeline.EndFigure();
            geometryPipeline.EndGeometry();
            return (GeometryPoint) builder.ConstructedGeometry;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as GeometryPoint);
        }

        public bool Equals(GeometryPoint other)
        {
            bool? nullable = base.BaseEquals(other);
            if (nullable.HasValue)
            {
                return nullable.GetValueOrDefault();
            }
            return ((((this.X == other.X) && (this.Y == other.Y)) && (this.Z == other.Z)) && (this.M == other.M));
        }

        public override int GetHashCode()
        {
            double[] fields = new double[4];
            fields[0] = this.IsEmpty ? 0.0 : this.X;
            fields[1] = this.IsEmpty ? 0.0 : this.Y;
            double? z = this.Z;
            fields[2] = z.HasValue ? z.GetValueOrDefault() : 0.0;
            double? m = this.M;
            fields[3] = m.HasValue ? m.GetValueOrDefault() : 0.0;
            return Geography.ComputeHashCodeFor<double>(base.CoordinateSystem, fields);
        }

        public abstract double? M { get; }

        public abstract double X { get; }

        public abstract double Y { get; }

        public abstract double? Z { get; }
    }
}

