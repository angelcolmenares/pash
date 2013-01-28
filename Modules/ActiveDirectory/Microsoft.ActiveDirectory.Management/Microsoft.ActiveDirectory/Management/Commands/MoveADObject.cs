using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Move", "ADObject", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219320", SupportsShouldProcess=true)]
	public class MoveADObject : ADMoveCmdletBase<MoveADObjectParameterSet, ADObjectFactory<ADObject>, ADObject>
	{
		public MoveADObject()
		{
		}
	}
}