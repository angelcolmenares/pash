using System.Diagnostics.CodeAnalysis;

namespace Microsoft.WSMan.Management
{
	[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
	[SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
	public enum ProxyAuthentication
	{
		Negotiate = 1,
		Basic = 2,
		Digest = 4
	}
}