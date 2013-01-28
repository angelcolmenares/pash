using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("1CFABA8C-1523-11D1-AD79-00C04FD8FDFF")]
	[InterfaceType(1)]
	[TypeLibType(0x200)]
	internal interface IUnsecuredApartment
	{
		int CreateObjectStub_(object pObject, out object ppStub);
	}
}