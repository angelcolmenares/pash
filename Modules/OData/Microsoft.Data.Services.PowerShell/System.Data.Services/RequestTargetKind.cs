namespace System.Data.Services
{
    using System;

    internal enum RequestTargetKind
    {
        Nothing,
        ServiceDirectory,
        Resource,
        ComplexObject,
        Primitive,
        PrimitiveValue,
        Metadata,
        VoidOperation,
        Batch,
        Link,
        OpenProperty,
        OpenPropertyValue,
        MediaResource,
        Collection
    }
}

