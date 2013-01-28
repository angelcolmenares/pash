using System;

namespace Microsoft.ActiveDirectory.Management
{
	internal interface IHasErrorCode
	{
		int ErrorCode
		{
			get;
		}

	}
}