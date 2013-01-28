using Microsoft.ActiveDirectory.Management;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public class ADFactoryCmdletBase<P, F, O> : ADCmdletBase<P>, IDynamicParameters
	where P : ADParameterSet, new()
	where F : ADXmlAttributeFactory<O>, new()
	where O : ADEntity, new()
	{
		protected internal F _factory;

		public ADFactoryCmdletBase()
		{
			this._factory = Activator.CreateInstance<F>();
		}
	}
}