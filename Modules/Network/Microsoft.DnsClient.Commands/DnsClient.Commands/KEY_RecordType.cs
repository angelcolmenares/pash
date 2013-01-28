using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct KEY_RecordType
	{
		public ushort Flags;

		public byte Protocol;

		public byte Algorithm;

		public IntPtr Key;

	}
}