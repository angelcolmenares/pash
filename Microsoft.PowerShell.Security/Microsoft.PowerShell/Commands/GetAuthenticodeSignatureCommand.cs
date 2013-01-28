using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Get", "AuthenticodeSignature", DefaultParameterSetName="ByPath", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113307")]
	[OutputType(new Type[] { typeof(Signature) })]
	public sealed class GetAuthenticodeSignatureCommand : SignatureCommandsBase
	{
		public GetAuthenticodeSignatureCommand() : base("Get-AuthenticodeSignature")
		{
		}

		protected override Signature PerformAction(string filePath)
		{
			return SignatureHelper.GetSignature(filePath, null);
		}
	}
}