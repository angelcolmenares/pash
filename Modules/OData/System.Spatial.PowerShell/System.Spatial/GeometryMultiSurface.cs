namespace System.Spatial
{
    using System;

    internal abstract class GeometryMultiSurface : GeometryCollection
    {
        internal GeometryMultiSurface(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
        }
    }
}

