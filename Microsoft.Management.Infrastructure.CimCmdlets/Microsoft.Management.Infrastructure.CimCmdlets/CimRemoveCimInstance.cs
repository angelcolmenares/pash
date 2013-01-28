using Microsoft.Management.Infrastructure;
using System;
using System.Collections.Generic;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal sealed class CimRemoveCimInstance : CimGetInstance
	{
		private const string action = "Remove-CimInstance";

		public CimRemoveCimInstance()
		{
		}

		public void RemoveCimInstance(RemoveCimInstanceCommand cmdlet)
		{
			string @namespace;
			DebugHelper.WriteLogEx();
			IEnumerable<string> computerNames = ConstValue.GetComputerNames(CimGetInstance.GetComputerName(cmdlet));
			List<CimSessionProxy> cimSessionProxies = new List<CimSessionProxy>();
			string parameterSetName = cmdlet.ParameterSetName;
			string str = parameterSetName;
			if (parameterSetName != null)
			{
				if (str == "CimInstanceComputerSet")
				{
					foreach (string computerName in computerNames)
					{
						cimSessionProxies.Add(base.CreateSessionProxy(computerName, cmdlet.CimInstance, cmdlet));
					}
				}
				else
				{
					if (str == "CimInstanceSessionSet")
					{
						CimSession[] cimSession = CimGetInstance.GetCimSession(cmdlet);
						for (int i = 0; i < (int)cimSession.Length; i++)
						{
							CimSession cimSession1 = cimSession[i];
							cimSessionProxies.Add(base.CreateSessionProxy(cimSession1, cmdlet));
						}
					}
				}
			}
			string parameterSetName1 = cmdlet.ParameterSetName;
			string str1 = parameterSetName1;
			if (parameterSetName1 != null)
			{
				if (str1 == "CimInstanceComputerSet" || str1 == "CimInstanceSessionSet")
				{
					if (cmdlet.ResourceUri == null)
					{
						@namespace = ConstValue.GetNamespace(CimGetInstance.GetCimInstanceParameter(cmdlet).CimSystemProperties.Namespace);
					}
					else
					{
						@namespace = CimGetInstance.GetCimInstanceParameter(cmdlet).CimSystemProperties.Namespace;
					}
					string str2 = cmdlet.CimInstance.ToString();
					foreach (CimSessionProxy cimSessionProxy in cimSessionProxies)
					{
						if (cmdlet.ShouldProcess(str2, "Remove-CimInstance"))
						{
							cimSessionProxy.DeleteInstanceAsync(@namespace, cmdlet.CimInstance);
						}
						else
						{
							return;
						}
					}
				}
				else
				{
					if (str1 == "QueryComputerSet" || str1 == "QuerySessionSet")
					{
						base.GetCimInstanceInternal(cmdlet);
					}
					else
					{
						return;
					}
				}
			}
		}

		internal void RemoveCimInstance(CimInstance cimInstance, XOperationContextBase context, CmdletOperationBase cmdlet)
		{
			DebugHelper.WriteLogEx();
			string str = cimInstance.ToString();
			if (cmdlet.ShouldProcess(str, "Remove-CimInstance"))
			{
				CimRemoveCimInstanceContext cimRemoveCimInstanceContext = context as CimRemoveCimInstanceContext;
				CimSessionProxy cimSessionProxy = base.CreateCimSessionProxy(cimRemoveCimInstanceContext.Proxy);
				cimSessionProxy.DeleteInstanceAsync(cimRemoveCimInstanceContext.Namespace, cimInstance);
				return;
			}
			else
			{
				return;
			}
		}
	}
}