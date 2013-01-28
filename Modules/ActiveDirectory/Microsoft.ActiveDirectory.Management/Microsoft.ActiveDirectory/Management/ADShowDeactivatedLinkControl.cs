using System.DirectoryServices.Protocols;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADShowDeactivatedLinkControl : DirectoryControl
	{
		public ADShowDeactivatedLinkControl() : base("1.2.840.113556.1.4.2065", null, false, true)
		{
		}
	}
}