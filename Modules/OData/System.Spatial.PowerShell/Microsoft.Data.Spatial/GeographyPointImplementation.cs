namespace Microsoft.Data.Spatial
{
    using System;
    using System.Spatial;

    internal class GeographyPointImplementation : GeographyPoint
    {
        private double latitude;
        private double longitude;
        private double? m;
        private double? z;

        internal GeographyPointImplementation(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
            this.latitude = double.NaN;
            this.longitude = double.NaN;
        }

        internal GeographyPointImplementation(CoordinateSystem coordinateSystem, SpatialImplementation creator, double latitude, double longitude, double? zvalue, double? mvalue) : base(coordinateSystem, creator)
        {
            if (double.IsNaN(latitude) || double.IsInfinity(latitude))
            {
                throw new ArgumentException(Strings.InvalidPointCoordinate(latitude, "latitude"));
            }
            if (double.IsNaN(longitude) || double.IsInfinity(longitude))
            {
                throw new ArgumentException(Strings.InvalidPointCoordinate(longitude, "longitude"));
            }
            this.latitude = latitude;
            this.longitude = longitude;
            this.z = zvalue;
            this.m = mvalue;
        }

        public override void SendTo(GeographyPipeline pipeline)
        {
            base.SendTo(pipeline);
            pipeline.BeginGeography(SpatialType.Point);
            if (!this.IsEmpty)
            {
                pipeline.BeginFigure(new GeographyPosition(this.latitude, this.longitude, this.z, this.m));
                pipeline.EndFigure();
            }
            pipeline.EndGeography();
        }

        public override bool IsEmpty
        {
            get
            {
                return double.IsNaN(this.latitude);
            }
        }

        public override double Latitude
        {
            get
            {
                if (this.IsEmpty)
                {
                    throw new NotSupportedException(Strings.Point_AccessCoordinateWhenEmpty);
                }
                return this.latitude;
            }
        }

        public override double Longitude
        {
            get
            {
                if (this.IsEmpty)
                {
                    throw new NotSupportedException(Strings.Point_AccessCoordinateWhenEmpty);
                }
                return this.longitude;
            }
        }

        public override double? M
        {
            get
            {
                return this.m;
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

