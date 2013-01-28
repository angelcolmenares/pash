namespace System.Management.Automation.Internal
{
    using System;

    internal class PSDataCollectionWriter<T> : ObjectWriter
    {
        public PSDataCollectionWriter(PSDataCollectionStream<T> stream) : base(stream)
        {
        }
    }
}

