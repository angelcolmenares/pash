namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Spatial;

    internal class GeometryMultiPolygonImplementation : GeometryMultiPolygon
    {
        private GeometryPolygon[] polygons;

        internal GeometryMultiPolygonImplementation(SpatialImplementation creator, params GeometryPolygon[] polygons) : this(CoordinateSystem.DefaultGeometry, creator, polygons)
        {
        }

        internal GeometryMultiPolygonImplementation(CoordinateSystem coordinateSystem, SpatialImplementation creator, params GeometryPolygon[] polygons) : base(coordinateSystem, creator)
        {
            this.polygons = polygons;
        }

        public override void SendTo(GeometryPipeline pipeline)
        {
            base.SendTo(pipeline);
            pipeline.BeginGeometry(SpatialType.MultiPolygon);
            for (int i = 0; i < this.polygons.Length; i++)
            {
                this.polygons[i].SendTo(pipeline);
            }
            pipeline.EndGeometry();
        }

        public override ReadOnlyCollection<Geometry> Geometries
        {
            get
            {
                return this.polygons.AsReadOnly<Geometry>();
            }
        }

        public override bool IsEmpty
        {
            get
            {
                return (this.polygons.Length == 0);
            }
        }

        public override ReadOnlyCollection<GeometryPolygon> Polygons
        {
            get
            {
                return this.polygons.AsReadOnly<GeometryPolygon>();
            }
        }
    }
}

