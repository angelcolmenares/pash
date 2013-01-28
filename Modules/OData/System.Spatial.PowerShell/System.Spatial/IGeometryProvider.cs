namespace System.Spatial
{
    using System;

    internal interface IGeometryProvider
    {
        event Action<Geometry> ProduceGeometry;

        Geometry ConstructedGeometry { get; }
    }
}

