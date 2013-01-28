namespace Microsoft.Data.OData.Json
{
    using System;

    internal enum JsonNodeType
    {
        None,
        StartObject,
        EndObject,
        StartArray,
        EndArray,
        Property,
        PrimitiveValue,
        EndOfInput
    }
}

