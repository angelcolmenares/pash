using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.GenericInvoke;
using System;
using System.Collections.Generic;

namespace Microsoft.Management.Odata.Core
{
	internal class UserData : IDisposable
	{
		private bool disposed;

		private CacheController invokeCacheController;

		public PipelineInvocationCollection CommandInvocations
		{
			get;
			private set;
		}

		public Usage Usage
		{
			get;
			private set;
		}

		public UserData(CacheController invokeCacheController)
		{
			this.Usage = new Usage();
			this.CommandInvocations = new PipelineInvocationCollection();
			DataServiceController.Current.PerfCounters.ActiveUsers.Increment();
			this.invokeCacheController = invokeCacheController;
			this.invokeCacheController.RegisterCache(this.CommandInvocations);
		}

		public void Dispose()
		{
			this.Dispose(true);
		}

		protected void Dispose(bool disposing)
		{
			if (!this.disposed && disposing)
			{
				TraceHelper.Current.DebugMessage(string.Concat("Disposing user data. User data details ", this.ToString()));
				DataServiceController.Current.PerfCounters.ActiveUsers.Decrement();
				if (this.invokeCacheController != null)
				{
					this.invokeCacheController.UnregisterCache(this.CommandInvocations);
					this.invokeCacheController = null;
				}
				foreach (KeyValuePair<Guid, PipelineInvocation> list in this.CommandInvocations.ToList())
				{
					this.CommandInvocations.TryRemove(list.Key);
					list.Value.Dispose();
				}
				GC.SuppressFinalize(this);
				this.disposed = true;
			}
		}

		public override string ToString()
		{
			return string.Concat("UserData - Usage - ", this.Usage.ToString());
		}
	}
}