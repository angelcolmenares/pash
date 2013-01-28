using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Move", "ADDirectoryServer", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219321", SupportsShouldProcess=true)]
	public class MoveADDirectoryServer : ADCmdletBase<MoveADDirectoryServerParameterSet>, IADErrorTarget
	{
		private const string _debugCategory = "MoveADDirectoryServer";

		private ADDirectoryServer _identityDSObj;

		private ADReplicationSite _inputSiteObj;

		private ADObject _siteDirObj;

		public MoveADDirectoryServer()
		{
			base.BeginProcessPipeline.InsertAtEnd(new CmdletSubroutine(this.MoveADDirectoryServerBeginCSRoutine));
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.MoveADDirectoryServerProcessCSRoutine));
		}

		object Microsoft.ActiveDirectory.Management.Commands.IADErrorTarget.CurrentIdentity(Exception e)
		{
			return this._identityDSObj;
		}

		private bool MoveADDirectoryServerBeginCSRoutine()
		{
			this._inputSiteObj = this._cmdletParameters["Site"] as ADReplicationSite;
			base.SetPipelinedSessionInfo(this._inputSiteObj.SessionInfo);
			CmdletSessionInfo cmdletSessionInfo = base.GetCmdletSessionInfo();
			ADReplicationSiteFactory<ADReplicationSite> aDReplicationSiteFactory = new ADReplicationSiteFactory<ADReplicationSite>();
			aDReplicationSiteFactory.SetCmdletSessionInfo(cmdletSessionInfo);
			this._siteDirObj = aDReplicationSiteFactory.GetDirectoryObjectFromIdentity(this._inputSiteObj, cmdletSessionInfo.ADRootDSE.ConfigurationNamingContext);
			return true;
		}

		private bool MoveADDirectoryServerProcessCSRoutine()
		{
			this._identityDSObj = this._cmdletParameters["Identity"] as ADDirectoryServer;
			base.SetPipelinedSessionInfo(this._identityDSObj.SessionInfo);
			CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
			ADDirectoryServerFactory<ADDirectoryServer> aDDirectoryServerFactory = new ADDirectoryServerFactory<ADDirectoryServer>();
			aDDirectoryServerFactory.SetCmdletSessionInfo(cmdletSessionInfo);
			ADObject directoryObjectFromIdentity = aDDirectoryServerFactory.GetDirectoryObjectFromIdentity(this._identityDSObj, cmdletSessionInfo.DefaultPartitionPath);
			string str = string.Concat("CN=Servers,", this._siteDirObj.DistinguishedName);
			ADObjectSearcher aDObjectSearcher = SearchUtility.BuildSearcher(cmdletSessionInfo.ADSessionInfo, str, ADSearchScope.Base);
			using (aDObjectSearcher)
			{
				aDObjectSearcher.Filter = ADOPathUtil.CreateFilterClause(ADOperator.Like, "objectClass", "*");
				ADObject aDObject = aDObjectSearcher.FindOne();
				if (aDObject == null)
				{
					base.ThrowTerminatingError(ADUtilities.GetErrorRecord(new ADIdentityNotFoundException(string.Format(CultureInfo.CurrentCulture, StringResources.ObjectNotFound, new object[0])), "MoveADDirectoryServer:ProcessRecord", str));
				}
				StringBuilder stringBuilder = new StringBuilder("Move-ADObject -identity $args[0]  -Partition $args[1]  -TargetPath $args[2] ");
				try
				{
					object[] configurationNamingContext = new object[3];
					configurationNamingContext[0] = directoryObjectFromIdentity;
					configurationNamingContext[1] = cmdletSessionInfo.ADRootDSE.ConfigurationNamingContext;
					configurationNamingContext[2] = aDObject.DistinguishedName;
					base.InvokeCommand.InvokeScript(stringBuilder.ToString(), false, PipelineResultTypes.Output, null, configurationNamingContext);
				}
				catch (RuntimeException runtimeException1)
				{
					RuntimeException runtimeException = runtimeException1;
					object[] distinguishedName = new object[3];
					distinguishedName[0] = directoryObjectFromIdentity.DistinguishedName;
					distinguishedName[1] = this._siteDirObj.DistinguishedName;
					distinguishedName[2] = runtimeException.Message;
					string str1 = string.Format(CultureInfo.CurrentCulture, "Failed moving the directory server: {0} to new site: {1}. Error:  {2}", distinguishedName);
					DebugLogger.LogError("MoveADDirectoryServer", str1);
					base.WriteError(new ErrorRecord(runtimeException, "0", ErrorCategory.WriteError, this._identityDSObj));
				}
			}
			return true;
		}
	}
}