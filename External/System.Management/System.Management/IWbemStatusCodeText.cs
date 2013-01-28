using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("EB87E1BC-3233-11D2-AEC9-00C04FB68820")]
	[InterfaceType(1)]
	internal interface IWbemStatusCodeText
	{
		int GetErrorCodeText_(int hRes, uint LocaleId, int lFlags, out string MessageText);

		int GetFacilityCodeText_(int hRes, uint LocaleId, int lFlags, out string MessageText);
	}
}