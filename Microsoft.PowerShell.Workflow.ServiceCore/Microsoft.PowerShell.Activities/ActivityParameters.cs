using System;

namespace Microsoft.PowerShell.Activities
{
	internal class ActivityParameters
	{
		internal uint? ActionRetryCount
		{
			get;
			private set;
		}

		internal uint? ActionRetryInterval
		{
			get;
			private set;
		}

		internal uint? ConnectionRetryCount
		{
			get;
			private set;
		}

		internal uint? ConnectionRetryInterval
		{
			get;
			private set;
		}

		internal string[] PSRequiredModules
		{
			get;
			private set;
		}

		internal ActivityParameters(uint? connectionRetryCount, uint? connectionRetryInterval, uint? actionRetryCount, uint? actionRetryInterval, string[] requiredModule)
		{
			this.ConnectionRetryCount = connectionRetryCount;
			this.ConnectionRetryInterval = connectionRetryInterval;
			this.ActionRetryCount = actionRetryCount;
			this.ActionRetryInterval = actionRetryInterval;
			this.PSRequiredModules = requiredModule;
		}
	}
}