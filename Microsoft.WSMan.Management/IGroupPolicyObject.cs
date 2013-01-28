using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.WSMan.Management
{
	[Guid("EA502723-A23D-11d1-A7D3-0000F87571E3")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IGroupPolicyObject
	{
		void Delete();

		void GetDisplayName(StringBuilder pszName, int cchMaxLength);

		void GetDSPath(uint dwSection, StringBuilder pszPath, int cchMaxPath);

		void GetFileSysPath(uint dwSection, StringBuilder pszPath, int cchMaxPath);

		void GetMachineName(StringBuilder pszName, int cchMaxLength);

		void GetName(StringBuilder pszName, int cchMaxLength);

		uint GetOptions();

		void GetPath(StringBuilder pszPath, int cchMaxPath);

		uint GetPropertySheetPages(out IntPtr hPages);

		IntPtr GetRegistryKey(uint dwSection);

		void New(string pszDomainName, string pszDisplayName, uint dwFlags);

		void OpenDSGPO(string pszPath, uint dwFlags);

		void OpenLocalMachineGPO(uint dwFlags);

		void OpenRemoteMachineGPO(string pszComputerName, uint dwFlags);

		void Save(bool bMachine, bool bAdd, Guid pGuidExtension, Guid pGuid);

		void SetDisplayName(string pszName);

		void SetOptions(uint dwOptions, uint dwMask);
	}
}