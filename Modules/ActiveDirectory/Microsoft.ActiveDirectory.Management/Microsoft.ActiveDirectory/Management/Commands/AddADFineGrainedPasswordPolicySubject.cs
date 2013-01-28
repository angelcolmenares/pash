using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Add", "ADFineGrainedPasswordPolicySubject", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219289", SupportsShouldProcess=true)]
	public class AddADFineGrainedPasswordPolicySubject : SetADFineGrainedPasswordPolicySubject<AddADFineGrainedPasswordPolicySubjectParameterSet>
	{
		public AddADFineGrainedPasswordPolicySubject() : base(0)
		{
		}
	}
}