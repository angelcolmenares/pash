using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using System.Security;
using System.Threading;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.WSMan.Management
{
	[Cmdlet("Disable", "WSManCredSSP", HelpUri="http://go.microsoft.com/fwlink/?LinkId=141438")]
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="SSP")]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Cred")]
	public class DisableWSManCredSSPCommand : WSManCredSSPCommandBase, IDisposable
	{
		private const string applicationname = "wsman";

		public DisableWSManCredSSPCommand()
		{
		}

		protected override void BeginProcessing()
		{
			if (Environment.OSVersion.Version.Major >= 6)
			{
				WSManHelper.ThrowIfNotAdministrator();
				if (base.Role.Equals("Client", StringComparison.OrdinalIgnoreCase))
				{
					this.DisableClientSideSettings();
				}
				if (base.Role.Equals("Server", StringComparison.OrdinalIgnoreCase))
				{
					this.DisableServerSideSettings();
				}
				return;
			}
			else
			{
				WSManHelper wSManHelper = new WSManHelper(this);
				string str = wSManHelper.FormatResourceMsgFromResourcetext("CmdletNotAvailable", new object[0]);
				throw new InvalidOperationException(str);
			}
		}

		private void DeleteDelegateSettings(string applicationname, RegistryKey rootKey, string Registry_Path, IGroupPolicyObject GPO)
		{
			WSManHelper wSManHelper = new WSManHelper(this);
			int num = 0;
			bool flag = false;
			try
			{
				string str = string.Concat(Registry_Path, "\\CredentialsDelegation");
				RegistryKey registryKey = rootKey.OpenSubKey(string.Concat(str, "\\", wSManHelper.Key_Allow_Fresh_Credentials), true);
				if (registryKey != null)
				{
					string[] valueNames = registryKey.GetValueNames();
					if ((int)valueNames.Length > 0)
					{
						Collection<string> strs = new Collection<string>();
						string[] strArrays = valueNames;
						for (int i = 0; i < (int)strArrays.Length; i++)
						{
							string str1 = strArrays[i];
							object value = registryKey.GetValue(str1);
							if (value != null && !value.ToString().StartsWith(applicationname, StringComparison.OrdinalIgnoreCase))
							{
								strs.Add(value.ToString());
								flag = true;
							}
							registryKey.DeleteValue(str1);
						}
						foreach (string str2 in strs)
						{
							registryKey.SetValue(Convert.ToString(num + 1, CultureInfo.InvariantCulture), str2, RegistryValueKind.String);
							num++;
						}
					}
				}
				if (!flag)
				{
					RegistryKey registryKey1 = rootKey.OpenSubKey(str, true);
					if (registryKey1 != null)
					{
						object obj = registryKey1.GetValue(wSManHelper.Key_Allow_Fresh_Credentials);
						if (obj != null)
						{
							registryKey1.DeleteValue(wSManHelper.Key_Allow_Fresh_Credentials, false);
						}
						object value1 = registryKey1.GetValue(wSManHelper.Key_Concatenate_Defaults_AllowFresh);
						if (value1 != null)
						{
							registryKey1.DeleteValue(wSManHelper.Key_Concatenate_Defaults_AllowFresh, false);
						}
						if (registryKey1.OpenSubKey(wSManHelper.Key_Allow_Fresh_Credentials) != null)
						{
							registryKey1.DeleteSubKeyTree(wSManHelper.Key_Allow_Fresh_Credentials);
						}
					}
				}
				GPO.Save(true, true, new Guid("35378EAC-683F-11D2-A89A-00C04FBBCFA2"), new Guid("6AD20875-336C-4e22-968F-C709ACB15814"));
			}
			catch (InvalidOperationException invalidOperationException1)
			{
				InvalidOperationException invalidOperationException = invalidOperationException1;
				ErrorRecord errorRecord = new ErrorRecord(invalidOperationException, "InvalidOperation", ErrorCategory.InvalidOperation, null);
				base.WriteError(errorRecord);
			}
			catch (ArgumentException argumentException1)
			{
				ArgumentException argumentException = argumentException1;
				ErrorRecord errorRecord1 = new ErrorRecord(argumentException, "InvalidArgument", ErrorCategory.InvalidArgument, null);
				base.WriteError(errorRecord1);
			}
			catch (SecurityException securityException1)
			{
				SecurityException securityException = securityException1;
				ErrorRecord errorRecord2 = new ErrorRecord(securityException, "SecurityException", ErrorCategory.SecurityError, null);
				base.WriteError(errorRecord2);
			}
			catch (UnauthorizedAccessException unauthorizedAccessException1)
			{
				UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
				ErrorRecord errorRecord3 = new ErrorRecord(unauthorizedAccessException, "UnauthorizedAccess", ErrorCategory.SecurityError, null);
				base.WriteError(errorRecord3);
			}
		}

		private void DeleteUserDelegateSettings()
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
					this.DeleteDelegateSettings("wsman", Registry.CurrentUser, str2, gPClass);
				}
			}
		}

		private void DisableClientSideSettings()
		{
			WSManHelper wSManHelper = new WSManHelper(this);
			IWSManSession wSManSession = base.CreateWSManSession();
			if (wSManSession != null)
			{
				try
				{
					try
					{
						string str = wSManSession.Get(wSManHelper.CredSSP_RUri, 0);
						XmlDocument xmlDocument = new XmlDocument();
						xmlDocument.LoadXml(str);
						XmlNamespaceManager xmlNamespaceManagers = new XmlNamespaceManager(xmlDocument.NameTable);
						xmlNamespaceManagers.AddNamespace("cfg", wSManHelper.CredSSP_XMLNmsp);
						XmlNode xmlNodes = xmlDocument.SelectSingleNode(wSManHelper.CredSSP_SNode, xmlNamespaceManagers);
						if (xmlNodes == null)
						{
							InvalidOperationException invalidOperationException = new InvalidOperationException();
							ErrorRecord errorRecord = new ErrorRecord(invalidOperationException, wSManHelper.GetResourceMsgFromResourcetext("WinrmNotConfigured"), ErrorCategory.InvalidOperation, null);
							base.WriteError(errorRecord);
							return;
						}
						else
						{
							string str1 = "<cfg:Auth xmlns:cfg=\"http://schemas.microsoft.com/wbem/wsman/1/config/client/auth\"><cfg:CredSSP>false</cfg:CredSSP></cfg:Auth>";
							wSManSession.Put(wSManHelper.CredSSP_RUri, str1, 0);
							if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
							{
								ThreadStart threadStart = new ThreadStart(this.DeleteUserDelegateSettings);
								Thread thread = new Thread(threadStart);
								thread.SetApartmentState(ApartmentState.STA);
								thread.Start();
								thread.Join();
							}
							else
							{
								this.DeleteUserDelegateSettings();
							}
							if (!wSManHelper.ValidateCreadSSPRegistryRetry(false, null, "wsman"))
							{
								wSManHelper.AssertError(wSManHelper.GetResourceMsgFromResourcetext("DisableCredSSPPolicyValidateError"), false, null);
							}
						}
					}
					catch (XPathException xPathException1)
					{
						XPathException xPathException = xPathException1;
						ErrorRecord errorRecord1 = new ErrorRecord(xPathException, "XpathException", ErrorCategory.InvalidOperation, null);
						base.WriteError(errorRecord1);
					}
				}
				finally
				{
					if (!string.IsNullOrEmpty(wSManSession.Error))
					{
						wSManHelper.AssertError(wSManSession.Error, true, null);
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

		private void DisableServerSideSettings()
		{
			WSManHelper wSManHelper = new WSManHelper(this);
			IWSManSession wSManSession = base.CreateWSManSession();
			if (wSManSession != null)
			{
				try
				{
					string str = wSManSession.Get(wSManHelper.Service_CredSSP_Uri, 0);
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(str);
					XmlNamespaceManager xmlNamespaceManagers = new XmlNamespaceManager(xmlDocument.NameTable);
					xmlNamespaceManagers.AddNamespace("cfg", wSManHelper.Service_CredSSP_XMLNmsp);
					XmlNode xmlNodes = xmlDocument.SelectSingleNode(wSManHelper.CredSSP_SNode, xmlNamespaceManagers);
					if (xmlNodes == null)
					{
						InvalidOperationException invalidOperationException = new InvalidOperationException();
						ErrorRecord errorRecord = new ErrorRecord(invalidOperationException, wSManHelper.GetResourceMsgFromResourcetext("WinrmNotConfigured"), ErrorCategory.InvalidOperation, null);
						base.WriteError(errorRecord);
					}
					else
					{
						object[] serviceCredSSPXMLNmsp = new object[1];
						serviceCredSSPXMLNmsp[0] = wSManHelper.Service_CredSSP_XMLNmsp;
						string str1 = string.Format(CultureInfo.InvariantCulture, "<cfg:Auth xmlns:cfg=\"{0}\"><cfg:CredSSP>false</cfg:CredSSP></cfg:Auth>", serviceCredSSPXMLNmsp);
						wSManSession.Put(wSManHelper.Service_CredSSP_Uri, str1, 0);
					}
				}
				finally
				{
					if (!string.IsNullOrEmpty(wSManSession.Error))
					{
						wSManHelper.AssertError(wSManSession.Error, true, null);
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

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public void Dispose(IWSManSession sessionObject)
		{
			sessionObject = null;
			this.Dispose();
		}
	}
}