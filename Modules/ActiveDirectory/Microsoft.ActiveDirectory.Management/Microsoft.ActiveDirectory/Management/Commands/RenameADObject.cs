using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Rename", "ADObject", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219341", SupportsShouldProcess=true)]
	public class RenameADObject : ADRenameCmdletBase<RenameADObjectParameterSet, ADObjectFactory<ADObject>, ADObject>
	{
		public RenameADObject()
		{
		}

		protected internal override string GenerateRDNPrefix(ADObjectFactory<ADObject> factory, ADParameterSet cmdletParameters, RenameADObjectParameterSet dynamicParameters, string oldDN)
		{
			return oldDN.Substring(0, ADPathHelper.IndexOfFirstDelimiter(oldDN, '=', '\\'));
		}
	}
}