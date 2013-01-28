using System;
using System.ServiceModel;
using Microsoft.WSMan.Management;
using Microsoft.WSMan.Eventing;
using Microsoft.WSMan.Enumeration;
using Microsoft.WSMan.Transfer;
using System.ServiceModel.Description;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;

namespace Microsoft.WSMan
{
	public class WSManServiceHost : IDisposable
	{
		private readonly ServiceHost _host;

		public WSManServiceHost ()
		{
			var uri = new Uri ("http://localhost:5985/wsman");
			_host = new ServiceHost(ManagementServer.Create ());
			var binding = new WSManBinding();
			_host.AddServiceEndpoint (typeof(IWSTransferContract),
			                          binding, uri);
			_host.AddServiceEndpoint (typeof(IWSEnumerationContract),
			                          binding, uri);
			_host.AddServiceEndpoint (typeof(IWSEventingContract),
			                          binding, uri);

			
			ServiceCredentials cred = new ServiceCredentials ();
			cred.ServiceCertificate.Certificate = new X509Certificate2 ("powershell.pfx", "mono");
			cred.ClientCertificate.Authentication.CertificateValidationMode =
				X509CertificateValidationMode.None;
			cred.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom;
			cred.UserNameAuthentication.CustomUserNamePasswordValidator = new WSManUserNamePasswordValidator();
			_host.Description.Behaviors.Add (cred);
			var serviceBehavior = _host.Description.Behaviors.Find<ServiceBehaviorAttribute> ();
			serviceBehavior.ConcurrencyMode = ConcurrencyMode.Multiple;
			serviceBehavior.InstanceContextMode = InstanceContextMode.Single;
			serviceBehavior.IncludeExceptionDetailInFaults = true;
			_host.Description.Behaviors.Find<ServiceDebugBehavior> ()
				.IncludeExceptionDetailInFaults = true;
			ServiceMetadataBehavior smb = new ServiceMetadataBehavior ();
			smb.HttpGetEnabled = true;
			smb.HttpGetUrl = new Uri (uri.ToString () + "/wsdl");
			_host.Description.Behaviors.Add (smb);
			_host.Description.Behaviors.Add (new FilterMapExtensionServiceBehaviorAttribute());
			//_host.Description.Behaviors.Add (new AddressingVersionExtensionServiceBehaviorAttribute());
		}
		
		public void Open()
		{
			_host.Open ();
		}
		
		public void Close()
		{
			_host.Close ();
		}
		
		#region IDisposable implementation
		
		public void Dispose ()
		{
			if (_host.State == CommunicationState.Opened || _host.State == CommunicationState.Opening) {
				_host.Close ();
			}
		}
		
#endregion
	}
}

