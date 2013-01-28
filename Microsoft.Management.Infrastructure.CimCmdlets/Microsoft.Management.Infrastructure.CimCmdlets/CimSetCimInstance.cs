using Microsoft.Management.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal sealed class CimSetCimInstance : CimGetInstance
	{
		private const string action = "Set-CimInstance";

		public CimSetCimInstance()
		{
		}

		public void SetCimInstance(SetCimInstanceCommand cmdlet)
		{
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
						cimSessionProxies.Add(base.CreateSessionProxy(computerName, cmdlet.CimInstance, cmdlet, cmdlet.PassThru));
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
							cimSessionProxies.Add(base.CreateSessionProxy(cimSession1, cmdlet, cmdlet.PassThru));
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
					string @namespace = ConstValue.GetNamespace(CimGetInstance.GetCimInstanceParameter(cmdlet).CimSystemProperties.Namespace);
					string str2 = cmdlet.CimInstance.ToString();
					foreach (CimSessionProxy cimSessionProxy in cimSessionProxies)
					{
						if (cmdlet.ShouldProcess(str2, "Set-CimInstance"))
						{
							Exception exception = null;
							CimInstance cimInstance = cmdlet.CimInstance;
							if (cmdlet.Property == null || this.SetProperty(cmdlet.Property, ref cimInstance, ref exception))
							{
								cimSessionProxy.ModifyInstanceAsync(@namespace, cimInstance);
							}
							else
							{
								cmdlet.ThrowTerminatingError(exception, "Set-CimInstance");
								return;
							}
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

		public void SetCimInstance(CimInstance cimInstance, CimSetCimInstanceContext context, CmdletOperationBase cmdlet)
		{
			DebugHelper.WriteLog("CimSetCimInstance::SetCimInstance", 4);
			if (cmdlet.ShouldProcess(cimInstance.ToString(), "Set-CimInstance"))
			{
				Exception exception = null;
				if (this.SetProperty(context.Property, ref cimInstance, ref exception))
				{
					CimSessionProxy cimSessionProxy = base.CreateCimSessionProxy(context.Proxy, context.PassThru);
					cimSessionProxy.ModifyInstanceAsync(cimInstance.CimSystemProperties.Namespace, cimInstance);
					return;
				}
				else
				{
					cmdlet.ThrowTerminatingError(exception, "Set-CimInstance");
					return;
				}
			}
			else
			{
				return;
			}
		}

		private bool SetProperty(IDictionary properties, ref CimInstance cimInstance, ref Exception exception)
		{
			CimProperty cimProperty;
			bool flag;
			DebugHelper.WriteLogEx();
			if (properties.Count != 0)
			{
				IDictionaryEnumerator enumerator = properties.GetEnumerator();
				while (enumerator.MoveNext())
				{
					object baseObject = base.GetBaseObject(enumerator.Value);
					string str = enumerator.Key.ToString();
					object[] objArray = new object[2];
					objArray[0] = str;
					objArray[1] = baseObject;
					DebugHelper.WriteLog("Input property name '{0}' with value '{1}'", 1, objArray);
					try
					{
						CimProperty item = cimInstance.CimInstanceProperties[str];
						if (item == null)
						{
							if (baseObject != null)
							{
								CimType cimType = CimType.Unknown;
								object referenceOrReferenceArrayObject = base.GetReferenceOrReferenceArrayObject(baseObject, ref cimType);
								if (referenceOrReferenceArrayObject == null)
								{
									cimProperty = CimProperty.Create(str, baseObject, (CimFlags)((long)4));
								}
								else
								{
									cimProperty = CimProperty.Create(str, referenceOrReferenceArrayObject, cimType, (CimFlags)((long)4));
								}
							}
							else
							{
								cimProperty = CimProperty.Create(str, baseObject, CimType.String, (CimFlags)((long)4));
							}
							try
							{
								cimInstance.CimInstanceProperties.Add(cimProperty);
							}
							catch (CimException cimException1)
							{
								CimException cimException = cimException1;
								if (cimException.NativeErrorCode != NativeErrorCode.Failed)
								{
									exception = cimException;
								}
								else
								{
									object[] name = new object[2];
									name[0] = cimProperty.Name;
									name[1] = cimInstance;
									string str1 = string.Format(CultureInfo.CurrentUICulture, Strings.UnableToAddPropertyToInstance, name);
									exception = new CimException(str1, cimException);
								}
								flag = false;
								return flag;
							}
							object[] objArray1 = new object[2];
							objArray1[0] = str;
							objArray1[1] = baseObject;
							DebugHelper.WriteLog("Add non-key property name '{0}' with value '{1}'.", 3, objArray1);
						}
						else
						{
							if ((item.Flags & CimFlags.ReadOnly) != CimFlags.ReadOnly)
							{
								object[] value = new object[2];
								value[0] = str;
								value[1] = item.Value;
								DebugHelper.WriteLog("Set property name '{0}' has old value '{1}'", 4, value);
								item.Value = baseObject;
							}
							else
							{
								object[] objArray2 = new object[2];
								objArray2[0] = str;
								objArray2[1] = cimInstance;
								exception = new CimException(string.Format(CultureInfo.CurrentUICulture, Strings.CouldNotModifyReadonlyProperty, objArray2));
								flag = false;
								return flag;
							}
						}
						continue;
					}
					catch (Exception exception2)
					{
						Exception exception1 = exception2;
						object[] objArray3 = new object[1];
						objArray3[0] = exception1;
						DebugHelper.WriteLog("Exception {0}", 4, objArray3);
						exception = exception1;
						flag = false;
					}
					return flag;
				}
				return true;
			}
			else
			{
				return true;
			}
		}
	}
}