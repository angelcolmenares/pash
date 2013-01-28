using Microsoft.Management.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal sealed class CimNewCimInstance : CimAsyncOperation
	{
		private const string action = "New-CimInstance";

		public CimNewCimInstance()
		{
		}

		private CimInstance CreateCimInstance(string className, string cimNamespace, IEnumerable<string> key, IDictionary properties, NewCimInstanceCommand cmdlet)
		{
			CimInstance cimInstance = new CimInstance(className, cimNamespace);
			if (properties != null)
			{
				List<string> strs = new List<string>();
				if (key != null)
				{
					foreach (string str in key)
					{
						strs.Add(str);
					}
				}
				IDictionaryEnumerator enumerator = properties.GetEnumerator();
				while (enumerator.MoveNext())
				{
					CimFlags cimFlag = CimFlags.None;
					string str1 = enumerator.Key.ToString().Trim();
					if (strs.Contains<string>(str1, StringComparer.OrdinalIgnoreCase))
					{
						cimFlag = CimFlags.Key;
					}
					object baseObject = base.GetBaseObject(enumerator.Value);
					object[] objArray = new object[3];
					objArray[0] = str1;
					objArray[1] = baseObject;
					objArray[2] = cimFlag;
					DebugHelper.WriteLog("Create and add new property to ciminstance: name = {0}; value = {1}; flags = {2}", 5, objArray);
					PSReference pSReference = baseObject as PSReference;
					if (pSReference == null)
					{
						CimProperty cimProperty = CimProperty.Create(str1, baseObject, cimFlag);
						cimInstance.CimInstanceProperties.Add(cimProperty);
					}
					else
					{
						CimProperty cimProperty1 = CimProperty.Create(str1, base.GetBaseObject(pSReference.Value), CimType.Reference, cimFlag);
						cimInstance.CimInstanceProperties.Add(cimProperty1);
					}
				}
				return cimInstance;
			}
			else
			{
				return cimInstance;
			}
		}

		private CimInstance CreateCimInstance(CimClass cimClass, IDictionary properties, NewCimInstanceCommand cmdlet)
		{
			CimInstance cimInstance;
			CimInstance cimInstance1 = new CimInstance(cimClass);
			if (properties != null)
			{
				List<string> strs = new List<string>();
				IEnumerator enumerator = properties.Keys.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						string current = (string)enumerator.Current;
						if (cimInstance1.CimInstanceProperties[current] != null)
						{
							object baseObject = base.GetBaseObject(properties[current]);
							cimInstance1.CimInstanceProperties[current].Value = baseObject;
						}
						else
						{
							strs.Add(current);
							cmdlet.ThrowInvalidProperty(strs, cmdlet.CimClass.CimSystemProperties.ClassName, "Property", "New-CimInstance", properties);
							cimInstance = null;
							return cimInstance;
						}
					}
					return cimInstance1;
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
				return cimInstance;
			}
			else
			{
				return cimInstance1;
			}
		}

		private CimSessionProxy CreateSessionProxy(string computerName, NewCimInstanceCommand cmdlet)
		{
			CimSessionProxy cimSessionProxyNewCimInstance = new CimSessionProxyNewCimInstance(computerName, this);
			base.SubscribeEventAndAddProxytoCache(cimSessionProxyNewCimInstance);
			this.SetSessionProxyProperties(ref cimSessionProxyNewCimInstance, cmdlet);
			return cimSessionProxyNewCimInstance;
		}

		private CimSessionProxy CreateSessionProxy(CimSession session, NewCimInstanceCommand cmdlet)
		{
			CimSessionProxy cimSessionProxyNewCimInstance = new CimSessionProxyNewCimInstance(session, this);
			base.SubscribeEventAndAddProxytoCache(cimSessionProxyNewCimInstance);
			this.SetSessionProxyProperties(ref cimSessionProxyNewCimInstance, cmdlet);
			return cimSessionProxyNewCimInstance;
		}

		internal void GetCimInstance(CimInstance cimInstance, XOperationContextBase context)
		{
			string @namespace;
			DebugHelper.WriteLogEx();
			CimNewCimInstanceContext cimNewCimInstanceContext = context as CimNewCimInstanceContext;
			if (cimNewCimInstanceContext != null)
			{
				CimSessionProxy cimSessionProxy = base.CreateCimSessionProxy(cimNewCimInstanceContext.Proxy);
				if (cimInstance.CimSystemProperties.Namespace == null)
				{
					@namespace = cimNewCimInstanceContext.Namespace;
				}
				else
				{
					@namespace = cimInstance.CimSystemProperties.Namespace;
				}
				string str = @namespace;
				cimSessionProxy.GetInstanceAsync(str, cimInstance);
				return;
			}
			else
			{
				DebugHelper.WriteLog("Invalid (null) CimNewCimInstanceContext", 1);
				return;
			}
		}

		public void NewCimInstance(NewCimInstanceCommand cmdlet)
		{
			string @namespace;
			DebugHelper.WriteLogEx();
			CimInstance cimInstance = null;
			try
			{
				string parameterSetName = cmdlet.ParameterSetName;
				string str = parameterSetName;
				if (parameterSetName != null)
				{
					if (str == "ClassNameComputerSet" || str == "ClassNameSessionSet")
					{
						@namespace = ConstValue.GetNamespace(cmdlet.Namespace);
						cimInstance = this.CreateCimInstance(cmdlet.ClassName, @namespace, cmdlet.Key, cmdlet.Property, cmdlet);
					}
					else if (str == "ResourceUriSessionSet" || str == "ResourceUriComputerSet")
					{
						@namespace = cmdlet.Namespace;
						cimInstance = this.CreateCimInstance("DummyClass", @namespace, cmdlet.Key, cmdlet.Property, cmdlet);
					}
					else if (str == "CimClassComputerSet" || str == "CimClassSessionSet")
					{
						@namespace = ConstValue.GetNamespace(cmdlet.CimClass.CimSystemProperties.Namespace);
						cimInstance = this.CreateCimInstance(cmdlet.CimClass, cmdlet.Property, cmdlet);
					}
					else
					{
						return;
					}
					goto Label0;
				}
			}
			catch (ArgumentNullException argumentNullException1)
			{
				ArgumentNullException argumentNullException = argumentNullException1;
				cmdlet.ThrowTerminatingError(argumentNullException, "New-CimInstance");
			}
			catch (ArgumentException argumentException1)
			{
				ArgumentException argumentException = argumentException1;
				cmdlet.ThrowTerminatingError(argumentException, "New-CimInstance");
			}
			return;
		Label0:
			if (!cmdlet.ClientOnly)
			{
				string str1 = cimInstance.ToString();
				if (cmdlet.ShouldProcess(str1, "New-CimInstance"))
				{
					List<CimSessionProxy> cimSessionProxies = new List<CimSessionProxy>();
					string parameterSetName1 = cmdlet.ParameterSetName;
					string str2 = parameterSetName1;
					if (parameterSetName1 != null)
					{
						if (str2 == "ClassNameComputerSet" || str2 == "CimClassComputerSet" || str2 == "ResourceUriComputerSet")
						{
							IEnumerable<string> computerNames = ConstValue.GetComputerNames(cmdlet.ComputerName);
							foreach (string computerName in computerNames)
							{
								cimSessionProxies.Add(this.CreateSessionProxy(computerName, cmdlet));
							}
						}
						else
						{
							if (str2 == "CimClassSessionSet" || str2 == "ClassNameSessionSet" || str2 == "ResourceUriSessionSet")
							{
								CimSession[] cimSession = cmdlet.CimSession;
								for (int i = 0; i < (int)cimSession.Length; i++)
								{
									CimSession cimSession1 = cimSession[i];
									cimSessionProxies.Add(this.CreateSessionProxy(cimSession1, cmdlet));
								}
							}
						}
					}
					foreach (CimSessionProxy cimSessionProxy in cimSessionProxies)
					{
						cimSessionProxy.ContextObject = new CimNewCimInstanceContext(cimSessionProxy, @namespace);
						cimSessionProxy.CreateInstanceAsync(@namespace, cimInstance);
					}
					return;
				}
				else
				{
					return;
				}
			}
			else
			{
				cmdlet.CmdletOperation.WriteObject(cimInstance, null);
				return;
			}
		}

		private void SetSessionProxyProperties(ref CimSessionProxy proxy, NewCimInstanceCommand cmdlet)
		{
			proxy.OperationTimeout = cmdlet.OperationTimeoutSec;
			if (cmdlet.ResourceUri != null)
			{
				proxy.ResourceUri = cmdlet.ResourceUri;
			}
		}
	}
}