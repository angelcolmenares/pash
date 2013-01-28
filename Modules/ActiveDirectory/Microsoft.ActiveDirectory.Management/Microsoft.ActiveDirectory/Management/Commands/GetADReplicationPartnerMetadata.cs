using Microsoft.ActiveDirectory.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADReplicationPartnerMetadata", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216354", DefaultParameterSetName="Target")]
	public class GetADReplicationPartnerMetadata : ADTargetScopeEnumerationServerCmdletBase<GetADReplicationPartnerMetadataParameterSet, ADReplicationPartnerMetadataFactory<ADReplicationPartnerMetadata>, ADReplicationPartnerMetadata>
	{
		// Fields
		protected internal string[] _propertiesRequested;
		
		// Methods
		public GetADReplicationPartnerMetadata()
		{
			base.BeginProcessPipeline.InsertAtEnd(new CmdletSubroutine(this.GetADReplicationPartnerMetadataBeginCSRoutine));
		}
		
		private bool GetADReplicationPartnerMetadataBeginCSRoutine()
		{
			ADPartnerType? nullable = base._cmdletParameters["PartnerType"] as ADPartnerType?;
			if (((ADPartnerType) nullable) == ADPartnerType.Outbound)
			{
				this._propertiesRequested = new string[] { "msDS-NCReplOutboundNeighbors" };
			}
			else if (((ADPartnerType) nullable) == ADPartnerType.Both)
			{
				this._propertiesRequested = new string[] { "msDS-NCReplInboundNeighbors", "msDS-NCReplOutboundNeighbors" };
			}
			else
			{
				this._propertiesRequested = new string[] { "msDS-NCReplInboundNeighbors" };
			}
			return true;
		}
		
		internal override void PerServerProcessRecord()
		{
			ADObjectFactory<ADObject> factory = new ADObjectFactory<ADObject>();
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			factory.SetCmdletSessionInfo(cmdletSessionInfo);
			base._factory.SetCmdletSessionInfo(cmdletSessionInfo);
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
					identityObj = factory.GetExtendedObjectFromIdentity(identityObj, cmdletSessionInfo.DefaultPartitionPath, this._propertiesRequested);
				}
				catch (Exception exception)
				{
					if (!(exception is ADIdentityNotFoundException) && !(exception is ADReferralException))
					{
						throw exception;
					}
					continue;
				}
				if (this._propertiesRequested.Contains<string>("msDS-NCReplInboundNeighbors"))
				{
					foreach (ADReplicationPartnerMetadata metadata in base._factory.GetExtendedObjectFromDirectoryObject(identityObj, "msDS-NCReplInboundNeighbors", "DS_REPL_NEIGHBOR"))
					{
						base.WriteObject(metadata);
					}
				}
				if (this._propertiesRequested.Contains<string>("msDS-NCReplOutboundNeighbors"))
				{
					foreach (ADReplicationPartnerMetadata metadata2 in base._factory.GetExtendedObjectFromDirectoryObject(identityObj, "msDS-NCReplOutboundNeighbors", "DS_REPL_NEIGHBOR"))
					{
						base.WriteObject(metadata2);
					}
				}
			}
		}
	}

}