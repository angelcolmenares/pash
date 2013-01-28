using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADPrincipalGroupMembership", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219309", DefaultParameterSetName="Identity")]
	public class GetADPrincipalGroupMembership : ADCmdletBase<GetADPrincipalGroupMembershipParameterSet>, IADErrorTarget
	{
		private const string _debugCategory = "GetADPrincipalGroupMembership";

		private ADPrincipal _identityADPrincipal;

		private string _partitionPath;

		private string _resourceContextServer;

		private string _resourceContextPartition;

		public GetADPrincipalGroupMembership()
		{
			base.BeginProcessPipeline.InsertAtStart(new CmdletSubroutine(base.GetADCmdletBaseExternalDelegates().AddSessionOptionWritableDCRequiredCSRoutine));
			base.BeginProcessPipeline.InsertAtEnd(new CmdletSubroutine(this.GetGroupMembershipBeginCSRoutine));
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.GetGroupMembershipProcessCSRoutine));
		}

		private bool GetGroupMembershipBeginCSRoutine()
		{
			this._resourceContextPartition = this._cmdletParameters["ResourceContextPartition"] as string;
			this._resourceContextServer = this._cmdletParameters["ResourceContextServer"] as string;
			if (!string.IsNullOrEmpty(this._resourceContextPartition) && string.IsNullOrEmpty(this._resourceContextServer))
			{
				base.ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.GetGroupMembershipResourceContextParameterCheck, new object[0])), "1", ErrorCategory.InvalidArgument, this._resourceContextPartition));
			}
			this.ValidateParameters();
			return true;
		}

		private bool GetGroupMembershipProcessCSRoutine()
		{
			this._partitionPath = this._cmdletParameters["Partition"] as string;
			this._identityADPrincipal = this._cmdletParameters["Identity"] as ADPrincipal;
			base.SetPipelinedSessionInfo(this._identityADPrincipal.SessionInfo);
			CmdletSessionInfo cmdletSessionInfo = base.GetCmdletSessionInfo();
			ADPrincipalFactory<ADPrincipal> aDPrincipalFactory = new ADPrincipalFactory<ADPrincipal>();
			aDPrincipalFactory.SetCmdletSessionInfo(cmdletSessionInfo);
			this.ValidateParameters();
			ADObject directoryObjectFromIdentity = aDPrincipalFactory.GetDirectoryObjectFromIdentity(this._identityADPrincipal, cmdletSessionInfo.DefaultPartitionPath);
			using (ADAccountManagement aDAccountManagement = new ADAccountManagement(cmdletSessionInfo.ADSessionInfo))
			{
				if (!string.IsNullOrEmpty(this._resourceContextServer) && string.IsNullOrEmpty(this._resourceContextPartition))
				{
					ADSessionInfo aDSessionInfo = cmdletSessionInfo.ADSessionInfo.Copy();
					aDSessionInfo.Server = this._resourceContextServer;
					using (ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(aDSessionInfo))
					{
						ADRootDSE rootDSE = aDObjectSearcher.GetRootDSE();
						if (rootDSE.DefaultNamingContext != null)
						{
							this._resourceContextPartition = rootDSE.DefaultNamingContext;
						}
						else
						{
							object[] objArray = new object[1];
							objArray[0] = "ResourceContextPartition";
							base.ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequired, objArray)), "1", ErrorCategory.InvalidArgument, null));
						}
					}
				}
				ADGroup[] principalGroupMembership = aDAccountManagement.GetPrincipalGroupMembership(cmdletSessionInfo.DefaultPartitionPath, directoryObjectFromIdentity.DistinguishedName, this._resourceContextServer, this._resourceContextPartition);
				ADGroup[] aDGroupArray = principalGroupMembership;
				for (int i = 0; i < (int)aDGroupArray.Length; i++)
				{
					ADGroup aDGroup = aDGroupArray[i];
					base.WriteObject(aDGroup);
				}
			}
			return true;
		}

		object Microsoft.ActiveDirectory.Management.Commands.IADErrorTarget.CurrentIdentity(Exception e)
		{
			return this._identityADPrincipal;
		}

		protected internal virtual void ValidateParameters()
		{
			this.GetCmdletSessionInfo();
			if (!string.IsNullOrEmpty(this.GetDefaultPartitionPath()))
			{
				return;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = "Partition";
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ParameterRequired, objArray));
			}
		}
	}
}