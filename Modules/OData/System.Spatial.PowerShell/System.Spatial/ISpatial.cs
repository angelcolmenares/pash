namespace System.Spatial
{
    using System;

    internal interface ISpatial
    {
        System.Spatial.CoordinateSystem CoordinateSystem { get; }

        bool IsEmpty { get; }
    }
}

