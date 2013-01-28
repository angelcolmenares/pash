using System;

namespace Microsoft.DnsClient.Commands
{
	internal struct DNS_RECORD_FLAGS
	{
		public int bitvector1;

		public DNSCharset CharSet
		{
			get
			{
				return (DNSCharset)((this.bitvector1 & 24) / 8);
			}
			set
			{
				this.bitvector1 = (int)value * 8 | this.bitvector1;
			}
		}

		public int Delete
		{
			get
			{
				return (this.bitvector1 & 4) / 4;
			}
			set
			{
				this.bitvector1 = value * 4 | this.bitvector1;
			}
		}

		public int Reserved
		{
			get
			{
				return (this.bitvector1 & -256) / 0x100;
			}
			set
			{
				this.bitvector1 = value * 0x100 | this.bitvector1;
			}
		}

		public DNSSection Section
		{
			get
			{
				return (DNSSection)(this.bitvector1 & 3);
			}
			set
			{
				this.bitvector1 = (int)value | this.bitvector1;
			}
		}

		public int Unused
		{
			get
			{
				return (this.bitvector1 & 224) / 32;
			}
			set
			{
				this.bitvector1 = value * 32 | this.bitvector1;
			}
		}

	}
}