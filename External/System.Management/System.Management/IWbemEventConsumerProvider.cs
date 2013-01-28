using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("E246107A-B06E-11D0-AD61-00C04FD8FDFF")]
	[InterfaceType(1)]
	[TypeLibType(0x200)]
	internal interface IWbemEventConsumerProvider
	{
		int FindConsumer_(IWbemClassObject_DoNotMarshal pLogicalConsumer, out IWbemUnboundObjectSink ppConsumer);
	}
}