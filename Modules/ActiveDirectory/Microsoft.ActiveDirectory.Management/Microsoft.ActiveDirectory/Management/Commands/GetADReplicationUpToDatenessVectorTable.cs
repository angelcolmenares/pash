using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADReplicationUpToDatenessVectorTable", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216361", DefaultParameterSetName="Target")]
	public class GetADReplicationUpToDatenessVectorTable : ADTargetScopeEnumerationServerCmdletBase<GetADReplicationUpToDatenessVectorTableParameterSet, ADReplicationUpToDatenessVectorTableFactory<ADReplicationUpToDatenessVectorTable>, ADReplicationUpToDatenessVectorTable>
	{
		// Methods
		internal override void PerServerProcessRecord()
		{
			ADObjectFactory<ADObject> factory = new ADObjectFactory<ADObject>();
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			factory.SetCmdletSessionInfo(cmdletSessionInfo);
			base._factory.SetCmdletSessionInfo(cmdletSessionInfo);
			string[] propertiesToFetch = new string[] { "msDS-NCReplCursors" };
			string[] partitionList = base._cmdletParameters["PartitionFilter"] as string[];
			if (partitionList == null)
			{
				partitionList = new string[] { "Default" };
			}
			foreach (string str in ADForestPartitionInfo.ConstructPartitionList(cmdletSessionInfo.ADRootDSE, partitionList, false))
			{
				ADObject identityObj = new ADObject(str);
				try
				{
					identityObj = factory.GetExtendedObjectFromIdentity(identityObj, cmdletSessionInfo.DefaultPartitionPath, propertiesToFetch);
				}
				catch (Exception exception)
				{
					if (!(exception is ADIdentityNotFoundException) && !(exception is ADReferralException))
					{
						throw exception;
					}
					continue;
				}
				foreach (ADReplicationUpToDatenessVectorTable table in base._factory.GetExtendedObjectFromDirectoryObject(identityObj, "msDS-NCReplCursors", "DS_REPL_CURSOR"))
				{
					base.WriteObject(table);
				}
			}
		}
	}

}