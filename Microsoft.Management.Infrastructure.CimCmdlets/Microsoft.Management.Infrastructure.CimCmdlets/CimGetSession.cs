using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimGetSession : CimSessionBase
	{
		public CimGetSession()
		{
		}

		public void GetCimSession(GetCimSessionCommand cmdlet)
		{
			DebugHelper.WriteLogEx();
			IEnumerable<PSObject> pSObjects = null;
			IEnumerable<ErrorRecord> errorRecords = null;
			string parameterSetName = cmdlet.ParameterSetName;
			string str = parameterSetName;
			if (parameterSetName != null)
			{
				if (str == "ComputerNameSet")
				{
					if (cmdlet.ComputerName != null)
					{
						pSObjects = this.sessionState.QuerySessionByComputerName(cmdlet.ComputerName, out errorRecords);
					}
					else
					{
						pSObjects = this.sessionState.QuerySession(ConstValue.DefaultSessionName, out errorRecords);
					}
				}
				else
				{
					if (str == "SessionIdSet")
					{
						pSObjects = this.sessionState.QuerySession(cmdlet.Id, out errorRecords);
					}
					else
					{
						if (str == "InstanceIdSet")
						{
							pSObjects = this.sessionState.QuerySession(cmdlet.InstanceId, out errorRecords);
						}
						else
						{
							if (str == "NameSet")
							{
								pSObjects = this.sessionState.QuerySession(cmdlet.Name, out errorRecords);
							}
						}
					}
				}
			}
			if (pSObjects != null)
			{
				foreach (PSObject pSObject in pSObjects)
				{
					cmdlet.WriteObject(pSObject);
				}
			}
			if (errorRecords != null)
			{
				foreach (ErrorRecord errorRecord in errorRecords)
				{
					cmdlet.WriteError(errorRecord);
				}
			}
		}
	}
}