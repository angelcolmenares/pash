using Microsoft.PowerShell.Commands.Management;
using System;
using System.Collections;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Runtime.InteropServices;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Invoke", "WmiMethod", DefaultParameterSetName="class", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113346", RemotingCapability=RemotingCapability.OwnedByCommand)]
	public sealed class InvokeWmiMethod : WmiBaseCmdlet
	{
		private string path;

		private string className;

		private string methodName;

		private ManagementObject inputObject;

		private object[] argumentList;

		[Alias(new string[] { "Args" })]
		[Parameter(ParameterSetName="object")]
		[Parameter(ParameterSetName="path")]
		[Parameter(Position=2, ParameterSetName="class")]
		public object[] ArgumentList
		{
			get
			{
				return this.argumentList;
			}
			set
			{
				this.argumentList = value;
			}
		}

		[Parameter(Position=0, Mandatory=true, ParameterSetName="class")]
		public string Class
		{
			get
			{
				return this.className;
			}
			set
			{
				this.className = value;
			}
		}

		[Parameter(ValueFromPipeline=true, Mandatory=true, ParameterSetName="object")]
		public ManagementObject InputObject
		{
			get
			{
				return this.inputObject;
			}
			set
			{
				this.inputObject = value;
			}
		}

		[Parameter(Position=1, Mandatory=true)]
		public string Name
		{
			get
			{
				return this.methodName;
			}
			set
			{
				this.methodName = value;
			}
		}

		[Parameter(ParameterSetName="path", Mandatory=true)]
		public string Path
		{
			get
			{
				return this.path;
			}
			set
			{
				this.path = value;
			}
		}

		public InvokeWmiMethod()
		{
		}

		private static object MakeBaseObjectArray(object argument)
		{
			bool flag = false;
			object obj;
			if (argument != null)
			{
				IList lists = argument as IList;
				if (lists != null)
				{
					foreach (object obj1 in lists)
					{
						if (obj1 as PSObject == null)
						{
							continue;
						}
						flag = true;
						break;
					}
					if (!flag)
					{
						return argument;
					}
					else
					{
						object[] objArray = new object[lists.Count];
						int num = 0;
						foreach (object obj2 in lists)
						{
							object[] objArray1 = objArray;
							int num1 = num;
							num = num1 + 1;
							if (obj2 != null)
							{
								obj = PSObject.Base(obj2);
							}
							else
							{
								obj = null;
							}
							objArray1[num1] = obj;
						}
						return objArray;
					}
				}
				else
				{
					object[] objArray2 = new object[1];
					objArray2[0] = argument;
					return objArray2;
				}
			}
			else
			{
				return null;
			}
		}

		protected override void ProcessRecord()
		{
			ManagementObject managementObject;
			if (!base.AsJob)
			{
				if (this.inputObject == null)
				{
					ConnectionOptions connectionOption = base.GetConnectionOption();
					ManagementPath managementPath = null;
					object obj = null;
					if (this.path != null)
					{
						managementPath = new ManagementPath(this.path);
						if (!string.IsNullOrEmpty(managementPath.NamespacePath))
						{
							if (this.namespaceSpecified)
							{
								base.ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(), "NamespaceSpecifiedWithPath", ErrorCategory.InvalidOperation, base.Namespace));
							}
						}
						else
						{
							managementPath.NamespacePath = base.Namespace;
						}
						if (managementPath.Server != "." && this.serverNameSpecified)
						{
							base.ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(), "ComputerNameSpecifiedWithPath", ErrorCategory.InvalidOperation, base.ComputerName));
						}
						if (!(managementPath.Server == ".") || !this.serverNameSpecified)
						{
							string[] server = new string[1];
							server[0] = managementPath.Server;
							string[] strArrays = server;
							base.ComputerName = strArrays;
						}
					}
					string[] computerName = base.ComputerName;
					for (int i = 0; i < (int)computerName.Length; i++)
					{
						string str = computerName[i];
						obj = null;
						try
						{
							if (this.path == null)
							{
								ManagementScope managementScope = new ManagementScope(WMIHelper.GetScopeString(str, base.Namespace), connectionOption);
								ManagementClass managementClass = new ManagementClass(this.className);
								managementObject = managementClass;
								managementObject.Scope = managementScope;
							}
							else
							{
								managementPath.Server = str;
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
							ManagementBaseObject methodParameters = managementObject.GetMethodParameters(this.methodName);
							if (this.argumentList != null)
							{
								int length = (int)this.argumentList.Length;
								foreach (PropertyData property in methodParameters.Properties)
								{
									if (length == 0)
									{
										break;
									}
									object obj1 = PSObject.Base(this.argumentList[(int)this.argumentList.Length - length]);
									if (!property.IsArray)
									{
										property.Value = obj1;
									}
									else
									{
										property.Value = InvokeWmiMethod.MakeBaseObjectArray(obj1);
									}
									length--;
								}
							}
							if (base.ShouldProcess(StringUtil.Format(WmiResources.WmiMethodNameForConfirmation, managementObject["__CLASS"].ToString(), this.Name)))
							{
								obj = managementObject.InvokeMethod(this.methodName, methodParameters, null);
							}
							else
							{
								return;
							}
						}
						catch (ManagementException managementException1)
						{
							ManagementException managementException = managementException1;
							ErrorRecord errorRecord = new ErrorRecord(managementException, "InvokeWMIManagementException", ErrorCategory.InvalidOperation, null);
							base.WriteError(errorRecord);
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							ErrorRecord errorRecord1 = new ErrorRecord(cOMException, "InvokeWMICOMException", ErrorCategory.InvalidOperation, null);
							base.WriteError(errorRecord1);
						}
						if (obj != null)
						{
							base.WriteObject(obj);
						}
					}
					return;
				}
				else
				{
					object obj2 = null;
					try
					{
						ManagementBaseObject managementBaseObject = this.inputObject.GetMethodParameters(this.methodName);
						if (this.argumentList != null)
						{
							int num = (int)this.argumentList.Length;
							foreach (PropertyData propertyDatum in managementBaseObject.Properties)
							{
								if (num == 0)
								{
									break;
								}
								propertyDatum.Value = this.argumentList[(int)this.argumentList.Length - num];
								num--;
							}
						}
						if (base.ShouldProcess(StringUtil.Format(WmiResources.WmiMethodNameForConfirmation, this.inputObject["__CLASS"].ToString(), this.Name)))
						{
							obj2 = this.inputObject.InvokeMethod(this.methodName, managementBaseObject, null);
						}
						else
						{
							return;
						}
					}
					catch (ManagementException managementException3)
					{
						ManagementException managementException2 = managementException3;
						ErrorRecord errorRecord2 = new ErrorRecord(managementException2, "InvokeWMIManagementException", ErrorCategory.InvalidOperation, null);
						base.WriteError(errorRecord2);
					}
					catch (COMException cOMException3)
					{
						COMException cOMException2 = cOMException3;
						ErrorRecord errorRecord3 = new ErrorRecord(cOMException2, "InvokeWMICOMException", ErrorCategory.InvalidOperation, null);
						base.WriteError(errorRecord3);
					}
					if (obj2 != null)
					{
						base.WriteObject(obj2);
					}
				}
				return;
			}
			else
			{
				base.RunAsJob("Invoke-WMIMethod");
				return;
			}
		}
	}
}