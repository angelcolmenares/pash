using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("631F7D97-D993-11D2-B339-00105A1F4AAF")]
	[InterfaceType(1)]
	[TypeLibType(0x200)]
	internal interface IWbemProviderIdentity
	{
		int SetRegistrationObject_(int lFlags, IWbemClassObject_DoNotMarshal pProvReg);
	}
}