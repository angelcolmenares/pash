using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Clear", "ADAccountExpiration", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219293", SupportsShouldProcess=true)]
	public class ClearADAccountExpiration : ADSetCmdletBase<ClearADAccountExpirationParameterSet, ADAccountFactory<ADAccount>, ADAccount>
	{
		public ClearADAccountExpiration()
		{
			this._cmdletParameters["AccountExpirationDate"] = new DateTime(0x89f7ff5f7b58000L, DateTimeKind.Utc);
		}
	}
}