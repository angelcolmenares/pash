namespace Microsoft.Data.OData
{
    using System;

    [Flags]
    internal enum ODataUndeclaredPropertyBehaviorKinds
    {
        None,
        IgnoreUndeclaredValueProperty,
        ReportUndeclaredLinkProperty
    }
}

