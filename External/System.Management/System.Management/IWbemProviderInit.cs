using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("1BE41572-91DD-11D1-AEB2-00C04FB68820")]
	[InterfaceType(1)]
	internal interface IWbemProviderInit
	{
		int Initialize_(string wszUser, int lFlags, string wszNamespace, string wszLocale, IWbemServices pNamespace, IWbemContext pCtx, IWbemProviderInitSink pInitSink);
	}
}