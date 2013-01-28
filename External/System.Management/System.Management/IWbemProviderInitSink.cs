using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("1BE41571-91DD-11D1-AEB2-00C04FB68820")]
	[InterfaceType(1)]
	internal interface IWbemProviderInitSink
	{
		int SetStatus_(int lStatus, int lFlags);
	}
}