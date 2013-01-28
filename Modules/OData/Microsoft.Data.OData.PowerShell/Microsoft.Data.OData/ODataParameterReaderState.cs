namespace Microsoft.Data.OData
{
    using System;

    internal enum ODataParameterReaderState
    {
        Start,
        Value,
        Collection,
        Exception,
        Completed
    }
}

