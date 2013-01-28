using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal abstract class XOperationContextBase
	{
		protected string nameSpace;

		protected CimSessionProxy proxy;

		internal string Namespace
		{
			get
			{
				return this.nameSpace;
			}
		}

		internal CimSessionProxy Proxy
		{
			get
			{
				return this.proxy;
			}
		}

		protected XOperationContextBase()
		{
		}
	}
}