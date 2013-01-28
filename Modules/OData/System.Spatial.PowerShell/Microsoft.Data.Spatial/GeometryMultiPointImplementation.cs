namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Spatial;

    internal class GeometryMultiPointImplementation : GeometryMultiPoint
    {
        private GeometryPoint[] points;

        internal GeometryMultiPointImplementation(SpatialImplementation creator, params GeometryPoint[] points) : this(CoordinateSystem.DefaultGeometry, creator, points)
        {
        }

        internal GeometryMultiPointImplementation(CoordinateSystem coordinateSystem, SpatialImplementation creator, params GeometryPoint[] points) : base(coordinateSystem, creator)
        {
            this.points = points ?? new GeometryPoint[0];
        }

        public override void SendTo(GeometryPipeline pipeline)
        {
            base.SendTo(pipeline);
            pipeline.BeginGeometry(SpatialType.MultiPoint);
            for (int i = 0; i < this.points.Length; i++)
            {
                this.points[i].SendTo(pipeline);
            }
            pipeline.EndGeometry();
        }

        public override ReadOnlyCollection<Geometry> Geometries
        {
            get
            {
                return this.points.AsReadOnly<Geometry>();
            }
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

