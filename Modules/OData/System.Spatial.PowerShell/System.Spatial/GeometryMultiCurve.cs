namespace System.Spatial
{
    using System;

    internal abstract class GeometryMultiCurve : GeometryCollection
    {
        protected GeometryMultiCurve(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
        }
    }
}

