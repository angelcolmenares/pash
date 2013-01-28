using System;
using System.Configuration;
using System.Globalization;
using System.Web;
using System.Web.Configuration;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class Global : HttpApplication
	{
		public Global()
		{
			var root = ConfigurationManager.AppSettings["RootPath"];
			AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs args) => {
				var assemblyName = new System.Reflection.AssemblyName(args.Name);
				string path = System.IO.Path.Combine (root, assemblyName.Name + ".dll");
				if (System.IO.File.Exists (path))
				{
					return System.Reflection.Assembly.LoadFile (path);
				}
				return null;
			};
		}

		private void Application_BeginRequest(object sender, EventArgs e)
		{
			base.Response.AddHeader("X-FRAME-OPTIONS", "DENY");
		}

		private void Application_Error(object sender, EventArgs e)
		{
			HttpRequestValidationException lastError = base.Server.GetLastError() as HttpRequestValidationException;
			if (lastError != null)
			{
				PowwaEvents.PowwaEVENT_MALICIOUS_DATA(SessionHelper.GetSourceIPAddressRemoteAddr(), SessionHelper.GetSourceIPAddressHttpXForwardedFor(), lastError.Message);
			}
		}

		private void Application_Start(object sender, EventArgs e)
		{
			ScriptingJsonSerializationSection section;
			string item = ConfigurationManager.AppSettings["maxSessionsAllowedPerUser"];
			if (item != null)
			{
				int num = 0;
				if (!int.TryParse(item, NumberStyles.Integer, CultureInfo.InvariantCulture, out num))
				{
					PowwaEvents.PowwaEVENT_INVALID_APPLICATION_SETTING("maxSessionsAllowedPerUser", item);
				}
				else
				{
					PowwaAuthorizationManager.Instance.UserSessionsLimit = num;
				}
			}
			try
			{
				Configuration configuration = WebConfigurationManager.OpenWebConfiguration("/");
				section = (ScriptingJsonSerializationSection)configuration.GetSection("system.web.extensions/scripting/webServices/jsonSerialization");
			}
			catch
			{
				section = new ScriptingJsonSerializationSection();
			}
			PowwaSessionManager.Instance.JsonSerializer.MaxJsonLength = section.MaxJsonLength;
		}

		private void Session_End(object sender, EventArgs e)
		{
			try
			{
				SessionHelper.TerminateSession(base.Session.SessionID, false, Resources.EventLog_ASPNET_SessionTimeout);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				PowwaEvents.PowwaEVENT_TERMINATE_SESSION_ERROR(SessionHelper.GetAuthenticatedUser(), exception.Message);
			}
		}
	}
}