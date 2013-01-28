using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal interface IADErrorTarget
	{
		object CurrentIdentity(Exception e);
	}
}