namespace System.Spatial
{
    using System;

    internal abstract class GeographyMultiSurface : GeographyCollection
    {
        protected GeographyMultiSurface(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
        }
    }
}

