using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Add", "ADGroupMember", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219287", SupportsShouldProcess=true)]
	public class AddADGroupMember : SetADGroupMember<AddADGroupMemberParameterSet>
	{
		public AddADGroupMember() : base(0)
		{
		}
	}
}