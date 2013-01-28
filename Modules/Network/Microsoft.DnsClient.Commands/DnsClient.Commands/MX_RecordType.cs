using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct MX_RecordType
	{
		public IntPtr pNameExchange;

		public ushort wPreference;

		public ushort Pad;

	}
}