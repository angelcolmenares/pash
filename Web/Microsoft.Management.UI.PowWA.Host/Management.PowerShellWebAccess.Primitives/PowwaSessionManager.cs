using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Web.Script.Serialization;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public sealed class PowwaSessionManager
	{
		private readonly static PowwaSessionManager instance;

		private readonly Dictionary<string, PowwaSession> sessions;

		private readonly object sessionsLock;

		public static PowwaSessionManager Instance
		{
			get
			{
				return PowwaSessionManager.instance;
			}
		}

		public JavaScriptSerializer JsonSerializer
		{
			get;
			private set;
		}

		static PowwaSessionManager()
		{
			PowwaSessionManager.instance = new PowwaSessionManager();
		}

		private PowwaSessionManager()
		{
			this.sessions = new Dictionary<string, PowwaSession>();
			this.sessionsLock = new object();
			this.JsonSerializer = new JavaScriptSerializer();
		}

		[CLSCompliant(false)]
		public PowwaSession CreateSession(string sessionId, RunspaceConnectionInfo connectionInfo, ClientInfo clientInfo, string authenticatedUserName)
		{
			bool flag = false;
			PowwaSession powwaSession;
			PowwaSession stringSid = new PowwaSession(sessionId, authenticatedUserName, connectionInfo, clientInfo);
			string str = null;
			stringSid.AuthenticatedUserSid = PowwaAuthorizationManager.Instance.activeDirectoryHelper.ConvertAccountNameToStringSid(authenticatedUserName, out flag, out str);
			lock (this.sessionsLock)
			{
				bool userActiveSessions = this.GetUserActiveSessions(stringSid.AuthenticatedUserSid) < PowwaAuthorizationManager.Instance.UserSessionsLimit;
				if (userActiveSessions)
				{
					stringSid.Name = this.GetSessionName(stringSid);
					try
					{
						this.sessions.Add(sessionId, stringSid);
					}
					catch (ArgumentException argumentException)
					{
						PowwaEvents.PowwaEVENT_DEBUG_LOG0("CreateSession: Attempt to create a session that already exists");
						throw new ArgumentException("There is already a PowWA session with the given ID", "sessionId");
					}
					powwaSession = stringSid;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = authenticatedUserName;
					throw new Exception(string.Format(CultureInfo.CurrentCulture, Resources.UserActiveSessionLimitReached, objArray));
				}
			}
			return powwaSession;
		}

		public PowwaSession GetSession(string sessionId)
		{
			PowwaSession item = null;
			lock (this.sessionsLock)
			{
				try
				{
					if (this.sessions.ContainsKey (sessionId))
					{
						item = this.sessions[sessionId];
					}
				}
				catch (KeyNotFoundException keyNotFoundException)
				{
					throw new ArgumentException("Invalid sessionId", "sessionId");
				}
			}
			return item;
		}

		private string GetSessionName(PowwaSession session)
		{
			string str;
			lock (this.sessionsLock)
			{
				DateTime now = DateTime.Now;
				object[] userName = new object[7];
				userName[0] = session.UserName;
				userName[1] = now.Year % 100;
				userName[2] = now.Month;
				userName[3] = now.Day;
				userName[4] = now.Hour;
				userName[5] = now.Minute;
				userName[6] = now.Second;
				string str1 = string.Format(CultureInfo.InvariantCulture, "{0}.{1:00}{2:00}{3:00}.{4:00}{5:00}{6:00}", userName);
				string str2 = str1;
				long num = (long)2;
				while (true)
				{
					if (this.sessions.Values.All<PowwaSession>((PowwaSession v) => string.Compare(v.Name, str2, StringComparison.OrdinalIgnoreCase) != 0))
					{
						break;
					}
					object[] objArray = new object[2];
					objArray[0] = str1;
					objArray[1] = num;
					str2 = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", objArray);
					num = num + (long)1;
				}
				str = str2;
			}
			return str;
		}

		internal int GetUserActiveSessions(string userSid)
		{
			int num;
			lock (this.sessionsLock)
			{
				int num1 = 0;
				foreach (PowwaSession value in this.sessions.Values)
				{
					if (!value.AuthenticatedUserSid.Equals(userSid))
					{
						continue;
					}
					num1++;
				}
				num = num1;
			}
			return num;
		}

		public bool SessionExists(string sessionId)
		{
			bool flag;
			lock (this.sessionsLock)
			{
				flag = this.sessions.ContainsKey(sessionId);
			}
			return flag;
		}

		public string TerminateSession(string sessionId)
		{
			string str = null;
			lock (this.sessionsLock)
			{
				PowwaSession session = this.GetSession(sessionId);
				if (session != null) {
					string name = session.Name;
					this.sessions.Remove(sessionId);
					session.Close();
					str = name;
				}
			}
			return str;
		}
	}
}