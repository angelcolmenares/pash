namespace System.Data.Services
{
    using System;

    [Flags]
    internal enum EntitySetRights
    {
        All = 0x3f,
        AllRead = 3,
        AllWrite = 60,
        None = 0,
        ReadMultiple = 2,
        ReadSingle = 1,
        WriteAppend = 4,
        WriteDelete = 0x10,
        WriteMerge = 0x20,
        WriteReplace = 8
    }
}

