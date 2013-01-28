using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Remove", "ADFineGrainedPasswordPolicySubject", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216391", SupportsShouldProcess=true, ConfirmImpact=ConfirmImpact.High)]
	public class RemoveADFineGrainedPasswordPolicySubject : SetADFineGrainedPasswordPolicySubject<RemoveADFineGrainedPasswordPolicySubjectParameterSet>
	{
		public RemoveADFineGrainedPasswordPolicySubject() : base((SetSubjectOperationType)1)
		{

		}
	}
}