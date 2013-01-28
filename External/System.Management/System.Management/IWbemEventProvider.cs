using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("E245105B-B06E-11D0-AD61-00C04FD8FDFF")]
	[InterfaceType(1)]
	[TypeLibType(0x200)]
	internal interface IWbemEventProvider
	{
		int ProvideEvents_(IWbemObjectSink pSink, int lFlags);
	}
}