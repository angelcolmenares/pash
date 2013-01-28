using Microsoft.ActiveDirectory.Management;
using Microsoft.ActiveDirectory.Management.Provider;
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Security.Authentication;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	[Cmdlet("Get", "ADRootDSE", HelpUri="http://go.microsoft.com/fwlink/?LinkId=219299")]
	public class GetADRootDSE : ADCmdletBase<GetADRootDSEParameterSet>, IADErrorTarget
	{
		public GetADRootDSE()
		{
			base.BeginProcessPipeline.InsertAtEnd(new CmdletSubroutine(this.GetADRootDSEBeginCSRoutine));
		}

		private bool GetADRootDSEBeginCSRoutine()
		{
			bool flag;
			Collection<string> strs = new Collection<string>();
			string[] item = this._cmdletParameters["Properties"] as string[];
			strs.Add("*");
			if (item != null)
			{
				for (int i = 0; i < (int)item.Length; i++)
				{
					strs.Add(item[i]);
				}
			}
			ADObjectSearcher aDObjectSearcher = null;
			using (aDObjectSearcher)
			{
				try
				{
					aDObjectSearcher = new ADObjectSearcher(this.GetSessionInfo());
					ADRootDSE rootDSE = aDObjectSearcher.GetRootDSE(strs);
					rootDSE.SessionInfo = base.GetCmdletSessionInfo().ADSessionInfo;
					base.WriteObject(rootDSE);
					flag = true;
				}
				catch (ADException aDException1)
				{
					ADException aDException = aDException1;
					base.WriteError(ADUtilities.GetErrorRecord(aDException, "GetADRootDSE:BeginProcessing:ADError", null));
					flag = false;
				}
				catch (AuthenticationException authenticationException1)
				{
					AuthenticationException authenticationException = authenticationException1;
					base.WriteError(ADUtilities.GetErrorRecord(authenticationException, "GetADRootDSE:BeginProcessing:InvalidCredentials", null));
					flag = false;
				}
			}
			return flag;
		}

		object Microsoft.ActiveDirectory.Management.Commands.IADErrorTarget.CurrentIdentity(Exception e)
		{
			if (this._cmdletParameters.Contains("Server"))
			{
				return this._cmdletParameters["Server"];
			}
			else
			{
				return null;
			}
		}
	}
}