using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal interface IADExceptionFilter
	{
		bool FilterException(Exception e, ref bool isTerminating);
	}
}