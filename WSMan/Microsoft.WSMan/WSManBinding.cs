using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.ServiceModel;

namespace Microsoft.WSMan
{
	public class WSManBinding : CustomBinding
	{
		public WSManBinding ()
			: base(CreateSecurityBindingElement (), CreateTransportBindingElement())
		{

		}

		private static SecurityBindingElement CreateSecurityBindingElement()
		{
			SymmetricSecurityBindingElement sbe = SecurityBindingElement.CreateUserNameForSslBindingElement();
			//sbe.IncludeTimestamp = false;
			//sbe.LocalServiceSettings.DetectReplays = false;
			sbe.ProtectionTokenParameters = new X509SecurityTokenParameters ();
			// This "Never" is somehow mandatory (though I wonder why ...)
			sbe.ProtectionTokenParameters.InclusionMode = SecurityTokenInclusionMode.Never;
			sbe.MessageSecurityVersion = MessageSecurityVersion.Default;
			//sbe.RequireSignatureConfirmation = true;
			//sbe.KeyEntropyMode = SecurityKeyEntropyMode.ServerEntropy;

			sbe.SetKeyDerivation (false);
			sbe.MessageProtectionOrder = MessageProtectionOrder.SignBeforeEncrypt;
			return sbe;
		}
		
		public static TransportBindingElement CreateTransportBindingElement()
		{
			return new HttpTransportBindingElement();
		}
	}
}

