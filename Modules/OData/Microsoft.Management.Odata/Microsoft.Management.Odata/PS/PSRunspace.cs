using Microsoft.Management.Odata;
using Microsoft.Management.Odata.Common;
using Microsoft.Management.Odata.Core;
using System;
using System.Management.Automation.Runspaces;

namespace Microsoft.Management.Odata.PS
{
	internal class PSRunspace : IDisposable
	{
		public Runspace Runspace
		{
			get;
			private set;
		}

		public PSRunspace(InitialSessionState initialSessionState, bool executeCmdletInSameThread = false)
		{
			try
			{
				this.Runspace = RunspaceFactory.CreateRunspace(initialSessionState);
				if (executeCmdletInSameThread)
				{
					this.Runspace.ThreadOptions = PSThreadOptions.UseCurrentThread;
				}
				this.Runspace.Open();
				DataServiceController.Current.PerfCounters.ActiveRunspaces.Increment();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				TraceHelper.Current.FailedToCreateRunspace(exception.ToTraceMessage("Exception"));
				throw new InvalidOperationException(ExceptionHelpers.GetExceptionMessage(exception, Resources.PowerShellConstrainedRunspaceCreationFailed, new object[0]), exception);
			}
		}

		public void Dispose()
		{
			this.Runspace.Dispose();
			DataServiceController.Current.PerfCounters.ActiveRunspaces.Decrement();
			GC.SuppressFinalize(this);
		}
	}
}