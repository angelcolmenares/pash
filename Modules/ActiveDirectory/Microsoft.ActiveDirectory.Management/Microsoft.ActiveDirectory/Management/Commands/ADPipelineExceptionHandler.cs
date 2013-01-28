using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class ADPipelineExceptionHandler : IADExceptionFilter
	{
		private PSCmdlet _cmdlet;

		public ADPipelineExceptionHandler(PSCmdlet cmdletInstance)
		{
			this._cmdlet = cmdletInstance;
		}

		bool Microsoft.ActiveDirectory.Management.Commands.IADExceptionFilter.FilterException(Exception e, ref bool isTerminating)
		{
			if (this._cmdlet.MyInvocation.ExpectingInput)
			{
				return false;
			}
			else
			{
				isTerminating = true;
				return true;
			}
		}
	}
}