using System;
using System.Collections;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimSetCimInstanceContext : XOperationContextBase
	{
		private IDictionary property;

		private string parameterSetName;

		private bool passThru;

		internal string ParameterSetName
		{
			get
			{
				return this.parameterSetName;
			}
		}

		internal bool PassThru
		{
			get
			{
				return this.passThru;
			}
		}

		internal IDictionary Property
		{
			get
			{
				return this.property;
			}
		}

		internal CimSetCimInstanceContext(string theNamespace, IDictionary theProperty, CimSessionProxy theProxy, string theParameterSetName, bool passThru)
		{
			this.proxy = theProxy;
			this.property = theProperty;
			this.nameSpace = theNamespace;
			this.parameterSetName = theParameterSetName;
			this.passThru = passThru;
		}
	}
}