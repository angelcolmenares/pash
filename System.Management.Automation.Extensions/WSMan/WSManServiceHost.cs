using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security.Tokens;

namespace System.Management.Automation.Extensions.Remoting.WSMan
{
	public class WSManServiceHost : IDisposable
	{
		private ServiceHost _host;

		public WSManServiceHost ()
		{ 
			try 
			{ 
				//localhost:5985/wsman
				SymmetricSecurityBindingElement sbe = SecurityBindingElement.CreateUserNameForSslBindingElement();
				//sbe.IncludeTimestamp = false;
				//sbe.LocalServiceSettings.DetectReplays = false;
				sbe.ProtectionTokenParameters = new X509SecurityTokenParameters ();
				// This "Never" is somehow mandatory (though I wonder why ...)
				sbe.ProtectionTokenParameters.InclusionMode = SecurityTokenInclusionMode.Never;
				
				sbe.SetKeyDerivation (false);
				sbe.MessageProtectionOrder = MessageProtectionOrder.SignBeforeEncrypt;
				_host = new ServiceHost (typeof (WSManHttpService));
				HttpTransportBindingElement hbe = new HttpTransportBindingElement ();
				CustomBinding binding = new CustomBinding (sbe, hbe);
				binding.ReceiveTimeout = TimeSpan.FromSeconds (5);
				_host.AddServiceEndpoint (typeof(IWSManHttpService),
				                         binding, new Uri ("http://localhost:5985/wsman"));
				
				ServiceCredentials cred = new ServiceCredentials ();
				cred.ServiceCertificate.Certificate = new X509Certificate2 ("powershell.pfx", "mono");
				cred.ClientCertificate.Authentication.CertificateValidationMode =
					X509CertificateValidationMode.None;
				cred.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom;
				cred.UserNameAuthentication.CustomUserNamePasswordValidator = new WSManUserNamePasswordValidator();
				_host.Description.Behaviors.Add (cred);
				_host.Description.Behaviors.Find<ServiceDebugBehavior> ()
					.IncludeExceptionDetailInFaults = false;
				ServiceMetadataBehavior smb = new ServiceMetadataBehavior ();
				smb.HttpGetEnabled = true;
				smb.HttpGetUrl = new Uri ("http://localhost:5985/wsman/wsdl");
				_host.Description.Behaviors.Add (smb);



			} catch (Exception ex) {
				Console.WriteLine ("Could not create service...");
				Console.WriteLine (ex.Message);
			}
		}

		public void Open()
		{
			_host.Open (TimeSpan.FromSeconds (60));
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

