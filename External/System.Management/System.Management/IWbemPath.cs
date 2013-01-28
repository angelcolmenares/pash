using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("3BC15AF2-736C-477E-9E51-238AF8667DCC")]
	[InterfaceType(1)]
	internal interface IWbemPath
	{
		int CreateClassPart_(int lFlags, string Name);

		int DeleteClassPart_(int lFlags);

		int GetClassName_(out int puBuffLength, string pszName);

		int GetInfo_(uint uRequestedInfo, out ulong puResponse);

		int GetKeyList_(out IWbemPathKeyList pOut);

		int GetNamespaceAt_(uint uIndex, out int puNameBufLength, string pName);

		int GetNamespaceCount_(out uint puCount);

		int GetScope_(uint uIndex, out uint puClassNameBufSize, string pszClass, out IWbemPathKeyList pKeyList);

		int GetScopeAsText_(uint uIndex, out uint puTextBufSize, string pszText);

		int GetScopeCount_(out uint puCount);

		int GetServer_(out int puNameBufLength, string pName);

		int GetText_(int lFlags, out int puBuffLength, string pszText);

		int IsLocal_(string wszMachine);

		int IsRelative_(string wszMachine, string wszNamespace);

		int IsRelativeOrChild_(string wszMachine, string wszNamespace, int lFlags);

		int IsSameClassName_(string wszClass);

		int RemoveAllNamespaces_();

		int RemoveAllScopes_();

		int RemoveNamespaceAt_(uint uIndex);

		int RemoveScope_(uint uIndex);

		int SetClassName_(string Name);

		int SetNamespaceAt_(uint uIndex, string pszName);

		int SetScope_(uint uIndex, string pszClass);

		int SetScopeFromText_(uint uIndex, string pszText);

		int SetServer_(string Name);

		int SetText_(uint uMode, string pszPath);
	}
}