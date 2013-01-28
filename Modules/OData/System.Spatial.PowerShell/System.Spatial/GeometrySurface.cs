namespace System.Spatial
{
    using System;

    internal abstract class GeometrySurface : Geometry
    {
        internal GeometrySurface(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
        }
    }
}

