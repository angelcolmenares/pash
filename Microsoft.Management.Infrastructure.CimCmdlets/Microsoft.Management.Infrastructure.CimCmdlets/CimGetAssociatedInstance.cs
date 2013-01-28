using Microsoft.Management.Infrastructure;
using System;
using System.Collections.Generic;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal sealed class CimGetAssociatedInstance : CimAsyncOperation
	{
		public CimGetAssociatedInstance()
		{
		}

		private CimSessionProxy CreateSessionProxy(string computerName, CimInstance cimInstance, GetCimAssociatedInstanceCommand cmdlet)
		{
			CimSessionProxy cimSessionProxy = base.CreateCimSessionProxy(computerName, cimInstance);
			this.SetSessionProxyProperties(ref cimSessionProxy, cmdlet);
			return cimSessionProxy;
		}

		private CimSessionProxy CreateSessionProxy(CimSession session, GetCimAssociatedInstanceCommand cmdlet)
		{
			CimSessionProxy cimSessionProxy = base.CreateCimSessionProxy(session);
			this.SetSessionProxyProperties(ref cimSessionProxy, cmdlet);
			return cimSessionProxy;
		}

		public void GetCimAssociatedInstance(GetCimAssociatedInstanceCommand cmdlet)
		{
			IEnumerable<string> computerNames = ConstValue.GetComputerNames(cmdlet.ComputerName);
			string @namespace = cmdlet.Namespace;
			if (@namespace == null && cmdlet.ResourceUri == null)
			{
				@namespace = ConstValue.GetNamespace(cmdlet.CimInstance.CimSystemProperties.Namespace);
			}
			List<CimSessionProxy> cimSessionProxies = new List<CimSessionProxy>();
			string parameterSetName = cmdlet.ParameterSetName;
			string str = parameterSetName;
			if (parameterSetName == null)
			{
				return;
			}
			else
			{
				if (str == "ComputerSet")
				{
					foreach (string computerName in computerNames)
					{
						CimSessionProxy cimSessionProxy = this.CreateSessionProxy(computerName, cmdlet.CimInstance, cmdlet);
						cimSessionProxies.Add(cimSessionProxy);
					}
				}
				else
				{
					if (str == "SessionSet")
					{
						CimSession[] cimSession = cmdlet.CimSession;
						for (int i = 0; i < (int)cimSession.Length; i++)
						{
							CimSession cimSession1 = cimSession[i];
							CimSessionProxy cimSessionProxy1 = this.CreateSessionProxy(cimSession1, cmdlet);
							cimSessionProxies.Add(cimSessionProxy1);
						}
					}
					else
					{
						return;
					}
				}
				foreach (CimSessionProxy cimSessionProxy2 in cimSessionProxies)
				{
					cimSessionProxy2.EnumerateAssociatedInstancesAsync(@namespace, cmdlet.CimInstance, cmdlet.Association, cmdlet.ResultClassName, null, null);
				}
				return;
			}
		}

		private void SetSessionProxyProperties(ref CimSessionProxy proxy, GetCimAssociatedInstanceCommand cmdlet)
		{
			proxy.OperationTimeout = cmdlet.OperationTimeoutSec;
			proxy.KeyOnly = cmdlet.KeyOnly;
			if (cmdlet.ResourceUri != null)
			{
				proxy.ResourceUri = cmdlet.ResourceUri;
			}
		}
	}
}