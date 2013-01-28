namespace System.Data.Services
{
    using System;

    [Flags]
    internal enum ServiceOperationRights
    {
        All = 3,
        AllRead = 3,
        None = 0,
        OverrideEntitySetRights = 4,
        ReadMultiple = 2,
        ReadSingle = 1
    }
}

