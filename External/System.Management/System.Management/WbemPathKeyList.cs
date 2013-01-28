using System;

namespace System.Management
{
	internal class WbemPathKeyList : IWbemPathKeyList
	{
		public WbemPathKeyList ()
		{

		}

		#region IWbemPathKeyList implementation

		public int GetCount_ (out uint puKeyCount)
		{
			puKeyCount = 0;
			return 0;
		}

		public int GetInfo_ (uint uRequestedInfo, out ulong puResponse)
		{
			puResponse = 0;
			return 0;
		}

		public int GetKey_ (uint uKeyIx, uint uFlags, out uint puNameBufSize, string pszKeyName, out uint puKeyValBufSize, IntPtr pKeyVal, out uint puApparentCimType)
		{
			puNameBufSize = 0;
			puKeyValBufSize = 0;
			pKeyVal = IntPtr.Zero;
			puApparentCimType = (uint)CimType.Object;
			return 0;
		}

		public int GetKey2_ (uint uKeyIx, uint uFlags, out uint puNameBufSize, string pszKeyName, out object pKeyValue, out uint puApparentCimType)
		{
			puNameBufSize = 0;
			pKeyValue = null;
			puApparentCimType = (uint)CimType.Object;
			return 0;
		}

		public int GetText_ (int lFlags, out uint puBuffLength, string pszText)
		{
			puBuffLength = 0;
			return 0;
		}

		public int MakeSingleton_ (sbyte bSet)
		{
			return 0;
		}

		public int RemoveAllKeys_ (uint uFlags)
		{
			return 0;
		}

		public int RemoveKey_ (string wszName, uint uFlags)
		{
			return 0;
		}

		public int SetKey_ (string wszName, uint uFlags, uint uCimType, IntPtr pKeyVal)
		{
			return 0;
		}

		public int SetKey2_ (string wszName, uint uFlags, uint uCimType, ref object pKeyVal)
		{
			return 0;
		}

		#endregion
	}
}

