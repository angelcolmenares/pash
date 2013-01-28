namespace Microsoft.Data.Spatial
{
    using System;
    using System.Spatial;

    internal class TypeWashedToGeometryPipeline : TypeWashedPipeline
    {
        private readonly GeometryPipeline output;

        public TypeWashedToGeometryPipeline(SpatialPipeline output)
        {
            this.output = (GeometryPipeline) output;
        }

        internal override void BeginFigure(double coordinate1, double coordinate2, double? coordinate3, double? coordinate4)
        {
            this.output.BeginFigure(new GeometryPosition(coordinate1, coordinate2, coordinate3, coordinate4));
        }

        internal override void BeginGeo(SpatialType type)
        {
            this.output.BeginGeometry(type);
        }

        internal override void EndFigure()
        {
            this.output.EndFigure();
        }

        internal override void EndGeo()
        {
            this.output.EndGeometry();
        }

        internal override void LineTo(double coordinate1, double coordinate2, double? coordinate3, double? coordinate4)
        {
            this.output.LineTo(new GeometryPosition(coordinate1, coordinate2, coordinate3, coordinate4));
        }

        internal override void Reset()
        {
            this.output.Reset();
        }

        internal override void SetCoordinateSystem(int? epsgId)
        {
            CoordinateSystem coordinateSystem = CoordinateSystem.Geometry(epsgId);
            this.output.SetCoordinateSystem(coordinateSystem);
        }

        public override bool IsGeography
        {
            get
            {
                return false;
            }
        }
    }
}

