namespace System.Spatial
{
    using System;

    internal enum SpatialType : byte
    {
        Collection = 7,
        FullGlobe = 11,
        LineString = 2,
        MultiLineString = 5,
        MultiPoint = 4,
        MultiPolygon = 6,
        Point = 1,
        Polygon = 3,
        Unknown = 0
    }
}

