using System;
using System.Activities;
using System.ComponentModel;

namespace Microsoft.PowerShell.Activities
{
	public interface IImplementsConnectionRetry
	{
		[BehaviorCategory]
		[DefaultValue(null)]
		InArgument<uint?> PSConnectionRetryCount
		{
			get;
			set;
		}

		[BehaviorCategory]
		[DefaultValue(null)]
		InArgument<uint?> PSConnectionRetryIntervalSec
		{
			get;
			set;
		}

	}
}