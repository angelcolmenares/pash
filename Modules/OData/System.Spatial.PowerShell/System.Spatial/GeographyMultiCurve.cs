namespace System.Spatial
{
    using System;

    internal abstract class GeographyMultiCurve : GeographyCollection
    {
        protected GeographyMultiCurve(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
        }
    }
}

