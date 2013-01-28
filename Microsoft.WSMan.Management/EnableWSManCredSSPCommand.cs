using Microsoft.Win32;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Xml;

namespace Microsoft.WSMan.Management
{
	[Cmdlet("Enable", "WSManCredSSP", HelpUri="http://go.microsoft.com/fwlink/?LinkId=141442")]
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="SSP")]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Cred")]
	public class EnableWSManCredSSPCommand : WSManCredSSPCommandBase, IDisposable
	{
		private const string applicationname = "wsman";

		private string[] delegatecomputer;

		private bool force;

		private WSManHelper helper;

		[Parameter(Position=1)]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		[ValidateNotNullOrEmpty]
		public string[] DelegateComputer
		{
			get
			{
				return this.delegatecomputer;
			}
			set
			{
				this.delegatecomputer = value;
			}
		}

		[Parameter]
		public SwitchParameter Force
		{
			get
			{
				return this.force;
			}
			set
			{
				this.force = value;
			}
		}

		public EnableWSManCredSSPCommand()
		{
		}

		protected override void BeginProcessing()
		{
			WSManHelper.ThrowIfNotAdministrator();
			this.helper = new WSManHelper(this);
			if (Environment.OSVersion.Version.Major >= 6)
			{
				WSManHelper.ThrowIfNotAdministrator();
				if (this.delegatecomputer == null || base.Role.Equals("Client", StringComparison.OrdinalIgnoreCase))
				{
					if (!base.Role.Equals("Client", StringComparison.OrdinalIgnoreCase) || this.delegatecomputer != null)
					{
						if (base.Role.Equals("Client", StringComparison.OrdinalIgnoreCase))
						{
							this.EnableClientSideSettings();
						}
						if (base.Role.Equals("Server", StringComparison.OrdinalIgnoreCase))
						{
							this.EnableServerSideSettings();
						}
						return;
					}
					else
					{
						object[] objArray = new object[3];
						objArray[0] = "DelegateComputer";
						objArray[1] = "Role";
						objArray[2] = "Client";
						string str = this.helper.FormatResourceMsgFromResourcetext("CredSSPClientAndDelegateMustBeSpecified", objArray);
						throw new InvalidOperationException(str);
					}
				}
				else
				{
					object[] role = new object[4];
					role[0] = "DelegateComputer";
					role[1] = "Role";
					role[2] = base.Role;
					role[3] = "Client";
					string str1 = this.helper.FormatResourceMsgFromResourcetext("CredSSPRoleAndDelegateCannotBeSpecified", role);
					throw new InvalidOperationException(str1);
				}
			}
			else
			{
				string str2 = this.helper.FormatResourceMsgFromResourcetext("CmdletNotAvailable", new object[0]);
				throw new InvalidOperationException(str2);
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public void Dispose(IWSManSession sessionObject)
		{
			sessionObject = null;
			this.Dispose();
		}

		private void EnableClientSideSettings()
		{
			string resourceMsgFromResourcetext = this.helper.GetResourceMsgFromResourcetext("CredSSPContinueQuery");
			string str = this.helper.GetResourceMsgFromResourcetext("CredSSPContinueCaption");
			if (this.force || base.ShouldContinue(resourceMsgFromResourcetext, str))
			{
				IWSManSession wSManSession = base.CreateWSManSession();
				if (wSManSession != null)
				{
					try
					{
						string str1 = wSManSession.Get(this.helper.CredSSP_RUri, 0);
						XmlNode xmlNode = this.helper.GetXmlNode(str1, this.helper.CredSSP_SNode, this.helper.CredSSP_XMLNmsp);
						if (xmlNode != null)
						{
							string str2 = "<cfg:Auth xmlns:cfg=\"http://schemas.microsoft.com/wbem/wsman/1/config/client/auth\"><cfg:CredSSP>true</cfg:CredSSP></cfg:Auth>";
							try
							{
								XmlDocument xmlDocument = new XmlDocument();
								xmlDocument.LoadXml(wSManSession.Put(this.helper.CredSSP_RUri, str2, 0));
								if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
								{
									ThreadStart threadStart = new ThreadStart(this.UpdateCurrentUserRegistrySettings);
									Thread thread = new Thread(threadStart);
									thread.SetApartmentState(ApartmentState.STA);
									thread.Start();
									thread.Join();
								}
								else
								{
									this.UpdateCurrentUserRegistrySettings();
								}
								if (!this.helper.ValidateCreadSSPRegistryRetry(true, this.delegatecomputer, "wsman"))
								{
									this.helper.AssertError(this.helper.GetResourceMsgFromResourcetext("EnableCredSSPPolicyValidateError"), false, this.delegatecomputer);
								}
								else
								{
									base.WriteObject(xmlDocument.FirstChild);
								}
							}
							catch (COMException cOMException)
							{
								this.helper.AssertError(wSManSession.Error, true, this.delegatecomputer);
							}
						}
						else
						{
							InvalidOperationException invalidOperationException = new InvalidOperationException();
							ErrorRecord errorRecord = new ErrorRecord(invalidOperationException, this.helper.GetResourceMsgFromResourcetext("WinrmNotConfigured"), ErrorCategory.InvalidOperation, null);
							base.WriteError(errorRecord);
						}
					}
					finally
					{
						if (!string.IsNullOrEmpty(wSManSession.Error))
						{
							this.helper.AssertError(wSManSession.Error, true, this.delegatecomputer);
						}
						if (wSManSession != null)
						{
							this.Dispose(wSManSession);
						}
					}
					return;
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		private void EnableServerSideSettings()
		{
			string resourceMsgFromResourcetext = this.helper.GetResourceMsgFromResourcetext("CredSSPServerContinueQuery");
			string str = this.helper.GetResourceMsgFromResourcetext("CredSSPContinueCaption");
			if (this.force || base.ShouldContinue(resourceMsgFromResourcetext, str))
			{
				IWSManSession wSManSession = base.CreateWSManSession();
				if (wSManSession != null)
				{
					try
					{
						string str1 = wSManSession.Get(this.helper.Service_CredSSP_Uri, 0);
						XmlNode xmlNode = this.helper.GetXmlNode(str1, this.helper.CredSSP_SNode, this.helper.Service_CredSSP_XMLNmsp);
						if (xmlNode != null)
						{
							try
							{
								XmlDocument xmlDocument = new XmlDocument();
								object[] serviceCredSSPXMLNmsp = new object[1];
								serviceCredSSPXMLNmsp[0] = this.helper.Service_CredSSP_XMLNmsp;
								string str2 = string.Format(CultureInfo.InvariantCulture, "<cfg:Auth xmlns:cfg=\"{0}\"><cfg:CredSSP>true</cfg:CredSSP></cfg:Auth>", serviceCredSSPXMLNmsp);
								xmlDocument.LoadXml(wSManSession.Put(this.helper.Service_CredSSP_Uri, str2, 0));
								base.WriteObject(xmlDocument.FirstChild);
							}
							catch (COMException cOMException)
							{
								this.helper.AssertError(wSManSession.Error, true, this.delegatecomputer);
							}
						}
						else
						{
							InvalidOperationException invalidOperationException = new InvalidOperationException();
							ErrorRecord errorRecord = new ErrorRecord(invalidOperationException, this.helper.GetResourceMsgFromResourcetext("WinrmNotConfigured"), ErrorCategory.InvalidOperation, null);
							base.WriteError(errorRecord);
						}
					}
					finally
					{
						if (!string.IsNullOrEmpty(wSManSession.Error))
						{
							this.helper.AssertError(wSManSession.Error, true, this.delegatecomputer);
						}
						if (wSManSession != null)
						{
							this.Dispose(wSManSession);
						}
					}
					return;
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		private void UpdateCurrentUserRegistrySettings()
		{
			IGroupPolicyObject gPClass = (IGroupPolicyObject)(new GPClass());
			gPClass.OpenLocalMachineGPO(1);
			gPClass.GetRegistryKey(2);
			RegistryKey currentUser = Registry.CurrentUser;
			string str = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Group Policy Objects";
			RegistryKey registryKey = currentUser.OpenSubKey(str, true);
			string[] subKeyNames = registryKey.GetSubKeyNames();
			for (int i = 0; i < (int)subKeyNames.Length; i++)
			{
				string str1 = subKeyNames[i];
				if (str1.EndsWith("Machine", StringComparison.OrdinalIgnoreCase))
				{
					string str2 = string.Concat(str, "\\", str1, "\\Software\\Policies\\Microsoft\\Windows");
					this.UpdateGPORegistrySettings("wsman", this.delegatecomputer, Registry.CurrentUser, str2);
				}
			}
			gPClass.Save(true, true, new Guid("35378EAC-683F-11D2-A89A-00C04FBBCFA2"), new Guid("7A9206BD-33AF-47af-B832-D4128730E990"));
		}

		private void UpdateGPORegistrySettings(string applicationname, string[] delegatestring, RegistryKey rootKey, string Registry_Path)
		{
			try
			{
				string str = string.Concat(Registry_Path, "\\CredentialsDelegation");
				RegistryKey registryKey = rootKey.OpenSubKey(str, true);
				if (registryKey == null)
				{
					registryKey = rootKey.CreateSubKey(str, RegistryKeyPermissionCheck.ReadWriteSubTree);
				}
				registryKey.SetValue(this.helper.Key_Allow_Fresh_Credentials, 1, RegistryValueKind.DWord);
				registryKey.SetValue(this.helper.Key_Concatenate_Defaults_AllowFresh, 1, RegistryValueKind.DWord);
				RegistryKey registryKey1 = rootKey.OpenSubKey(string.Concat(str, "\\", this.helper.Key_Allow_Fresh_Credentials), true);
				if (registryKey1 == null)
				{
					registryKey1 = rootKey.CreateSubKey(string.Concat(str, "\\", this.helper.Key_Allow_Fresh_Credentials), RegistryKeyPermissionCheck.ReadWriteSubTree);
				}
				if (registryKey1 != null)
				{
					int valueCount = registryKey1.ValueCount;
					string[] strArrays = delegatestring;
					for (int i = 0; i < (int)strArrays.Length; i++)
					{
						string str1 = strArrays[i];
						registryKey1.SetValue(Convert.ToString(valueCount + 1, CultureInfo.InvariantCulture), string.Concat(applicationname, "/", str1), RegistryValueKind.String);
						valueCount++;
					}
				}
			}
			catch (UnauthorizedAccessException unauthorizedAccessException1)
			{
				UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
				ErrorRecord errorRecord = new ErrorRecord(unauthorizedAccessException, "UnauthorizedAccessException", ErrorCategory.PermissionDenied, null);
				base.WriteError(errorRecord);
			}
			catch (SecurityException securityException1)
			{
				SecurityException securityException = securityException1;
				ErrorRecord errorRecord1 = new ErrorRecord(securityException, "SecurityException", ErrorCategory.InvalidOperation, null);
				base.WriteError(errorRecord1);
			}
			catch (ArgumentException argumentException1)
			{
				ArgumentException argumentException = argumentException1;
				ErrorRecord errorRecord2 = new ErrorRecord(argumentException, "ArgumentException", ErrorCategory.InvalidOperation, null);
				base.WriteError(errorRecord2);
			}
		}
	}
}