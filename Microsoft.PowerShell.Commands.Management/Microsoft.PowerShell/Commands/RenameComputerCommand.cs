using Microsoft.PowerShell.Commands.Management;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Net;
using System.Runtime.InteropServices;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Rename", "Computer", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=219990", RemotingCapability=RemotingCapability.SupportedByCommand)]
	public class RenameComputerCommand : PSCmdlet
	{
		private string _computerName;

		private SwitchParameter _passThru;

		private PSCredential _domainCredential;

		private PSCredential _localCredential;

		private string _newComputerName;

		private bool _force;

		private bool _restart;

		private bool _containsLocalHost;

		private string _newNameForLocalHost;

		private ManagementObjectSearcher _searcher;

		private readonly string _shortLocalMachineName;

		private readonly string _fullLocalMachineName;

		[Parameter(ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
		public string ComputerName
		{
			get
			{
				return this._computerName;
			}
			set
			{
				this._computerName = value;
			}
		}

		[Credential]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public PSCredential DomainCredential
		{
			get
			{
				return this._domainCredential;
			}
			set
			{
				this._domainCredential = value;
			}
		}

		[Parameter]
		public SwitchParameter Force
		{
			get
			{
				return this._force;
			}
			set
			{
				this._force = value;
			}
		}

		[Credential]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public PSCredential LocalCredential
		{
			get
			{
				return this._localCredential;
			}
			set
			{
				this._localCredential = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
		public string NewName
		{
			get
			{
				return this._newComputerName;
			}
			set
			{
				this._newComputerName = value;
			}
		}

		[Parameter]
		public SwitchParameter PassThru
		{
			get
			{
				return this._passThru;
			}
			set
			{
				this._passThru = value;
			}
		}

		[Parameter]
		public SwitchParameter Restart
		{
			get
			{
				return this._restart;
			}
			set
			{
				this._restart = value;
			}
		}

		public RenameComputerCommand()
		{
			this._computerName = "localhost";
			this._shortLocalMachineName = Dns.GetHostName();
			this._fullLocalMachineName = Dns.GetHostEntry("").HostName;
		}

		private void DoRenameComputerAction(string computer, string newName, bool isLocalhost)
		{
			string str;
			string userName;
			string stringFromSecureString;
			EnumerationOptions enumerationOption = new EnumerationOptions();
			enumerationOption.UseAmendedQualifiers = true;
			enumerationOption.DirectRead = true;
			EnumerationOptions enumerationOption1 = enumerationOption;
			ObjectQuery objectQuery = new ObjectQuery("select * from Win32_ComputerSystem");
			bool flag = false;
			if (isLocalhost)
			{
				str = this._shortLocalMachineName;
			}
			else
			{
				str = computer;
			}
			string str1 = str;
			if (base.ShouldProcess(str1))
			{
				if (newName != null && newName.Length > 15)
				{
					string str2 = newName.Substring(0, 15);
					string str3 = StringUtil.Format(ComputerResources.TruncateNetBIOSName, str2);
					string truncateNetBIOSNameCaption = ComputerResources.TruncateNetBIOSNameCaption;
					if (!this.Force && !base.ShouldContinue(str3, truncateNetBIOSNameCaption))
					{
						return;
					}
				}
				ConnectionOptions connectionOption = new ConnectionOptions();
				connectionOption.Authentication = AuthenticationLevel.PacketPrivacy;
				connectionOption.Impersonation = ImpersonationLevel.Impersonate;
				connectionOption.EnablePrivileges = true;
				ConnectionOptions password = connectionOption;
				if (!isLocalhost)
				{
					if (this.LocalCredential == null)
					{
						if (this.DomainCredential != null)
						{
							//password.SecurePassword = this.DomainCredential.Password;
							password.Username = this.DomainCredential.UserName;
						}
					}
					else
					{
						//password.SecurePassword = this.LocalCredential.Password;
						password.Username = ComputerWMIHelper.GetLocalAdminUserName(str1, this.LocalCredential);
					}
				}
				else
				{
					password.Username = null;
					//password.SecurePassword = null;
				}
				ManagementScope managementScope = new ManagementScope(ComputerWMIHelper.GetScopeString(computer, "\\root\\cimv2"), password);
				try
				{
					try
					{
						this._searcher = new ManagementObjectSearcher(managementScope, objectQuery, enumerationOption1);
						foreach (ManagementObject managementObject in this._searcher.Get())
						{
							string item = (string)managementObject["DNSHostName"];
							if (!item.Equals(newName, StringComparison.OrdinalIgnoreCase))
							{
								string str4 = null;
								string str5 = null;
								if ((bool)managementObject["PartOfDomain"])
								{
									if (this.DomainCredential != null)
									{
										userName = this.DomainCredential.UserName;
									}
									else
									{
										userName = null;
									}
									str4 = userName;
									if (this.DomainCredential != null)
									{
										stringFromSecureString = Utils.GetStringFromSecureString(this.DomainCredential.Password);
									}
									else
									{
										stringFromSecureString = null;
									}
									str5 = stringFromSecureString;
								}
								ManagementBaseObject methodParameters = managementObject.GetMethodParameters("Rename");
								methodParameters.SetPropertyValue("Name", newName);
								methodParameters.SetPropertyValue("UserName", str4);
								methodParameters.SetPropertyValue("Password", str5);
								ManagementBaseObject managementBaseObject = managementObject.InvokeMethod("Rename", methodParameters, null);
								int num = Convert.ToInt32(managementBaseObject["ReturnValue"], CultureInfo.CurrentCulture);
								if (num == 0)
								{
									flag = true;
								}
								else
								{
									Win32Exception win32Exception = new Win32Exception(num);
									object[] message = new object[3];
									message[0] = str1;
									message[1] = newName;
									message[2] = win32Exception.Message;
									string str6 = StringUtil.Format(ComputerResources.FailToRename, message);
									ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str6), "FailToRenameComputer", ErrorCategory.OperationStopped, str1);
									base.WriteError(errorRecord);
								}
								if (!this._passThru)
								{
									continue;
								}
								base.WriteObject(ComputerWMIHelper.GetRenameComputerStatusObject(num, newName, str1));
							}
							else
							{
								string str7 = StringUtil.Format(ComputerResources.NewNameIsOldName, str1, newName);
								ErrorRecord errorRecord1 = new ErrorRecord(new InvalidOperationException(str7), "NewNameIsOldName", ErrorCategory.InvalidArgument, newName);
								base.WriteError(errorRecord1);
							}
						}
						if (flag && this._restart)
						{
							object[] objArray = new object[2];
							objArray[0] = 6;
							objArray[1] = 0;
							object[] objArray1 = objArray;
							RestartComputerCommand.RestartOneComputerUsingDcom(this, isLocalhost, str1, objArray1, password);
						}
						if (flag && !this._restart)
						{
							base.WriteWarning(StringUtil.Format(ComputerResources.RestartNeeded, null, str1));
						}
					}
					catch (ManagementException managementException1)
					{
						ManagementException managementException = managementException1;
						string str8 = StringUtil.Format(ComputerResources.FailToConnectToComputer, str1, managementException.Message);
						ErrorRecord errorRecord2 = new ErrorRecord(new InvalidOperationException(str8), "RenameComputerException", ErrorCategory.OperationStopped, str1);
						base.WriteError(errorRecord2);
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						string str9 = StringUtil.Format(ComputerResources.FailToConnectToComputer, str1, cOMException.Message);
						ErrorRecord errorRecord3 = new ErrorRecord(new InvalidOperationException(str9), "RenameComputerException", ErrorCategory.OperationStopped, str1);
						base.WriteError(errorRecord3);
					}
					catch (UnauthorizedAccessException unauthorizedAccessException1)
					{
						UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
						string str10 = StringUtil.Format(ComputerResources.FailToConnectToComputer, str1, unauthorizedAccessException.Message);
						ErrorRecord errorRecord4 = new ErrorRecord(new InvalidOperationException(str10), "RenameComputerException", ErrorCategory.OperationStopped, str1);
						base.WriteError(errorRecord4);
					}
				}
				finally
				{
					this._searcher.Dispose();
				}
				return;
			}
			else
			{
				return;
			}
		}

		protected override void EndProcessing()
		{
			if (this._containsLocalHost)
			{
				this.DoRenameComputerAction("localhost", this._newNameForLocalHost, true);
				return;
			}
			else
			{
				return;
			}
		}

		protected override void ProcessRecord()
		{
			string str = this.ValidateComputerName();
			if (str != null)
			{
				bool flag = str.Equals("localhost", StringComparison.OrdinalIgnoreCase);
				if (!flag)
				{
					this.DoRenameComputerAction(str, this._newComputerName, false);
					return;
				}
				else
				{
					if (!this._containsLocalHost)
					{
						this._containsLocalHost = true;
					}
					this._newNameForLocalHost = this._newComputerName;
					return;
				}
			}
			else
			{
				return;
			}
		}

		private string ValidateComputerName()
		{
			IPAddress pAddress = null;
			string str;
			object obj;
			string str1 = null;
			if (this._computerName.Equals(".", StringComparison.OrdinalIgnoreCase) || this._computerName.Equals("localhost", StringComparison.OrdinalIgnoreCase) || this._computerName.Equals(this._shortLocalMachineName, StringComparison.OrdinalIgnoreCase) || this._computerName.Equals(this._fullLocalMachineName, StringComparison.OrdinalIgnoreCase))
			{
				str1 = "localhost";
			}
			else
			{
				bool flag = false;
				try
				{
					flag = IPAddress.TryParse(this._computerName, out pAddress);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					CommandProcessorBase.CheckForSevereException(exception);
				}
				try
				{
					string hostName = Dns.GetHostEntry(this._computerName).HostName;
					if (hostName.Equals(this._shortLocalMachineName, StringComparison.OrdinalIgnoreCase) || hostName.Equals(this._fullLocalMachineName, StringComparison.OrdinalIgnoreCase))
					{
						str1 = "localhost";
					}
					else
					{
						str1 = this._computerName;
					}
					goto Label0;
				}
				catch (Exception exception3)
				{
					Exception exception2 = exception3;
					CommandProcessorBase.CheckForSevereException(exception2);
					if (flag)
					{
						str1 = this._computerName;
						goto Label0;
					}
					else
					{
						string str2 = StringUtil.Format(ComputerResources.CannotResolveComputerName, this._computerName, exception2.Message);
						ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str2), "AddressResolutionException", ErrorCategory.InvalidArgument, this._computerName);
						base.WriteError(errorRecord);
						str = null;
					}
				}
				return str;
			}
		Label0:
			bool flag1 = str1.Equals("localhost", StringComparison.OrdinalIgnoreCase);
			if (ComputerWMIHelper.IsComputerNameValid(this._newComputerName))
			{
				return str1;
			}
			else
			{
				string invalidNewName = ComputerResources.InvalidNewName;
				if (flag1)
				{
					obj = this._shortLocalMachineName;
				}
				else
				{
					obj = str1;
				}
				string str3 = StringUtil.Format(invalidNewName, obj, this._newComputerName);
				ErrorRecord errorRecord1 = new ErrorRecord(new InvalidOperationException(str3), "InvalidNewName", ErrorCategory.InvalidArgument, this._newComputerName);
				base.WriteError(errorRecord1);
				return null;
			}
		}
	}
}