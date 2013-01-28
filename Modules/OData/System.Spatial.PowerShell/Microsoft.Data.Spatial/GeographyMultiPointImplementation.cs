namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Spatial;

    internal class GeographyMultiPointImplementation : GeographyMultiPoint
    {
        private GeographyPoint[] points;

        internal GeographyMultiPointImplementation(SpatialImplementation creator, params GeographyPoint[] points) : this(CoordinateSystem.DefaultGeography, creator, points)
        {
        }

        internal GeographyMultiPointImplementation(CoordinateSystem coordinateSystem, SpatialImplementation creator, params GeographyPoint[] points) : base(coordinateSystem, creator)
        {
            this.points = points ?? new GeographyPoint[0];
        }

        public override void SendTo(GeographyPipeline pipeline)
        {
            base.SendTo(pipeline);
            pipeline.BeginGeography(SpatialType.MultiPoint);
            for (int i = 0; i < this.points.Length; i++)
            {
                this.points[i].SendTo(pipeline);
            }
            pipeline.EndGeography();
        }

        public override ReadOnlyCollection<Geography> Geographies
        {
            get
            {
                return this.points.AsReadOnly<Geography>();
            }
        }

        public override bool IsEmpty
        {
            get
            {
                return (this.points.Length == 0);
            }
        }

        public override ReadOnlyCollection<GeographyPoint> Points
        {
            get
            {
                return this.points.AsReadOnly<GeographyPoint>();
            }
        }
    }
}

