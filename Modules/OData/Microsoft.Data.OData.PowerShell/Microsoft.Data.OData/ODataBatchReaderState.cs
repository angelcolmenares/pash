namespace Microsoft.Data.OData
{
    using System;

    internal enum ODataBatchReaderState
    {
        Initial,
        Operation,
        ChangesetStart,
        ChangesetEnd,
        Completed,
        Exception
    }
}

