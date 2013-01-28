using System;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public interface IEtwEventCorrelator
	{
		Guid CurrentActivityId
		{
			get;
			set;
		}

		IEtwActivity StartActivity(Guid relatedActivityId);

		IEtwActivity StartActivity();
	}
}