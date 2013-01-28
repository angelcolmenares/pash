using System;

namespace Microsoft.ActiveDirectory.Management
{
	internal interface IHasServerErrorMessage
	{
		string ServerErrorMessage
		{
			get;
		}

	}
}