namespace System.Spatial
{
    using System;

    internal interface IGeographyProvider
    {
        event Action<Geography> ProduceGeography;

        Geography ConstructedGeography { get; }
    }
}

