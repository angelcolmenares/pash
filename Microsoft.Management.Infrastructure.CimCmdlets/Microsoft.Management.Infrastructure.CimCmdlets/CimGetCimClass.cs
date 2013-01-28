using Microsoft.Management.Infrastructure;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Management.Infrastructure.Options;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal sealed class CimGetCimClass : CimAsyncOperation
	{
		public CimGetCimClass()
		{
		}

		private CimSessionProxy CreateSessionProxy(string computerName, GetCimClassCommand cmdlet)
		{
			CimSessionProxy cimSessionProxyGetCimClass = null;
			if (!string.IsNullOrEmpty (computerName) && !computerName.Equals ("localhost", StringComparison.OrdinalIgnoreCase)) {
				/* Set on the fly credentials */
				System.Management.Automation.PSCredential credential = GetOnTheFlyCredentials(cmdlet);
				if (credential == null) 
					cimSessionProxyGetCimClass = new CimSessionProxyGetCimClass(computerName);
				else {
					CimSessionOptions options = new WSManSessionOptions ();
					options.AddDestinationCredentials (cmdlet.CreateCimCredentials (credential, PasswordAuthenticationMechanism.Default, "Get-CimClass", "Authentication"));
					cimSessionProxyGetCimClass = new CimSessionProxyGetCimClass (computerName, options);
				}
			}
			else {
				cimSessionProxyGetCimClass = new CimSessionProxyGetCimClass(computerName);
			}
			base.SubscribeEventAndAddProxytoCache(cimSessionProxyGetCimClass);
			this.SetSessionProxyProperties(ref cimSessionProxyGetCimClass, cmdlet);
			return cimSessionProxyGetCimClass;
		}

		private CimSessionProxy CreateSessionProxy(CimSession session, GetCimClassCommand cmdlet)
		{
			CimSessionProxy cimSessionProxyGetCimClass = new CimSessionProxyGetCimClass(session);
			base.SubscribeEventAndAddProxytoCache(cimSessionProxyGetCimClass);
			this.SetSessionProxyProperties(ref cimSessionProxyGetCimClass, cmdlet);
			return cimSessionProxyGetCimClass;
		}

		public void GetCimClass(GetCimClassCommand cmdlet)
		{
			string className;
			List<CimSessionProxy> cimSessionProxies = new List<CimSessionProxy>();
			string @namespace = ConstValue.GetNamespace(cmdlet.Namespace);
			if (cmdlet.ClassName == null)
			{
				className = "*";
			}
			else
			{
				className = cmdlet.ClassName;
			}
			string str = className;
			CimGetCimClassContext cimGetCimClassContext = new CimGetCimClassContext(cmdlet.ClassName, cmdlet.MethodName, cmdlet.PropertyName, cmdlet.QualifierName);
			string parameterSetName = cmdlet.ParameterSetName;
			string str1 = parameterSetName;
			if (parameterSetName == null)
			{
				return;
			}
			else
			{
				if (str1 == "ComputerSet")
				{
					IEnumerable<string> computerNames = ConstValue.GetComputerNames(cmdlet.ComputerName);
					foreach (string computerName in computerNames)
					{
						CimSessionProxy cimSessionProxy = this.CreateSessionProxy(computerName, cmdlet);
						cimSessionProxy.ContextObject = cimGetCimClassContext;
						cimSessionProxies.Add(cimSessionProxy);
					}
				}
				else
				{
					if (str1 == "SessionSet")
					{
						CimSession[] cimSession = cmdlet.CimSession;
						for (int i = 0; i < (int)cimSession.Length; i++)
						{
							CimSession cimSession1 = cimSession[i];
							CimSessionProxy cimSessionProxy1 = this.CreateSessionProxy(cimSession1, cmdlet);
							cimSessionProxy1.ContextObject = cimGetCimClassContext;
							cimSessionProxies.Add(cimSessionProxy1);
						}
					}
					else
					{
						return;
					}
				}
				if (!WildcardPattern.ContainsWildcardCharacters(str))
				{
					foreach (CimSessionProxy cimSessionProxy2 in cimSessionProxies)
					{
						cimSessionProxy2.GetClassAsync(@namespace, str);
					}
				}
				else
				{
					foreach (CimSessionProxy cimSessionProxy3 in cimSessionProxies)
					{
						cimSessionProxy3.EnumerateClassesAsync(@namespace);
					}
				}
				return;
			}
		}

		private void SetSessionProxyProperties(ref CimSessionProxy proxy, GetCimClassCommand cmdlet)
		{
			proxy.OperationTimeout = cmdlet.OperationTimeoutSec;
		}
	}
}