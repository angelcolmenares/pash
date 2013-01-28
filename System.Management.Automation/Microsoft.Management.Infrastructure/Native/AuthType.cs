using System;

namespace Microsoft.Management.Infrastructure.Native
{
	internal class AuthType
	{
		internal static string AuthTypeDefault;

		internal static string AuthTypeNone;

		internal static string AuthTypeDigest;

		internal static string AuthTypeNegoWithCredentials;

		internal static string AuthTypeNegoNoCredentials;

		internal static string AuthTypeBasic;

		internal static string AuthTypeKerberos;

		internal static string AuthTypeClientCerts;

		internal static string AuthTypeNTLM;

		internal static string AuthTypeCredSSP;

		internal static string AuthTypeIssuerCert;

		static AuthType()
		{
			AuthType.AuthTypeDefault = "Default";
			AuthType.AuthTypeNone = "None";
			AuthType.AuthTypeDigest = "Digest";
			AuthType.AuthTypeNegoWithCredentials = "NegoWithCreds";
			AuthType.AuthTypeNegoNoCredentials = "NegoNoCreds";
			AuthType.AuthTypeBasic = "Basic";
			AuthType.AuthTypeKerberos = "Kerberos";
			AuthType.AuthTypeClientCerts = "ClientCerts";
			AuthType.AuthTypeNTLM = "Ntlmdomain";
			AuthType.AuthTypeCredSSP = "CredSSP";
			AuthType.AuthTypeIssuerCert = "IssuerCert";
		}

		private AuthType()
		{
		}
	}
}