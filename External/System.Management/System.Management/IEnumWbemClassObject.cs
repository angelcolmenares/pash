using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("027947E1-D731-11CE-A357-000000000001")]
	[InterfaceType(1)]
	[TypeLibType(0x200)]
	internal interface IEnumWbemClassObject
	{
		int Clone_(out IEnumWbemClassObject ppEnum);

		int Next_(int lTimeout, int uCount, IWbemClassObject_DoNotMarshal[] apObjects, out uint puReturned);

		int NextAsync_(uint uCount, IWbemObjectSink pSink);

		int Reset_();

		int Skip_(int lTimeout, uint nCount);
	}
}