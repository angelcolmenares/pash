using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Internal;
using Microsoft.Management.Infrastructure.Native;
using System;
using System.Threading;

namespace Microsoft.Management.Infrastructure.Internal.Operations
{
	internal class CimOperation : IDisposable
	{
		private OperationHandle _handle;

		private IDisposable _cancellationTokenRegistration;

		private readonly object _cancellationModeLock;

		private readonly object _cancelVsCloseLock;

		private CancellationMode _cancellationMode;

		private bool _disposed;

		internal CancellationMode CancellationMode
		{
			get
			{
				CancellationMode cancellationMode;
				lock (this._cancellationModeLock)
				{
					cancellationMode = this._cancellationMode;
				}
				return cancellationMode;
			}
		}

		internal OperationHandle Handle
		{
			get
			{
				this.AssertNotDisposed();
				return this._handle;
			}
		}

		internal CimOperation(OperationHandle handle, CancellationToken? cancellationToken)
		{
			Action action = null;
			this._cancellationModeLock = new object();
			this._cancelVsCloseLock = new object();
			this._handle = handle;
			if (cancellationToken.HasValue)
			{
				CimOperation cimOperation = this;
				CancellationToken value = cancellationToken.Value;
				CancellationToken cancellationTokenPointer = value;
				if (action == null)
				{
					action = () => this.Cancel(CancellationMode.ThrowOperationCancelledException);
				}
				cimOperation._cancellationTokenRegistration = cancellationTokenPointer.Register(action);
			}
			CimApplication.AddTracking(this);
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

		internal void Cancel(CancellationMode cancellationMode)
		{
			lock (this._cancellationModeLock)
			{
				if (this._cancellationMode != CancellationMode.IgnoreCancellationRequests)
				{
					this._cancellationMode = cancellationMode;
				}
				else
				{
					return;
				}
			}
			lock (this._cancelVsCloseLock)
			{
				if (this._handle != null)
				{
					MiResult miResult = OperationMethods.Cancel(this._handle, MiCancellationReason.None);
					CimException.ThrowIfMiResultFailure(miResult);
				}
			}
			this.Cancelled.SafeInvoke<EventArgs>(this, EventArgs.Empty);
		}

		private void Close()
		{
			lock (this._cancelVsCloseLock)
			{
				this._handle.Close();
				this._handle = null;
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
					this.Close();
					if (this._cancellationTokenRegistration != null)
					{
						this._cancellationTokenRegistration.Dispose();
						this._cancellationTokenRegistration = null;
					}
					CimApplication.RemoveTracking(this);
				}
				this._disposed = true;
				return;
			}
			else
			{
				return;
			}
		}

		internal void IgnoreSubsequentCancellationRequests()
		{
			lock (this._cancellationModeLock)
			{
				if (this._cancellationMode == CancellationMode.NoCancellationOccured)
				{
					this._cancellationMode = CancellationMode.IgnoreCancellationRequests;
				}
			}
		}

		internal event EventHandler<EventArgs> Cancelled;
	}
}