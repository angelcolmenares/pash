using System;
using System.Management.Automation.Remoting;
using System.Collections.Generic;
using System.Security.Principal;

namespace System.Management.Automation.Remoting.WSMan
{
	public class WSManLocalService : IWSManService
	{
		private static Dictionary<Guid, ServerRemoteSession> _sessions =  new Dictionary<Guid, ServerRemoteSession>();
		private static readonly object _lock = new object();

		#region IWSManService implementation

		public void Initialize()
		{

		}

		public Guid CreateSession(string connection, string username, string password, int authMechanism, int protocolVersion)
		{
			Guid sessionId = Guid.NewGuid ();
			var identity = new PSIdentity ("", true, username, null);
			var principal = new PSPrincipal (identity, WindowsIdentity.GetCurrent ());
			var sender = new PSSenderInfo (principal, connection);
			var session = ServerRemoteSession.CreateServerRemoteSession (sender, null, new WSManServerSessionTransportManager());
			lock (_lock) {
				_sessions.Add (sessionId, session);
			}
			return sessionId;
		}

		public void CreateShell(Guid sessionId, Guid runspacePoolId, byte[] openContent)
		{
			ServerRemoteSession session;
			if (_sessions.TryGetValue (sessionId, out session)) {
				session.SessionDataStructureHandler.TransportManager.ProcessRawData (openContent, "stdin");
			}
		}

		public void RunCommand(Guid sessionId, string command, Guid runspacePoolId, Guid powerShellCmdId, byte[] arguments)
		{
			ServerRemoteSession session;
			if (_sessions.TryGetValue (sessionId, out session)) {
				if (command.StartsWith ("Enter-PSSession"))
				{
					session.SessionDataStructureHandler.TransportManager.ProcessRawDataAsync(arguments, "stdin");
				}
				else 
				{
					session.SessionDataStructureHandler.TransportManager.ProcessRawData(arguments, "stdin");
				}
			}
		}

		public void CompleteCommand (Guid sessionId)
		{
			ServerRemoteSession session;
			if (_sessions.TryGetValue (sessionId, out session)) {
				session.SessionDataStructureHandler.TransportManager.CompleteProcessRawData();
			}
		}

		public byte[] ReceiveData (Guid sessionId, Guid powerShellCmdId)
		{
			ServerRemoteSession session;
			if (_sessions.TryGetValue (sessionId, out session)) {
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
				session.SessionDataStructureHandler.TransportManager.ProcessRawData (content, "stdin");
			}
		}

		public byte[] ConnectShell(Guid sessionId, Guid runspacePoolId, byte[] connectData)
		{
			byte[] response = null;
			ServerRemoteSession session;
			if (_sessions.TryGetValue (sessionId, out session)) {
				session.ExecuteConnect(connectData, out response);
			}
			return response;
		}

		public void CloseShell (Guid sessionId, Guid runspacePoolId)
		{
			ServerRemoteSession session;
			if (_sessions.TryGetValue (sessionId, out session)) {
				var driver = session.GetRunspacePoolDriver (runspacePoolId);
				if (driver != null)
				{
					driver.Close ();
				}
			}
		}

		public void CloseSession(Guid sessionId, string reason)
		{
			ServerRemoteSession session;
			if (_sessions.TryGetValue (sessionId, out session)) {
				var args = new RemoteSessionStateMachineEventArgs(RemoteSessionEvent.Close, string.IsNullOrEmpty (reason) ? null : new RemoteException(reason));
				session.Close (args);
			}
			_sessions.Remove (sessionId);
		}

		#endregion
	}
}

