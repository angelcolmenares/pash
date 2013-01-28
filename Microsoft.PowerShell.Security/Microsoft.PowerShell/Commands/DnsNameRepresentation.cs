using System;

namespace Microsoft.PowerShell.Commands
{
	public struct DnsNameRepresentation
	{
		private string punycodeName;

		private string unicodeName;

		public string Punycode
		{
			get
			{
				return this.punycodeName;
			}
		}

		public string Unicode
		{
			get
			{
				return this.unicodeName;
			}
		}

		public DnsNameRepresentation(string inputDnsName)
		{
			this.punycodeName = inputDnsName;
			this.unicodeName = inputDnsName;
		}

		public DnsNameRepresentation(string inputPunycodeName, string inputUnicodeName)
		{
			this.punycodeName = inputPunycodeName;
			this.unicodeName = inputUnicodeName;
		}

		public bool Equals(DnsNameRepresentation dnsName)
		{
			bool flag = false;
			if (this.unicodeName == null || dnsName.unicodeName == null)
			{
				if (this.unicodeName == null && dnsName.unicodeName == null)
				{
					flag = true;
				}
			}
			else
			{
				if (string.Equals(this.unicodeName, dnsName.unicodeName, StringComparison.OrdinalIgnoreCase))
				{
					flag = true;
				}
			}
			return flag;
		}

		public override string ToString()
		{
			if (string.Equals(this.punycodeName, this.unicodeName))
			{
				return this.punycodeName;
			}
			else
			{
				return string.Concat(this.unicodeName, " (", this.punycodeName, ")");
			}
		}
	}
}