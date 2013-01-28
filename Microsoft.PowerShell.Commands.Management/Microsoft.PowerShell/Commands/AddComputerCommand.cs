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
	[Cmdlet("Add", "Computer", SupportsShouldProcess=true, DefaultParameterSetName="Domain", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135194", RemotingCapability=RemotingCapability.SupportedByCommand)]
	[OutputType(new Type[] { typeof(ComputerChangeInfo) })]
	public class AddComputerCommand : PSCmdlet
	{
		private const string DomainParameterSet = "Domain";

		private const string WorkgroupParameterSet = "Workgroup";

		private string[] _computerName;

		private PSCredential _localCredential;

		private PSCredential _unjoinDomainCredential;

		private PSCredential _domainCredential;

		private string _domainName;

		private string _ouPath;

		private string _server;

		private SwitchParameter _unsecure;

		private JoinOptions _joinOptions;

		private string _workgroupName;

		private SwitchParameter _restart;

		private SwitchParameter _passThru;

		private string _newName;

		private bool _force;

		private int _joinDomainflags;

		private bool _containsLocalHost;

		private string _newNameForLocalHost;

		private readonly string _shortLocalMachineName;

		private readonly string _fullLocalMachineName;

		[Parameter(ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
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

		[Alias(new string[] { "DomainCredential" })]
		[Credential]
		[Parameter(ParameterSetName="Domain", Mandatory=true)]
		[Parameter(ParameterSetName="Workgroup")]
		[ValidateNotNullOrEmpty]
		public PSCredential Credential
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

		[Alias(new string[] { "DN", "Domain" })]
		[Parameter(Position=0, Mandatory=true, ParameterSetName="Domain")]
		[ValidateNotNullOrEmpty]
		public string DomainName
		{
			get
			{
				return this._domainName;
			}
			set
			{
				this._domainName = value;
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

		[Parameter(ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
		public string NewName
		{
			get
			{
				return this._newName;
			}
			set
			{
				this._newName = value;
			}
		}

		[Parameter(ParameterSetName="Domain")]
		public JoinOptions Options
		{
			get
			{
				return this._joinOptions;
			}
			set
			{
				this._joinOptions = value;
			}
		}

		[Alias(new string[] { "OU" })]
		[Parameter(ParameterSetName="Domain")]
		[ValidateNotNullOrEmpty]
		public string OUPath
		{
			get
			{
				return this._ouPath;
			}
			set
			{
				this._ouPath = value;
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

		[Alias(new string[] { "DC" })]
		[Parameter(ParameterSetName="Domain")]
		[ValidateNotNullOrEmpty]
		public string Server
		{
			get
			{
				return this._server;
			}
			set
			{
				this._server = value;
			}
		}

		[Credential]
		[Parameter(ParameterSetName="Domain")]
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

		[Parameter(ParameterSetName="Domain")]
		public SwitchParameter Unsecure
		{
			get
			{
				return this._unsecure;
			}
			set
			{
				this._unsecure = value;
			}
		}

		[Alias(new string[] { "WGN" })]
		[Parameter(Position=0, Mandatory=true, ParameterSetName="Workgroup")]
		[ValidateNotNullOrEmpty]
		public string WorkGroupName
		{
			get
			{
				return this._workgroupName;
			}
			set
			{
				this._workgroupName = value;
			}
		}

		public AddComputerCommand()
		{
			string[] strArrays = new string[1];
			strArrays[0] = "localhost";
			this._computerName = strArrays;
			this._joinOptions = JoinOptions.AccountCreate;
			this._restart = false;
			this._joinDomainflags = 1;
			this._shortLocalMachineName = Dns.GetHostName();
			this._fullLocalMachineName = Dns.GetHostEntry("").HostName;
		}

		protected override void BeginProcessing()
		{
			if (base.ParameterSetName == "Domain")
			{
				if ((this._joinOptions & JoinOptions.PasswordPass) != 0 && (this._joinOptions & JoinOptions.UnsecuredJoin) == 0)
				{
					object[] str = new object[2];
					str[0] = JoinOptions.PasswordPass.ToString();
					str[1] = JoinOptions.UnsecuredJoin.ToString();
					this.WriteErrorHelper(ComputerResources.InvalidJoinOptions, "InvalidJoinOptions", this._joinOptions, ErrorCategory.InvalidArgument, true, str);
				}
				if ((this._joinOptions & JoinOptions.AccountCreate) != 0)
				{
					AddComputerCommand addComputerCommand = this;
					addComputerCommand._joinDomainflags = addComputerCommand._joinDomainflags | 2;
				}
				if ((this._joinOptions & JoinOptions.Win9XUpgrade) != 0)
				{
					AddComputerCommand addComputerCommand1 = this;
					addComputerCommand1._joinDomainflags = addComputerCommand1._joinDomainflags | 16;
				}
				if ((this._joinOptions & JoinOptions.UnsecuredJoin) != 0)
				{
					AddComputerCommand addComputerCommand2 = this;
					addComputerCommand2._joinDomainflags = addComputerCommand2._joinDomainflags | 64;
				}
				if ((this._joinOptions & JoinOptions.PasswordPass) != 0)
				{
					AddComputerCommand addComputerCommand3 = this;
					addComputerCommand3._joinDomainflags = addComputerCommand3._joinDomainflags | 128;
				}
				if ((this._joinOptions & JoinOptions.DeferSPNSet) != 0)
				{
					AddComputerCommand addComputerCommand4 = this;
					addComputerCommand4._joinDomainflags = addComputerCommand4._joinDomainflags | 0x100;
				}
				if ((this._joinOptions & JoinOptions.JoinWithNewName) != 0)
				{
					AddComputerCommand addComputerCommand5 = this;
					addComputerCommand5._joinDomainflags = addComputerCommand5._joinDomainflags | 0x400;
				}
				if ((this._joinOptions & JoinOptions.JoinReadOnly) != 0)
				{
					AddComputerCommand addComputerCommand6 = this;
					addComputerCommand6._joinDomainflags = addComputerCommand6._joinDomainflags | 0x800;
				}
				if ((this._joinOptions & JoinOptions.InstallInvoke) != 0)
				{
					AddComputerCommand addComputerCommand7 = this;
					addComputerCommand7._joinDomainflags = addComputerCommand7._joinDomainflags | 0x40000;
				}
				if (this._unsecure)
				{
					AddComputerCommand addComputerCommand8 = this;
					addComputerCommand8._joinDomainflags = addComputerCommand8._joinDomainflags | 192;
				}
				if (this._server != null)
				{
					try
					{
						Dns.GetHostEntry(this._server);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						CommandsCommon.CheckForSevereException(this, exception);
						object[] objArray = new object[1];
						objArray[0] = this._server;
						this.WriteErrorHelper(ComputerResources.CannotResolveServerName, "AddressResolutionException", this._server, ErrorCategory.InvalidArgument, true, objArray);
					}
					this._domainName = string.Concat(this._domainName, "\\", this._server);
				}
			}
		}

		private void DoAddComputerAction(string computer, string newName, bool isLocalhost, ConnectionOptions options, EnumerationOptions enumOptions, ObjectQuery computerSystemQuery)
		{
			string str;
			string userName;
			string stringFromSecureString;
			string userName1;
			string stringFromSecureString1;
			int num = 0;
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
			if (base.ParameterSetName != "Domain")
			{
				string str2 = StringUtil.Format(ComputerResources.AddComputerActionWorkgroup, this._workgroupName);
				if (!base.ShouldProcess(str1, str2))
				{
					return;
				}
			}
			else
			{
				string str3 = StringUtil.Format(ComputerResources.AddComputerActionDomain, this._domainName);
				if (!base.ShouldProcess(str1, str3))
				{
					return;
				}
			}
			if (newName != null && newName.Length > 15)
			{
				string str4 = newName.Substring(0, 15);
				string str5 = StringUtil.Format(ComputerResources.TruncateNetBIOSName, str4);
				string truncateNetBIOSNameCaption = ComputerResources.TruncateNetBIOSNameCaption;
				if (!this.Force && !base.ShouldContinue(str5, truncateNetBIOSNameCaption))
				{
					return;
				}
			}
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
						string item = (string)managementObject["DNSHostName"];
						if (newName == null && item.Length > 15)
						{
							string str6 = item.Substring(0, 15);
							string str7 = StringUtil.Format(ComputerResources.TruncateNetBIOSName, str6);
							string truncateNetBIOSNameCaption1 = ComputerResources.TruncateNetBIOSNameCaption;
							if (!this.Force && !base.ShouldContinue(str7, truncateNetBIOSNameCaption1))
							{
								continue;
							}
						}
						if (newName == null || !item.Equals(newName, StringComparison.OrdinalIgnoreCase))
						{
							if (base.ParameterSetName != "Domain")
							{
								if (!(bool)managementObject["PartOfDomain"])
								{
									string str8 = (string)LanguagePrimitives.ConvertTo(managementObject["Domain"], typeof(string), CultureInfo.InvariantCulture);
									if (!str8.Equals(this._workgroupName, StringComparison.OrdinalIgnoreCase))
									{
										num = this.JoinWorkgroup(managementObject, str1, null);
										if (num == 0 && newName != null)
										{
											num = this.RenameComputer(managementObject, str1, newName);
										}
										flag = num == 0;
									}
									else
									{
										object[] objArray = new object[2];
										objArray[0] = str1;
										objArray[1] = this._workgroupName;
										this.WriteErrorHelper(ComputerResources.AddComputerToSameWorkgroup, "AddComputerToSameWorkgroup", str1, ErrorCategory.InvalidOperation, false, objArray);
										continue;
									}
								}
								else
								{
									string removeComputerConfirm = ComputerResources.RemoveComputerConfirm;
									if (!this.Force && !base.ShouldContinue(removeComputerConfirm, null))
									{
										continue;
									}
									string str9 = (string)LanguagePrimitives.ConvertTo(managementObject["Domain"], typeof(string), CultureInfo.InvariantCulture);
									if (this.Credential != null)
									{
										userName = this.Credential.UserName;
									}
									else
									{
										userName = null;
									}
									string str10 = userName;
									if (this.Credential != null)
									{
										stringFromSecureString = Utils.GetStringFromSecureString(this.Credential.Password);
									}
									else
									{
										stringFromSecureString = null;
									}
									string str11 = stringFromSecureString;
									num = this.UnjoinDomain(managementObject, str1, str9, str10, str11);
									if (num == 0)
									{
										num = this.JoinWorkgroup(managementObject, str1, str9);
										if (num == 0 && newName != null)
										{
											num = this.RenameComputer(managementObject, str1, newName);
										}
									}
									flag = num == 0;
								}
							}
							else
							{
								if (!(bool)managementObject["PartOfDomain"])
								{
									string str12 = (string)LanguagePrimitives.ConvertTo(managementObject["Domain"], typeof(string), CultureInfo.InvariantCulture);
									num = this.JoinDomain(managementObject, str1, null, str12);
									if (num == 0 && newName != null)
									{
										num = this.RenameComputer(managementObject, str1, newName);
									}
									flag = num == 0;
								}
								else
								{
									string str13 = (string)LanguagePrimitives.ConvertTo(managementObject["Domain"], typeof(string), CultureInfo.InvariantCulture);
									string str14 = "";
									if (str13.Contains("."))
									{
										int num1 = str13.IndexOf(".", StringComparison.OrdinalIgnoreCase);
										str14 = str13.Substring(0, num1);
									}
									if (str13.Equals(this._domainName, StringComparison.OrdinalIgnoreCase) || str14.Equals(this._domainName, StringComparison.OrdinalIgnoreCase))
									{
										object[] objArray1 = new object[2];
										objArray1[0] = str1;
										objArray1[1] = this._domainName;
										this.WriteErrorHelper(ComputerResources.AddComputerToSameDomain, "AddComputerToSameDomain", str1, ErrorCategory.InvalidOperation, false, objArray1);
										continue;
									}
									else
									{
										PSCredential unjoinDomainCredential = this.UnjoinDomainCredential;
										PSCredential credential = unjoinDomainCredential;
										if (unjoinDomainCredential == null)
										{
											credential = this.Credential;
										}
										PSCredential pSCredential = credential;
										if (pSCredential != null)
										{
											userName1 = pSCredential.UserName;
										}
										else
										{
											userName1 = null;
										}
										string str15 = userName1;
										if (pSCredential != null)
										{
											stringFromSecureString1 = Utils.GetStringFromSecureString(pSCredential.Password);
										}
										else
										{
											stringFromSecureString1 = null;
										}
										string str16 = stringFromSecureString1;
										num = this.UnjoinDomain(managementObject, str1, str13, str15, str16);
										if (num == 0)
										{
											num = this.JoinDomain(managementObject, str1, str13, null);
											if (num == 0 && newName != null)
											{
												num = this.RenameComputer(managementObject, str1, newName);
											}
										}
										flag = num == 0;
									}
								}
							}
							if (!this._passThru)
							{
								continue;
							}
							base.WriteObject(ComputerWMIHelper.GetComputerStatusObject(num, str1));
						}
						else
						{
							object[] objArray2 = new object[2];
							objArray2[0] = str1;
							objArray2[1] = newName;
							this.WriteErrorHelper(ComputerResources.NewNameIsOldName, "NewNameIsOldName", newName, ErrorCategory.InvalidArgument, false, objArray2);
						}
					}
					if (flag && this._restart)
					{
						object[] objArray3 = new object[2];
						objArray3[0] = 6;
						objArray3[1] = 0;
						object[] objArray4 = objArray3;
						RestartComputerCommand.RestartOneComputerUsingDcom(this, isLocalhost, str1, objArray4, options);
					}
					if (flag && !this._restart)
					{
						base.WriteWarning(StringUtil.Format(ComputerResources.RestartNeeded, null, str1));
					}
				}
				catch (ManagementException managementException1)
				{
					ManagementException managementException = managementException1;
					object[] message = new object[2];
					message[0] = str1;
					message[1] = managementException.Message;
					this.WriteErrorHelper(ComputerResources.FailToConnectToComputer, "AddComputerException", str1, ErrorCategory.OperationStopped, false, message);
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					object[] message1 = new object[2];
					message1[0] = str1;
					message1[1] = cOMException.Message;
					this.WriteErrorHelper(ComputerResources.FailToConnectToComputer, "AddComputerException", str1, ErrorCategory.OperationStopped, false, message1);
				}
				catch (UnauthorizedAccessException unauthorizedAccessException1)
				{
					UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
					object[] message2 = new object[2];
					message2[0] = str1;
					message2[1] = unauthorizedAccessException.Message;
					this.WriteErrorHelper(ComputerResources.FailToConnectToComputer, "AddComputerException", str1, ErrorCategory.OperationStopped, false, message2);
				}
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
				ConnectionOptions connectionOption1 = connectionOption;
				EnumerationOptions enumerationOption = new EnumerationOptions();
				enumerationOption.UseAmendedQualifiers = true;
				enumerationOption.DirectRead = true;
				EnumerationOptions enumerationOption1 = enumerationOption;
				ObjectQuery objectQuery = new ObjectQuery("select * from Win32_ComputerSystem");
				this.DoAddComputerAction("localhost", this._newNameForLocalHost, true, connectionOption1, enumerationOption1, objectQuery);
				return;
			}
			else
			{
				return;
			}
		}

		private int JoinDomain(ManagementObject computerSystem, string computerName, string oldDomainName, string curWorkgroupName)
		{
			string str;
			string str1;
			string userName;
			string stringFromSecureString;
			if (this.Credential != null)
			{
				userName = this.Credential.UserName;
			}
			else
			{
				userName = null;
			}
			string str2 = userName;
			if (this.Credential != null)
			{
				stringFromSecureString = Utils.GetStringFromSecureString(this.Credential.Password);
			}
			else
			{
				stringFromSecureString = null;
			}
			string str3 = stringFromSecureString;
			ManagementBaseObject methodParameters = computerSystem.GetMethodParameters("JoinDomainOrWorkgroup");
			methodParameters.SetPropertyValue("Name", this._domainName);
			methodParameters.SetPropertyValue("UserName", str2);
			methodParameters.SetPropertyValue("Password", str3);
			methodParameters.SetPropertyValue("AccountOU", this._ouPath);
			methodParameters.SetPropertyValue("FJoinOptions", this._joinDomainflags);
			ManagementBaseObject managementBaseObject = computerSystem.InvokeMethod("JoinDomainOrWorkgroup", methodParameters, null);
			int num = Convert.ToInt32(managementBaseObject["ReturnValue"], CultureInfo.CurrentCulture);
			if (num != 0)
			{
				Win32Exception win32Exception = new Win32Exception(num);
				if (oldDomainName == null)
				{
					object[] message = new object[4];
					message[0] = computerName;
					message[1] = this._domainName;
					message[2] = curWorkgroupName;
					message[3] = win32Exception.Message;
					str = StringUtil.Format(ComputerResources.FailToJoinDomainFromWorkgroup, message);
					str1 = "FailToJoinDomainFromWorkgroup";
				}
				else
				{
					object[] objArray = new object[4];
					objArray[0] = computerName;
					objArray[1] = oldDomainName;
					objArray[2] = this._domainName;
					objArray[3] = win32Exception.Message;
					str = StringUtil.Format(ComputerResources.FailToJoinNewDomainAfterUnjoinOldDomain, objArray);
					str1 = "FailToJoinNewDomainAfterUnjoinOldDomain";
				}
				this.WriteErrorHelper(str, str1, computerName, ErrorCategory.OperationStopped, false, new object[0]);
			}
			return num;
		}

		private int JoinWorkgroup(ManagementObject computerSystem, string computerName, string oldDomainName)
		{
			string str;
			ManagementBaseObject methodParameters = computerSystem.GetMethodParameters("JoinDomainOrWorkgroup");
			methodParameters.SetPropertyValue("Name", this._workgroupName);
			methodParameters.SetPropertyValue("UserName", null);
			methodParameters.SetPropertyValue("Password", null);
			methodParameters.SetPropertyValue("FJoinOptions", 0);
			ManagementBaseObject managementBaseObject = computerSystem.InvokeMethod("JoinDomainOrWorkgroup", methodParameters, null);
			int num = Convert.ToInt32(managementBaseObject["ReturnValue"], CultureInfo.CurrentCulture);
			if (num != 0)
			{
				Win32Exception win32Exception = new Win32Exception(num);
				if (oldDomainName == null)
				{
					object[] message = new object[3];
					message[0] = computerName;
					message[1] = this._workgroupName;
					message[2] = win32Exception.Message;
					str = StringUtil.Format(ComputerResources.FailToJoinWorkGroup, message);
				}
				else
				{
					object[] objArray = new object[4];
					objArray[0] = computerName;
					objArray[1] = oldDomainName;
					objArray[2] = this._workgroupName;
					objArray[3] = win32Exception.Message;
					str = StringUtil.Format(ComputerResources.FailToSwitchFromDomainToWorkgroup, objArray);
				}
				this.WriteErrorHelper(str, "FailToJoinWorkGroup", computerName, ErrorCategory.OperationStopped, false, new object[0]);
			}
			return num;
		}

		protected override void ProcessRecord()
		{
			if (this._newName == null || (int)this._computerName.Length == 1)
			{
				ConnectionOptions connectionOption = new ConnectionOptions();
				connectionOption.Authentication = AuthenticationLevel.PacketPrivacy;
				connectionOption.Impersonation = ImpersonationLevel.Impersonate;
				connectionOption.EnablePrivileges = true;
				ConnectionOptions connectionOption1 = connectionOption;
				EnumerationOptions enumerationOption = new EnumerationOptions();
				enumerationOption.UseAmendedQualifiers = true;
				enumerationOption.DirectRead = true;
				EnumerationOptions enumerationOption1 = enumerationOption;
				ObjectQuery objectQuery = new ObjectQuery("select * from Win32_ComputerSystem");
				int num = this._joinDomainflags;
				if (this._newName != null && base.ParameterSetName == "Domain")
				{
					AddComputerCommand addComputerCommand = this;
					addComputerCommand._joinDomainflags = addComputerCommand._joinDomainflags | 0x100;
				}
				try
				{
					string[] strArrays = this._computerName;
					for (int i = 0; i < (int)strArrays.Length; i++)
					{
						string str = strArrays[i];
						string str1 = this.ValidateComputerName(str, this._newName != null);
						if (str1 != null)
						{
							bool flag = str1.Equals("localhost", StringComparison.OrdinalIgnoreCase);
							if (!flag)
							{
								this.DoAddComputerAction(str1, this._newName, false, connectionOption1, enumerationOption1, objectQuery);
							}
							else
							{
								if (!this._containsLocalHost)
								{
									this._containsLocalHost = true;
								}
								this._newNameForLocalHost = this._newName;
							}
						}
					}
				}
				finally
				{
					if (this._newName != null && base.ParameterSetName == "Domain")
					{
						this._joinDomainflags = num;
					}
				}
				return;
			}
			else
			{
				this.WriteErrorHelper(ComputerResources.CannotRenameMultipleComputers, "CannotRenameMultipleComputers", this._newName, ErrorCategory.InvalidArgument, false, new object[0]);
				return;
			}
		}

		private int RenameComputer(ManagementObject computerSystem, string computerName, string newName)
		{
			string str;
			string str1;
			string userName = null;
			string stringFromSecureString = null;
			if (this._domainName != null && this.Credential != null)
			{
				userName = this.Credential.UserName;
				stringFromSecureString = Utils.GetStringFromSecureString(this.Credential.Password);
			}
			ManagementBaseObject methodParameters = computerSystem.GetMethodParameters("Rename");
			methodParameters.SetPropertyValue("Name", newName);
			methodParameters.SetPropertyValue("UserName", userName);
			methodParameters.SetPropertyValue("Password", stringFromSecureString);
			ManagementBaseObject managementBaseObject = computerSystem.InvokeMethod("Rename", methodParameters, null);
			int num = Convert.ToInt32(managementBaseObject["ReturnValue"], CultureInfo.CurrentCulture);
			if (num != 0)
			{
				Win32Exception win32Exception = new Win32Exception(num);
				if (this._workgroupName == null)
				{
					object[] message = new object[4];
					message[0] = computerName;
					message[1] = this._domainName;
					message[2] = newName;
					message[3] = win32Exception.Message;
					str = StringUtil.Format(ComputerResources.FailToRenameAfterJoinDomain, message);
					str1 = "FailToRenameAfterJoinDomain";
				}
				else
				{
					object[] objArray = new object[4];
					objArray[0] = computerName;
					objArray[1] = this._workgroupName;
					objArray[2] = newName;
					objArray[3] = win32Exception.Message;
					str = StringUtil.Format(ComputerResources.FailToRenameAfterJoinWorkgroup, objArray);
					str1 = "FailToRenameAfterJoinWorkgroup";
				}
				this.WriteErrorHelper(str, str1, computerName, ErrorCategory.OperationStopped, false, new object[0]);
			}
			return num;
		}

		private int UnjoinDomain(ManagementObject computerSystem, string computerName, string curDomainName, string dUserName, string dPassword)
		{
			ManagementBaseObject methodParameters = computerSystem.GetMethodParameters("UnjoinDomainOrWorkgroup");
			methodParameters.SetPropertyValue("UserName", dUserName);
			methodParameters.SetPropertyValue("Password", dPassword);
			methodParameters.SetPropertyValue("FUnjoinOptions", 4);
			ManagementBaseObject managementBaseObject = computerSystem.InvokeMethod("UnjoinDomainOrWorkgroup", methodParameters, null);
			int num = Convert.ToInt32(managementBaseObject["ReturnValue"], CultureInfo.CurrentCulture);
			if (num != 0)
			{
				Win32Exception win32Exception = new Win32Exception(num);
				object[] message = new object[3];
				message[0] = computerName;
				message[1] = curDomainName;
				message[2] = win32Exception.Message;
				this.WriteErrorHelper(ComputerResources.FailToUnjoinDomain, "FailToUnjoinDomain", computerName, ErrorCategory.OperationStopped, false, message);
			}
			return num;
		}

		private string ValidateComputerName(string computer, bool validateNewName)
		{
			IPAddress pAddress = null;
			string str;
			object obj;
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
					goto Label0;
				}
				catch (Exception exception3)
				{
					Exception exception2 = exception3;
					CommandProcessorBase.CheckForSevereException(exception2);
					if (flag)
					{
						str1 = computer;
						goto Label0;
					}
					else
					{
						object[] message = new object[2];
						message[0] = computer;
						message[1] = exception2.Message;
						this.WriteErrorHelper(ComputerResources.CannotResolveComputerName, "AddressResolutionException", computer, ErrorCategory.InvalidArgument, false, message);
						str = null;
					}
				}
				return str;
			}
		Label0:
			bool flag1 = str1.Equals("localhost", StringComparison.OrdinalIgnoreCase);
			if (!validateNewName || this._newName == null || ComputerWMIHelper.IsComputerNameValid(this._newName))
			{
				return str1;
			}
			else
			{
				AddComputerCommand addComputerCommand = this;
				string invalidNewName = ComputerResources.InvalidNewName;
				string str2 = "InvalidNewName";
				string str3 = this._newName;
				int num = 5;
				int num1 = 0;
				object[] objArray = new object[2];
				object[] objArray1 = objArray;
				int num2 = 0;
				if (flag1)
				{
					obj = this._shortLocalMachineName;
				}
				else
				{
					obj = str1;
				}
				objArray1[num2] = obj;
				objArray[1] = this._newName;
				addComputerCommand.WriteErrorHelper(invalidNewName, str2, str3, (ErrorCategory)num, num1 == 1 ? true : false, objArray);
				return null;
			}
		}

		private void WriteErrorHelper(string resourceString, string errorId, object targetObj, ErrorCategory category, bool terminating, object[] args)
		{
			string str;
			if (args == null || (int)args.Length == 0)
			{
				str = resourceString;
			}
			else
			{
				str = StringUtil.Format(resourceString, args);
			}
			string.IsNullOrEmpty(str);
			ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str), errorId, category, targetObj);
			if (!terminating)
			{
				base.WriteError(errorRecord);
				return;
			}
			else
			{
				base.ThrowTerminatingError(errorRecord);
				return;
			}
		}
	}
}