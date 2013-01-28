using System;
using System.Management.Automation.Remoting.WSMan;
using Microsoft.WSMan.Management;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel;
using Microsoft.WSMan.Transfer;

namespace Microsoft.WSMan.PowerShell
{
	public class WSManClientHandler : IWSManService
	{
		private Dictionary<Guid, ManagementClient> _services;

		public WSManClientHandler ()
		{

		}

		#region IWSManService implementation

		public void Initialize ()
		{
			_services = new Dictionary<Guid, ManagementClient>();
		}

		private ManagementClient GetService (Guid sessionId)
		{
			ManagementClient service;
			if (_services.TryGetValue (sessionId, out service)) {
				return service;
			}
			return null;
		}

		public Guid CreateSession (string connection, string username, string password, int authMechanism, int protocolVersion)
		{
			ChannelFactory<IWSTransferContract> cf = new ChannelFactory<IWSTransferContract>(new WSManBinding());
			cf.Credentials.UserName.UserName = username;
			cf.Credentials.UserName.Password = password;
			if (connection.IndexOf ("://") == -1) {
				//Default to http connection
				connection = "http://" + connection;
			}
			UriBuilder builder = new UriBuilder (connection);
			if (builder.Port == 80 || builder.Port == 443) {
				builder.Port = 5985;
			}

			ManagementClient client = new ManagementClient(builder.Uri, cf, MessageVersion.Soap12WSAddressing10);
			var sessionData = client.Get<XmlFragment<SessionData>>(PowerShellNamespaces.Namespace, "CreateSession", new Selector("ProtocolVersion", protocolVersion.ToString()));
			_services.Add (sessionData.Value.Id, client);
			return sessionData.Value.Id;
		}

		public void CreateShell (Guid sessionId, Guid runspacePoolId, byte[] openContent)
		{
			var service = GetService (sessionId);
			if (service == null) return;
			var shellData = service.Get<XmlFragment<CreateShellData>>(PowerShellNamespaces.Namespace, "CreateShell", new Selector("SessionId", sessionId.ToString ()), new Selector("RunspacePoolId", runspacePoolId.ToString ()), new Selector("Stream", openContent));
			var idleTimeOut = shellData.Value.IdleTimeOut;
		}

		public void RunCommand (Guid sessionId, string command, Guid runspacePoolId, Guid powerShellCmdId, byte[] arguments)
		{
			var service = GetService (sessionId);
			if (service == null) return;
			var data = service.Get<XmlFragment<CommandData>>(PowerShellNamespaces.Namespace, "RunCommand", new Selector("SessionId", sessionId.ToString ()), new Selector("RunspacePoolId", runspacePoolId.ToString ()), new Selector("CommandId", powerShellCmdId.ToString ()), new Selector("Stream", arguments));
			return;
		}

		public byte[] ReceiveData (Guid sessionId, Guid powerShellCmdId)
		{
			var service = GetService (sessionId);
			if (service == null) return new byte[0];
			var data = service.Get<XmlFragment<ReceiveResponseData>>(PowerShellNamespaces.Namespace, "ReceiveData", new Selector("SessionId", sessionId.ToString ()), new Selector("CommandId", powerShellCmdId.ToString ()));
			return Convert.FromBase64String (data.Value.Stream.Value);
		}

		public void SendInput (Guid sessionId, Guid runspacePoolId, Guid powerShellCmdId, string streamId, byte[] content)
		{
			var service = GetService (sessionId);
			if (service == null) return;
			var data = service.Get<XmlFragment<SendInputData>>(PowerShellNamespaces.Namespace, "SendInput", new Selector("SessionId", sessionId.ToString ()), new Selector("RunspacePoolId", runspacePoolId.ToString ()), new Selector("CommandId", powerShellCmdId.ToString ()), new Selector("StreamName", streamId), new Selector("Stream", content));
			
		}

		public byte[] ConnectShell (Guid sessionId, Guid runspacePoolId, byte[] connectData)
		{
			var service = GetService (sessionId);
			if (service == null) return new byte[0];
			var data = service.Get<XmlFragment<ConnectShellData>>(PowerShellNamespaces.Namespace, "ConnectShell", new Selector("SessionId", sessionId.ToString ()), new Selector("RunspacePoolId", runspacePoolId.ToString ()), new Selector("Stream", connectData));
			return Convert.FromBase64String (data.Value.Stream.Value);
		}

		public void CloseShell (Guid sessionId, Guid runspacePoolId)
		{
			var service = GetService (sessionId);
			if (service == null) return;
			var data = service.Get<XmlFragment<SessionData>>(PowerShellNamespaces.Namespace, "CloseShell", new Selector("sessionId", sessionId.ToString()), new Selector("RunspacePoolId", runspacePoolId.ToString ()));
		}

		public void CompleteCommand (Guid sessionId)
		{
			var service = GetService (sessionId);
			if (service == null) return;
			service.Get<XmlFragment<SessionData>>(PowerShellNamespaces.Namespace, "CompleteCommand", new Selector("sessionId", sessionId.ToString()));
		}

		public void CloseSession (Guid sessionId, string reason)
		{
			var service = GetService (sessionId);
			if (service == null) return;
			var data = service.Get<XmlFragment<SessionData>>(PowerShellNamespaces.Namespace, "CloseSession", new Selector("sessionId", sessionId.ToString()), new Selector("Reason", reason));
		}

		#endregion
	}
}

