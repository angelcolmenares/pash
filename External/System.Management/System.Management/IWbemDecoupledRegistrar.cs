using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("1005CBCF-E64F-4646-BCD3-3A089D8A84B4")]
	[InterfaceType(1)]
	internal interface IWbemDecoupledRegistrar
	{
		int Register_(int flags, IWbemContext context, string user, string locale, string scope, string registration, object unknown);

		int UnRegister_();
	}
}