using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class ADStoreKey : StoreKey
	{
		private Guid objectGuid;

		private bool wellKnownSid;

		private string domainName;

		private byte[] sid;

		public ADStoreKey(Guid guid)
		{
			this.objectGuid = guid;
			this.wellKnownSid = false;
		}

		public ADStoreKey(string domainName, byte[] sid)
		{
			this.sid = new byte[(int)sid.Length];
			Array.Copy(sid, this.sid, (int)sid.Length);
			this.domainName = domainName;
			this.wellKnownSid = true;
		}

		public override bool Equals(object o)
		{
			if (o as ADStoreKey != null)
			{
				ADStoreKey aDStoreKey = (ADStoreKey)o;
				if (this.wellKnownSid == aDStoreKey.wellKnownSid)
				{
					if (this.wellKnownSid)
					{
						if (string.Compare(this.domainName, aDStoreKey.domainName, StringComparison.OrdinalIgnoreCase) == 0 && Utils.AreBytesEqual(this.sid, aDStoreKey.sid))
						{
							return true;
						}
					}
					else
					{
						if (this.objectGuid == aDStoreKey.objectGuid)
						{
							return true;
						}
					}
					return false;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			if (!this.wellKnownSid)
			{
				return this.objectGuid.GetHashCode();
			}
			else
			{
				return this.domainName.GetHashCode() ^ this.sid.GetHashCode();
			}
		}
	}
}