namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Spatial;

    internal class GeometryLineStringImplementation : GeometryLineString
    {
        private GeometryPoint[] points;

        internal GeometryLineStringImplementation(SpatialImplementation creator, params GeometryPoint[] points) : this(CoordinateSystem.DefaultGeometry, creator, points)
        {
        }

        internal GeometryLineStringImplementation(CoordinateSystem coordinateSystem, SpatialImplementation creator, params GeometryPoint[] points) : base(coordinateSystem, creator)
        {
            this.points = points ?? new GeometryPoint[0];
        }

        public override void SendTo(GeometryPipeline pipeline)
        {
            base.SendTo(pipeline);
            pipeline.BeginGeometry(SpatialType.LineString);
            this.SendFigure(pipeline);
            pipeline.EndGeometry();
        }

        public override bool IsEmpty
        {
            get
            {
                return (this.points.Length == 0);
            }
        }

        public override ReadOnlyCollection<GeometryPoint> Points
        {
            get
            {
                return this.points.AsReadOnly<GeometryPoint>();
            }
        }
    }
}

