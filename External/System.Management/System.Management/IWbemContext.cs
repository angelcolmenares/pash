using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("44ACA674-E8FC-11D0-A07C-00C04FB68820")]
	[InterfaceType(1)]
	[TypeLibType(0x200)]
	internal interface IWbemContext
	{
		int BeginEnumeration_(int lFlags);

		int Clone_(out IWbemContext ppNewCopy);

		int DeleteAll_();

		int DeleteValue_(string wszName, int lFlags);

		int EndEnumeration_();

		int GetNames_(int lFlags, out string[] pNames);

		int GetValue_(string wszName, int lFlags, out object pValue);

		int Next_(int lFlags, out string pstrName, out object pValue);

		int SetValue_(string wszName, int lFlags, ref object pValue);
	}
}