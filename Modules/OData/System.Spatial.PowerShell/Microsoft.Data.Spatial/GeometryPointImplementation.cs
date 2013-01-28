namespace Microsoft.Data.Spatial
{
    using System;
    using System.Spatial;

    internal class GeometryPointImplementation : GeometryPoint
    {
        private double? m;
        private double x;
        private double y;
        private double? z;

        internal GeometryPointImplementation(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
            this.x = double.NaN;
            this.y = double.NaN;
        }

        internal GeometryPointImplementation(CoordinateSystem coordinateSystem, SpatialImplementation creator, double x, double y, double? z, double? m) : base(coordinateSystem, creator)
        {
            if (double.IsNaN(x) || double.IsInfinity(x))
            {
                throw new ArgumentException(Strings.InvalidPointCoordinate(x, "x"));
            }
            if (double.IsNaN(y) || double.IsInfinity(y))
            {
                throw new ArgumentException(Strings.InvalidPointCoordinate(y, "y"));
            }
            this.x = x;
            this.y = y;
            this.z = z;
            this.m = m;
        }

        public override void SendTo(GeometryPipeline pipeline)
        {
            base.SendTo(pipeline);
            pipeline.BeginGeometry(SpatialType.Point);
            if (!this.IsEmpty)
            {
                pipeline.BeginFigure(new GeometryPosition(this.x, this.y, this.z, this.m));
                pipeline.EndFigure();
            }
            pipeline.EndGeometry();
        }

        public override bool IsEmpty
        {
            get
            {
                return double.IsNaN(this.x);
            }
        }

        public override double? M
        {
            get
            {
                return this.m;
            }
        }

        public override double X
        {
            get
            {
                if (this.IsEmpty)
                {
                    throw new NotSupportedException(Strings.Point_AccessCoordinateWhenEmpty);
                }
                return this.x;
            }
        }

        public override double Y
        {
            get
            {
                if (this.IsEmpty)
                {
                    throw new NotSupportedException(Strings.Point_AccessCoordinateWhenEmpty);
                }
                return this.y;
            }
        }

        public override double? Z
        {
            get
            {
                return this.z;
            }
        }
    }
}

