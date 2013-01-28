using Microsoft.ActiveDirectory.Management;
using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	public abstract class ADSetDomainCmdletBase<P, F, O> : ADSetCmdletBase<P, F, O>
	where P : ADParameterSet, new()
	where F : ADFactory<O>, new()
	where O : ADEntity, new()
	{
		public ADSetDomainCmdletBase()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return this.GetRootDSE().DefaultNamingContext;
		}

		internal override ADSessionInfo GetSessionInfo()
		{
			return ADDomainUtil.ConstructSessionFromIdentity<P, ADDomain>(this, base.GetSessionInfo(), true);
		}
	}
}