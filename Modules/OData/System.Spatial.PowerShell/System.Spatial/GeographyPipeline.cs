namespace System.Spatial
{
    using System;

    internal abstract class GeographyPipeline
    {
        protected GeographyPipeline()
        {
        }

        public abstract void BeginFigure(GeographyPosition position);
        public abstract void BeginGeography(SpatialType type);
        public abstract void EndFigure();
        public abstract void EndGeography();
        public abstract void LineTo(GeographyPosition position);
        public abstract void Reset();
        public abstract void SetCoordinateSystem(CoordinateSystem coordinateSystem);
    }
}

