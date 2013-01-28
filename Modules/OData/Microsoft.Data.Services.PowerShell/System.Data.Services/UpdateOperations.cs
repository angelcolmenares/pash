namespace System.Data.Services
{
    using System;

    [Flags]
    internal enum UpdateOperations
    {
        Add = 1,
        Change = 2,
        Delete = 4,
        None = 0
    }
}

