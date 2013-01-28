using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("3AE0080A-7E3A-4366-BF89-0FEEDC931659")]
	[InterfaceType(1)]
	[TypeLibType(0x200)]
	internal interface IWbemEventSink
	{
		int GetRestrictedSink_(int lNumQueries, ref string awszQueries, object pCallback, out IWbemEventSink ppSink);

		int Indicate_(int lObjectCount, ref IWbemClassObject_DoNotMarshal apObjArray);

		int IndicateWithSD_(int lNumObjects, ref object apObjects, int lSDLength, ref byte pSD);

		int IsActive_();

		int SetBatchingParameters_(int lFlags, uint dwMaxBufferSize, uint dwMaxSendLatency);

		int SetSinkSecurity_(int lSDLength, ref byte pSD);

		int SetStatus_(int lFlags, int hResult, string strParam, IWbemClassObject_DoNotMarshal pObjParam);
	}
}