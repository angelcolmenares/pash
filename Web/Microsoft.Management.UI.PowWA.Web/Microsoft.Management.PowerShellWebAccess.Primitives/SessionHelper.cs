using System;
using System.Web;
using System.Web.Security;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public static class SessionHelper
	{
		public static void DisablePageCaching(HttpResponse response)
		{
			response.Expires = -1;
			response.Cache.SetNoServerCaching();
			response.Cache.SetAllowResponseInBrowserHistory(false);
			response.CacheControl = "no-cache";
			response.Cache.SetNoStore();
		}

		public static string GetAuthenticatedUser()
		{
			return HttpContext.Current.User.Identity.Name;
		}

		public static string GetSessionId()
		{
			return HttpContext.Current.Session.SessionID;
		}

		public static string GetSourceIPAddressHttpXForwardedFor()
		{
			string item = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
			string unknownIpAddress = item;
			if (item == null)
			{
				unknownIpAddress = Resources.UnknownIpAddress;
			}
			return unknownIpAddress;
		}

		public static string GetSourceIPAddressRemoteAddr()
		{
			string item = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
			string unknownIpAddress = item;
			if (item == null)
			{
				unknownIpAddress = Resources.UnknownIpAddress;
			}
			return unknownIpAddress;
		}

		public static void RemoveSessionCookie(HttpResponse response)
		{
			HttpCookie httpCookie = new HttpCookie("ASP.NET_SessionId", "");
			DateTime now = DateTime.Now;
			httpCookie.Expires = now.AddDays(-1);
			response.Cookies.Add(httpCookie);
		}

		public static void TerminateSession(string sessionId, bool terminateAspNetSession, string reasonForTermination)
		{
			IEtwActivity etwActivity = PowwaEvents.EventCorrelator.StartActivity();
			using (etwActivity)
			{
				try
				{
					string str = PowwaSessionManager.Instance.TerminateSession(sessionId);
					PowwaEvents.PowwaEVENT_SESSION_END(str, reasonForTermination);
				}
				catch (ArgumentException argumentException)
				{

				}
				if (terminateAspNetSession)
				{
					FormsAuthentication.SignOut();
					HttpContext.Current.Session.Abandon();
					SessionHelper.RemoveSessionCookie(HttpContext.Current.Response);
				}
			}
		}
	}
}