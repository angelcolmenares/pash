using System;

namespace Microsoft.DnsClient.Commands
{
	public class DnsRecord_NSEC3 : DnsRecord
	{
		public byte Algorithm;

		public byte Flags;

		public ushort Iterations;

		public byte SaltLength;

		public byte HashLength;

		public ushort TypeBitMapsLength;

		public byte[] Data;

		public DnsRecord_NSEC3()
		{
		}
	}
}