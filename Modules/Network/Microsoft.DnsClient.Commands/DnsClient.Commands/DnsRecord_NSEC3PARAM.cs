using System;

namespace Microsoft.DnsClient.Commands
{
	public class DnsRecord_NSEC3PARAM : DnsRecord
	{
		public byte Algorithm;

		public byte Flags;

		public ushort Iterations;

		public byte SaltLength;

		public byte[] Salt;

		public DnsRecord_NSEC3PARAM()
		{
		}
	}
}