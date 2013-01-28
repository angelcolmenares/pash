namespace Microsoft.Management.Infrastructure
{
    using Microsoft.Management.Infrastructure.Generic;
    using System;

    public class CimMethodResult : CimMethodResultBase, IDisposable
    {
        private CimMethodParametersCollection _backingMethodParametersCollection;
        private bool _disposed;

        internal CimMethodResult(CimInstance backingInstance)
        {
            this._backingMethodParametersCollection = new CimMethodParametersCollection(backingInstance);
        }

        internal void AssertNotDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.ToString());
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    this._backingMethodParametersCollection.Dispose();
                    this._backingMethodParametersCollection = null;
                }
                this._disposed = true;
            }
        }

        public CimReadOnlyKeyedCollection<CimMethodParameter> OutParameters
        {
            get
            {
                this.AssertNotDisposed();
                return this._backingMethodParametersCollection;
            }
        }

        public CimMethodParameter ReturnValue
        {
            get
            {
                this.AssertNotDisposed();
                return this.OutParameters["ReturnValue"];
            }
        }
    }
}

