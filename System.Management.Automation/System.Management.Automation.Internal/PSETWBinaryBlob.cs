namespace System.Management.Automation.Internal
{
    using System;

    internal sealed class PSETWBinaryBlob
    {
        public readonly byte[] blob;
        public readonly int length;
        public readonly int offset;

        public PSETWBinaryBlob(byte[] blob, int offset, int length)
        {
            this.blob = blob;
            this.offset = offset;
            this.length = length;
        }
    }
}

