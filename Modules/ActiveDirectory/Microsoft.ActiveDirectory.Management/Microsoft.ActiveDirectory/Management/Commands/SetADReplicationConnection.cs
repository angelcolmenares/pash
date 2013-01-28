using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Set", "ADReplicationConnection", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216413", SupportsShouldProcess=true)]
	public class SetADReplicationConnection : ADSetCmdletBase<SetADReplicationConnectionParameterSet, ADReplicationConnectionFactory<ADReplicationConnection>, ADReplicationConnection>
	{
		private const string _debugCategory = "SetADReplicationConnection";

		public SetADReplicationConnection()
		{
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Sites,", ADPathFormat.X500);
		}
	}
}