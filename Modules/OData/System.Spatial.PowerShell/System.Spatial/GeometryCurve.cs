namespace System.Spatial
{
    using System;

    internal abstract class GeometryCurve : Geometry
    {
        protected GeometryCurve(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
        }
    }
}

