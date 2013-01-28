using Microsoft.ActiveDirectory.Management;
using System;
using System.Security.Authentication;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class ADSystemExceptionHandler : IADExceptionFilter
	{
		public ADSystemExceptionHandler()
		{
		}

		bool Microsoft.ActiveDirectory.Management.Commands.IADExceptionFilter.FilterException(Exception e, ref bool isTerminating)
		{
			bool flag = false;
			if (e as SystemException == null)
			{
				isTerminating = false;
				flag = true;
			}
			else
			{
				if (e as ADFilterParsingException != null || e as ArgumentException != null || e as AuthenticationException != null || e as ArgumentNullException != null || e as ArgumentOutOfRangeException != null || e as ADInvalidOperationException != null)
				{
					isTerminating = false;
					flag = true;
				}
			}
			return flag;
		}
	}
}