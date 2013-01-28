using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("580ACAF8-FA1C-11D0-AD72-00C04FD8FDFF")]
	[InterfaceType(1)]
	[TypeLibType(0x200)]
	internal interface IWbemEventProviderQuerySink
	{
		int CancelQuery_(uint dwId);

		int NewQuery_(uint dwId, string wszQueryLanguage, string wszQuery);
	}
}