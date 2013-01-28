using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("44ACA675-E8FC-11D0-A07C-00C04FB68820")]
	[InterfaceType(1)]
	[TypeLibType(0x200)]
	internal interface IWbemCallResult
	{
		int GetCallStatus_(int lTimeout, out int plStatus);

		int GetResultObject_(int lTimeout, out IWbemClassObjectFreeThreaded ppResultObject);

		int GetResultServices_(int lTimeout, out IWbemServices ppServices);

		int GetResultString_(int lTimeout, out string pstrResultString);
	}
}