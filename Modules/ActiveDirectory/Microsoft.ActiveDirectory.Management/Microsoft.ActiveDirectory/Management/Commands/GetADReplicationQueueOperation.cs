using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADReplicationQueueOperation", HelpUri="http://go.microsoft.com/fwlink/?LinkId=216355")]
	public class GetADReplicationQueueOperation : ADFactoryCmdletBase<GetADReplicationQueueOperationParameterSet, ADReplicationQueueOperationFactory<ADReplicationQueueOperation>, ADReplicationQueueOperation>
	{
		public GetADReplicationQueueOperation()
		{
			base.BeginProcessPipeline.InsertAtEnd(new CmdletSubroutine(this.GetADReplicationQueueOperationBeginCSRoutine));
			base.ProcessRecordPipeline.InsertAtEnd(new CmdletSubroutine(this.GetADReplicationQueueOperationProcessCSRoutine));
		}

		private bool GetADReplicationQueueOperationBeginCSRoutine()
		{
			string item = this._cmdletParameters["Server"] as string;
			if (!base.DoesServerNameRepresentDomainName(item))
			{
				return true;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = item;
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ServerIsNotDirectoryServer, objArray));
			}
		}

		protected bool GetADReplicationQueueOperationProcessCSRoutine()
		{
			ADEntity rootDSE;
			string item = this._cmdletParameters["Server"] as string;
			if (!base.DoesServerNameRepresentDomainName(item))
			{
				CmdletSessionInfo cmdletSessionInfo = this.GetCmdletSessionInfo();
				this._factory.SetCmdletSessionInfo(cmdletSessionInfo);
				string[] strArrays = new string[1];
				strArrays[0] = "msDS-ReplPendingOps";
				string[] strArrays1 = strArrays;
				using (ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(cmdletSessionInfo.ADSessionInfo))
				{
					rootDSE = aDObjectSearcher.GetRootDSE(strArrays1);
				}
				foreach (ADReplicationQueueOperation extendedObjectFromDirectoryObject in this._factory.GetExtendedObjectFromDirectoryObject(rootDSE, "msDS-ReplPendingOps", "DS_REPL_OP"))
				{
					base.WriteObject(extendedObjectFromDirectoryObject);
				}
				return true;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = item;
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.ServerIsNotDirectoryServer, objArray));
			}
		}
	}
}