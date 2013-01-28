using System;

namespace Microsoft.PowerShell.Commands
{
	public struct EnhancedKeyUsageRepresentation
	{
		private string friendlyName;

		private string oid;

		public string FriendlyName
		{
			get
			{
				return this.friendlyName;
			}
		}

		public string ObjectId
		{
			get
			{
				return this.oid;
			}
		}

		public EnhancedKeyUsageRepresentation(string inputFriendlyName, string inputOid)
		{
			this.friendlyName = inputFriendlyName;
			this.oid = inputOid;
		}

		public bool Equals(EnhancedKeyUsageRepresentation keyUsage)
		{
			bool flag = false;
			if (this.oid == null || keyUsage.oid == null)
			{
				if (this.oid == null && keyUsage.oid == null)
				{
					flag = true;
				}
			}
			else
			{
				if (string.Equals(this.oid, keyUsage.oid, StringComparison.Ordinal))
				{
					flag = true;
				}
			}
			return flag;
		}

		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.friendlyName))
			{
				return this.oid;
			}
			else
			{
				return string.Concat(this.friendlyName, " (", this.oid, ")");
			}
		}
	}
}