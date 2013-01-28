using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	[Guid("B7B31DF9-D515-11D3-A11C-00105A1F515A")]
	[InterfaceType(1)]
	internal interface IWbemShutdown
	{
		int Shutdown_(int uReason, uint uMaxMilliseconds, IWbemContext pCtx);
	}
}