namespace Microsoft.Data.OData
{
    using System;
    using System.IO;

    internal abstract class ODataBatchOperationStream : Stream
    {
        private IODataBatchOperationListener listener;

        internal ODataBatchOperationStream(IODataBatchOperationListener listener)
        {
            this.listener = listener;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.listener != null))
            {
                this.listener.BatchOperationContentStreamDisposed();
                this.listener = null;
            }
            base.Dispose(disposing);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        protected void ValidateNotDisposed()
        {
            if (this.listener == null)
            {
                throw new ObjectDisposedException(null, Strings.ODataBatchOperationStream_Disposed);
            }
        }
    }
}

