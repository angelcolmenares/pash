namespace System.Spatial
{
    using System;

    internal abstract class GeographyCurve : Geography
    {
        protected GeographyCurve(CoordinateSystem coordinateSystem, SpatialImplementation creator) : base(coordinateSystem, creator)
        {
        }
    }
}

