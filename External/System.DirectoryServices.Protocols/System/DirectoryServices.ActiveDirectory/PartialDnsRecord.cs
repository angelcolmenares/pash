using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class PartialDnsRecord
	{
		public IntPtr next;

		public string name;

		public short type;

		public short dataLength;

		public int flags;

		public int ttl;

		public int reserved;

		public IntPtr data;

		public PartialDnsRecord()
		{
		}
	}
}