namespace System.Data.Services.Providers
{
    using System;

    internal enum ServiceOperationResultKind
    {
        DirectValue,
        Enumeration,
        QueryWithMultipleResults,
        QueryWithSingleResult,
        Void
    }
}

