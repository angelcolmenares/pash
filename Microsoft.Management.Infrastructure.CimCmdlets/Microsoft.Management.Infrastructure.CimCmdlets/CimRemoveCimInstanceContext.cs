using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimRemoveCimInstanceContext : XOperationContextBase
	{
		internal CimRemoveCimInstanceContext(string theNamespace, CimSessionProxy theProxy)
		{
			this.proxy = theProxy;
			this.nameSpace = theNamespace;
		}
	}
}