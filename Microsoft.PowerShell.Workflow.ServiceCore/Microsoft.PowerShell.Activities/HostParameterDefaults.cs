using System;
using System.Collections.Generic;

namespace Microsoft.PowerShell.Activities
{
	public sealed class HostParameterDefaults : IDisposable
	{
		public Action<object> ActivateDelegate
		{
			get;
			set;
		}

		public Dictionary<string, PSActivityContext> AsyncExecutionCollection
		{
			get;
			set;
		}

		public HostSettingCommandMetadata HostCommandMetadata
		{
			get;
			set;
		}

		public Func<bool> HostPersistenceDelegate
		{
			get;
			set;
		}

		public Guid JobInstanceId
		{
			get;
			set;
		}

		public Dictionary<string, object> Parameters
		{
			get;
			set;
		}

		public PSWorkflowHost Runtime
		{
			get;
			set;
		}

		public HostParameterDefaults()
		{
			this.Parameters = new Dictionary<string, object>();
			this.HostCommandMetadata = new HostSettingCommandMetadata();
			this.Runtime = null;
			this.HostPersistenceDelegate = null;
			this.ActivateDelegate = null;
			this.AsyncExecutionCollection = null;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.Parameters = null;
				this.ActivateDelegate = null;
				this.AsyncExecutionCollection = null;
				this.HostPersistenceDelegate = null;
				this.Runtime = null;
				return;
			}
			else
			{
				return;
			}
		}
	}
}