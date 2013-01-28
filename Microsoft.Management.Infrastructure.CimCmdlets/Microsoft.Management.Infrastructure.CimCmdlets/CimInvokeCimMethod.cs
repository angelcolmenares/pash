using Microsoft.Management.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal sealed class CimInvokeCimMethod : CimAsyncOperation
	{
		private const string targetClass = "{0}";

		private const string actionTemplate = "Invoke-CimMethod: {0}";

		public CimInvokeCimMethod()
		{
		}

		private CimMethodParametersCollection CreateParametersCollection(IDictionary parameters, CimClass cimClass, CimInstance cimInstance, string methodName)
		{
			DebugHelper.WriteLogEx();
			CimMethodParametersCollection cimMethodParametersCollection = null;
			if (parameters != null)
			{
				if (parameters.Count != 0)
				{
					cimMethodParametersCollection = new CimMethodParametersCollection();
					IDictionaryEnumerator enumerator = parameters.GetEnumerator();
					while (enumerator.MoveNext())
					{
						string str = enumerator.Key.ToString();
						CimFlags cimFlag = CimFlags.In;
						object baseObject = base.GetBaseObject(enumerator.Value);
						object[] objArray = new object[3];
						objArray[0] = str;
						objArray[1] = baseObject;
						objArray[2] = cimFlag;
						DebugHelper.WriteLog("Create parameter name= {0}, value= {1}, flags= {2}.", 4, objArray);
						CimMethodParameter cimMethodParameter = null;
						CimMethodDeclaration item = null;
						string className = null;
						if (cimClass == null)
						{
							if (cimInstance != null)
							{
								className = cimInstance.CimClass.CimSystemProperties.ClassName;
								item = cimInstance.CimClass.CimClassMethods[methodName];
							}
						}
						else
						{
							className = cimClass.CimSystemProperties.ClassName;
							item = cimClass.CimClassMethods[methodName];
							if (item == null)
							{
								object[] objArray1 = new object[2];
								objArray1[0] = methodName;
								objArray1[1] = className;
								throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, Strings.InvalidMethod, objArray1));
							}
						}
						if (item == null)
						{
							if (baseObject != null)
							{
								CimType cimType = CimType.Unknown;
								object referenceOrReferenceArrayObject = base.GetReferenceOrReferenceArrayObject(baseObject, ref cimType);
								if (referenceOrReferenceArrayObject == null)
								{
									cimMethodParameter = CimMethodParameter.Create(str, baseObject, cimFlag);
								}
								else
								{
									cimMethodParameter = CimMethodParameter.Create(str, referenceOrReferenceArrayObject, cimType, cimFlag);
								}
							}
							else
							{
								cimMethodParameter = CimMethodParameter.Create(str, baseObject, CimType.String, cimFlag);
							}
						}
						else
						{
							CimMethodParameterDeclaration cimMethodParameterDeclaration = item.Parameters[str];
							if (cimMethodParameterDeclaration != null)
							{
								cimMethodParameter = CimMethodParameter.Create(str, baseObject, cimMethodParameterDeclaration.CimType, cimFlag);
							}
							else
							{
								object[] objArray2 = new object[3];
								objArray2[0] = str;
								objArray2[1] = methodName;
								objArray2[2] = className;
								throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, Strings.InvalidMethodParameter, objArray2));
							}
						}
						if (cimMethodParameter == null)
						{
							continue;
						}
						cimMethodParametersCollection.Add(cimMethodParameter);
					}
					return cimMethodParametersCollection;
				}
				else
				{
					return cimMethodParametersCollection;
				}
			}
			else
			{
				return cimMethodParametersCollection;
			}
		}

		private CimSessionProxy CreateSessionProxy(string computerName, InvokeCimMethodCommand cmdlet)
		{
			CimSessionProxy cimSessionProxy = base.CreateCimSessionProxy(computerName);
			this.SetSessionProxyProperties(ref cimSessionProxy, cmdlet);
			return cimSessionProxy;
		}

		private CimSessionProxy CreateSessionProxy(string computerName, CimInstance cimInstance, InvokeCimMethodCommand cmdlet)
		{
			CimSessionProxy cimSessionProxy = base.CreateCimSessionProxy(computerName, cimInstance);
			this.SetSessionProxyProperties(ref cimSessionProxy, cmdlet);
			return cimSessionProxy;
		}

		private CimSessionProxy CreateSessionProxy(CimSession session, InvokeCimMethodCommand cmdlet)
		{
			CimSessionProxy cimSessionProxy = base.CreateCimSessionProxy(session);
			this.SetSessionProxyProperties(ref cimSessionProxy, cmdlet);
			return cimSessionProxy;
		}

		public void InvokeCimMethod(InvokeCimMethodCommand cmdlet)
		{
			string @namespace;
			IEnumerable<string> computerNames = ConstValue.GetComputerNames(cmdlet.ComputerName);
			List<CimSessionProxy> cimSessionProxies = new List<CimSessionProxy>();
			object[] methodName = new object[1];
			methodName[0] = cmdlet.MethodName;
			string str = string.Format(CultureInfo.CurrentUICulture, "Invoke-CimMethod: {0}", methodName);
			string parameterSetName = cmdlet.ParameterSetName;
			string str1 = parameterSetName;
			if (parameterSetName != null)
			{
				switch (str1)
				{
					case "CimInstanceComputerSet":
					{
						IEnumerator<string> enumerator = computerNames.GetEnumerator();
						using (enumerator)
						{
							while (enumerator.MoveNext())
							{
								string current = enumerator.Current;
								cimSessionProxies.Add(this.CreateSessionProxy(current, cmdlet.CimInstance, cmdlet));
							}
						}
					}
					break;
					case "ClassNameComputerSet":
					case "CimClassComputerSet":
					case "ResourceUriComputerSet":
					case "QueryComputerSet":
					{
						IEnumerator<string> enumerator1 = computerNames.GetEnumerator();
						using (enumerator1)
						{
							while (enumerator1.MoveNext())
							{
								string current1 = enumerator1.Current;
								cimSessionProxies.Add(this.CreateSessionProxy(current1, cmdlet));
							}
						}
					}
					break;
					case "ClassNameSessionSet":
					case "CimClassSessionSet":
					case "QuerySessionSet":
					case "CimInstanceSessionSet":
					case "ResourceUriSessionSet":
					{
						CimSession[] cimSession = cmdlet.CimSession;
						for (int i = 0; i < (int)cimSession.Length; i++)
						{
							CimSession cimSession1 = cimSession[i];
							CimSessionProxy cimSessionProxy = this.CreateSessionProxy(cimSession1, cmdlet);
							cimSessionProxies.Add(cimSessionProxy);
						}
					}
					break;
				}
			}
			CimMethodParametersCollection cimMethodParametersCollection = this.CreateParametersCollection(cmdlet.Arguments, cmdlet.CimClass, cmdlet.CimInstance, cmdlet.MethodName);
			string parameterSetName1 = cmdlet.ParameterSetName;
			string str2 = parameterSetName1;
			if (parameterSetName1 != null)
			{
				if (str2 == "ClassNameComputerSet" || str2 == "ClassNameSessionSet" || str2 == "ResourceUriSessionSet" || str2 == "ResourceUriComputerSet")
				{
					object[] className = new object[1];
					className[0] = cmdlet.ClassName;
					string str3 = string.Format(CultureInfo.CurrentUICulture, "{0}", className);
					if (cmdlet.ResourceUri == null)
					{
						@namespace = ConstValue.GetNamespace(cmdlet.Namespace);
					}
					else
					{
						@namespace = cmdlet.Namespace;
					}
					List<CimSessionProxy>.Enumerator enumerator2 = cimSessionProxies.GetEnumerator();
					try
					{
						while (enumerator2.MoveNext())
						{
							CimSessionProxy cimSessionProxy1 = enumerator2.Current;
							if (cmdlet.ShouldProcess(str3, str))
							{
								cimSessionProxy1.InvokeMethodAsync(@namespace, cmdlet.ClassName, cmdlet.MethodName, cimMethodParametersCollection);
							}
							else
							{
								return;
							}
						}
						return;
					}
					finally
					{
						enumerator2.Dispose();
					}
				}
				else if (str2 == "CimClassComputerSet" || str2 == "CimClassSessionSet")
				{
					object[] objArray = new object[1];
					objArray[0] = cmdlet.CimClass.CimSystemProperties.ClassName;
					string str4 = string.Format(CultureInfo.CurrentUICulture, "{0}", objArray);
					@namespace = ConstValue.GetNamespace(cmdlet.CimClass.CimSystemProperties.Namespace);
					List<CimSessionProxy>.Enumerator enumerator3 = cimSessionProxies.GetEnumerator();
					try
					{
						while (enumerator3.MoveNext())
						{
							CimSessionProxy current2 = enumerator3.Current;
							if (cmdlet.ShouldProcess(str4, str))
							{
								current2.InvokeMethodAsync(@namespace, cmdlet.CimClass.CimSystemProperties.ClassName, cmdlet.MethodName, cimMethodParametersCollection);
							}
							else
							{
								return;
							}
						}
						return;
					}
					finally
					{
						enumerator3.Dispose();
					}
				}
				else if (str2 == "QueryComputerSet" || str2 == "QuerySessionSet")
				{
					@namespace = ConstValue.GetNamespace(cmdlet.Namespace);
					List<CimSessionProxy>.Enumerator enumerator4 = cimSessionProxies.GetEnumerator();
					try
					{
						while (enumerator4.MoveNext())
						{
							CimSessionProxy cimSessionProxy2 = enumerator4.Current;
							CimInvokeCimMethod.CimInvokeCimMethodContext cimInvokeCimMethodContext = new CimInvokeCimMethod.CimInvokeCimMethodContext(@namespace, cmdlet.MethodName, cimMethodParametersCollection, cimSessionProxy2);
							cimSessionProxy2.ContextObject = cimInvokeCimMethodContext;
							cimSessionProxy2.QueryInstancesAsync(@namespace, ConstValue.GetQueryDialectWithDefault(cmdlet.QueryDialect), cmdlet.Query);
						}
						return;
					}
					finally
					{
						enumerator4.Dispose();
					}
				}
				else if (str2 == "CimInstanceComputerSet" || str2 == "CimInstanceSessionSet")
				{
					string str5 = cmdlet.CimInstance.ToString();
					if (cmdlet.ResourceUri == null)
					{
						@namespace = ConstValue.GetNamespace(cmdlet.CimInstance.CimSystemProperties.Namespace);
					}
					else
					{
						@namespace = cmdlet.Namespace;
					}
					List<CimSessionProxy>.Enumerator enumerator5 = cimSessionProxies.GetEnumerator();
					try
					{
						while (enumerator5.MoveNext())
						{
							CimSessionProxy current3 = enumerator5.Current;
							if (cmdlet.ShouldProcess(str5, str))
							{
								current3.InvokeMethodAsync(@namespace, cmdlet.CimInstance, cmdlet.MethodName, cimMethodParametersCollection);
							}
							else
							{
								return;
							}
						}
						return;
					}
					finally
					{
						enumerator5.Dispose();
					}
				}
				return;
			}
		}

		public void InvokeCimMethodOnCimInstance(CimInstance cimInstance, XOperationContextBase context, CmdletOperationBase operation)
		{
			DebugHelper.WriteLogEx();
			CimInvokeCimMethod.CimInvokeCimMethodContext cimInvokeCimMethodContext = context as CimInvokeCimMethod.CimInvokeCimMethodContext;
			object[] methodName = new object[1];
			methodName[0] = cimInvokeCimMethodContext.MethodName;
			string str = string.Format(CultureInfo.CurrentUICulture, "Invoke-CimMethod: {0}", methodName);
			if (operation.ShouldProcess(cimInstance.ToString(), str))
			{
				CimSessionProxy cimSessionProxy = base.CreateCimSessionProxy(cimInvokeCimMethodContext.Proxy);
				cimSessionProxy.InvokeMethodAsync(cimInvokeCimMethodContext.Namespace, cimInstance, cimInvokeCimMethodContext.MethodName, cimInvokeCimMethodContext.ParametersCollection);
				return;
			}
			else
			{
				return;
			}
		}

		private void SetSessionProxyProperties(ref CimSessionProxy proxy, InvokeCimMethodCommand cmdlet)
		{
			proxy.OperationTimeout = cmdlet.OperationTimeoutSec;
			if (cmdlet.ResourceUri != null)
			{
				proxy.ResourceUri = cmdlet.ResourceUri;
			}
		}

		internal class CimInvokeCimMethodContext : XOperationContextBase
		{
			private string methodName;

			private CimMethodParametersCollection collection;

			internal string MethodName
			{
				get
				{
					return this.methodName;
				}
			}

			internal CimMethodParametersCollection ParametersCollection
			{
				get
				{
					return this.collection;
				}
			}

			internal CimInvokeCimMethodContext(string theNamespace, string theMethodName, CimMethodParametersCollection theCollection, CimSessionProxy theProxy)
			{
				this.proxy = theProxy;
				this.methodName = theMethodName;
				this.collection = theCollection;
				this.nameSpace = theNamespace;
			}
		}
	}
}