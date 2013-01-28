using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("E246107B-B06E-11D0-AD61-00C04FD8FDFF")]
	[InterfaceType(1)]
	[TypeLibType(0x200)]
	internal interface IWbemUnboundObjectSink
	{
		int IndicateToConsumer_(IWbemClassObject_DoNotMarshal pLogicalConsumer, int lNumObjects, ref IWbemClassObject_DoNotMarshal apObjects);
	}
}