using System.Diagnostics.CodeAnalysis;

namespace Microsoft.WSMan.Management
{
	[SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
	public enum AuthenticationMechanism
	{
		None = 0,
		Default = 1,
		Digest = 2,
		Negotiate = 4,
		Basic = 8,
		Kerberos = 16,
		ClientCertificate = 32,
		Credssp = 128
	}
}