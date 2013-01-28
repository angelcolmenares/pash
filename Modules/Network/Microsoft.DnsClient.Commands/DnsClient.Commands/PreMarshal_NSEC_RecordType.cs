using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct PreMarshal_NSEC_RecordType
	{
		public IntPtr NextDomainName;

		public ushort BitMapLength;

		public ushort Pad;

	}
}