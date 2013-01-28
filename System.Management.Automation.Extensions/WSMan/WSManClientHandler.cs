using System;
using System.Management.Automation.Remoting.WSMan;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Security.Cryptography.X509Certificates;

namespace System.Management.Automation.Extensions.Remoting.WSMan
{
	public class WSManClientHandler : IWSManService
	{
		private Dictionary<Guid, WSManHttpServiceProxy> _services;

		public WSManClientHandler ()
		{

		}


		private Guid CreateService (string url, string username, string password)
		{
			Guid sessionId;
			SymmetricSecurityBindingElement sbe = SecurityBindingElement.CreateUserNameForCertificateBindingElement (); 
			//sbe.IncludeTimestamp = false;
			//sbe.LocalClientSettings.DetectReplays = false;
			
			sbe.ProtectionTokenParameters = new X509SecurityTokenParameters ();
			sbe.ProtectionTokenParameters.InclusionMode = SecurityTokenInclusionMode.Never;
			sbe.SetKeyDerivation (false);
			sbe.MessageProtectionOrder = MessageProtectionOrder.SignBeforeEncrypt;
			HttpTransportBindingElement hbe = new HttpTransportBindingElement ();

			CustomBinding binding = new CustomBinding (sbe, hbe);
			
			X509Certificate2 cert = new X509Certificate2 ("powershell.pfx", "mono");
			if (url.IndexOf ("://") == -1) {
				//Default to http connection
				url = "http://" + url;
			}
			UriBuilder builder = new UriBuilder (url);
			if (builder.Port == 80 || builder.Port == 443) {
				builder.Port = 5985;
			}
			WSManHttpServiceProxy proxy = new WSManHttpServiceProxy(binding,
			                               new EndpointAddress (builder.Uri, new X509CertificateEndpointIdentity (cert)));

			proxy.ClientCredentials.UserName.UserName = username;
			proxy.ClientCredentials.UserName.Password = password;
			proxy.Open ();
			sessionId = proxy.CreateSession ();
			proxy.SessionId = sessionId;

			_services.Add (sessionId, proxy);
			return sessionId;
		}

		private WSManHttpServiceProxy GetService (Guid sessionId)
		{
			WSManHttpServiceProxy service;
			if (_services.TryGetValue (sessionId, out service)) {
				return service;
			}
			return null;
		}

		#region IWSManService implementation

		
		public void Initialize ()
		{
			_services = new Dictionary<Guid, WSManHttpServiceProxy>();
		}

		public Guid CreateSession (string connection, string username, string password, int authMechanism, int protocolVersion)
		{
			return CreateService (connection, username, password);
		}

		public void CreateShell (Guid sessionId, Guid shellId, byte[] openContent)
		{
			GetService (sessionId).CreateShell (shellId, openContent);
		}

		public void RunCommand (Guid sessionId, string command, Guid runspacePoolId, Guid powerShellCmdId, byte[] arguments)
		{
			GetService (sessionId).RunCommand (command, runspacePoolId, powerShellCmdId, arguments);
		}

		public byte[] ReceiveData (Guid sessionId, Guid powerShellCmdId)
		{
			return GetService (sessionId).ReceiveData (powerShellCmdId);
		}

		public void SendInput (Guid sessionId, Guid shellId, Guid guid, string streamId, byte[] content)
		{
			GetService (sessionId).SendInput (shellId, guid, streamId, content);
		}

		public byte[] ConnectShell (Guid sessionId, Guid runspacePoolId, byte[] connectData)
		{
			return GetService (sessionId).ConnectShell (runspacePoolId, connectData);
		}

		public void CloseShell (Guid sessionId, Guid runspacePoolId)
		{
			GetService (sessionId).CloseShell (runspacePoolId);
		}

		public void CloseSession (Guid sessionId, string reason)
		{
			GetService (sessionId).CloseSession (reason);
		}

		#endregion
	}
}

