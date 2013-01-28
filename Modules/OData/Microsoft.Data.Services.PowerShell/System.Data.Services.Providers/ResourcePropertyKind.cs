namespace System.Data.Services.Providers
{
    using System;

    [Flags]
    internal enum ResourcePropertyKind
    {
        Collection = 0x40,
        ComplexType = 4,
        ETag = 0x20,
        Key = 2,
        Primitive = 1,
        ResourceReference = 8,
        ResourceSetReference = 0x10,
        Stream = 0x80
    }
}

