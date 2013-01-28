using Microsoft.Management.Infrastructure.Options;
using System;
using System.Threading;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimSyncAction : CimBaseAction, IDisposable
	{
		private ManualResetEventSlim completeEvent;

		protected CimResponseType responseType;

		private bool _disposed;

		internal CimResponseType ResponseType
		{
			set
			{
				this.responseType = value;
			}
		}

		public CimSyncAction()
		{
			this.completeEvent = new ManualResetEventSlim(false);
			this.responseType = CimResponseType.None;
		}

		protected virtual void Block()
		{
			this.completeEvent.Wait();
			this.completeEvent.Dispose();
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
				if (disposing && this.completeEvent != null)
				{
					this.completeEvent.Dispose();
				}
				this._disposed = true;
			}
		}

		public virtual CimResponseType GetResponse()
		{
			this.Block();
			return this.responseType;
		}

		internal virtual void OnComplete()
		{
			this.completeEvent.Set();
		}
	}
}