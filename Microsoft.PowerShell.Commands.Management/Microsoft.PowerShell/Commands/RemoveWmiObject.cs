using System;
using System.Management;
using System.Management.Automation;
using System.Runtime.InteropServices;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Remove", "WmiObject", DefaultParameterSetName="class", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113381", RemotingCapability=RemotingCapability.OwnedByCommand)]
	public class RemoveWmiObject : WmiBaseCmdlet
	{
		private string path;

		private string className;

		private ManagementObject inputObject;

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

		[Parameter(Mandatory=true, ParameterSetName="path")]
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

		public RemoveWmiObject()
		{
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
							if (base.ShouldProcess(managementObject["__PATH"].ToString()))
							{
								managementObject.Delete();
							}
						}
						catch (ManagementException managementException1)
						{
							ManagementException managementException = managementException1;
							ErrorRecord errorRecord = new ErrorRecord(managementException, "RemoveWMIManagementException", ErrorCategory.InvalidOperation, null);
							base.WriteError(errorRecord);
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							ErrorRecord errorRecord1 = new ErrorRecord(cOMException, "RemoveWMICOMException", ErrorCategory.InvalidOperation, null);
							base.WriteError(errorRecord1);
						}
					}
					return;
				}
				else
				{
					try
					{
						if (base.ShouldProcess(this.inputObject["__PATH"].ToString()))
						{
							this.inputObject.Delete();
						}
					}
					catch (ManagementException managementException3)
					{
						ManagementException managementException2 = managementException3;
						ErrorRecord errorRecord2 = new ErrorRecord(managementException2, "RemoveWMIManagementException", ErrorCategory.InvalidOperation, null);
						base.WriteError(errorRecord2);
					}
					catch (COMException cOMException3)
					{
						COMException cOMException2 = cOMException3;
						ErrorRecord errorRecord3 = new ErrorRecord(cOMException2, "RemoveWMICOMException", ErrorCategory.InvalidOperation, null);
						base.WriteError(errorRecord3);
					}
					return;
				}
			}
			else
			{
				base.RunAsJob("Remove-WMIObject");
				return;
			}
		}
	}
}