using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("CE61E841-65BC-11D0-B6BD-00AA003240C7")]
	[InterfaceType(1)]
	[TypeLibType(0x200)]
	internal interface IWbemPropertyProvider
	{
		int GetProperty_(int lFlags, string strLocale, string strClassMapping, string strInstMapping, string strPropMapping, out object pvValue);

		int PutProperty_(int lFlags, string strLocale, string strClassMapping, string strInstMapping, string strPropMapping, ref object pvValue);
	}
}