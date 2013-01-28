using System;
using System.Runtime.InteropServices;

namespace System.Management.Instrumentation
{
	[Guid("809c652e-7396-11d2-9771-00a0c9b4d50c")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[TypeLibType(TypeLibTypeFlags.FRestricted)]
	internal interface IMetaDataDispenser
	{
		object DefineScope(ref Guid rclsid, uint dwCreateFlags, ref Guid riid);

		object OpenScope(string szScope, uint dwOpenFlags, ref Guid riid);

		object OpenScopeOnMemory(IntPtr pData, uint cbData, uint dwOpenFlags, ref Guid riid);
	}
}