using System;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class EtwActivity : IEtwActivity, IDisposable
	{
		private readonly IEtwEventCorrelator _correlator;

		private readonly Guid _oldActivityId;

		private bool _isDisposed;

		public EtwActivity(IEtwEventCorrelator correlator)
		{
			if (correlator != null)
			{
				this._correlator = correlator;
				this._oldActivityId = correlator.CurrentActivityId;
				return;
			}
			else
			{
				throw new ArgumentNullException("correlator");
			}
		}

		public void Dispose()
		{
			if (!this._isDisposed)
			{
				this.RevertCurrentActivityId();
				this._isDisposed = true;
				GC.SuppressFinalize(this);
			}
		}

		public void RevertCurrentActivityId()
		{
			this._correlator.CurrentActivityId = this._oldActivityId;
		}
	}
}