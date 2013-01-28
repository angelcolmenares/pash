using Microsoft.ActiveDirectory.Management;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADComputer", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219301", DefaultParameterSetName="Filter")]
	public class GetADComputer : ADGetCmdletBase<GetADComputerParameterSet, ADComputerFactory<ADComputer>, ADComputer>
	{
		public GetADComputer()
		{
		}
	}
}