using Microsoft.Win32;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Security;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.WSMan.Management
{
	[Cmdlet("Get", "WSManCredSSP", HelpUri="http://go.microsoft.com/fwlink/?LinkId=141443")]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Cred")]
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId="SSP")]
	public class GetWSManCredSSPCommand : PSCmdlet, IDisposable
	{
		private WSManHelper helper;

		public GetWSManCredSSPCommand()
		{
		}

		protected override void BeginProcessing()
		{
			WSManHelper.ThrowIfNotAdministrator();
			this.helper = new WSManHelper(this);
			if (Environment.OSVersion.Version.Major >= 6)
			{
				WSManHelper.ThrowIfNotAdministrator();
				IWSManSession wSManSession = null;
				try
				{
					try
					{
						IWSManEx wSManClass = (IWSManEx)(new WSManClass());
						wSManSession = (IWSManSession)wSManClass.CreateSession(null, 0, null);
						string str = wSManSession.Get(this.helper.CredSSP_RUri, 0);
						XmlNode xmlNode = this.helper.GetXmlNode(str, this.helper.CredSSP_SNode, this.helper.CredSSP_XMLNmsp);
						if (xmlNode != null)
						{
							string str1 = "wsman";
							string delegateSettings = this.GetDelegateSettings(str1);
							if (!string.IsNullOrEmpty(delegateSettings))
							{
								base.WriteObject(string.Concat(this.helper.GetResourceMsgFromResourcetext("DelegateFreshCred"), delegateSettings));
							}
							else
							{
								base.WriteObject(this.helper.GetResourceMsgFromResourcetext("NoDelegateFreshCred"));
							}
							str = wSManSession.Get(this.helper.Service_CredSSP_Uri, 0);
							xmlNode = this.helper.GetXmlNode(str, this.helper.CredSSP_SNode, this.helper.Service_CredSSP_XMLNmsp);
							if (xmlNode != null)
							{
								if (!xmlNode.InnerText.Equals("true", StringComparison.OrdinalIgnoreCase))
								{
									base.WriteObject(this.helper.GetResourceMsgFromResourcetext("CredSSPServiceNotConfigured"));
								}
								else
								{
									base.WriteObject(this.helper.GetResourceMsgFromResourcetext("CredSSPServiceConfigured"));
								}
							}
							else
							{
								InvalidOperationException invalidOperationException = new InvalidOperationException();
								ErrorRecord errorRecord = new ErrorRecord(invalidOperationException, this.helper.GetResourceMsgFromResourcetext("WinrmNotConfigured"), ErrorCategory.InvalidOperation, null);
								base.WriteError(errorRecord);
								return;
							}
						}
						else
						{
							InvalidOperationException invalidOperationException1 = new InvalidOperationException();
							ErrorRecord errorRecord1 = new ErrorRecord(invalidOperationException1, this.helper.GetResourceMsgFromResourcetext("WinrmNotConfigured"), ErrorCategory.InvalidOperation, null);
							base.WriteError(errorRecord1);
							return;
						}
					}
					catch (UnauthorizedAccessException unauthorizedAccessException1)
					{
						UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
						ErrorRecord errorRecord2 = new ErrorRecord(unauthorizedAccessException, "UnauthorizedAccess", ErrorCategory.PermissionDenied, null);
						base.WriteError(errorRecord2);
					}
					catch (SecurityException securityException1)
					{
						SecurityException securityException = securityException1;
						ErrorRecord errorRecord3 = new ErrorRecord(securityException, "SecurityException", ErrorCategory.InvalidOperation, null);
						base.WriteError(errorRecord3);
					}
					catch (ArgumentException argumentException1)
					{
						ArgumentException argumentException = argumentException1;
						ErrorRecord errorRecord4 = new ErrorRecord(argumentException, "InvalidArgument", ErrorCategory.InvalidOperation, null);
						base.WriteError(errorRecord4);
					}
					catch (XPathException xPathException1)
					{
						XPathException xPathException = xPathException1;
						ErrorRecord errorRecord5 = new ErrorRecord(xPathException, "XPathException", ErrorCategory.InvalidOperation, null);
						base.WriteError(errorRecord5);
					}
				}
				finally
				{
					if (!string.IsNullOrEmpty(wSManSession.Error))
					{
						this.helper.AssertError(wSManSession.Error, true, null);
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

		private string GetDelegateSettings(string applicationname)
		{
			RegistryKey localMachine = Registry.LocalMachine;
			string empty = string.Empty;
			try
			{
				string str = string.Concat(this.helper.Registry_Path_Credentials_Delegation, "\\CredentialsDelegation");
				RegistryKey registryKey = localMachine.OpenSubKey(str);
				if (registryKey != null)
				{
					registryKey = registryKey.OpenSubKey(this.helper.Key_Allow_Fresh_Credentials);
					if (registryKey != null)
					{
						string[] valueNames = registryKey.GetValueNames();
						if ((int)valueNames.Length > 0)
						{
							RegistryKey registryKey1 = Registry.CurrentUser.OpenSubKey("Control Panel\\International");
							string str1 = registryKey1.GetValue("sList").ToString();
							string[] strArrays = valueNames;
							for (int i = 0; i < (int)strArrays.Length; i++)
							{
								string str2 = strArrays[i];
								object value = registryKey.GetValue(str2);
								if (value != null && value.ToString().StartsWith(applicationname, StringComparison.OrdinalIgnoreCase))
								{
									empty = string.Concat(value.ToString(), str1, empty);
								}
							}
							if (empty.EndsWith(str1, StringComparison.OrdinalIgnoreCase))
							{
								empty = empty.Remove(empty.Length - 1);
							}
						}
					}
				}
			}
			catch (ArgumentException argumentException1)
			{
				ArgumentException argumentException = argumentException1;
				ErrorRecord errorRecord = new ErrorRecord(argumentException, "ArgumentException", ErrorCategory.PermissionDenied, null);
				base.WriteError(errorRecord);
			}
			catch (SecurityException securityException1)
			{
				SecurityException securityException = securityException1;
				ErrorRecord errorRecord1 = new ErrorRecord(securityException, "SecurityException", ErrorCategory.PermissionDenied, null);
				base.WriteError(errorRecord1);
			}
			catch (ObjectDisposedException objectDisposedException1)
			{
				ObjectDisposedException objectDisposedException = objectDisposedException1;
				ErrorRecord errorRecord2 = new ErrorRecord(objectDisposedException, "ObjectDisposedException", ErrorCategory.PermissionDenied, null);
				base.WriteError(errorRecord2);
			}
			return empty;
		}
	}
}