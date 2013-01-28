using System;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Management
{
	[Guid("7C857801-7381-11CF-884D-00AA004B2E24")]
	[InterfaceType(1)]
	[SuppressUnmanagedCodeSecurity]
	[TypeLibType(0x200)]
	internal interface IWbemObjectSink
	{
		[SuppressUnmanagedCodeSecurity]
		int Indicate_(int lObjectCount, IntPtr[] apObjArray);

		int SetStatus_(int lFlags, int hResult, string strParam, IntPtr pObjParam);
	}
}