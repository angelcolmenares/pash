using Microsoft.PowerShell.Commands.Management;
using System;
using System.ComponentModel;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Set", "Service", SupportsShouldProcess=true, DefaultParameterSetName="Name", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113399", RemotingCapability=RemotingCapability.SupportedByCommand)]
	[OutputType(new Type[] { typeof(ServiceController) })]
	public class SetServiceCommand : ServiceOperationBaseCommand
	{
		private string[] computername;

		internal string serviceName;

		internal string displayName;

		internal string description;

		internal ServiceStartMode startupType;

		internal string serviceStatus;

		private ServiceController inputobject;

		internal string[] include;

		internal string[] exclude;

		[Alias(new string[] { "cn" })]
		[Parameter(ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
		public string[] ComputerName
		{
			get
			{
				return this.computername;
			}
			set
			{
				this.computername = value;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public string Description
		{
			get
			{
				return this.description;
			}
			set
			{
				this.description = value;
			}
		}

		[Alias(new string[] { "DN" })]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public string DisplayName
		{
			get
			{
				return this.displayName;
			}
			set
			{
				this.displayName = value;
			}
		}

		public string[] Exclude
		{
			get
			{
				return this.exclude;
			}
			set
			{
				this.exclude = null;
			}
		}

		public string[] Include
		{
			get
			{
				return this.include;
			}
			set
			{
				this.include = null;
			}
		}

		[Parameter(ValueFromPipeline=true, ParameterSetName="InputObject")]
		public ServiceController InputObject
		{
			get
			{
				return this.inputobject;
			}
			set
			{
				this.inputobject = value;
			}
		}

		[Alias(new string[] { "ServiceName", "SN" })]
		[Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="Name")]
		public string Name
		{
			get
			{
				return this.serviceName;
			}
			set
			{
				this.serviceName = value;
			}
		}

		[Alias(new string[] { "StartMode", "SM", "ST" })]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public ServiceStartMode StartupType
		{
			get
			{
				return this.startupType;
			}
			set
			{
				this.startupType = value;
			}
		}

		[Parameter]
		[ValidateSet(new string[] { "Running", "Stopped", "Paused" })]
		public string Status
		{
			get
			{
				return this.serviceStatus;
			}
			set
			{
				this.serviceStatus = value;
			}
		}

		public SetServiceCommand()
		{
			string[] strArrays = new string[1];
			strArrays[0] = ".";
			this.computername = strArrays;
			this.startupType = ServiceStartMode.Manual | ServiceStartMode.Automatic | ServiceStartMode.Disabled;
		}

		[ArchitectureSensitive]
		protected override void ProcessRecord()
		{
			ServiceController serviceController = null;
			string machineName = null;
			string[] strArrays = this.computername;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				bool flag = false;
				try
				{
					if (!base._ParameterSetName.Equals("InputObject", StringComparison.OrdinalIgnoreCase) || this.inputobject == null)
					{
						machineName = str;
						serviceController = new ServiceController(this.serviceName, machineName);
						flag = true;
					}
					else
					{
						serviceController = this.inputobject;
						this.Name = serviceController.ServiceName;
						machineName = serviceController.MachineName;
						flag = false;
					}
				}
				catch (ArgumentException argumentException1)
				{
					ArgumentException argumentException = argumentException1;
					ErrorRecord errorRecord = new ErrorRecord(argumentException, "ArgumentException", ErrorCategory.ObjectNotFound, str);
					base.WriteError(errorRecord);
					goto Label0;
				}
				catch (InvalidOperationException invalidOperationException1)
				{
					InvalidOperationException invalidOperationException = invalidOperationException1;
					ErrorRecord errorRecord1 = new ErrorRecord(invalidOperationException, "InvalidOperationException", ErrorCategory.ObjectNotFound, str);
					base.WriteError(errorRecord1);
					goto Label0;
				}
				using (serviceController)
				{
					if (!flag)
					{
						if (base.ShouldProcessServiceOperation(serviceController))
						{
							IntPtr zero = IntPtr.Zero;
							IntPtr intPtr = IntPtr.Zero;
							try
							{
								zero = NativeMethods.OpenSCManagerW(machineName, null, 1);
								if (IntPtr.Zero != zero)
								{
									intPtr = NativeMethods.OpenServiceW(zero, this.Name, 2);
									if (IntPtr.Zero != intPtr)
									{
										if (!string.IsNullOrEmpty(this.DisplayName) || (ServiceStartMode.Manual | ServiceStartMode.Automatic | ServiceStartMode.Disabled) != this.StartupType)
										{
											uint num = 0;
											ServiceStartMode startupType = this.StartupType;
											switch (startupType)
											{
												case ServiceStartMode.Automatic:
												{
													num = 2;
													break;
												}
												case ServiceStartMode.Manual:
												{
													num = 3;
													break;
												}
												case ServiceStartMode.Disabled:
												{
													num = 4;
													break;
												}
											}
											bool flag1 = NativeMethods.ChangeServiceConfigW(intPtr, 0, num, 0, null, null, IntPtr.Zero, null, null, IntPtr.Zero, this.DisplayName);
											if (!flag1)
											{
												int lastWin32Error = Marshal.GetLastWin32Error();
												Win32Exception win32Exception = new Win32Exception(lastWin32Error);
												base.WriteNonTerminatingError(serviceController, win32Exception, "CouldNotSetService", ServiceResources.CouldNotSetService, ErrorCategory.PermissionDenied);
												goto Label0;
											}
										}
										NativeMethods.SERVICE_DESCRIPTIONW description = new NativeMethods.SERVICE_DESCRIPTIONW();
										description.lpDescription = this.Description;
										int num1 = Marshal.SizeOf(description);
										IntPtr intPtr1 = Marshal.AllocCoTaskMem(num1);
										Marshal.StructureToPtr(description, intPtr1, false);
										bool flag2 = NativeMethods.ChangeServiceConfig2W(intPtr, 1, intPtr1);
										if (!flag2)
										{
											int lastWin32Error1 = Marshal.GetLastWin32Error();
											Win32Exception win32Exception1 = new Win32Exception(lastWin32Error1);
											base.WriteNonTerminatingError(serviceController, win32Exception1, "CouldNotSetServiceDescription", ServiceResources.CouldNotSetServiceDescription, ErrorCategory.PermissionDenied);
										}
										if (!string.IsNullOrEmpty(this.Status))
										{
											if (!this.Status.Equals("Running", StringComparison.OrdinalIgnoreCase))
											{
												if (!this.Status.Equals("Stopped", StringComparison.CurrentCultureIgnoreCase))
												{
													if (this.Status.Equals("Paused", StringComparison.CurrentCultureIgnoreCase) && !serviceController.Status.Equals(ServiceControllerStatus.Paused))
													{
														base.DoPauseService(serviceController);
													}
												}
												else
												{
													if (!serviceController.Status.Equals(ServiceControllerStatus.Stopped))
													{
														ServiceController[] dependentServices = serviceController.DependentServices;
														if (dependentServices == null || (int)dependentServices.Length <= 0)
														{
															ServiceController[] servicesDependedOn = serviceController.ServicesDependedOn;
															if (servicesDependedOn == null || (int)servicesDependedOn.Length <= 0)
															{
																base.DoStopService(serviceController, true);
															}
															else
															{
																base.WriteNonTerminatingError(serviceController, null, "ServiceIsDependentOnNoForce", ServiceResources.ServiceIsDependentOnNoForce, ErrorCategory.InvalidOperation);
																goto Label0;
															}
														}
														else
														{
															base.WriteNonTerminatingError(serviceController, null, "ServiceHasDependentServicesNoForce", ServiceResources.ServiceHasDependentServicesNoForce, ErrorCategory.InvalidOperation);
															goto Label0;
														}
													}
												}
											}
											else
											{
												if (!serviceController.Status.Equals(ServiceControllerStatus.Running))
												{
													if (!serviceController.Status.Equals(ServiceControllerStatus.Paused))
													{
														base.DoStartService(serviceController);
													}
													else
													{
														base.DoResumeService(serviceController);
													}
												}
											}
										}
										SwitchParameter passThru = base.PassThru;
										if (passThru.IsPresent)
										{
											ServiceController serviceController1 = new ServiceController(this.Name, machineName);
											base.WriteObject(serviceController1);
										}
									}
									else
									{
										int lastWin32Error2 = Marshal.GetLastWin32Error();
										Win32Exception win32Exception2 = new Win32Exception(lastWin32Error2);
										base.WriteNonTerminatingError(serviceController, win32Exception2, "CouldNotSetService", ServiceResources.CouldNotSetService, ErrorCategory.PermissionDenied);
										goto Label0;
									}
								}
								else
								{
									int num2 = Marshal.GetLastWin32Error();
									Win32Exception win32Exception3 = new Win32Exception(num2);
									base.WriteNonTerminatingError(serviceController, machineName, win32Exception3, "ComputerAccessDenied", ServiceResources.ComputerAccessDenied, ErrorCategory.PermissionDenied);
									goto Label0;
								}
							}
							finally
							{
								if (IntPtr.Zero != intPtr)
								{
									bool flag3 = NativeMethods.CloseServiceHandle(intPtr);
									if (!flag3)
									{
										int lastWin32Error3 = Marshal.GetLastWin32Error();
										Win32Exception win32Exception4 = new Win32Exception(lastWin32Error3);
										base.WriteNonTerminatingError(serviceController, win32Exception4, "CouldNotSetServiceDescription", ServiceResources.CouldNotSetServiceDescription, ErrorCategory.PermissionDenied);
									}
								}
								if (IntPtr.Zero != zero)
								{
									bool flag4 = NativeMethods.CloseServiceHandle(zero);
									if (!flag4)
									{
										int num3 = Marshal.GetLastWin32Error();
										Win32Exception win32Exception5 = new Win32Exception(num3);
										base.WriteNonTerminatingError(serviceController, win32Exception5, "CouldNotSetServiceDescription", ServiceResources.CouldNotSetServiceDescription, ErrorCategory.PermissionDenied);
									}
								}
							}
						}
					}
				}
            Label0:
                continue;
			}
		}
	}
}