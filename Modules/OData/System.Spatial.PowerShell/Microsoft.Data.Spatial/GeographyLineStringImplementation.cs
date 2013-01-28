namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Spatial;

    internal class GeographyLineStringImplementation : GeographyLineString
    {
        private GeographyPoint[] points;

        internal GeographyLineStringImplementation(CoordinateSystem coordinateSystem, SpatialImplementation creator, params GeographyPoint[] points) : base(coordinateSystem, creator)
        {
            this.points = points ?? new GeographyPoint[0];
        }

        public override void SendTo(GeographyPipeline pipeline)
        {
            base.SendTo(pipeline);
            pipeline.BeginGeography(SpatialType.LineString);
            this.SendFigure(pipeline);
            pipeline.EndGeography();
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

