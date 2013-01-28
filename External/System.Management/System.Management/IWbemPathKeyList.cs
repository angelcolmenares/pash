using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("9AE62877-7544-4BB0-AA26-A13824659ED6")]
	[InterfaceType(1)]
	internal interface IWbemPathKeyList
	{
		int GetCount_(out uint puKeyCount);

		int GetInfo_(uint uRequestedInfo, out ulong puResponse);

		int GetKey_(uint uKeyIx, uint uFlags, out uint puNameBufSize, string pszKeyName, out uint puKeyValBufSize, IntPtr pKeyVal, out uint puApparentCimType);

		int GetKey2_(uint uKeyIx, uint uFlags, out uint puNameBufSize, string pszKeyName, out object pKeyValue, out uint puApparentCimType);

		int GetText_(int lFlags, out uint puBuffLength, string pszText);

		int MakeSingleton_(sbyte bSet);

		int RemoveAllKeys_(uint uFlags);

		int RemoveKey_(string wszName, uint uFlags);

		int SetKey_(string wszName, uint uFlags, uint uCimType, IntPtr pKeyVal);

		int SetKey2_(string wszName, uint uFlags, uint uCimType, ref object pKeyVal);
	}
}