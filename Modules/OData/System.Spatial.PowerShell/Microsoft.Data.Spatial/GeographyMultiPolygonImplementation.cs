namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Spatial;

    internal class GeographyMultiPolygonImplementation : GeographyMultiPolygon
    {
        private GeographyPolygon[] polygons;

        internal GeographyMultiPolygonImplementation(SpatialImplementation creator, params GeographyPolygon[] polygons) : this(CoordinateSystem.DefaultGeography, creator, polygons)
        {
        }

        internal GeographyMultiPolygonImplementation(CoordinateSystem coordinateSystem, SpatialImplementation creator, params GeographyPolygon[] polygons) : base(coordinateSystem, creator)
        {
            this.polygons = polygons;
        }

        public override void SendTo(GeographyPipeline pipeline)
        {
            base.SendTo(pipeline);
            pipeline.BeginGeography(SpatialType.MultiPolygon);
            for (int i = 0; i < this.polygons.Length; i++)
            {
                this.polygons[i].SendTo(pipeline);
            }
            pipeline.EndGeography();
        }

        public override ReadOnlyCollection<Geography> Geographies
        {
            get
            {
                return this.polygons.AsReadOnly<Geography>();
            }
        }

        public override bool IsEmpty
        {
            get
            {
                return (this.polygons.Length == 0);
            }
        }

        public override ReadOnlyCollection<GeographyPolygon> Polygons
        {
            get
            {
                return this.polygons.AsReadOnly<GeographyPolygon>();
            }
        }
    }
}

