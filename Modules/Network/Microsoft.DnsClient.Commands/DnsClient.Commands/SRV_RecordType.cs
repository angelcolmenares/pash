using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct SRV_RecordType
	{
		public IntPtr NameTarget;

		public ushort Priority;

		public ushort Weight;

		public ushort Port;

		public ushort Pad;

	}
}