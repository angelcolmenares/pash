using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class SAMStoreKey : StoreKey
	{
		private byte[] sid;

		private string machineName;

		public SAMStoreKey(string machineName, byte[] sid)
		{
			this.machineName = machineName;
			this.sid = new byte[(int)sid.Length];
			Array.Copy(sid, this.sid, (int)sid.Length);
		}

		public override bool Equals(object o)
		{
			if (o as SAMStoreKey != null)
			{
				SAMStoreKey sAMStoreKey = (SAMStoreKey)o;
				if (string.Compare(this.machineName, sAMStoreKey.machineName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return Utils.AreBytesEqual(this.sid, sAMStoreKey.sid);
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
			return this.machineName.GetHashCode() ^ this.sid.GetHashCode();
		}
	}
}