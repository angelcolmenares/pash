using System;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.Security;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class Console : Page
	{
		private const int SessionTimeoutWarning = 5;

		protected HtmlForm mainForm;

		protected ScriptManager ScriptManager;

		protected HiddenField sessionKey;

		public Console()
		{
		}

		[WebMethod]
		public static PowwaReturnValue<object> CancelCommand(string sessionKey)
		{
			PowwaReturnValue<object> powwaReturnValue;
			IEtwActivity etwActivity = PowwaEvents.EventCorrelator.StartActivity();
			using (etwActivity)
			{
				string str = sessionKey;
				powwaReturnValue = Console.HttpEndpointWrapper<object>(str, (PowwaSession session) => {
					session.CancelCommand();
					return null;
				}
				);
			}
			return powwaReturnValue;
		}

		[WebMethod]
		public static PowwaReturnValue<ClientMessage[]> ExecuteCommand(string sessionKey, string command)
		{
			return Console.HttpEndpointWrapper<ClientMessage[]>(sessionKey, (PowwaSession session) => session.ExecuteCommand(command));
		}

		[WebMethod]
		public static PowwaReturnValue<ClientConfiguration> GetClientConfiguration(string sessionKey)
		{
			PowwaReturnValue<ClientConfiguration> powwaReturnValue;
			IEtwActivity etwActivity = PowwaEvents.EventCorrelator.StartActivity();
			using (etwActivity)
			{
				string str = sessionKey;
				powwaReturnValue = Console.HttpEndpointWrapper<ClientConfiguration>(str, (PowwaSession session) => {
					ClientConfiguration clientConfiguration = session.GetClientConfiguration();
					if (HttpContext.Current.Session.Timeout > 5)
					{
						clientConfiguration.SessionTimeout = HttpContext.Current.Session.Timeout;
						clientConfiguration.SessionTimeoutWarning = 5;
					}
					else
					{
						clientConfiguration.SessionTimeout = 0;
						clientConfiguration.SessionTimeoutWarning = 0;
					}
					return clientConfiguration;
				}
				);
			}
			return powwaReturnValue;
		}

		[WebMethod]
		public static PowwaReturnValue<ClientMessage[]> GetClientMessages(string sessionKey)
		{
			string str = sessionKey;
			return Console.HttpEndpointWrapper<ClientMessage[]>(str, (PowwaSession session) => session.GetClientMessages());
		}

		[WebMethod]
		public static PowwaReturnValue<PowwaSessionStatusInfo> GetSessionStatus(string sessionKey)
		{
			PowwaReturnValue<PowwaSessionStatusInfo> powwaReturnValue;
			IEtwActivity etwActivity = PowwaEvents.EventCorrelator.StartActivity();
			using (etwActivity)
			{
				string str = sessionKey;
				powwaReturnValue = Console.HttpEndpointWrapper<PowwaSessionStatusInfo>(str, (PowwaSession session) => session.GetSessionStatus());
			}
			return powwaReturnValue;
		}

		[WebMethod]
		public static PowwaReturnValue<string[]> GetTabCompletion(string sessionKey, string commandLine)
		{
			return Console.HttpEndpointWrapper<string[]>(sessionKey, (PowwaSession session) => session.GetTabCompletion(commandLine));
		}

		private static PowwaReturnValue<T> HttpEndpointWrapper<T>(string sessionKey, Func<PowwaSession, T> function)
		{
			PowwaReturnValue<T> powwaReturnValue;
			PowwaSession session = null;
			try
			{
				try
				{
					session = PowwaSessionManager.Instance.GetSession(SessionHelper.GetSessionId());
				}
				catch (ArgumentException argumentException)
				{
					powwaReturnValue = PowwaReturnValue<T>.CreateError(PowwaException.CreateInvalidSessionException());
					return powwaReturnValue;
				}
				if (string.Compare(sessionKey, session.SessionKey, StringComparison.OrdinalIgnoreCase) == 0)
				{
					if (string.Compare(SessionHelper.GetAuthenticatedUser(), session.UserName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						powwaReturnValue = PowwaReturnValue<T>.CreateSuccess(function(session));
					}
					else
					{
						PowwaEvents.PowwaEVENT_INVALID_SESSION_USER(session.Name, SessionHelper.GetAuthenticatedUser(), session.UserName, SessionHelper.GetSourceIPAddressRemoteAddr(), SessionHelper.GetSourceIPAddressHttpXForwardedFor());
						powwaReturnValue = PowwaReturnValue<T>.CreateError(PowwaException.CreateInvalidSessionException());
					}
				}
				else
				{
					PowwaEvents.PowwaEVENT_INVALID_SESSION_KEY(session.Name, SessionHelper.GetAuthenticatedUser(), SessionHelper.GetSourceIPAddressRemoteAddr(), SessionHelper.GetSourceIPAddressHttpXForwardedFor());
					powwaReturnValue = PowwaReturnValue<T>.CreateError(PowwaException.CreateInvalidSessionException());
				}
			}
			catch (PowwaException powwaException1)
			{
				PowwaException powwaException = powwaException1;
				powwaReturnValue = PowwaReturnValue<T>.CreateError(powwaException);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				PowwaEvents.PowwaEVENT_GENERIC_FAILURE(session.Name, exception.Message);
				powwaReturnValue = PowwaReturnValue<T>.CreateGenericError(exception);
			}
			return powwaReturnValue;
		}

		protected void Page_Init(object sender, EventArgs e)
		{
			base.ViewStateUserKey = this.Session.SessionID;
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			SessionHelper.DisablePageCaching(base.Response);
			PowwaSession session = null;
			try
			{
				session = PowwaSessionManager.Instance.GetSession(SessionHelper.GetSessionId());
			}
			catch (ArgumentException argumentException)
			{
				base.Response.Redirect("./timeout.aspx");
			}
			if (!session.InUse)
			{
				session.InUse = true;
			}
			else
			{
				base.Response.Redirect("./session.aspx");
			}
			if (FormsAuthentication.RequireSSL && !base.Request.IsSecureConnection)
			{
				base.Response.Redirect("./error.aspx");
			}
			this.sessionKey.Value = session.SessionKey;
		}

		[WebMethod]
		public static PowwaReturnValue<object> ResetSessionTimeout(string sessionKey)
		{
			PowwaReturnValue<object> powwaReturnValue;
			IEtwActivity etwActivity = PowwaEvents.EventCorrelator.StartActivity();
			using (etwActivity)
			{
				PowwaEvents.PowwaEVENT_DEBUG_LOG0("ResetSessionTimeout");
				string str = sessionKey;
				powwaReturnValue = Console.HttpEndpointWrapper<object>(str, (PowwaSession session) => null);
			}
			return powwaReturnValue;
		}

		[WebMethod]
		public static PowwaReturnValue<ClientMessage[]> SetPromptForChoiceReply(string sessionKey, int reply)
		{
			PowwaReturnValue<ClientMessage[]> powwaReturnValue;
			Func<PowwaSession, ClientMessage[]> func = null;
			IEtwActivity etwActivity = PowwaEvents.EventCorrelator.StartActivity();
			using (etwActivity)
			{
				string str = sessionKey;
				if (func == null)
				{
					func = (PowwaSession session) => session.SetPromptForChoiceReply(reply);
				}
				powwaReturnValue = Console.HttpEndpointWrapper<ClientMessage[]>(str, func);
			}
			return powwaReturnValue;
		}

		[WebMethod]
		public static PowwaReturnValue<ClientMessage[]> SetPromptForCredentialReply(string sessionKey, string userName, char[] password)
		{
			PowwaReturnValue<ClientMessage[]> powwaReturnValue;
			Func<PowwaSession, ClientMessage[]> func = null;
			IEtwActivity etwActivity = PowwaEvents.EventCorrelator.StartActivity();
			using (etwActivity)
			{
				string str = sessionKey;
				if (func == null)
				{
					func = (PowwaSession session) => session.SetPromptForCredentialReply(userName, password);
				}
				powwaReturnValue = Console.HttpEndpointWrapper<ClientMessage[]>(str, func);
			}
			return powwaReturnValue;
		}

		[WebMethod]
		public static PowwaReturnValue<ClientMessage[]> SetPromptReply(string sessionKey, object[] reply)
		{
			PowwaReturnValue<ClientMessage[]> powwaReturnValue;
			Func<PowwaSession, ClientMessage[]> func = null;
			IEtwActivity etwActivity = PowwaEvents.EventCorrelator.StartActivity();
			using (etwActivity)
			{
				string str = sessionKey;
				if (func == null)
				{
					func = (PowwaSession session) => session.SetPromptReply(reply);
				}
				powwaReturnValue = Console.HttpEndpointWrapper<ClientMessage[]>(str, func);
			}
			return powwaReturnValue;
		}

		[WebMethod]
		public static PowwaReturnValue<ClientMessage[]> SetReadLineAsSecureStringReply(string sessionKey, char[] reply)
		{
			PowwaReturnValue<ClientMessage[]> powwaReturnValue;
			Func<PowwaSession, ClientMessage[]> func = null;
			IEtwActivity etwActivity = PowwaEvents.EventCorrelator.StartActivity();
			using (etwActivity)
			{
				string str = sessionKey;
				if (func == null)
				{
					func = (PowwaSession session) => session.SetReadLineAsSecureStringReply(reply);
				}
				powwaReturnValue = Console.HttpEndpointWrapper<ClientMessage[]>(str, func);
			}
			return powwaReturnValue;
		}

		[WebMethod]
		public static PowwaReturnValue<ClientMessage[]> SetReadLineReply(string sessionKey, string reply)
		{
			PowwaReturnValue<ClientMessage[]> powwaReturnValue;
			Func<PowwaSession, ClientMessage[]> func = null;
			IEtwActivity etwActivity = PowwaEvents.EventCorrelator.StartActivity();
			using (etwActivity)
			{
				string str = sessionKey;
				if (func == null)
				{
					func = (PowwaSession session) => session.SetReadLineReply(reply);
				}
				powwaReturnValue = Console.HttpEndpointWrapper<ClientMessage[]>(str, func);
			}
			return powwaReturnValue;
		}

		[WebMethod]
		public static PowwaReturnValue<object> TerminateSession(string sessionKey)
		{
			PowwaReturnValue<object> powwaReturnValue;
			IEtwActivity etwActivity = PowwaEvents.EventCorrelator.StartActivity();
			using (etwActivity)
			{
				string str = sessionKey;
				powwaReturnValue = Console.HttpEndpointWrapper<object>(str, (PowwaSession session) => {
					SessionHelper.TerminateSession(session.Id, true, Resources.EventLog_BrowserInitiated);
					return null;
				}
				);
			}
			return powwaReturnValue;
		}
	}
}