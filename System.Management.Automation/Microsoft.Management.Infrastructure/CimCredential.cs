using Microsoft.Management.Infrastructure.Native;
using System;
using System.Security;

namespace Microsoft.Management.Infrastructure.Options
{
	public class CimCredential
	{
		private NativeCimCredentialHandle credential;

		public CimCredential(string authenticationMechanism, string certificateThumbprint)
		{
			if (authenticationMechanism != null)
			{
				NativeCimCredential.CreateCimCredential(authenticationMechanism, certificateThumbprint, out this.credential);
				return;
			}
			else
			{
				throw new ArgumentNullException("authenticationMechanism");
			}
		}

		public CimCredential(string authenticationMechanism, string domain, string userName, SecureString password)
		{
			if (authenticationMechanism != null)
			{
				if (userName != null)
				{
					NativeCimCredential.CreateCimCredential(authenticationMechanism, domain, userName, password, out this.credential);
					return;
				}
				else
				{
					throw new ArgumentNullException("userName");
				}
			}
			else
			{
				throw new ArgumentNullException("authenticationMechanism");
			}
		}

		public CimCredential(string authenticationMechanism)
		{
			if (authenticationMechanism != null)
			{
				NativeCimCredential.CreateCimCredential(authenticationMechanism, out this.credential);
				return;
			}
			else
			{
				throw new ArgumentNullException("authenticationMechanism");
			}
		}

		public CimCredential(CertificateAuthenticationMechanism authenticationMechanism, string certificateThumbprint)
		{
			string authTypeIssuerCert = null;
			if (authenticationMechanism != CertificateAuthenticationMechanism.Default)
			{
				if (authenticationMechanism != CertificateAuthenticationMechanism.ClientCertificate)
				{
					if (authenticationMechanism != CertificateAuthenticationMechanism.IssuerCertificate)
					{
						throw new ArgumentOutOfRangeException("authenticationMechanism");
					}
					else
					{
						authTypeIssuerCert = AuthType.AuthTypeIssuerCert;
					}
				}
				else
				{
					authTypeIssuerCert = AuthType.AuthTypeClientCerts;
				}
			}
			else
			{
				authTypeIssuerCert = AuthType.AuthTypeClientCerts;
			}
			NativeCimCredential.CreateCimCredential(authTypeIssuerCert, certificateThumbprint, out this.credential);
		}

		public CimCredential(PasswordAuthenticationMechanism authenticationMechanism, string domain, string userName, SecureString password)
		{
			if (userName != null)
			{
				string authTypeCredSSP = null;
				if (authenticationMechanism != PasswordAuthenticationMechanism.Default)
				{
					if (authenticationMechanism != PasswordAuthenticationMechanism.Basic)
					{
						if (authenticationMechanism != PasswordAuthenticationMechanism.Digest)
						{
							if (authenticationMechanism != PasswordAuthenticationMechanism.Negotiate)
							{
								if (authenticationMechanism != PasswordAuthenticationMechanism.Kerberos)
								{
									if (authenticationMechanism != PasswordAuthenticationMechanism.NtlmDomain)
									{
										if (authenticationMechanism != PasswordAuthenticationMechanism.CredSsp)
										{
											throw new ArgumentOutOfRangeException("authenticationMechanism");
										}
										else
										{
											authTypeCredSSP = AuthType.AuthTypeCredSSP;
										}
									}
									else
									{
										authTypeCredSSP = AuthType.AuthTypeNTLM;
									}
								}
								else
								{
									authTypeCredSSP = AuthType.AuthTypeKerberos;
								}
							}
							else
							{
								authTypeCredSSP = AuthType.AuthTypeNegoWithCredentials;
							}
						}
						else
						{
							authTypeCredSSP = AuthType.AuthTypeDigest;
						}
					}
					else
					{
						authTypeCredSSP = AuthType.AuthTypeBasic;
					}
				}
				else
				{
					authTypeCredSSP = AuthType.AuthTypeDefault;
				}
				NativeCimCredential.CreateCimCredential(authTypeCredSSP, domain, userName, password, out this.credential);
				return;
			}
			else
			{
				throw new ArgumentNullException("userName");
			}
		}

		public CimCredential(ImpersonatedAuthenticationMechanism authenticationMechanism)
		{
			string authTypeNTLM = null;
			if (authenticationMechanism != ImpersonatedAuthenticationMechanism.None)
			{
				if (authenticationMechanism != ImpersonatedAuthenticationMechanism.Negotiate)
				{
					if (authenticationMechanism != ImpersonatedAuthenticationMechanism.Kerberos)
					{
						if (authenticationMechanism != ImpersonatedAuthenticationMechanism.NtlmDomain)
						{
							throw new ArgumentOutOfRangeException("authenticationMechanism");
						}
						else
						{
							authTypeNTLM = AuthType.AuthTypeNTLM;
						}
					}
					else
					{
						authTypeNTLM = AuthType.AuthTypeKerberos;
					}
				}
				else
				{
					authTypeNTLM = AuthType.AuthTypeNegoNoCredentials;
				}
			}
			else
			{
				authTypeNTLM = AuthType.AuthTypeNone;
			}
			NativeCimCredential.CreateCimCredential(authTypeNTLM, out this.credential);
		}

		internal NativeCimCredentialHandle GetCredential()
		{
			return this.credential;
		}
	}
}