namespace System.Spatial
{
    using System;

    internal abstract class GeometryPipeline
    {
        protected GeometryPipeline()
        {
        }

        public abstract void BeginFigure(GeometryPosition position);
        public abstract void BeginGeometry(SpatialType type);
        public abstract void EndFigure();
        public abstract void EndGeometry();
        public abstract void LineTo(GeometryPosition position);
        public abstract void Reset();
        public abstract void SetCoordinateSystem(CoordinateSystem coordinateSystem);
    }
}

