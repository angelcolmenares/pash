using Microsoft.PowerShell.Commands.Internal;
using Microsoft.PowerShell.Commands.Management;
using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Management.Automation.Remoting;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Microsoft.PowerShell.Commands
{
	internal class WmiAsyncCmdletHelper : AsyncCmdletHelper
	{
		private string computerName;

		private ManagementOperationObserver results;

		private int cmdCount;

		private PSWmiChildJob Job;

		private WmiState state;

		private Cmdlet wmiObject;

		internal WmiState State
		{
			get
			{
				return this.state;
			}
			set
			{
				this.state = value;
			}
		}

		internal WmiAsyncCmdletHelper(PSWmiChildJob childJob, Cmdlet wmiObject, string computerName, ManagementOperationObserver results)
		{
			this.cmdCount = 1;
			this.wmiObject = wmiObject;
			this.computerName = computerName;
			this.results = results;
			this.State = WmiState.NotStarted;
			this.Job = childJob;
		}

		internal WmiAsyncCmdletHelper(PSWmiChildJob childJob, Cmdlet wmiObject, string computerName, ManagementOperationObserver results, int count) : this(childJob, wmiObject, computerName, results)
		{
			this.cmdCount = count;
		}

		private void ConnectGetWMI()
		{
			string wmiQueryString;
			GetWmiObjectCommand getWmiObjectCommand = (GetWmiObjectCommand)this.wmiObject;
			this.state = WmiState.Running;
			this.RaiseWmiOperationState(null, WmiState.Running);
			ConnectionOptions connectionOption = getWmiObjectCommand.GetConnectionOption();
			SwitchParameter list = getWmiObjectCommand.List;
			if (!list.IsPresent)
			{
				if (string.IsNullOrEmpty(getWmiObjectCommand.Query))
				{
					wmiQueryString = this.GetWmiQueryString();
				}
				else
				{
					wmiQueryString = getWmiObjectCommand.Query;
				}
				string str = wmiQueryString;
				ObjectQuery objectQuery = new ObjectQuery(str.ToString());
				try
				{
					ManagementScope managementScope = new ManagementScope(WMIHelper.GetScopeString(this.computerName, getWmiObjectCommand.Namespace), connectionOption);
					EnumerationOptions enumerationOption = new EnumerationOptions();
					enumerationOption.UseAmendedQualifiers = getWmiObjectCommand.Amended;
					enumerationOption.DirectRead = getWmiObjectCommand.DirectRead;
					ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(managementScope, objectQuery, enumerationOption);
					for (int i = 0; i < this.cmdCount; i++)
					{
						managementObjectSearcher.Get(this.results);
					}
				}
				catch (ManagementException managementException1)
				{
					ManagementException managementException = managementException1;
					this.internalException = managementException;
					this.state = WmiState.Failed;
					this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					this.internalException = cOMException;
					this.state = WmiState.Failed;
					this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
				}
				catch (UnauthorizedAccessException unauthorizedAccessException1)
				{
					UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
					this.internalException = unauthorizedAccessException;
					this.state = WmiState.Failed;
					this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
				}
				return;
			}
			else
			{
				if (getWmiObjectCommand.ValidateClassFormat())
				{
					try
					{
						SwitchParameter recurse = getWmiObjectCommand.Recurse;
						if (!recurse.IsPresent)
						{
							ManagementScope managementScope1 = new ManagementScope(WMIHelper.GetScopeString(this.computerName, getWmiObjectCommand.Namespace), connectionOption);
							managementScope1.Connect();
							ManagementObjectSearcher objectList = getWmiObjectCommand.GetObjectList(managementScope1);
							if (objectList != null)
							{
								objectList.Get(this.results);
							}
							else
							{
								throw new ManagementException();
							}
						}
						else
						{
							ArrayList arrayLists = new ArrayList();
							ArrayList arrayLists1 = new ArrayList();
							ArrayList arrayLists2 = new ArrayList();
							int num = 0;
							arrayLists.Add(getWmiObjectCommand.Namespace);
							bool flag = true;
							while (num < arrayLists.Count)
							{
								string item = (string)arrayLists[num];
								ManagementScope managementScope2 = new ManagementScope(WMIHelper.GetScopeString(this.computerName, item), connectionOption);
								managementScope2.Connect();
								ManagementClass managementClass = new ManagementClass(managementScope2, new ManagementPath("__Namespace"), new ObjectGetOptions());
								foreach (ManagementBaseObject instance in managementClass.GetInstances())
								{
									if (getWmiObjectCommand.IsLocalizedNamespace((string)instance["Name"]))
									{
										continue;
									}
									arrayLists.Add(string.Concat(item, "\\", instance["Name"]));
								}
								if (!flag)
								{
									arrayLists1.Add(this.Job.GetNewSink());
								}
								else
								{
									flag = false;
									arrayLists1.Add(this.results);
								}
								arrayLists2.Add(managementScope2);
								num++;
							}
							if (arrayLists1.Count != arrayLists.Count || arrayLists2.Count != arrayLists.Count)
							{
								this.internalException = new InvalidOperationException();
								this.state = WmiState.Failed;
								this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
								return;
							}
							else
							{
								num = 0;
								while (num < arrayLists.Count)
								{
									ManagementObjectSearcher objectList1 = getWmiObjectCommand.GetObjectList((ManagementScope)arrayLists2[num]);
									if (objectList1 != null)
									{
										if (!flag)
										{
											objectList1.Get((ManagementOperationObserver)arrayLists1[num]);
										}
										else
										{
											flag = false;
											objectList1.Get(this.results);
										}
										num++;
									}
									else
									{
										num++;
									}
								}
							}
						}
					}
					catch (ManagementException managementException3)
					{
						ManagementException managementException2 = managementException3;
						this.internalException = managementException2;
						this.state = WmiState.Failed;
						this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
					}
					catch (COMException cOMException3)
					{
						COMException cOMException2 = cOMException3;
						this.internalException = cOMException2;
						this.state = WmiState.Failed;
						this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
					}
					catch (UnauthorizedAccessException unauthorizedAccessException3)
					{
						UnauthorizedAccessException unauthorizedAccessException2 = unauthorizedAccessException3;
						this.internalException = unauthorizedAccessException2;
						this.state = WmiState.Failed;
						this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
					}
					return;
				}
				else
				{
					object[] @class = new object[1];
					@class[0] = getWmiObjectCommand.Class;
					ArgumentException argumentException = new ArgumentException(string.Format(Thread.CurrentThread.CurrentCulture, "Class", @class));
					this.internalException = argumentException;
					this.state = WmiState.Failed;
					this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
					return;
				}
			}
		}

		private void ConnectInvokeWmi()
		{
			ManagementObject managementObject;
			object obj;
			string str;
			InvokeWmiMethod invokeWmiMethod = (InvokeWmiMethod)this.wmiObject;
			this.state = WmiState.Running;
			this.RaiseWmiOperationState(null, WmiState.Running);
			if (invokeWmiMethod.InputObject == null)
			{
				ConnectionOptions connectionOption = invokeWmiMethod.GetConnectionOption();
				ManagementPath managementPath = null;
				if (invokeWmiMethod.Path != null)
				{
					managementPath = new ManagementPath(invokeWmiMethod.Path);
					if (!string.IsNullOrEmpty(managementPath.NamespacePath))
					{
						if (invokeWmiMethod.namespaceSpecified)
						{
							InvalidOperationException invalidOperationException = new InvalidOperationException("NamespaceSpecifiedWithPath");
							this.internalException = invalidOperationException;
							this.state = WmiState.Failed;
							this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
							return;
						}
					}
					else
					{
						managementPath.NamespacePath = invokeWmiMethod.Namespace;
					}
					if (!(managementPath.Server != ".") || !invokeWmiMethod.serverNameSpecified)
					{
						if (!(managementPath.Server == ".") || !invokeWmiMethod.serverNameSpecified)
						{
							this.computerName = managementPath.Server;
						}
					}
					else
					{
						InvalidOperationException invalidOperationException1 = new InvalidOperationException("ComputerNameSpecifiedWithPath");
						this.internalException = invalidOperationException1;
						this.state = WmiState.Failed;
						this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
						return;
					}
				}
				bool flag = false;
				bool enablePrivilege = false;
				Win32Native.TOKEN_PRIVILEGE tOKENPRIVILEGE = new Win32Native.TOKEN_PRIVILEGE();
				try
				{
					try
					{
						enablePrivilege = this.NeedToEnablePrivilege(this.computerName, invokeWmiMethod.Name, ref flag);
						if (!enablePrivilege || flag && ComputerWMIHelper.EnableTokenPrivilege("SeShutdownPrivilege", ref tOKENPRIVILEGE) || !flag && ComputerWMIHelper.EnableTokenPrivilege("SeRemoteShutdownPrivilege", ref tOKENPRIVILEGE))
						{
							if (invokeWmiMethod.Path == null)
							{
								ManagementScope managementScope = new ManagementScope(WMIHelper.GetScopeString(this.computerName, invokeWmiMethod.Namespace), connectionOption);
								ManagementClass managementClass = new ManagementClass(invokeWmiMethod.Class);
								managementObject = managementClass;
								managementObject.Scope = managementScope;
							}
							else
							{
								managementPath.Server = this.computerName;
								if (!managementPath.IsClass)
								{
									ManagementObject managementObject1 = new ManagementObject(managementPath);
									managementObject = managementObject1;
								}
								else
								{
									ManagementClass managementClass1 = new ManagementClass(managementPath);
									managementObject = managementClass1;
								}
								ManagementScope managementScope1 = new ManagementScope(managementPath, connectionOption);
								managementObject.Scope = managementScope1;
							}
							ManagementBaseObject methodParameters = managementObject.GetMethodParameters(invokeWmiMethod.Name);
							if (invokeWmiMethod.ArgumentList != null)
							{
								int length = (int)invokeWmiMethod.ArgumentList.Length;
								foreach (PropertyData property in methodParameters.Properties)
								{
									if (length == 0)
									{
										break;
									}
									property.Value = invokeWmiMethod.ArgumentList[(int)invokeWmiMethod.ArgumentList.Length - length];
									length--;
								}
							}
							if (!enablePrivilege)
							{
								managementObject.InvokeMethod(this.results, invokeWmiMethod.Name, methodParameters, null);
							}
							else
							{
								ManagementBaseObject managementBaseObject = managementObject.InvokeMethod(invokeWmiMethod.Name, methodParameters, null);
								int num = Convert.ToInt32(managementBaseObject["ReturnValue"], CultureInfo.CurrentCulture);
								if (num == 0)
								{
									this.ShutdownComplete.SafeInvoke<EventArgs>(this, null);
								}
								else
								{
									Win32Exception win32Exception = new Win32Exception(num);
									this.internalException = win32Exception;
									this.state = WmiState.Failed;
									this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
								}
							}
						}
						else
						{
							string privilegeNotEnabled = ComputerResources.PrivilegeNotEnabled;
							string str1 = this.computerName;
							if (flag)
							{
								obj = "SeShutdownPrivilege";
							}
							else
							{
								obj = "SeRemoteShutdownPrivilege";
							}
							string str2 = StringUtil.Format(privilegeNotEnabled, str1, obj);
							InvalidOperationException invalidOperationException2 = new InvalidOperationException(str2);
							this.internalException = invalidOperationException2;
							this.state = WmiState.Failed;
							this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
							return;
						}
					}
					catch (ManagementException managementException1)
					{
						ManagementException managementException = managementException1;
						this.internalException = managementException;
						this.state = WmiState.Failed;
						this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						this.internalException = cOMException;
						this.state = WmiState.Failed;
						this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
					}
					catch (UnauthorizedAccessException unauthorizedAccessException1)
					{
						UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
						this.internalException = unauthorizedAccessException;
						this.state = WmiState.Failed;
						this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
					}
				}
				finally
				{
					if (enablePrivilege)
					{
						if (flag)
						{
							str = "SeShutdownPrivilege";
						}
						else
						{
							str = "SeRemoteShutdownPrivilege";
						}
						ComputerWMIHelper.RestoreTokenPrivilege(str, ref tOKENPRIVILEGE);
					}
				}
				return;
			}
			else
			{
				try
				{
					ManagementBaseObject methodParameters1 = invokeWmiMethod.InputObject.GetMethodParameters(invokeWmiMethod.Name);
					if (invokeWmiMethod.ArgumentList != null)
					{
						int length1 = (int)invokeWmiMethod.ArgumentList.Length;
						foreach (PropertyData argumentList in methodParameters1.Properties)
						{
							if (length1 == 0)
							{
								break;
							}
							argumentList.Value = invokeWmiMethod.ArgumentList[(int)invokeWmiMethod.ArgumentList.Length - length1];
							length1--;
						}
					}
					invokeWmiMethod.InputObject.InvokeMethod(this.results, invokeWmiMethod.Name, methodParameters1, null);
				}
				catch (ManagementException managementException3)
				{
					ManagementException managementException2 = managementException3;
					this.internalException = managementException2;
					this.state = WmiState.Failed;
					this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
				}
				catch (COMException cOMException3)
				{
					COMException cOMException2 = cOMException3;
					this.internalException = cOMException2;
					this.state = WmiState.Failed;
					this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
				}
				catch (UnauthorizedAccessException unauthorizedAccessException3)
				{
					UnauthorizedAccessException unauthorizedAccessException2 = unauthorizedAccessException3;
					this.internalException = unauthorizedAccessException2;
					this.state = WmiState.Failed;
					this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
				}
				return;
			}
		}

		private void ConnectRemoveWmi()
		{
			ManagementObject managementObject;
			RemoveWmiObject removeWmiObject = (RemoveWmiObject)this.wmiObject;
			this.state = WmiState.Running;
			this.RaiseWmiOperationState(null, WmiState.Running);
			if (removeWmiObject.InputObject == null)
			{
				ConnectionOptions connectionOption = removeWmiObject.GetConnectionOption();
				ManagementPath managementPath = null;
				if (removeWmiObject.Path != null)
				{
					managementPath = new ManagementPath(removeWmiObject.Path);
					if (!string.IsNullOrEmpty(managementPath.NamespacePath))
					{
						if (removeWmiObject.namespaceSpecified)
						{
							InvalidOperationException invalidOperationException = new InvalidOperationException("NamespaceSpecifiedWithPath");
							this.internalException = invalidOperationException;
							this.state = WmiState.Failed;
							this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
							return;
						}
					}
					else
					{
						managementPath.NamespacePath = removeWmiObject.Namespace;
					}
					if (!(managementPath.Server != ".") || !removeWmiObject.serverNameSpecified)
					{
						if (!(managementPath.Server == ".") || !removeWmiObject.serverNameSpecified)
						{
							this.computerName = managementPath.Server;
						}
					}
					else
					{
						InvalidOperationException invalidOperationException1 = new InvalidOperationException("ComputerNameSpecifiedWithPath");
						this.internalException = invalidOperationException1;
						this.state = WmiState.Failed;
						this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
						return;
					}
				}
				try
				{
					if (removeWmiObject.Path == null)
					{
						ManagementScope managementScope = new ManagementScope(WMIHelper.GetScopeString(this.computerName, removeWmiObject.Namespace), connectionOption);
						ManagementClass managementClass = new ManagementClass(removeWmiObject.Class);
						managementObject = managementClass;
						managementObject.Scope = managementScope;
					}
					else
					{
						managementPath.Server = this.computerName;
						if (!managementPath.IsClass)
						{
							ManagementObject managementObject1 = new ManagementObject(managementPath);
							managementObject = managementObject1;
						}
						else
						{
							ManagementClass managementClass1 = new ManagementClass(managementPath);
							managementObject = managementClass1;
						}
						ManagementScope managementScope1 = new ManagementScope(managementPath, connectionOption);
						managementObject.Scope = managementScope1;
					}
					managementObject.Delete(this.results);
				}
				catch (ManagementException managementException1)
				{
					ManagementException managementException = managementException1;
					this.internalException = managementException;
					this.state = WmiState.Failed;
					this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					this.internalException = cOMException;
					this.state = WmiState.Failed;
					this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
				}
				catch (UnauthorizedAccessException unauthorizedAccessException1)
				{
					UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
					this.internalException = unauthorizedAccessException;
					this.state = WmiState.Failed;
					this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
				}
				return;
			}
			else
			{
				try
				{
					removeWmiObject.InputObject.Delete(this.results);
				}
				catch (ManagementException managementException3)
				{
					ManagementException managementException2 = managementException3;
					this.internalException = managementException2;
					this.state = WmiState.Failed;
					this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
				}
				catch (COMException cOMException3)
				{
					COMException cOMException2 = cOMException3;
					this.internalException = cOMException2;
					this.state = WmiState.Failed;
					this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
				}
				catch (UnauthorizedAccessException unauthorizedAccessException3)
				{
					UnauthorizedAccessException unauthorizedAccessException2 = unauthorizedAccessException3;
					this.internalException = unauthorizedAccessException2;
					this.state = WmiState.Failed;
					this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
				}
				return;
			}
		}

		private void ConnectSetWmi()
		{
			ManagementObject value;
			SetWmiInstance setWmiInstance = (SetWmiInstance)this.wmiObject;
			this.state = WmiState.Running;
			this.RaiseWmiOperationState(null, WmiState.Running);
			if (setWmiInstance.InputObject == null)
			{
				ManagementPath managementPath = null;
				if (setWmiInstance.Class == null)
				{
					managementPath = new ManagementPath(setWmiInstance.Path);
					if (!string.IsNullOrEmpty(managementPath.NamespacePath))
					{
						if (setWmiInstance.namespaceSpecified)
						{
							InvalidOperationException invalidOperationException = new InvalidOperationException("NamespaceSpecifiedWithPath");
							this.internalException = invalidOperationException;
							this.state = WmiState.Failed;
							this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
							return;
						}
					}
					else
					{
						managementPath.NamespacePath = setWmiInstance.Namespace;
					}
					if (!(managementPath.Server != ".") || !setWmiInstance.serverNameSpecified)
					{
						if (!managementPath.IsClass)
						{
							if (!setWmiInstance.flagSpecified)
							{
								setWmiInstance.PutType = PutType.UpdateOrCreate;
							}
							else
							{
								if (setWmiInstance.PutType != PutType.UpdateOnly && setWmiInstance.PutType != PutType.UpdateOrCreate)
								{
									InvalidOperationException invalidOperationException1 = new InvalidOperationException("NonUpdateFlagSpecifiedWithInstancePath");
									this.internalException = invalidOperationException1;
									this.state = WmiState.Failed;
									this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
									return;
								}
							}
						}
						else
						{
							if (!setWmiInstance.flagSpecified || setWmiInstance.PutType == PutType.CreateOnly)
							{
								setWmiInstance.PutType = PutType.CreateOnly;
							}
							else
							{
								InvalidOperationException invalidOperationException2 = new InvalidOperationException("CreateOnlyFlagNotSpecifiedWithClassPath");
								this.internalException = invalidOperationException2;
								this.state = WmiState.Failed;
								this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
								return;
							}
						}
					}
					else
					{
						InvalidOperationException invalidOperationException3 = new InvalidOperationException("ComputerNameSpecifiedWithPath");
						this.internalException = invalidOperationException3;
						this.state = WmiState.Failed;
						this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
						return;
					}
				}
				else
				{
					if (setWmiInstance.flagSpecified && setWmiInstance.PutType != PutType.CreateOnly)
					{
						InvalidOperationException invalidOperationException4 = new InvalidOperationException("CreateOnlyFlagNotSpecifiedWithClassPath");
						this.internalException = invalidOperationException4;
						this.state = WmiState.Failed;
						this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
						return;
					}
					setWmiInstance.PutType = PutType.CreateOnly;
				}
				if (managementPath != null && (!(managementPath.Server == ".") || !setWmiInstance.serverNameSpecified))
				{
					this.computerName = managementPath.Server;
				}
				ConnectionOptions connectionOption = setWmiInstance.GetConnectionOption();
				try
				{
					if (setWmiInstance.Path == null)
					{
						ManagementScope managementScope = new ManagementScope(WMIHelper.GetScopeString(this.computerName, setWmiInstance.Namespace), connectionOption);
						ManagementClass managementClass = new ManagementClass(setWmiInstance.Class);
						managementClass.Scope = managementScope;
						value = managementClass.CreateInstance();
					}
					else
					{
						managementPath.Server = this.computerName;
						ManagementScope managementScope1 = new ManagementScope(managementPath, connectionOption);
						if (!managementPath.IsClass)
						{
							ManagementObject managementObject = new ManagementObject(managementPath);
							managementObject.Scope = managementScope1;
							try
							{
								managementObject.Get();
							}
							catch (ManagementException managementException1)
							{
								ManagementException managementException = managementException1;
								if (managementException.ErrorCode == ManagementStatus.NotFound)
								{
									int num = setWmiInstance.Path.IndexOf(':');
									if (num != -1)
									{
										int num1 = setWmiInstance.Path.Substring(num).IndexOf('.');
										if (num1 != -1)
										{
											string str = setWmiInstance.Path.Substring(0, num1 + num);
											ManagementPath managementPath1 = new ManagementPath(str);
											ManagementClass managementClass1 = new ManagementClass(managementPath1);
											managementClass1.Scope = managementScope1;
											managementObject = managementClass1.CreateInstance();
										}
										else
										{
											this.internalException = managementException;
											this.state = WmiState.Failed;
											this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
											return;
										}
									}
									else
									{
										this.internalException = managementException;
										this.state = WmiState.Failed;
										this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
										return;
									}
								}
								else
								{
									this.internalException = managementException;
									this.state = WmiState.Failed;
									this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
									return;
								}
							}
							value = managementObject;
						}
						else
						{
							ManagementClass managementClass2 = new ManagementClass(managementPath);
							managementClass2.Scope = managementScope1;
							value = managementClass2.CreateInstance();
						}
					}
					if (setWmiInstance.Arguments != null)
					{
						IDictionaryEnumerator enumerator = setWmiInstance.Arguments.GetEnumerator();
						while (enumerator.MoveNext())
						{
							value[enumerator.Key as string] = enumerator.Value;
						}
					}
					PutOptions putOption = new PutOptions();
					putOption.Type = setWmiInstance.PutType;
					if (value == null)
					{
						InvalidOperationException invalidOperationException5 = new InvalidOperationException();
						this.internalException = invalidOperationException5;
						this.state = WmiState.Failed;
						this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
					}
					else
					{
						value.Put(this.results, putOption);
					}
				}
				catch (ManagementException managementException3)
				{
					ManagementException managementException2 = managementException3;
					this.internalException = managementException2;
					this.state = WmiState.Failed;
					this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					this.internalException = cOMException;
					this.state = WmiState.Failed;
					this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
				}
				catch (UnauthorizedAccessException unauthorizedAccessException1)
				{
					UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
					this.internalException = unauthorizedAccessException;
					this.state = WmiState.Failed;
					this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
				}
			}
			else
			{
				ManagementObject value1 = null;
				try
				{
					PutOptions putType = new PutOptions();
					if (setWmiInstance.InputObject.GetType() != typeof(ManagementClass))
					{
						if (!setWmiInstance.flagSpecified)
						{
							setWmiInstance.PutType = PutType.UpdateOrCreate;
						}
						else
						{
							if (setWmiInstance.PutType != PutType.UpdateOnly && setWmiInstance.PutType != PutType.UpdateOrCreate)
							{
								InvalidOperationException invalidOperationException6 = new InvalidOperationException("NonUpdateFlagSpecifiedWithInstancePath");
								this.internalException = invalidOperationException6;
								this.state = WmiState.Failed;
								this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
								return;
							}
						}
						value1 = (ManagementObject)setWmiInstance.InputObject.Clone();
					}
					else
					{
						if (!setWmiInstance.flagSpecified || setWmiInstance.PutType == PutType.CreateOnly)
						{
							value1 = ((ManagementClass)setWmiInstance.InputObject).CreateInstance();
							setWmiInstance.PutType = PutType.CreateOnly;
						}
						else
						{
							InvalidOperationException invalidOperationException7 = new InvalidOperationException("CreateOnlyFlagNotSpecifiedWithClassPath");
							this.internalException = invalidOperationException7;
							this.state = WmiState.Failed;
							this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
							return;
						}
					}
					if (setWmiInstance.Arguments != null)
					{
						IDictionaryEnumerator dictionaryEnumerator = setWmiInstance.Arguments.GetEnumerator();
						while (dictionaryEnumerator.MoveNext())
						{
							value1[dictionaryEnumerator.Key as string] = dictionaryEnumerator.Value;
						}
					}
					putType.Type = setWmiInstance.PutType;
					if (value1 == null)
					{
						InvalidOperationException invalidOperationException8 = new InvalidOperationException();
						this.internalException = invalidOperationException8;
						this.state = WmiState.Failed;
						this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
					}
					else
					{
						value1.Put(this.results, putType);
					}
				}
				catch (ManagementException managementException5)
				{
					ManagementException managementException4 = managementException5;
					this.internalException = managementException4;
					this.state = WmiState.Failed;
					this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
				}
				catch (COMException cOMException3)
				{
					COMException cOMException2 = cOMException3;
					this.internalException = cOMException2;
					this.state = WmiState.Failed;
					this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
				}
				catch (UnauthorizedAccessException unauthorizedAccessException3)
				{
					UnauthorizedAccessException unauthorizedAccessException2 = unauthorizedAccessException3;
					this.internalException = unauthorizedAccessException2;
					this.state = WmiState.Failed;
					this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
				}
			}
		}

		private string GetWmiQueryString()
		{
			GetWmiObjectCommand getWmiObjectCommand = (GetWmiObjectCommand)this.wmiObject;
			StringBuilder stringBuilder = new StringBuilder("select ");
			stringBuilder.Append(string.Join(", ", getWmiObjectCommand.Property));
			stringBuilder.Append(" from ");
			stringBuilder.Append(getWmiObjectCommand.Class);
			if (!string.IsNullOrEmpty(getWmiObjectCommand.Filter))
			{
				stringBuilder.Append(" where ");
				stringBuilder.Append(getWmiObjectCommand.Filter);
			}
			return stringBuilder.ToString();
		}

		private bool NeedToEnablePrivilege(string computer, string methodName, ref bool isLocal)
		{
			bool flag = false;
			if (methodName.Equals("Win32Shutdown", StringComparison.OrdinalIgnoreCase))
			{
				flag = true;
				string hostName = Dns.GetHostName();
				string str = Dns.GetHostEntry("").HostName;
				if (computer.Equals(".") || computer.Equals("localhost", StringComparison.OrdinalIgnoreCase) || computer.Equals(hostName, StringComparison.OrdinalIgnoreCase) || computer.Equals(str, StringComparison.OrdinalIgnoreCase))
				{
					isLocal = true;
				}
			}
			return flag;
		}

		internal void RaiseOperationCompleteEvent(EventArgs baseEventArgs, OperationState state)
		{
			OperationStateEventArgs operationStateEventArg = new OperationStateEventArgs();
			operationStateEventArg.OperationState = state;
			this.OperationComplete.SafeInvoke<OperationStateEventArgs>(this, operationStateEventArg);
		}

		internal void RaiseWmiOperationState(EventArgs baseEventArgs, WmiState state)
		{
			WmiJobStateEventArgs wmiJobStateEventArg = new WmiJobStateEventArgs();
			wmiJobStateEventArg.WmiState = state;
			this.WmiOperationState.SafeInvoke<WmiJobStateEventArgs>(this, wmiJobStateEventArg);
		}

		internal override void StartOperation()
		{
			Thread thread;
			if (this.wmiObject.GetType() != typeof(GetWmiObjectCommand))
			{
				if (this.wmiObject.GetType() != typeof(RemoveWmiObject))
				{
					if (this.wmiObject.GetType() != typeof(InvokeWmiMethod))
					{
						if (this.wmiObject.GetType() != typeof(SetWmiInstance))
						{
							InvalidOperationException invalidOperationException = new InvalidOperationException("This operation is not supported for this cmdlet.");
							this.internalException = invalidOperationException;
							this.state = WmiState.Failed;
							this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
							return;
						}
						else
						{
							thread = new Thread(new ThreadStart(this.ConnectSetWmi));
						}
					}
					else
					{
						thread = new Thread(new ThreadStart(this.ConnectInvokeWmi));
					}
				}
				else
				{
					thread = new Thread(new ThreadStart(this.ConnectRemoveWmi));
				}
			}
			else
			{
				thread = new Thread(new ThreadStart(this.ConnectGetWMI));
			}
			thread.IsBackground = true;
			thread.Start();
		}

		internal override void StopOperation()
		{
			this.results.Cancel();
			this.state = WmiState.Stopped;
			this.RaiseOperationCompleteEvent(null, OperationState.StopComplete);
		}

		internal override event EventHandler<OperationStateEventArgs> OperationComplete;
		internal event EventHandler<EventArgs> ShutdownComplete;
		internal event EventHandler<WmiJobStateEventArgs> WmiOperationState;
	}
}