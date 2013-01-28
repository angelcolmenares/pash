using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Activities
{
	internal sealed class PSResumableActivityContext
	{
		internal Exception Error
		{
			get;
			set;
		}

		internal bool Failed
		{
			get;
			set;
		}

		internal PowerShellStreams<PSObject, PSObject> Streams
		{
			get;
			set;
		}

		internal bool SupportDisconnectedStreams
		{
			get;
			set;
		}

		internal PSResumableActivityContext(PowerShellStreams<PSObject, PSObject> streams)
		{
			this.Streams = streams;
			this.Error = null;
			this.Failed = false;
			this.SupportDisconnectedStreams = true;
		}
	}
}