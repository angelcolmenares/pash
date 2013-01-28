using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("DC12A687-737F-11CF-884D-00AA004B2E24")]
	[InterfaceType(1)]
	[TypeLibType(0x200)]
	internal interface IWbemLocator
	{
		int ConnectServer_(string strNetworkResource, string strUser, IntPtr strPassword, string strLocale, int lSecurityFlags, string strAuthority, IWbemContext pCtx, out IWbemServices ppNamespace);
	}
}