using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimRemoveSession : CimSessionBase
	{
		internal static string RemoveCimSessionActionName;

		static CimRemoveSession()
		{
			CimRemoveSession.RemoveCimSessionActionName = "Remove CimSession";
		}

		public CimRemoveSession()
		{
		}

		public void RemoveCimSession(RemoveCimSessionCommand cmdlet)
		{
			DebugHelper.WriteLogEx();
			IEnumerable<PSObject> pSObjects = null;
			IEnumerable<ErrorRecord> errorRecords = null;
			string parameterSetName = cmdlet.ParameterSetName;
			string str = parameterSetName;
			if (parameterSetName != null)
			{
				if (str == "CimSessionSet")
				{
					pSObjects = this.sessionState.QuerySession(cmdlet.CimSession, out errorRecords);
				}
				else
				{
					if (str == "ComputerNameSet")
					{
						pSObjects = this.sessionState.QuerySessionByComputerName(cmdlet.ComputerName, out errorRecords);
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
			}
			if (pSObjects != null)
			{
				foreach (PSObject pSObject in pSObjects)
				{
					if (!cmdlet.ShouldProcess(this.sessionState.GetRemoveSessionObjectTarget(pSObject), CimRemoveSession.RemoveCimSessionActionName))
					{
						continue;
					}
					this.sessionState.RemoveOneSessionObjectFromCache(pSObject);
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