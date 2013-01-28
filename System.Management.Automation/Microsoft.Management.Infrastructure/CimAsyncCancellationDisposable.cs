using System;
using System.Security;

namespace Microsoft.Management.Infrastructure.Internal.Operations
{
	internal sealed class CimAsyncCancellationDisposable : IDisposable
	{
		private readonly CimOperation _operation;

		private bool _disposed;

		private readonly object _disposeThreadSafetyLock;

		private readonly SecurityContext _securityContext;

		internal CimAsyncCancellationDisposable(CimOperation operation)
		{
			this._disposeThreadSafetyLock = new object();
			this._securityContext = SecurityContext.Capture();
			this._operation = operation;
		}

		public void Dispose()
		{
			lock (this._disposeThreadSafetyLock)
			{
				if (!this._disposed)
				{
					this._disposed = true;
				}
				else
				{
					return;
				}
			}
			SecurityContext.Run(this._securityContext, (object argument0) => this._operation.Cancel(CancellationMode.SilentlyStopProducingResults), null);
			this._securityContext.Dispose();
		}
	}
}