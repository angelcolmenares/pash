namespace Microsoft.Data.Spatial
{
    using System;
    using System.Collections.ObjectModel;
    using System.Spatial;

    internal class GeographyPolygonImplementation : GeographyPolygon
    {
        private GeographyLineString[] rings;

        internal GeographyPolygonImplementation(SpatialImplementation creator, params GeographyLineString[] rings) : this(CoordinateSystem.DefaultGeography, creator, rings)
        {
        }

        internal GeographyPolygonImplementation(CoordinateSystem coordinateSystem, SpatialImplementation creator, params GeographyLineString[] rings) : base(coordinateSystem, creator)
        {
            this.rings = rings ?? new GeographyLineString[0];
        }

        public override void SendTo(GeographyPipeline pipeline)
        {
            base.SendTo(pipeline);
            pipeline.BeginGeography(SpatialType.Polygon);
            for (int i = 0; i < this.rings.Length; i++)
            {
                this.rings[i].SendFigure(pipeline);
            }
            pipeline.EndGeography();
        }

        public override bool IsEmpty
        {
            get
            {
                return (this.rings.Length == 0);
            }
        }

        public override ReadOnlyCollection<GeographyLineString> Rings
        {
            get
            {
                return this.rings.AsReadOnly<GeographyLineString>();
            }
        }
    }
}

