using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimNewCimInstanceContext : XOperationContextBase
	{
		internal CimNewCimInstanceContext(CimSessionProxy theProxy, string theNamespace)
		{
			this.proxy = theProxy;
			this.nameSpace = theNamespace;
		}
	}
}