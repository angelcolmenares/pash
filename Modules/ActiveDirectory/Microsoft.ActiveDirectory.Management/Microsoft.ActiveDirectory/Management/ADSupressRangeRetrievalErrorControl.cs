using System.DirectoryServices.Protocols;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADSupressRangeRetrievalErrorControl : DirectoryControl
	{
		public ADSupressRangeRetrievalErrorControl() : base("1.2.840.113556.1.4.1948", null, false, true)
		{
		}
	}
}