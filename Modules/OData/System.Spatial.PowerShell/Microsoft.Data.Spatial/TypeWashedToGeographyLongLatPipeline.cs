namespace Microsoft.Data.Spatial
{
    using System;
    using System.Spatial;

    internal class TypeWashedToGeographyLongLatPipeline : TypeWashedPipeline
    {
        private readonly GeographyPipeline output;

        public TypeWashedToGeographyLongLatPipeline(SpatialPipeline output)
        {
            this.output = (GeographyPipeline) output;
        }

        internal override void BeginFigure(double coordinate1, double coordinate2, double? coordinate3, double? coordinate4)
        {
            this.output.BeginFigure(new GeographyPosition(coordinate2, coordinate1, coordinate3, coordinate4));
        }

        internal override void BeginGeo(SpatialType type)
        {
            this.output.BeginGeography(type);
        }

        internal override void EndFigure()
        {
            this.output.EndFigure();
        }

        internal override void EndGeo()
        {
            this.output.EndGeography();
        }

        internal override void LineTo(double coordinate1, double coordinate2, double? coordinate3, double? coordinate4)
        {
            this.output.LineTo(new GeographyPosition(coordinate2, coordinate1, coordinate3, coordinate4));
        }

        internal override void Reset()
        {
            this.output.Reset();
        }

        internal override void SetCoordinateSystem(int? epsgId)
        {
            CoordinateSystem coordinateSystem = CoordinateSystem.Geography(epsgId);
            this.output.SetCoordinateSystem(coordinateSystem);
        }

        public override bool IsGeography
        {
            get
            {
                return true;
            }
        }
    }
}

