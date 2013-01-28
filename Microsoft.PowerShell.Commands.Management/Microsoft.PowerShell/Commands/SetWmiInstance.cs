using System;
using System.Collections;
using System.Management;
using System.Management.Automation;
using System.Runtime.InteropServices;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Set", "WmiInstance", DefaultParameterSetName="class", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113402", RemotingCapability=RemotingCapability.OwnedByCommand)]
	public sealed class SetWmiInstance : WmiBaseCmdlet
	{
		internal bool flagSpecified;

		private string path;

		private string className;

		private ManagementObject inputObject;

		private Hashtable propertyBag;

		private PutType putType;

		[Alias(new string[] { "Args", "Property" })]
		[Parameter(ParameterSetName="object")]
		[Parameter(ParameterSetName="path")]
		[Parameter(Position=2, ParameterSetName="class")]
		public Hashtable Arguments
		{
			get
			{
				return this.propertyBag;
			}
			set
			{
				this.propertyBag = value;
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

		[Parameter]
		public PutType PutType
		{
			get
			{
				return this.putType;
			}
			set
			{
				this.putType = value;
				this.flagSpecified = true;
			}
		}

		public SetWmiInstance()
		{
		}

		protected override void ProcessRecord()
		{
			if (!base.AsJob)
			{
				if (this.inputObject == null)
				{
					ManagementPath managementPath = base.SetWmiInstanceBuildManagementPath();
					if (managementPath != null && (!(managementPath.Server == ".") || !this.serverNameSpecified))
					{
						string[] server = new string[1];
						server[0] = managementPath.Server;
						string[] strArrays = server;
						base.ComputerName = strArrays;
					}
					base.GetConnectionOption();
					object obj = null;
					string[] computerName = base.ComputerName;
					for (int i = 0; i < (int)computerName.Length; i++)
					{
						string str = computerName[i];
						obj = null;
						try
						{
							ManagementObject managementObject = base.SetWmiInstanceGetObject(managementPath, str);
							PutOptions putOption = new PutOptions();
							putOption.Type = this.putType;
							if (managementObject == null)
							{
								InvalidOperationException invalidOperationException = new InvalidOperationException();
								throw invalidOperationException;
							}
							else
							{
								if (base.ShouldProcess(managementObject.Path.Path.ToString()))
								{
									managementObject.Put(putOption);
									obj = managementObject;
								}
								else
								{
									goto Label0;
								}
							}
						}
						catch (ManagementException managementException1)
						{
							ManagementException managementException = managementException1;
							ErrorRecord errorRecord = new ErrorRecord(managementException, "SetWMIManagementException", ErrorCategory.InvalidOperation, null);
							base.WriteError(errorRecord);
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							ErrorRecord errorRecord1 = new ErrorRecord(cOMException, "SetWMICOMException", ErrorCategory.InvalidOperation, null);
							base.WriteError(errorRecord1);
						}
						if (obj != null)
						{
							base.WriteObject(obj);
						}
                    Label0:
                        continue;
					}
				}
				else
				{
					object obj1 = null;
					try
					{
						PutOptions putOption1 = new PutOptions();
						ManagementObject managementObject1 = base.SetWmiInstanceGetPipelineObject();
						putOption1.Type = this.putType;
						if (managementObject1 == null)
						{
							InvalidOperationException invalidOperationException1 = new InvalidOperationException();
							throw invalidOperationException1;
						}
						else
						{
							if (base.ShouldProcess(managementObject1.Path.Path.ToString()))
							{
								managementObject1.Put(putOption1);
								obj1 = managementObject1;
							}
							else
							{
								return;
							}
						}
					}
					catch (ManagementException managementException3)
					{
						ManagementException managementException2 = managementException3;
						ErrorRecord errorRecord2 = new ErrorRecord(managementException2, "SetWMIManagementException", ErrorCategory.InvalidOperation, null);
						base.WriteError(errorRecord2);
					}
					catch (COMException cOMException3)
					{
						COMException cOMException2 = cOMException3;
						ErrorRecord errorRecord3 = new ErrorRecord(cOMException2, "SetWMICOMException", ErrorCategory.InvalidOperation, null);
						base.WriteError(errorRecord3);
					}
					base.WriteObject(obj1);
					return;
				}
				return;
			}
			else
			{
				base.RunAsJob("Set-WMIInstance");
				return;
			}
		}
	}
}