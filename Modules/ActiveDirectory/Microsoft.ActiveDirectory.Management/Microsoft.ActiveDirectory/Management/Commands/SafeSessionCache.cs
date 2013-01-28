using Microsoft.ActiveDirectory.Management;
using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class SafeSessionCache : IDisposable
	{
		private ADSession _session;

		private SafeSessionCache()
		{
		}

		public SafeSessionCache(ADSessionInfo info)
		{
			this._session = ADSession.ConstructSession(info);
		}

		public SafeSessionCache(ADSession session)
		{
			this._session = session;
		}

		private void Dispose (bool disposing)
		{
			if (!disposing) {
				GC.SuppressFinalize (this);
			}
			if (this._session != null) {
				this._session.Delete ();
			}
		}

		public void Dispose()
		{
			this.Dispose(false);
		}

		~SafeSessionCache()
		{
			try
			{
				this.Dispose(true);
			}
			finally
			{
				//this.Finalize();
			}
		}
	}
}