using System;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Collections.Generic;
using System.Security.Principal;
using System.ServiceModel;
using System.Management.Automation.Remoting.WSMan;

namespace Microsoft.WSMan.PowerShell
{
	internal class WSManPowerShellService
	{
		private readonly WSManServerSessionTransportManager sessionTransportManager = new WSManServerSessionTransportManager();
		private static Dictionary<Guid, ServerRemoteSession> _sessions =  new Dictionary<Guid, ServerRemoteSession>();
		private static readonly object _lock = new object();

		#region IWSManService implementation


		public void CloseSession (Guid sessionId)
		{
			ServerRemoteSession session;
			if (_sessions.TryGetValue (sessionId, out session)) {
				session.Close (new RemoteSessionStateMachineEventArgs (RemoteSessionEvent.Close));
				session = null;
				lock(_lock)
				{
					if (_sessions.ContainsKey (sessionId))
					{
						_sessions.Remove (sessionId);
					}
				}
			}
		}

		private void EnsureSessionSecurity (ServerRemoteSession session)
		{
			if (session.Identity.Name != System.Threading.Thread.CurrentPrincipal.Identity.Name) {
				throw new PSSecurityException("Access Denied to requested session");
			}
		}

		public Guid CreateSession ()
		{
			var username = System.Threading.Thread.CurrentPrincipal.Identity.Name;
			string connection =  OperationContext.Current.Host.Description.Endpoints[0].Address.Uri.ToString ();
			var identity = new PSIdentity ("", true, username, null);
			var principal = new PSPrincipal (identity, WindowsIdentity.GetCurrent ());
			var sender = new PSSenderInfo (principal, connection);
			var session = ServerRemoteSession.CreateServerRemoteSession (sender, null, sessionTransportManager);
			lock (_lock) {
				_sessions.Add (session.InstanceId, session);
			}
			return session.InstanceId;
		}

		public void CreateShell(Guid sessionId, Guid runspacePoolId, byte[] openContent)
		{
			ServerRemoteSession session;
			if (_sessions.TryGetValue (sessionId, out session)) {
				EnsureSessionSecurity (session);
				session.SessionDataStructureHandler.TransportManager.ProcessRawData (openContent, "stdin");
			}
		}

		public void RunCommand(Guid sessionId, string command, Guid runspacePoolId, Guid powerShellCmdId, byte[] arguments)
		{
			ServerRemoteSession session;
			if (_sessions.TryGetValue (sessionId, out session)) {
				EnsureSessionSecurity (session);
				session.SessionDataStructureHandler.TransportManager.ProcessRawData (arguments, "stdin");
			}
		}

		public byte[] ReceiveData (Guid sessionId, Guid powerShellCmdId)
		{
			ServerRemoteSession session;
			if (_sessions.TryGetValue (sessionId, out session)) {
				EnsureSessionSecurity (session);
				if (powerShellCmdId == Guid.Empty) {
					WSManServerSessionTransportManager transport = (WSManServerSessionTransportManager)session.SessionDataStructureHandler.TransportManager;
					return transport.GetBuffer ();
				} else {
					WSManServerTransportManager manager = (WSManServerTransportManager)session.SessionDataStructureHandler.TransportManager.GetCommandTransportManager (powerShellCmdId);
					return manager.GetBuffer();
				}
			}
			return new byte[0];
		}

		public void SendInput (Guid sessionId, Guid runspacePoolId, Guid powerShellId, string streamId, byte[] content)
		{
			ServerRemoteSession session;
			if (_sessions.TryGetValue (sessionId, out session)) {
				EnsureSessionSecurity (session);
				session.SessionDataStructureHandler.TransportManager.ProcessRawData (content, "stdin");
			}
		}

		public byte[] ConnectShell(Guid sessionId, Guid runspacePoolId, byte[] connectData)
		{
			byte[] response = null;
			ServerRemoteSession session;
			if (_sessions.TryGetValue (sessionId, out session)) {
				EnsureSessionSecurity (session);
				session.ExecuteConnect(connectData, out response);
			}
			return response;
		}

		public void CloseShell (Guid sessionId, Guid runspacePoolId)
		{
			ServerRemoteSession session;
			if (_sessions.TryGetValue (sessionId, out session)) {
				EnsureSessionSecurity (session);
				var driver = session.GetRunspacePoolDriver (runspacePoolId);
				if (driver != null)
				{
					driver.Close ();
				}
			}
		}

		public void CompleteCommand (Guid sessionId)
		{
			ServerRemoteSession session;
			if (_sessions.TryGetValue (sessionId, out session)) {
				session.SessionDataStructureHandler.TransportManager.CompleteProcessRawData ();
			}
		}

		public void CloseSession(Guid sessionId, string reason)
		{
			ServerRemoteSession session;
			if (_sessions.TryGetValue (sessionId, out session)) {
				EnsureSessionSecurity (session);
				var args = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close, string.IsNullOrEmpty (reason) ? null : new RemoteException(reason));
				session.Close (args);
			}
			_sessions.Remove (sessionId);
		}

		#endregion
	}
}

