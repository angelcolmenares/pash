using Microsoft.Management.PowerShellWebAccess;
using System;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class LogOn : Page
	{
		private const int DefaultPort = 0x1761;

		private const string DefaultPortString = "5985";

		private const int DefaultSslPort = 0x1762;

		private const string DefaultSslPortString = "5986";

		private const string DefaultAppName = "WSMAN";

		private const string DefaultConfigUri = "http://schemas.microsoft.com/powershell/";

		private const string redirectCookieName = ".redirect.";

		protected HtmlForm logOnForm;

		protected Label messageLabel;

		protected HtmlGenericControl fieldSet;

		protected HtmlInputText userNameTextBox;

		protected HtmlInputPassword passwordTextBox;

		protected HtmlSelect connectionTypeSelection;

		protected HtmlInputText targetNodeTextBox;

		protected HtmlInputText connectionUriTextBox;

		protected HtmlInputText altUserNameTextBox;

		protected HtmlInputPassword altPasswordTextBox;

		protected HtmlInputText configurationNameTextBox;

		protected HtmlSelect authenticationTypeSelection;

		protected HtmlSelect useSslSelection;

		protected HtmlInputText portTextBox;

		protected HtmlInputText applicationNameTextBox;

		protected HtmlSelect allowRedirectionSelection;

		protected HtmlInputText advancedPanelShowLabel;

		protected Button ButtonLogOn;

		public LogOn()
		{
		}

		private void AddRedirectCookie()
		{
			DateTime now = DateTime.Now;
			FormsAuthenticationTicket formsAuthenticationTicket = new FormsAuthenticationTicket(1, "PSWA", DateTime.Now, now.AddSeconds(10), false, "");
			string str = FormsAuthentication.Encrypt(formsAuthenticationTicket);
			base.Response.Cookies.Add(new HttpCookie(".redirect.", str));
			FormsAuthentication.Decrypt(base.Response.Cookies[".redirect."].Value);
		}

		private void CreateSession(LogOn.FormInfo formInfo)
		{
			string str;
			string applicationName;
			string str1;
			string originalString;
			WSManConnectionInfo wSManConnectionInfo = null;
			if (formInfo.ConfigurationName.Length == 0)
			{
				str = null;
			}
			else
			{
				str = string.Concat("http://schemas.microsoft.com/powershell/", formInfo.ConfigurationName);
			}
			string str2 = str;
			PSCredential pSCredential = new PSCredential(formInfo.DestinationUserName, formInfo.DestinationPassword);
			if (!formInfo.IsUriConnection)
			{
				if (string.Compare(formInfo.ApplicationName, "WSMAN", StringComparison.OrdinalIgnoreCase) == 0)
				{
					applicationName = null;
				}
				else
				{
					applicationName = formInfo.ApplicationName;
				}
				string str3 = applicationName;
				try
				{
					wSManConnectionInfo = new WSManConnectionInfo(formInfo.UseSsl, formInfo.ComputerName, formInfo.Port, str3, str2, pSCredential);
				}
				catch (UriFormatException uriFormatException)
				{
					throw PowwaException.CreateValidationErrorException(Resources.LogonError_InvalidComputerNameUriFormat);
				}
				wSManConnectionInfo.AuthenticationMechanism = formInfo.AuthenticationType;
				PowwaEvents.PowwaEVENT_DEBUG_CONNECT_USING_COMPUTERNAME(formInfo.DestinationUserName, wSManConnectionInfo.ComputerName, wSManConnectionInfo.Port, wSManConnectionInfo.AppName, wSManConnectionInfo.ShellUri, wSManConnectionInfo.AuthenticationMechanism.ToString());
			}
			else
			{
				wSManConnectionInfo = new WSManConnectionInfo(formInfo.ConnectionUri, str2, pSCredential);
				if (!formInfo.AllowRedirection)
				{
					wSManConnectionInfo.MaximumConnectionRedirectionCount = 0;
				}
				PowwaEvents.PowwaEVENT_DEBUG_CONNECT_USING_URI(formInfo.DestinationUserName, wSManConnectionInfo.ConnectionUri.AbsoluteUri, wSManConnectionInfo.ShellUri);
			}
			string sourceIPAddressRemoteAddr = SessionHelper.GetSourceIPAddressRemoteAddr();
			string sourceIPAddressHttpXForwardedFor = SessionHelper.GetSourceIPAddressHttpXForwardedFor();
			if (formInfo.IsUriConnection)
			{
				str1 = null;
			}
			else
			{
				str1 = PswaHelper.TranslateLocalComputerName(formInfo.ComputerName);
			}
			string str4 = str1;
			PowwaAuthorizationManager.Instance.CheckLogOnCredential(formInfo.UserName, formInfo.Password, str4, formInfo.ConnectionUri, formInfo.ConfigurationName, sourceIPAddressRemoteAddr, sourceIPAddressHttpXForwardedFor);
			ClientInfo clientInfo = new ClientInfo(HttpContext.Current.Request.UserAgent, CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
			PowwaSession powwaSession = PowwaSessionManager.Instance.CreateSession(this.Session.SessionID, wSManConnectionInfo, clientInfo, formInfo.UserName);
			string name = powwaSession.Name;
			string userName = formInfo.UserName;
			string str5 = sourceIPAddressRemoteAddr;
			string str6 = sourceIPAddressHttpXForwardedFor;
			if (formInfo.IsUriConnection)
			{
				originalString = wSManConnectionInfo.ConnectionUri.OriginalString;
			}
			else
			{
				originalString = wSManConnectionInfo.ComputerName;
			}
			PowwaEvents.PowwaEVENT_SESSION_START(name, userName, str5, str6, originalString, formInfo.DestinationUserName, wSManConnectionInfo.Port, wSManConnectionInfo.AppName, wSManConnectionInfo.ShellUri);
			HttpCookie item = base.Request.Cookies["ASP.NET_SessionId"];
			if (FormsAuthentication.RequireSSL && item != null)
			{
				item.Secure = true;
			}
			FormsAuthentication.SetAuthCookie(formInfo.UserName, false, "/");
			base.Response.Redirect ("~/default.aspx");
			//FormsAuthentication.RedirectFromLoginPage(formInfo.UserName, false);
		}

		private static void GetLogonErrorMessage(Exception exception, out string userMessage, out string adminMessage)
		{
			PSRemotingTransportException pSRemotingTransportException = exception as PSRemotingTransportException;
			if (pSRemotingTransportException == null)
			{
				PowwaException powwaException = exception as PowwaException;
				if (powwaException == null)
				{
					ThreadAbortException threadAbortException = exception as ThreadAbortException;
					if (threadAbortException == null)
					{
						userMessage = Resources.LogonError_UnknownError;
						object[] message = new object[1];
						message[0] = exception.Message;
						adminMessage = string.Format(CultureInfo.CurrentCulture, Resources.LogonError_UnknownErrorAdminMessage, message);
						return;
					}
					else
					{
						string logonErrorThreadAborted = Resources.LogonError_ThreadAborted;
						string str = logonErrorThreadAborted;
						adminMessage = logonErrorThreadAborted;
						userMessage = str;
						return;
					}
				}
				else
				{
					string message1 = powwaException.Message;
					string str1 = message1;
					adminMessage = message1;
					userMessage = str1;
					return;
				}
			}
			else
			{
				int errorCode = pSRemotingTransportException.ErrorCode;
				if (errorCode > -2144108250)
				{
					if (errorCode > 5)
					{
						if (errorCode == 53)
						{
							userMessage = Resources.LogonError_InvalidComputer;
						}
						else
						{
							if (errorCode == 0x51f)
							{
								userMessage = Resources.LogonError_NoLogonServers;
							}
							else
							{
								if (errorCode != 0x52e)
								{
									goto Label0;
								}
								userMessage = Resources.LogonError_InvalidCredentials;
							}
						}
					}
					else
					{
						if (errorCode == -2144108126)
						{
							userMessage = Resources.LogonError_InvalidAuthenticationTypeCredSPP;
						}
						else
						{
							if (errorCode != 5)
							{
								goto Label0;
							}
							userMessage = Resources.LogonError_AccessDenied;
						}
					}
				}
				else
				{
					if (errorCode > -2144108485)
					{
						if (errorCode == -2144108322)
						{
							userMessage = Resources.LogonError_InvalidAuthenticationTypeBasicOrDigest;
						}
						else
						{
							if (errorCode != -2144108250)
							{
								goto Label0;
							}
							userMessage = Resources.LogonError_InvalidEndpoint;
						}
					}
					else
					{
						if (errorCode == -2144108526)
						{
							userMessage = Resources.LogonError_RemotingNotEnabled;
						}
						else
						{
							if (errorCode != -2144108485)
							{
								goto Label0;
							}
							userMessage = Resources.LogonError_InvalidConfigurationName;
						}
					}
				}
				adminMessage = pSRemotingTransportException.Message;
				return;
			}
		Label0:
			if (((long)pSRemotingTransportException.ErrorCode & (long)-65536) != (long)-2144141312)
			{
				userMessage = Resources.LogonError_ConnectionError;
				adminMessage = pSRemotingTransportException.Message;
				return;
			}
			else
			{
				object[] transportMessage = new object[1];
				transportMessage[0] = pSRemotingTransportException.TransportMessage;
				userMessage = string.Format(CultureInfo.CurrentCulture, Resources.LogonError_ConnectionErrorExtended, transportMessage);
				adminMessage = pSRemotingTransportException.Message;
				return;
			}
		}

		protected void OnLogOnButtonClick(object sender, EventArgs e)
		{
			string str = null;
			string str1 = null;
			IEtwActivity etwActivity = PowwaEvents.EventCorrelator.StartActivity();
			using (etwActivity)
			{
				try
				{
					LogOn.FormInfo formInfo = this.ValidateForm();
					using (formInfo)
					{
						this.CreateSession(formInfo);
					}
				}
				catch (PowwaValidationException powwaValidationException1)
				{
					PowwaValidationException powwaValidationException = powwaValidationException1;
					this.ShowError(powwaValidationException.Message);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					LogOn.GetLogonErrorMessage(exception, out str, out str1);
					this.ShowError(str);
					object[] objArray = new object[1];
					objArray[0] = str1;
					PowwaEvents.PowwaEVENT_LOGON_FAILURE(this.userNameTextBox.Value, SessionHelper.GetSourceIPAddressRemoteAddr(), SessionHelper.GetSourceIPAddressHttpXForwardedFor(), string.Format(CultureInfo.CurrentCulture, Resources.LogonError_LogMessage, objArray));
					Thread.Sleep(0x3e8);
				}
			}
		}

		protected void Page_Init(object sender, EventArgs e)
		{
			if (PowwaSessionManager.Instance.SessionExists(this.Session.SessionID))
			{
				base.Response.Redirect("./session.aspx");
			}
			base.ViewStateUserKey = this.Session.SessionID;
			this.Session["pswa"] = true;
			if (!base.IsPostBack)
			{
				if (base.Request.Cookies[".redirect."] == null || base.Request.Cookies[".redirect."].Value == "")
				{
					this.Session.Abandon();
					SessionHelper.RemoveSessionCookie(base.Response);
					this.AddRedirectCookie();
					base.Response.Redirect(base.Request.Path);
				}
				try
				{
					FormsAuthenticationTicket formsAuthenticationTicket = FormsAuthentication.Decrypt(base.Request.Cookies[".redirect."].Value);
					if (formsAuthenticationTicket == null || formsAuthenticationTicket.Expired)
					{
						throw new Exception();
					}
					else
					{
						this.RemoveRedirectCookie();
					}
				}
				catch
				{
					this.AddRedirectCookie();
					base.Response.Redirect(base.Request.Path);
				}
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			this.ButtonLogOn.Text = Resources.LogonButton_Text;

			if (!FormsAuthentication.RequireSSL || base.Request.IsSecureConnection)
			{
				SessionHelper.DisablePageCaching(base.Response);
				return;
			}
			else
			{
				this.ShowError(Resources.HttpsRequired);
				this.fieldSet.Disabled = true;
				return;
			}
		}

		private void RemoveRedirectCookie()
		{
			HttpCookie httpCookie = new HttpCookie(".redirect.", "");
			DateTime now = DateTime.Now;
			httpCookie.Expires = now.AddDays(-1);
			base.Response.Cookies.Add(httpCookie);
		}

		private static void SetErrorState(HtmlControl control, bool state)
		{
			if (!state)
			{
				control.RemoveCssClass("error");
				return;
			}
			else
			{
				control.AppendCssClass("error");
				return;
			}
		}

		private void ShowError(string message)
		{
			bool flag;
			this.messageLabel.Text = HttpUtility.HtmlEncode(message);
			LogOn.SetErrorState(this.userNameTextBox, true);
			LogOn.SetErrorState(this.passwordTextBox, true);
			LogOn.SetErrorState(this.altUserNameTextBox, this.altUserNameTextBox.Value.Length > 0);
			LogOn.SetErrorState(this.altPasswordTextBox, this.altPasswordTextBox.Value.Length > 0);
			LogOn.SetErrorState(this.targetNodeTextBox, this.targetNodeTextBox.Value.Length > 0);
			LogOn.SetErrorState(this.connectionUriTextBox, this.connectionUriTextBox.Value.Length > 0);
			HtmlInputText htmlInputText = this.portTextBox;
			if (string.Compare(this.portTextBox.Value.Trim(), "5985", StringComparison.OrdinalIgnoreCase) == 0)
			{
				flag = false;
			}
			else
			{
				flag = string.Compare(this.portTextBox.Value.Trim(), "5986", StringComparison.OrdinalIgnoreCase) != 0;
			}
			LogOn.SetErrorState(htmlInputText, flag);
			LogOn.SetErrorState(this.configurationNameTextBox, this.configurationNameTextBox.Value.Length > 0);
			LogOn.SetErrorState(this.applicationNameTextBox, string.Compare(this.applicationNameTextBox.Value.Trim(), "WSMAN", StringComparison.OrdinalIgnoreCase) != 0);
		}

		private static void ValidateCharactersInString(string s)
		{
			char[] chrArray = new char[] { '\0', '\uFFF9', '\uFFFA', '\uFFFB', '\uFFFC', '\uFFFD', '\uFFFE', '\uFFFF' };
			char[] chrArray1 = chrArray;
			char[] chrArray2 = chrArray1;
			int num = 0;
			while (num < (int)chrArray2.Length)
			{
				char chr = chrArray2[num];
				if (s.IndexOf(chr) < 0)
				{
					num++;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = (int)chr;
					throw PowwaException.CreateValidationErrorException(string.Format(CultureInfo.CurrentUICulture, Resources.LogonError_InvalidCharacterFormat, objArray));
				}
			}
		}

		private LogOn.FormInfo ValidateForm()
		{
			string value;
			HtmlInputControl htmlInputControl;
			LogOn.FormInfo formInfo = new LogOn.FormInfo();
			LogOn.ValidateCharactersInString(this.userNameTextBox.Value);
			LogOn.ValidateCharactersInString(this.passwordTextBox.Value);
			LogOn.ValidateCharactersInString(this.altUserNameTextBox.Value);
			LogOn.ValidateCharactersInString(this.passwordTextBox.Value);
			LogOn.ValidateCharactersInString(this.connectionUriTextBox.Value);
			LogOn.ValidateCharactersInString(this.targetNodeTextBox.Value);
			LogOn.ValidateCharactersInString(this.configurationNameTextBox.Value);
			LogOn.ValidateCharactersInString(this.applicationNameTextBox.Value);
			formInfo.UserName = PswaHelper.TranslateLocalAccountName(this.userNameTextBox.Value);
			formInfo.Password = this.passwordTextBox.Value;
			LogOn.FormInfo formInfo1 = formInfo;
			if (this.altUserNameTextBox.Value.Length > 0)
			{
				value = this.altUserNameTextBox.Value;
			}
			else
			{
				value = formInfo.UserName;
			}
			formInfo1.DestinationUserName = value;
			if (this.altPasswordTextBox.Value.Length > 0)
			{
				htmlInputControl = this.altPasswordTextBox;
			}
			else
			{
				htmlInputControl = this.passwordTextBox;
			}
			char[] charArray = htmlInputControl.Value.ToCharArray();
			formInfo.DestinationPassword = new SecureString();
			for (int i = 0; i < (int)charArray.Length; i++)
			{
				formInfo.DestinationPassword.AppendChar(charArray[i]);
				charArray[i] = '*';
			}
			formInfo.IsUriConnection = string.Compare(this.connectionTypeSelection.Value, "connection-uri", StringComparison.OrdinalIgnoreCase) == 0;
			if (!formInfo.IsUriConnection)
			{
				formInfo.ComputerName = this.targetNodeTextBox.Value;
				formInfo.UseSsl = this.useSslSelection.Value == "1";
				if (this.portTextBox.Value.Length != 0)
				{
					if (!int.TryParse(this.portTextBox.Value, out formInfo.Port))
					{
						throw PowwaException.CreateValidationErrorException(Resources.LogonError_InvalidPort);
					}
				}
				else
				{
					formInfo.Port = 0x1761;
				}
				formInfo.ApplicationName = this.applicationNameTextBox.Value;
			}
			else
			{
				try
				{
					formInfo.ConnectionUri = new Uri(this.connectionUriTextBox.Value);
				}
				catch (UriFormatException uriFormatException1)
				{
					UriFormatException uriFormatException = uriFormatException1;
					object[] message = new object[1];
					message[0] = uriFormatException.Message;
					throw PowwaException.CreateValidationErrorException(string.Format(CultureInfo.CurrentUICulture, Resources.LogonError_InvalidUri, message));
				}
				formInfo.AllowRedirection = this.allowRedirectionSelection.Value == "1";
			}
			formInfo.ConfigurationName = this.configurationNameTextBox.Value;
			string str = this.authenticationTypeSelection.Value;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "0")
				{
					formInfo.AuthenticationType = AuthenticationMechanism.Default;
				}
				else if (str1 == "1")
				{
					formInfo.AuthenticationType = AuthenticationMechanism.Basic;
				}
				else if (str1 == "2")
				{
					formInfo.AuthenticationType = AuthenticationMechanism.Negotiate;
				}
				else if (str1 == "4")
				{
					formInfo.AuthenticationType = AuthenticationMechanism.Credssp;
				}
				else if (str1 == "5")
				{
					formInfo.AuthenticationType = AuthenticationMechanism.Digest;
				}
				else if (str1 == "6")
				{
					formInfo.AuthenticationType = AuthenticationMechanism.Kerberos;
				}
				else
				{
					throw PowwaException.CreateValidationErrorException(Resources.InternalError_InvalidAuthenticationMechanism);
				}
				return formInfo;
			}
			throw PowwaException.CreateValidationErrorException(Resources.InternalError_InvalidAuthenticationMechanism);
		}

		private class FormInfo : IDisposable
		{
			public string UserName;

			public string Password;

			public string DestinationUserName;

			public SecureString DestinationPassword;

			public bool IsUriConnection;

			public Uri ConnectionUri;

			public bool AllowRedirection;

			public string ComputerName;

			public bool UseSsl;

			public int Port;

			public string ApplicationName;

			public string ConfigurationName;

			public AuthenticationMechanism AuthenticationType;

			public FormInfo()
			{
			}

			public void Dispose()
			{
				if (this.DestinationPassword != null)
				{
					//this.DestinationPassword.Dispose(); //TODO: Why is crashing?
					this.DestinationPassword = null;
					//GC.SuppressFinalize(this);
				}
			}
		}
	}
}