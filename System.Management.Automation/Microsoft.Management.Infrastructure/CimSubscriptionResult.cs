using Microsoft.Management.Infrastructure.Native;
using System;

namespace Microsoft.Management.Infrastructure
{
	public class CimSubscriptionResult : IDisposable
	{
		private CimInstance _resultInstance;

		private readonly string _bookmark;

		private readonly string _machineId;

		private bool _disposed;

		public string Bookmark
		{
			get
			{
				this.AssertNotDisposed();
				return this._bookmark;
			}
		}

		public CimInstance Instance
		{
			get
			{
				this.AssertNotDisposed();
				return this._resultInstance;
			}
		}

		public string MachineId
		{
			get
			{
				this.AssertNotDisposed();
				return this._machineId;
			}
		}

		internal CimSubscriptionResult(InstanceHandle handle, string bookmark, string machineId)
		{
			this._resultInstance = new CimInstance(handle, null);
			this._bookmark = bookmark;
			this._machineId = machineId;
		}

		internal void AssertNotDisposed()
		{
			if (!this._disposed)
			{
				return;
			}
			else
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
					this._resultInstance.Dispose();
					this._resultInstance = null;
				}
				this._disposed = true;
				return;
			}
			else
			{
				return;
			}
		}
	}
}