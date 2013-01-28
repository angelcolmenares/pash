namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Spatial;

    internal class GeometryPolygonImplementation : GeometryPolygon
    {
        private GeometryLineString[] rings;

        internal GeometryPolygonImplementation(SpatialImplementation creator, params GeometryLineString[] rings) : this(CoordinateSystem.DefaultGeometry, creator, rings)
        {
        }

        internal GeometryPolygonImplementation(CoordinateSystem coordinateSystem, SpatialImplementation creator, params GeometryLineString[] rings) : base(coordinateSystem, creator)
        {
            this.rings = rings ?? new GeometryLineString[0];
        }

        public override void SendTo(GeometryPipeline pipeline)
        {
            base.SendTo(pipeline);
            pipeline.BeginGeometry(SpatialType.Polygon);
            for (int i = 0; i < this.rings.Length; i++)
            {
                this.rings[i].SendFigure(pipeline);
            }
            pipeline.EndGeometry();
        }

        public override bool IsEmpty
        {
            get
            {
                return (this.rings.Length == 0);
            }
        }

        public override ReadOnlyCollection<GeometryLineString> Rings
        {
            get
            {
                return this.rings.AsReadOnly<GeometryLineString>();
            }
        }
    }
}

