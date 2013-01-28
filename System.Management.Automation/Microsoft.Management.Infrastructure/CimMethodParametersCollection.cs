namespace Microsoft.Management.Infrastructure
{
    using Microsoft.Management.Infrastructure.Generic;
    using Microsoft.Management.Infrastructure.Internal.Data;
    using Microsoft.Management.Infrastructure.Native;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class CimMethodParametersCollection : CimKeyedCollection<CimMethodParameter>, IDisposable
    {
        private CimInstance _backingInstance;
        private bool _disposed;

        public CimMethodParametersCollection()
        {
            this._backingInstance = new CimInstance(base.GetType().Name);
        }

        internal CimMethodParametersCollection(CimInstance backingInstance)
        {
            this._backingInstance = backingInstance;
        }

        public override void Add(CimMethodParameter newParameter)
        {
            this.AssertNotDisposed();
            if (newParameter == null)
            {
                throw new ArgumentNullException("newParameter");
            }
            CimProperty newItem = CimProperty.Create(newParameter.Name, newParameter.Value, newParameter.CimType, newParameter.Flags);
            this._backingInstance.CimInstanceProperties.Add(newItem);
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
                    this._backingInstance.Dispose();
                    this._backingInstance = null;
                }
                this._disposed = true;
            }
        }

        public override IEnumerator<CimMethodParameter> GetEnumerator()
        {
            this.AssertNotDisposed();
            return (IEnumerator<CimMethodParameter>) (from p in this._backingInstance.CimInstanceProperties select new CimMethodParameterBackedByCimProperty(p, this._backingInstance.GetCimSessionComputerName(), this._backingInstance.GetCimSessionInstanceId())).GetEnumerator();
        }

        public override int Count
        {
            get
            {
                this.AssertNotDisposed();
                return this._backingInstance.CimInstanceProperties.Count;
            }
        }

        internal InstanceHandle InstanceHandleForMethodInvocation
        {
            get
            {
                this.AssertNotDisposed();
                if (this._backingInstance.CimInstanceProperties.Count == 0)
                {
                    return null;
                }
                return this._backingInstance.InstanceHandle;
            }
        }

        public override CimMethodParameter this[string parameterName]
        {
            get
            {
                this.AssertNotDisposed();
                if (string.IsNullOrWhiteSpace(parameterName))
                {
                    throw new ArgumentNullException("parameterName");
                }
                CimProperty backingProperty = this._backingInstance.CimInstanceProperties[parameterName];
                if (backingProperty != null)
                {
                    return new CimMethodParameterBackedByCimProperty(backingProperty, this._backingInstance.GetCimSessionComputerName(), this._backingInstance.GetCimSessionInstanceId());
                }
                return null;
            }
        }
    }
}

