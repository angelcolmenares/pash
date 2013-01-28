using Microsoft.Management.Infrastructure;
using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class InvocationContext
	{
		internal string ComputerName
		{
			get;set;
		}


		internal CimInstance TargetCimInstance
		{
			get;set;
		}

		internal InvocationContext(CimSessionProxy proxy)
		{
			if (proxy != null)
			{
				this.ComputerName = proxy.CimSession.ComputerName;
				this.TargetCimInstance = proxy.TargetCimInstance;
			}
		}

		internal InvocationContext(string computerName, CimInstance targetCimInstance)
		{
			this.ComputerName = computerName;
			this.TargetCimInstance = targetCimInstance;
		}
	}
}