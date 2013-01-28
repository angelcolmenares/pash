using Microsoft.PowerShell.Commands.Management;
using System;
using System.ComponentModel;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("New", "Service", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113359")]
	[OutputType(new Type[] { typeof(ServiceController) })]
	public class NewServiceCommand : ServiceBaseCommand
	{
		internal string serviceName;

		internal string binaryPathName;

		internal string displayName;

		internal string description;

		internal ServiceStartMode startupType;

		internal PSCredential credential;

		internal string[] dependsOn;

		[Parameter(Position=1, Mandatory=true)]
		public string BinaryPathName
		{
			get
			{
				return this.binaryPathName;
			}
			set
			{
				this.binaryPathName = value;
			}
		}

		[Credential]
		[Parameter]
		public PSCredential Credential
		{
			get
			{
				return this.credential;
			}
			set
			{
				this.credential = value;
			}
		}

		[Parameter]
		public string[] DependsOn
		{
			get
			{
				return this.dependsOn;
			}
			set
			{
				this.dependsOn = value;
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

		[Alias(new string[] { "ServiceName" })]
		[Parameter(Position=0, Mandatory=true)]
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

		[Parameter]
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

		public NewServiceCommand()
		{
			this.startupType = ServiceStartMode.Automatic;
		}

		[ArchitectureSensitive]
		protected override void BeginProcessing()
		{
			string displayName;
			NewServiceCommand newServiceCommand = this;
			if (this.DisplayName == null)
			{
				displayName = "";
			}
			else
			{
				displayName = this.DisplayName;
			}
			if (newServiceCommand.ShouldProcessServiceOperation(displayName, this.Name))
			{
				IntPtr zero = IntPtr.Zero;
				IntPtr intPtr = IntPtr.Zero;
				IntPtr globalAllocUnicode = IntPtr.Zero;
				try
				{
					zero = NativeMethods.OpenSCManagerW(null, null, 3);
					if (IntPtr.Zero != zero)
					{
						uint num = 2;
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
						IntPtr zero1 = IntPtr.Zero;
						if (this.DependsOn != null)
						{
							int length = 1;
							string[] dependsOn = this.DependsOn;
							for (int i = 0; i < (int)dependsOn.Length; i++)
							{
								string str = dependsOn[i];
								length = length + str.Length + 1;
							}
							char[] chrArray = new char[length];
							int length1 = 0;
							string[] strArrays = this.DependsOn;
							for (int j = 0; j < (int)strArrays.Length; j++)
							{
								string str1 = strArrays[j];
								Array.Copy(str1.ToCharArray(), 0, chrArray, length1, str1.Length);
								length1 = length1 + str1.Length;
								int num1 = length1;
								length1 = num1 + 1;
								chrArray[num1] = '\0';
							}
							int num2 = length1;
							chrArray[num2] = '\0';
							zero1 = Marshal.AllocHGlobal(length * Marshal.SystemDefaultCharSize);
							Marshal.Copy(chrArray, 0, zero1, length);
						}
						string userName = null;
						if (this.Credential != null)
						{
							userName = this.Credential.UserName;
							globalAllocUnicode = Marshal.SecureStringToGlobalAllocUnicode(this.Credential.Password);
						}
						intPtr = NativeMethods.CreateServiceW(zero, this.Name, this.DisplayName, 2, 16, num, 1, this.BinaryPathName, null, null, zero1, userName, globalAllocUnicode);
						if (IntPtr.Zero != intPtr)
						{
							NativeMethods.SERVICE_DESCRIPTIONW description = new NativeMethods.SERVICE_DESCRIPTIONW();
							description.lpDescription = this.Description;
							int num3 = Marshal.SizeOf(description);
							IntPtr intPtr1 = Marshal.AllocCoTaskMem(num3);
							Marshal.StructureToPtr(description, intPtr1, false);
							bool flag = NativeMethods.ChangeServiceConfig2W(intPtr, 1, intPtr1);
							if (!flag)
							{
								int lastWin32Error = Marshal.GetLastWin32Error();
								Win32Exception win32Exception = new Win32Exception(lastWin32Error);
								base.WriteNonTerminatingError(this.Name, this.DisplayName, this.Name, win32Exception, "CouldNotNewServiceDescription", ServiceResources.CouldNotNewServiceDescription, ErrorCategory.PermissionDenied);
							}
							using (ServiceController serviceController = new ServiceController(this.Name))
							{
								base.WriteObject(serviceController);
							}
						}
						else
						{
							int lastWin32Error1 = Marshal.GetLastWin32Error();
							Win32Exception win32Exception1 = new Win32Exception(lastWin32Error1);
							base.WriteNonTerminatingError(this.Name, this.DisplayName, this.Name, win32Exception1, "CouldNotNewService", ServiceResources.CouldNotNewService, ErrorCategory.PermissionDenied);
						}
					}
					else
					{
						int lastWin32Error2 = Marshal.GetLastWin32Error();
						Win32Exception win32Exception2 = new Win32Exception(lastWin32Error2);
						base.WriteNonTerminatingError(this.Name, this.DisplayName, this.Name, win32Exception2, "CouldNotNewService", ServiceResources.CouldNotNewService, ErrorCategory.PermissionDenied);
					}
				}
				finally
				{
					if (IntPtr.Zero != globalAllocUnicode)
					{
						Marshal.ZeroFreeGlobalAllocUnicode(globalAllocUnicode);
					}
					if (IntPtr.Zero != intPtr)
					{
						bool flag1 = NativeMethods.CloseServiceHandle(intPtr);
						if (!flag1)
						{
							int lastWin32Error3 = Marshal.GetLastWin32Error();
							Win32Exception win32Exception3 = new Win32Exception(lastWin32Error3);
							base.WriteNonTerminatingError(this.Name, this.DisplayName, this.Name, win32Exception3, "CouldNotNewServiceDescription", ServiceResources.CouldNotNewServiceDescription, ErrorCategory.PermissionDenied);
						}
					}
					if (IntPtr.Zero != zero)
					{
						bool flag2 = NativeMethods.CloseServiceHandle(zero);
						if (!flag2)
						{
							int lastWin32Error4 = Marshal.GetLastWin32Error();
							Win32Exception win32Exception4 = new Win32Exception(lastWin32Error4);
							base.WriteNonTerminatingError(this.Name, this.DisplayName, this.Name, win32Exception4, "CouldNotNewServiceDescription", ServiceResources.CouldNotNewServiceDescription, ErrorCategory.PermissionDenied);
						}
					}
				}
				return;
			}
			else
			{
				return;
			}
		}
	}
}