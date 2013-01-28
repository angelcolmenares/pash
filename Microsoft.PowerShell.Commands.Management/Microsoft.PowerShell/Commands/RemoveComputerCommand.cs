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
	[Cmdlet("Remove", "Computer", SupportsShouldProcess=true, DefaultParameterSetName="Local", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135246", RemotingCapability=RemotingCapability.SupportedByCommand)]
	[OutputType(new Type[] { typeof(ComputerChangeInfo) })]
	public class RemoveComputerCommand : PSCmdlet
	{
		private const string LocalParameterSet = "Local";

		private const string RemoteParameterSet = "Remote";

		private PSCredential _unjoinDomainCredential;

		private PSCredential _localCredential;

		private SwitchParameter _restart;

		private string[] _computerName;

		private bool _force;

		private SwitchParameter _passThru;

		private string _workGroup;

		private bool _containsLocalHost;

		private readonly string _shortLocalMachineName;

		private readonly string _fullLocalMachineName;

		[Parameter(ParameterSetName="Remote", ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
		public string[] ComputerName
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
		[Parameter(ParameterSetName="Remote")]
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

		[Alias(new string[] { "Credential" })]
		[Credential]
		[Parameter(ParameterSetName="Remote", Mandatory=true)]
		[Parameter(Position=0, ParameterSetName="Local")]
		[ValidateNotNullOrEmpty]
		public PSCredential UnjoinDomainCredential
		{
			get
			{
				return this._unjoinDomainCredential;
			}
			set
			{
				this._unjoinDomainCredential = value;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public string Workgroup
		{
			get
			{
				return this._workGroup;
			}
			set
			{
				this._workGroup = value;
			}
		}

		public RemoveComputerCommand()
		{
			this._restart = false;
			string[] strArrays = new string[1];
			strArrays[0] = "localhost";
			this._computerName = strArrays;
			this._workGroup = "WORKGROUP";
			this._shortLocalMachineName = Dns.GetHostName();
			this._fullLocalMachineName = Dns.GetHostEntry("").HostName;
		}

		private void DoRemoveComputerAction(string computer, bool isLocalhost, ConnectionOptions options, EnumerationOptions enumOptions, ObjectQuery computerSystemQuery)
		{
			string str;
			string userName;
			string stringFromSecureString;
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
				if (this.LocalCredential != null)
				{
					//options.SecurePassword = this.LocalCredential.Password;
					options.Username = ComputerWMIHelper.GetLocalAdminUserName(str1, this.LocalCredential);
				}
				if (isLocalhost)
				{
					options.Username = null;
					//options.SecurePassword = null;
				}
				ManagementObjectSearcher managementObjectSearcher = null;
				ManagementScope managementScope = new ManagementScope(ComputerWMIHelper.GetScopeString(computer, "\\root\\cimv2"), options);
				using (managementObjectSearcher)
				{
					try
					{
						managementObjectSearcher = new ManagementObjectSearcher(managementScope, computerSystemQuery, enumOptions);
						foreach (ManagementObject managementObject in managementObjectSearcher.Get())
						{
							if ((bool)managementObject["PartOfDomain"])
							{
								string removeComputerConfirm = ComputerResources.RemoveComputerConfirm;
								if (!this.Force && !base.ShouldContinue(removeComputerConfirm, null))
								{
									continue;
								}
								string str2 = (string)LanguagePrimitives.ConvertTo(managementObject["Domain"], typeof(string), CultureInfo.InvariantCulture);
								if (this.UnjoinDomainCredential != null)
								{
									userName = this.UnjoinDomainCredential.UserName;
								}
								else
								{
									userName = null;
								}
								string str3 = userName;
								if (this.UnjoinDomainCredential != null)
								{
									stringFromSecureString = Utils.GetStringFromSecureString(this.UnjoinDomainCredential.Password);
								}
								else
								{
									stringFromSecureString = null;
								}
								string str4 = stringFromSecureString;
								ManagementBaseObject methodParameters = managementObject.GetMethodParameters("UnjoinDomainOrWorkgroup");
								methodParameters.SetPropertyValue("UserName", str3);
								methodParameters.SetPropertyValue("Password", str4);
								methodParameters.SetPropertyValue("FUnjoinOptions", 4);
								ManagementBaseObject managementBaseObject = managementObject.InvokeMethod("UnjoinDomainOrWorkgroup", methodParameters, null);
								int num = Convert.ToInt32(managementBaseObject["ReturnValue"], CultureInfo.CurrentCulture);
								if ((num == 0x54b || num == 53) && this.Force)
								{
									methodParameters.SetPropertyValue("FUnjoinOptions", 0);
									managementBaseObject = managementObject.InvokeMethod("UnjoinDomainOrWorkgroup", methodParameters, null);
									num = Convert.ToInt32(managementBaseObject["ReturnValue"], CultureInfo.CurrentCulture);
								}
								if (num == 0)
								{
									flag = true;
									if (this._workGroup != null)
									{
										ManagementBaseObject methodParameters1 = managementObject.GetMethodParameters("JoinDomainOrWorkgroup");
										methodParameters1.SetPropertyValue("Name", this._workGroup);
										methodParameters1.SetPropertyValue("Password", null);
										methodParameters1.SetPropertyValue("UserName", null);
										methodParameters1.SetPropertyValue("FJoinOptions", 0);
										managementBaseObject = managementObject.InvokeMethod("JoinDomainOrWorkgroup", methodParameters1, null);
										num = Convert.ToInt32(managementBaseObject["ReturnValue"], CultureInfo.CurrentCulture);
										if (num != 0)
										{
											Win32Exception win32Exception = new Win32Exception(num);
											object[] message = new object[4];
											message[0] = str1;
											message[1] = str2;
											message[2] = this._workGroup;
											message[3] = win32Exception.Message;
											string str5 = StringUtil.Format(ComputerResources.FailToSwitchFromDomainToWorkgroup, message);
											ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str5), "FailToJoinWorkGroup", ErrorCategory.OperationStopped, str1);
											base.WriteError(errorRecord);
										}
									}
								}
								else
								{
									Win32Exception win32Exception1 = new Win32Exception(num);
									object[] objArray = new object[3];
									objArray[0] = str1;
									objArray[1] = str2;
									objArray[2] = win32Exception1.Message;
									string str6 = StringUtil.Format(ComputerResources.FailToUnjoinDomain, objArray);
									ErrorRecord errorRecord1 = new ErrorRecord(new InvalidOperationException(str6), "FailToUnjoinDomain", ErrorCategory.OperationStopped, str1);
									base.WriteError(errorRecord1);
								}
								if (!this._passThru)
								{
									continue;
								}
								base.WriteObject(ComputerWMIHelper.GetComputerStatusObject(num, str1));
							}
							else
							{
								string str7 = StringUtil.Format(ComputerResources.ComputerNotInDomain, str1);
								ErrorRecord errorRecord2 = new ErrorRecord(new InvalidOperationException(str7), "ComputerNotInDomain", ErrorCategory.InvalidOperation, str1);
								base.WriteError(errorRecord2);
							}
						}
						if (flag && this._restart)
						{
							object[] objArray1 = new object[2];
							objArray1[0] = 6;
							objArray1[1] = 0;
							object[] objArray2 = objArray1;
							RestartComputerCommand.RestartOneComputerUsingDcom(this, isLocalhost, str1, objArray2, options);
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
						ErrorRecord errorRecord3 = new ErrorRecord(new InvalidOperationException(str8), "RemoveComputerException", ErrorCategory.OperationStopped, str1);
						base.WriteError(errorRecord3);
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						string str9 = StringUtil.Format(ComputerResources.FailToConnectToComputer, str1, cOMException.Message);
						ErrorRecord errorRecord4 = new ErrorRecord(new InvalidOperationException(str9), "RemoveComputerException", ErrorCategory.OperationStopped, str1);
						base.WriteError(errorRecord4);
					}
					catch (UnauthorizedAccessException unauthorizedAccessException1)
					{
						UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
						string str10 = StringUtil.Format(ComputerResources.FailToConnectToComputer, str1, unauthorizedAccessException.Message);
						ErrorRecord errorRecord5 = new ErrorRecord(new InvalidOperationException(str10), "RemoveComputerException", ErrorCategory.OperationStopped, str1);
						base.WriteError(errorRecord5);
					}
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
				ConnectionOptions connectionOption = new ConnectionOptions();
				connectionOption.Authentication = AuthenticationLevel.PacketPrivacy;
				connectionOption.Impersonation = ImpersonationLevel.Impersonate;
				connectionOption.EnablePrivileges = true;
				ConnectionOptions password = connectionOption;
				if (this.LocalCredential == null && this.UnjoinDomainCredential != null)
				{
					//password.SecurePassword = this.UnjoinDomainCredential.Password;
					password.Username = this.UnjoinDomainCredential.UserName;
				}
				EnumerationOptions enumerationOption = new EnumerationOptions();
				enumerationOption.UseAmendedQualifiers = true;
				enumerationOption.DirectRead = true;
				EnumerationOptions enumerationOption1 = enumerationOption;
				ObjectQuery objectQuery = new ObjectQuery("select * from Win32_ComputerSystem");
				this.DoRemoveComputerAction("localhost", true, password, enumerationOption1, objectQuery);
				return;
			}
			else
			{
				return;
			}
		}

		protected override void ProcessRecord()
		{
			ConnectionOptions connectionOption = new ConnectionOptions();
			connectionOption.Authentication = AuthenticationLevel.PacketPrivacy;
			connectionOption.Impersonation = ImpersonationLevel.Impersonate;
			connectionOption.EnablePrivileges = true;
			ConnectionOptions password = connectionOption;
			if (this.LocalCredential == null && this.UnjoinDomainCredential != null)
			{
				//password.SecurePassword = this.UnjoinDomainCredential.Password;
				password.Username = this.UnjoinDomainCredential.UserName;
			}
			EnumerationOptions enumerationOption = new EnumerationOptions();
			enumerationOption.UseAmendedQualifiers = true;
			enumerationOption.DirectRead = true;
			EnumerationOptions enumerationOption1 = enumerationOption;
			ObjectQuery objectQuery = new ObjectQuery("select * from Win32_ComputerSystem");
			string[] strArrays = this._computerName;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				string str1 = this.ValidateComputerNames(str);
				if (str1 != null)
				{
					bool flag = str1.Equals("localhost", StringComparison.OrdinalIgnoreCase);
					if (!flag)
					{
						this.DoRemoveComputerAction(str, false, password, enumerationOption1, objectQuery);
					}
					else
					{
						if (!this._containsLocalHost)
						{
							this._containsLocalHost = true;
						}
					}
				}
			}
		}

		private string ValidateComputerNames(string computer)
		{
			IPAddress pAddress = null;
			string str;
			string str1 = null;
			if (computer.Equals(".", StringComparison.OrdinalIgnoreCase) || computer.Equals("localhost", StringComparison.OrdinalIgnoreCase) || computer.Equals(this._shortLocalMachineName, StringComparison.OrdinalIgnoreCase) || computer.Equals(this._fullLocalMachineName, StringComparison.OrdinalIgnoreCase))
			{
				str1 = "localhost";
			}
			else
			{
				bool flag = false;
				try
				{
					flag = IPAddress.TryParse(computer, out pAddress);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					CommandProcessorBase.CheckForSevereException(exception);
				}
				try
				{
					string hostName = Dns.GetHostEntry(computer).HostName;
					if (hostName.Equals(this._shortLocalMachineName, StringComparison.OrdinalIgnoreCase) || hostName.Equals(this._fullLocalMachineName, StringComparison.OrdinalIgnoreCase))
					{
						str1 = "localhost";
					}
					else
					{
						str1 = computer;
					}
					return str1;
				}
				catch (Exception exception3)
				{
					Exception exception2 = exception3;
					CommandProcessorBase.CheckForSevereException(exception2);
					if (flag)
					{
						str1 = computer;
						return str1;
					}
					else
					{
						string str2 = StringUtil.Format(ComputerResources.CannotResolveComputerName, computer, exception2.Message);
						ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str2), "AddressResolutionException", ErrorCategory.InvalidArgument, computer);
						base.WriteError(errorRecord);
						str = null;
					}
				}
				return str;
			}
			return str1;
		}
	}
}