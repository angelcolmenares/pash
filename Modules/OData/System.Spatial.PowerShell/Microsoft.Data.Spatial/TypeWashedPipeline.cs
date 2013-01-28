namespace Microsoft.Data.Spatial
{
    using System;
    using System.Spatial;

    internal abstract class TypeWashedPipeline
    {
        protected TypeWashedPipeline()
        {
        }

        internal abstract void BeginFigure(double coordinate1, double coordinate2, double? coordinate3, double? coordinate4);
        internal abstract void BeginGeo(SpatialType type);
        internal abstract void EndFigure();
        internal abstract void EndGeo();
        internal abstract void LineTo(double coordinate1, double coordinate2, double? coordinate3, double? coordinate4);
        internal abstract void Reset();
        internal abstract void SetCoordinateSystem(int? epsgId);

        public abstract bool IsGeography { get; }
    }
}

