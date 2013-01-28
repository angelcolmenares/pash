using Microsoft.Management.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Management.Infrastructure.Options;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal class CimGetInstance : CimAsyncOperation
	{
		private const string queryWithWhere = "SELECT {0} FROM {1} WHERE {2}";

		private const string queryWithoutWhere = "SELECT {0} FROM {1}";

		public CimGetInstance()
		{
		}

		protected static string CreateQuery(CimBaseCommand cmdlet)
		{
			DebugHelper.WriteLogEx();
			GetCimInstanceCommand getCimInstanceCommand = cmdlet as GetCimInstanceCommand;
			if (getCimInstanceCommand == null)
			{
				return null;
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (getCimInstanceCommand.SelectProperties != null)
				{
					string[] selectProperties = getCimInstanceCommand.SelectProperties;
					for (int i = 0; i < (int)selectProperties.Length; i++)
					{
						string str = selectProperties[i];
						if (stringBuilder.Length > 0)
						{
							stringBuilder.Append(",");
						}
						stringBuilder.Append(str);
					}
				}
				else
				{
					stringBuilder.Append("*");
				}
				if (getCimInstanceCommand.Filter == null)
				{
					object[] className = new object[2];
					className[0] = stringBuilder;
					className[1] = getCimInstanceCommand.ClassName;
					return string.Format(CultureInfo.CurrentUICulture, "SELECT {0} FROM {1}", className);
				}
				else
				{
					object[] filter = new object[3];
					filter[0] = stringBuilder;
					filter[1] = getCimInstanceCommand.ClassName;
					filter[2] = getCimInstanceCommand.Filter;
					return string.Format(CultureInfo.CurrentUICulture, "SELECT {0} FROM {1} WHERE {2}", filter);
				}
			}
		}

		protected CimSessionProxy CreateSessionProxy (string computerName, CimBaseCommand cmdlet)
		{
			CimSessionProxy cimSessionProxy = null;
			if (!string.IsNullOrEmpty (computerName) && !computerName.Equals ("localhost", StringComparison.OrdinalIgnoreCase)) {
				/* Set on the fly credentials */
				System.Management.Automation.PSCredential credential = GetOnTheFlyCredentials(cmdlet);
				if (credential == null) 
					cimSessionProxy = base.CreateCimSessionProxy (computerName);
				else {
					CimSessionOptions options = new WSManSessionOptions ();
					options.AddDestinationCredentials (cmdlet.CreateCimCredentials (credential, PasswordAuthenticationMechanism.Default, "Get-CimIstance", "Authentication"));
					cimSessionProxy = base.CreateCimSessionProxy (computerName, options);
				}
			} 
			else {
				cimSessionProxy = base.CreateCimSessionProxy (computerName);
			}
			this.SetSessionProxyProperties(ref cimSessionProxy, cmdlet);
			return cimSessionProxy;
		}

		protected CimSessionProxy CreateSessionProxy(string computerName, CimInstance cimInstance, CimBaseCommand cmdlet, bool passThru)
		{
			CimSessionProxy cimSessionProxy = base.CreateCimSessionProxy(computerName, cimInstance, passThru);
			this.SetSessionProxyProperties(ref cimSessionProxy, cmdlet);
			return cimSessionProxy;
		}

		protected CimSessionProxy CreateSessionProxy(CimSession session, CimBaseCommand cmdlet)
		{
			CimSessionProxy cimSessionProxy = base.CreateCimSessionProxy(session);
			this.SetSessionProxyProperties(ref cimSessionProxy, cmdlet);
			return cimSessionProxy;
		}

		protected CimSessionProxy CreateSessionProxy(string computerName, CimInstance cimInstance, CimBaseCommand cmdlet)
		{
			CimSessionProxy cimSessionProxy = base.CreateCimSessionProxy(computerName, cimInstance);
			this.SetSessionProxyProperties(ref cimSessionProxy, cmdlet);
			return cimSessionProxy;
		}

		protected CimSessionProxy CreateSessionProxy(CimSession session, CimBaseCommand cmdlet, bool passThru)
		{
			CimSessionProxy cimSessionProxy = base.CreateCimSessionProxy(session, passThru);
			this.SetSessionProxyProperties(ref cimSessionProxy, cmdlet);
			return cimSessionProxy;
		}

		public void GetCimInstance(GetCimInstanceCommand cmdlet)
		{
			this.GetCimInstanceInternal(cmdlet);
		}

		protected void GetCimInstanceInternal(CimBaseCommand cmdlet)
		{
			string @namespace;
			IEnumerable<string> computerNames = ConstValue.GetComputerNames(CimGetInstance.GetComputerName(cmdlet));
			List<CimSessionProxy> cimSessionProxies = new List<CimSessionProxy>();
			bool flag = cmdlet is GetCimInstanceCommand;
			string parameterSetName = cmdlet.ParameterSetName;
			string str = parameterSetName;
			if (parameterSetName != null)
			{
				switch (str)
				{
					case "CimInstanceComputerSet":
					{
						IEnumerator<string> enumerator = computerNames.GetEnumerator();
						using (enumerator)
						{
							while (enumerator.MoveNext())
							{
								string current = enumerator.Current;
								CimInstance cimInstanceParameter = CimGetInstance.GetCimInstanceParameter(cmdlet);
								CimSessionProxy cimSessionProxy = this.CreateSessionProxy(current, cimInstanceParameter, cmdlet);
								if (flag)
								{
									this.SetPreProcess(cimSessionProxy, cmdlet as GetCimInstanceCommand);
								}
								cimSessionProxies.Add(cimSessionProxy);
							}
						}
					}
					break;
					case "ClassNameComputerSet":
					case "QueryComputerSet":
					case "ResourceUriComputerSet":
					{
						IEnumerator<string> enumerator1 = computerNames.GetEnumerator();
						using (enumerator1)
						{
							while (enumerator1.MoveNext())
							{
								string current1 = enumerator1.Current;
								CimSessionProxy cimSessionProxy1 = this.CreateSessionProxy(current1, cmdlet);
								if (flag)
								{
									this.SetPreProcess(cimSessionProxy1, cmdlet as GetCimInstanceCommand);
								}
								cimSessionProxies.Add(cimSessionProxy1);
							}
							break;
						}
					}
					case "ClassNameSessionSet":
					case "CimInstanceSessionSet":
					case "QuerySessionSet":
					case "ResourceUriSessionSet":
					{
						CimSession[] cimSession = CimGetInstance.GetCimSession(cmdlet);
						for (int i = 0; i < (int)cimSession.Length; i++)
						{
							CimSession cimSession1 = cimSession[i];
							CimSessionProxy cimSessionProxy2 = this.CreateSessionProxy(cimSession1, cmdlet);
							if (flag)
							{
								this.SetPreProcess(cimSessionProxy2, cmdlet as GetCimInstanceCommand);
							}
							cimSessionProxies.Add(cimSessionProxy2);
						}
					}
					break;
				}
			}
			string parameterSetName1 = cmdlet.ParameterSetName;
			string str1 = parameterSetName1;
			if (parameterSetName1 != null)
			{
				if (str1 == "ClassNameComputerSet" || str1 == "ClassNameSessionSet")
				{
					@namespace = ConstValue.GetNamespace(CimGetInstance.GetNamespace(cmdlet));
					if (!CimGetInstance.IsClassNameQuerySet(cmdlet))
					{
						List<CimSessionProxy>.Enumerator enumerator2 = cimSessionProxies.GetEnumerator();
						try
						{
							while (enumerator2.MoveNext())
							{
								CimSessionProxy current2 = enumerator2.Current;
								current2.EnumerateInstancesAsync(@namespace, CimGetInstance.GetClassName(cmdlet));
							}
							return;
						}
						finally
						{
							enumerator2.Dispose();
						}
					}
					else
					{
						string str2 = CimGetInstance.CreateQuery(cmdlet);
						object[] objArray = new object[1];
						objArray[0] = str2;
						DebugHelper.WriteLogEx("Query = {0}", 1, objArray);
						List<CimSessionProxy>.Enumerator enumerator3 = cimSessionProxies.GetEnumerator();
						try
						{
							while (enumerator3.MoveNext())
							{
								CimSessionProxy current3 = enumerator3.Current;
								current3.QueryInstancesAsync(@namespace, ConstValue.GetQueryDialectWithDefault(CimGetInstance.GetQueryDialect(cmdlet)), str2);
							}
							return;
						}
						finally
						{
							enumerator3.Dispose();
						}
					}
				}
				else if (str1 == "CimInstanceComputerSet" || str1 == "CimInstanceSessionSet")
				{
					CimInstance cimInstance = CimGetInstance.GetCimInstanceParameter(cmdlet);
					@namespace = ConstValue.GetNamespace(cimInstance.CimSystemProperties.Namespace);
					List<CimSessionProxy>.Enumerator enumerator4 = cimSessionProxies.GetEnumerator();
					try
					{
						while (enumerator4.MoveNext())
						{
							CimSessionProxy cimSessionProxy3 = enumerator4.Current;
							cimSessionProxy3.GetInstanceAsync(@namespace, cimInstance);
						}
						return;
					}
					finally
					{
						enumerator4.Dispose();
					}
				}
				else if (str1 == "QueryComputerSet" || str1 == "QuerySessionSet")
				{
					@namespace = ConstValue.GetNamespace(CimGetInstance.GetNamespace(cmdlet));
					List<CimSessionProxy>.Enumerator enumerator5 = cimSessionProxies.GetEnumerator();
					try
					{
						while (enumerator5.MoveNext())
						{
							CimSessionProxy current4 = enumerator5.Current;
							current4.QueryInstancesAsync(@namespace, ConstValue.GetQueryDialectWithDefault(CimGetInstance.GetQueryDialect(cmdlet)), CimGetInstance.GetQuery(cmdlet));
						}
						return;
					}
					finally
					{
						enumerator5.Dispose();
					}
				}
				else if (str1 == "ResourceUriSessionSet" || str1 == "ResourceUriComputerSet")
				{
					List<CimSessionProxy>.Enumerator enumerator6 = cimSessionProxies.GetEnumerator();
					try
					{
						while (enumerator6.MoveNext())
						{
							CimSessionProxy cimSessionProxy4 = enumerator6.Current;
							cimSessionProxy4.EnumerateInstancesAsync(CimGetInstance.GetNamespace(cmdlet), CimGetInstance.GetClassName(cmdlet));
						}
						return;
					}
					finally
					{
						enumerator6.Dispose();
					}
				}
				return;
			}
		}

		protected static CimInstance GetCimInstanceParameter(CimBaseCommand cmdlet)
		{
			if (cmdlet as GetCimInstanceCommand == null)
			{
				if (cmdlet as RemoveCimInstanceCommand == null)
				{
					if (cmdlet as SetCimInstanceCommand == null)
					{
						return null;
					}
					else
					{
						return (cmdlet as SetCimInstanceCommand).CimInstance;
					}
				}
				else
				{
					return (cmdlet as RemoveCimInstanceCommand).CimInstance;
				}
			}
			else
			{
				return (cmdlet as GetCimInstanceCommand).CimInstance;
			}
		}

		protected static CimSession[] GetCimSession(CimBaseCommand cmdlet)
		{
			if (cmdlet as GetCimInstanceCommand == null)
			{
				if (cmdlet as RemoveCimInstanceCommand == null)
				{
					if (cmdlet as SetCimInstanceCommand == null)
					{
						return null;
					}
					else
					{
						return (cmdlet as SetCimInstanceCommand).CimSession;
					}
				}
				else
				{
					return (cmdlet as RemoveCimInstanceCommand).CimSession;
				}
			}
			else
			{
				return (cmdlet as GetCimInstanceCommand).CimSession;
			}
		}

		protected static string GetClassName(CimBaseCommand cmdlet)
		{
			if (cmdlet as GetCimInstanceCommand == null)
			{
				return null;
			}
			else
			{
				return (cmdlet as GetCimInstanceCommand).ClassName;
			}
		}

		protected static string[] GetComputerName(CimBaseCommand cmdlet)
		{
			if (cmdlet as GetCimInstanceCommand == null)
			{
				if (cmdlet as RemoveCimInstanceCommand == null)
				{
					if (cmdlet as SetCimInstanceCommand == null)
					{
						return null;
					}
					else
					{
						return (cmdlet as SetCimInstanceCommand).ComputerName;
					}
				}
				else
				{
					return (cmdlet as RemoveCimInstanceCommand).ComputerName;
				}
			}
			else
			{
				return (cmdlet as GetCimInstanceCommand).ComputerName;
			}
		}

		protected static string GetNamespace(CimBaseCommand cmdlet)
		{
			if (cmdlet as GetCimInstanceCommand == null)
			{
				if (cmdlet as RemoveCimInstanceCommand == null)
				{
					if (cmdlet as SetCimInstanceCommand == null)
					{
						return null;
					}
					else
					{
						return (cmdlet as SetCimInstanceCommand).Namespace;
					}
				}
				else
				{
					return (cmdlet as RemoveCimInstanceCommand).Namespace;
				}
			}
			else
			{
				return (cmdlet as GetCimInstanceCommand).Namespace;
			}
		}

		protected static string GetQuery(CimBaseCommand cmdlet)
		{
			if (cmdlet as GetCimInstanceCommand == null)
			{
				if (cmdlet as RemoveCimInstanceCommand == null)
				{
					if (cmdlet as SetCimInstanceCommand == null)
					{
						return null;
					}
					else
					{
						return (cmdlet as SetCimInstanceCommand).Query;
					}
				}
				else
				{
					return (cmdlet as RemoveCimInstanceCommand).Query;
				}
			}
			else
			{
				return (cmdlet as GetCimInstanceCommand).Query;
			}
		}

		protected static string GetQueryDialect(CimBaseCommand cmdlet)
		{
			if (cmdlet as GetCimInstanceCommand == null)
			{
				if (cmdlet as RemoveCimInstanceCommand == null)
				{
					if (cmdlet as SetCimInstanceCommand == null)
					{
						return null;
					}
					else
					{
						return (cmdlet as SetCimInstanceCommand).QueryDialect;
					}
				}
				else
				{
					return (cmdlet as RemoveCimInstanceCommand).QueryDialect;
				}
			}
			else
			{
				return (cmdlet as GetCimInstanceCommand).QueryDialect;
			}
		}

		internal static bool IsClassNameQuerySet(CimBaseCommand cmdlet)
		{
			DebugHelper.WriteLogEx();
			GetCimInstanceCommand getCimInstanceCommand = cmdlet as GetCimInstanceCommand;
			if (getCimInstanceCommand == null || getCimInstanceCommand.QueryDialect == null && getCimInstanceCommand.SelectProperties == null && getCimInstanceCommand.Filter == null)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		private void SetPreProcess(CimSessionProxy proxy, GetCimInstanceCommand cmdlet)
		{
			if (cmdlet.KeyOnly || cmdlet.SelectProperties != null)
			{
				proxy.ObjectPreProcess = new FormatPartialCimInstance();
			}
		}

		private void SetSessionProxyProperties(ref CimSessionProxy proxy, CimBaseCommand cmdlet)
		{
			if (cmdlet as GetCimInstanceCommand == null)
			{
				if (cmdlet as RemoveCimInstanceCommand == null)
				{
					if (cmdlet as SetCimInstanceCommand != null)
					{
						SetCimInstanceCommand setCimInstanceCommand = cmdlet as SetCimInstanceCommand;
						proxy.OperationTimeout = setCimInstanceCommand.OperationTimeoutSec;
						if (setCimInstanceCommand.ResourceUri != null)
						{
							proxy.ResourceUri = setCimInstanceCommand.ResourceUri;
						}
						CimSetCimInstanceContext cimSetCimInstanceContext = new CimSetCimInstanceContext(ConstValue.GetNamespace(setCimInstanceCommand.Namespace), setCimInstanceCommand.Property, proxy, cmdlet.ParameterSetName, setCimInstanceCommand.PassThru);
						proxy.ContextObject = cimSetCimInstanceContext;
					}
				}
				else
				{
					RemoveCimInstanceCommand removeCimInstanceCommand = cmdlet as RemoveCimInstanceCommand;
					proxy.OperationTimeout = removeCimInstanceCommand.OperationTimeoutSec;
					if (removeCimInstanceCommand.ResourceUri != null)
					{
						proxy.ResourceUri = removeCimInstanceCommand.ResourceUri;
					}
					CimRemoveCimInstanceContext cimRemoveCimInstanceContext = new CimRemoveCimInstanceContext(ConstValue.GetNamespace(removeCimInstanceCommand.Namespace), proxy);
					proxy.ContextObject = cimRemoveCimInstanceContext;
					return;
				}
			}
			else
			{
				GetCimInstanceCommand getCimInstanceCommand = cmdlet as GetCimInstanceCommand;
				proxy.KeyOnly = getCimInstanceCommand.KeyOnly;
				proxy.Shallow = getCimInstanceCommand.Shallow;
				proxy.OperationTimeout = getCimInstanceCommand.OperationTimeoutSec;
				if (getCimInstanceCommand.ResourceUri != null)
				{
					proxy.ResourceUri = getCimInstanceCommand.ResourceUri;
					return;
				}
			}
		}
	}
}