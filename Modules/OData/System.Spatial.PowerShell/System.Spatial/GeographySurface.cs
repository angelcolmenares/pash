namespace System.Spatial
{
    using System;

    internal abstract class GeographySurface : Geography
    {
        protected GeographySurface(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
        }
    }
}

