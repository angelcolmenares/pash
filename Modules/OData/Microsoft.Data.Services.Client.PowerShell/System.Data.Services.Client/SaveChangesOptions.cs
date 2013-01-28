namespace System.Data.Services.Client
{
    using System;

    [Flags]
    internal enum SaveChangesOptions
    {
        Batch = 1,
        ContinueOnError = 2,
        None = 0,
        PatchOnUpdate = 8,
        ReplaceOnUpdate = 4
    }
}

