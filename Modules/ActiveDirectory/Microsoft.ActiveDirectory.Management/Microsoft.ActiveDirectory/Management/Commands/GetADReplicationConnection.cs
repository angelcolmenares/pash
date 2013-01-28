using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADReplicationConnection", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216352", DefaultParameterSetName="Filter")]
	public class GetADReplicationConnection : ADGetCmdletBase<GetADReplicationConnectionParameterSet, ADReplicationConnectionFactory<ADReplicationConnection>, ADReplicationConnection>
	{
		private string queryPath;

		public GetADReplicationConnection()
		{
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(this.ADGetConnectionCmdletCalculateFilterCSRoutine));
		}

		private bool ADGetConnectionCmdletCalculateFilterCSRoutine()
		{
			if (this._cmdletParameters["Identity"] == null)
			{
				if (this._cmdletParameters["Filter"] == null)
				{
					CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
					this.queryPath = cmdletSessionInfo.ADRootDSE.DSServiceName;
					this._cmdletParameters["Filter"] = "objectClass -like \"*\"";
					return true;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return true;
			}
		}

		private string GetConnectionContainerPath()
		{
			if (this.queryPath != null)
			{
				return this.queryPath;
			}
			else
			{
				return ADPathModule.MakePath(this.GetRootDSE().ConfigurationNamingContext, "CN=Sites,", ADPathFormat.X500);
			}
		}

		protected internal override string GetDefaultPartitionPath()
		{
			return this.GetConnectionContainerPath();
		}

		protected internal override string GetDefaultQueryPath()
		{
			return this.GetConnectionContainerPath();
		}
	}
}