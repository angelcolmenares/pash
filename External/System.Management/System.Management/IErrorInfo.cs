using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("1CF2B120-547D-101B-8E65-08002B2BD119")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IErrorInfo
	{
		string GetDescription();

		Guid GetGUID();

		uint GetHelpContext();

		string GetHelpFile();

		string GetSource();
	}
}