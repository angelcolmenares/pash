using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("87A5AD68-A38A-43ef-ACA9-EFE910E5D24C")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IWmiEventSource
	{
		void Indicate(IntPtr pIWbemClassObject);

		void SetStatus(int lFlags, int hResult, string strParam, IntPtr pObjParam);
	}
}