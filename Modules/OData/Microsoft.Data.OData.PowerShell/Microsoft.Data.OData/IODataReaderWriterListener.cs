namespace Microsoft.Data.OData
{
    using System;

    internal interface IODataReaderWriterListener
    {
        void OnCompleted();
        void OnException();
    }
}

