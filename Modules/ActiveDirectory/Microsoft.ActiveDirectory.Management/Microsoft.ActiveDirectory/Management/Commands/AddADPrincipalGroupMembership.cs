using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Add", "ADPrincipalGroupMembership", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219288", SupportsShouldProcess=true)]
	public class AddADPrincipalGroupMembership : SetADPrincipalGroupMembership<AddADPrincipalGroupMembershipParameterSet>
	{
		public AddADPrincipalGroupMembership() : base(0)
		{
		}
	}
}