using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Activities
{
	public abstract class PSGeneratedCIMActivity : PSActivity, IImplementsConnectionRetry
	{
		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<CimSession[]> CimSession
		{
			get;
			set;
		}

		protected abstract string ModuleDefinition
		{
			get;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<AuthenticationMechanism?> PSAuthentication
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<string> PSCertificateThumbprint
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<string[]> PSComputerName
		{
			get;
			set;
		}

		[BehaviorCategory]
		[DefaultValue(null)]
		public InArgument<uint?> PSConnectionRetryCount
		{
			get;
			set;
		}

		[BehaviorCategory]
		[DefaultValue(null)]
		public InArgument<uint?> PSConnectionRetryIntervalSec
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<PSCredential> PSCredential
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<uint?> PSPort
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<PSSessionOption> PSSessionOption
		{
			get;
			set;
		}

		[ConnectivityCategory]
		[DefaultValue(null)]
		public InArgument<bool?> PSUseSsl
		{
			get;
			set;
		}

		[BehaviorCategory]
		[DefaultValue(null)]
		public InArgument<Uri> ResourceUri
		{
			get;
			set;
		}

		protected PSGeneratedCIMActivity()
		{
		}

		internal static PasswordAuthenticationMechanism ConvertPSAuthenticationMechanismToCimPasswordAuthenticationMechanism(AuthenticationMechanism psAuthenticationMechanism)
		{
			AuthenticationMechanism authenticationMechanism = psAuthenticationMechanism;
			switch (authenticationMechanism)
			{
				case AuthenticationMechanism.Default:
				{
					return PasswordAuthenticationMechanism.Default;
				}
				case AuthenticationMechanism.Basic:
				{
					return PasswordAuthenticationMechanism.Basic;
				}
				case AuthenticationMechanism.Negotiate:
				case AuthenticationMechanism.NegotiateWithImplicitCredential:
				{
					return PasswordAuthenticationMechanism.Negotiate;
				}
				case AuthenticationMechanism.Credssp:
				{
					return PasswordAuthenticationMechanism.CredSsp;
				}
				case AuthenticationMechanism.Digest:
				{
					return PasswordAuthenticationMechanism.Digest;
				}
				case AuthenticationMechanism.Kerberos:
				{
					return PasswordAuthenticationMechanism.Kerberos;
				}
				default:
				{
					return PasswordAuthenticationMechanism.Default;
				}
			}
		}

		protected override List<ActivityImplementationContext> GetImplementation(NativeActivityContext context)
		{
			string empty;
			string str;
			string empty1;
			string str1;
			typeof(GenericCimCmdletActivity).IsAssignableFrom(base.GetType());
			string[] strArrays = this.PSComputerName.Get(context);
			CimSession[] cimSessionArray = this.CimSession.Get(context);
			Uri uri = null;
			if (this.ResourceUri != null)
			{
				uri = this.ResourceUri.Get(context);
			}
			List<ActivityImplementationContext> activityImplementationContexts = new List<ActivityImplementationContext>();
			if (strArrays == null || (int)strArrays.Length <= 0)
			{
				ActivityImplementationContext powerShell = this.GetPowerShell(context);
				CimActivityImplementationContext cimActivityImplementationContext = new CimActivityImplementationContext(powerShell, null, null, null, new AuthenticationMechanism?(AuthenticationMechanism.Default), false, 0, null, null, null, this.ModuleDefinition, uri);
				activityImplementationContexts.Add(cimActivityImplementationContext);
			}
			else
			{
				WSManSessionOptions wSManSessionOption = new WSManSessionOptions();
				uint? nullable = base.PSActionRunningTimeoutSec.Get(context);
				if (nullable.HasValue)
				{
					wSManSessionOption.Timeout = TimeSpan.FromSeconds((double)((float)nullable.Value));
				}
				bool? nullable1 = this.PSUseSsl.Get(context);
				bool value = false;
				if (nullable1.HasValue)
				{
					wSManSessionOption.UseSsl = nullable1.Value;
					value = nullable1.Value;
				}
				uint? nullable2 = this.PSPort.Get(context);
				uint num = 0;
				if (nullable2.HasValue)
				{
					wSManSessionOption.DestinationPort = nullable2.Value;
					num = nullable2.Value;
				}
				PSSessionOption pSSessionOption = this.PSSessionOption.Get(context);
				if (pSSessionOption != null)
				{
					wSManSessionOption.NoEncryption = pSSessionOption.NoEncryption;
					wSManSessionOption.CertCACheck = pSSessionOption.SkipCACheck;
					wSManSessionOption.CertCNCheck = pSSessionOption.SkipCNCheck;
					wSManSessionOption.CertRevocationCheck = pSSessionOption.SkipRevocationCheck;
					if (pSSessionOption.UseUTF16)
					{
						wSManSessionOption.PacketEncoding = PacketEncoding.Utf16;
					}
					if (pSSessionOption.Culture != null)
					{
						wSManSessionOption.Culture = pSSessionOption.Culture;
					}
					if (pSSessionOption.UICulture != null)
					{
						wSManSessionOption.UICulture = pSSessionOption.UICulture;
					}
					if (pSSessionOption.ProxyCredential != null)
					{
						char[] chrArray = new char[1];
						chrArray[0] = '\\';
						string[] strArrays1 = pSSessionOption.ProxyCredential.UserName.Split(chrArray);
						if ((int)strArrays1.Length >= 2)
						{
							empty = strArrays1[0];
							str = strArrays1[1];
						}
						else
						{
							empty = string.Empty;
							str = strArrays1[0];
						}
						wSManSessionOption.AddProxyCredentials(new CimCredential(PSGeneratedCIMActivity.ConvertPSAuthenticationMechanismToCimPasswordAuthenticationMechanism(pSSessionOption.ProxyAuthentication), empty, str, pSSessionOption.ProxyCredential.Password));
					}
					ProxyAccessType proxyAccessType = pSSessionOption.ProxyAccessType;
					if (proxyAccessType == ProxyAccessType.IEConfig)
					{
						wSManSessionOption.ProxyType = ProxyType.InternetExplorer;
						goto Label0;
					}
					else if (proxyAccessType == ProxyAccessType.WinHttpConfig)
					{
						wSManSessionOption.ProxyType = ProxyType.WinHttp;
						goto Label0;
					}
					else if (proxyAccessType == (ProxyAccessType.IEConfig | ProxyAccessType.WinHttpConfig))
					{
						goto Label0;
					}
					else if (proxyAccessType == ProxyAccessType.AutoDetect)
					{
						wSManSessionOption.ProxyType = ProxyType.Auto;
						goto Label0;
					}
				}
			Label0:
				PSCredential pSCredential = this.PSCredential.Get(context);
				string str2 = this.PSCertificateThumbprint.Get(context);
				if (pSCredential == null || str2 == null)
				{
					PasswordAuthenticationMechanism cimPasswordAuthenticationMechanism = PasswordAuthenticationMechanism.Default;
					AuthenticationMechanism? nullable3 = this.PSAuthentication.Get(context);
					if (nullable3.HasValue)
					{
						cimPasswordAuthenticationMechanism = PSGeneratedCIMActivity.ConvertPSAuthenticationMechanismToCimPasswordAuthenticationMechanism(nullable3.Value);
					}
					if (str2 != null)
					{
						wSManSessionOption.AddDestinationCredentials(new CimCredential(CertificateAuthenticationMechanism.Default, str2));
					}
					if (pSCredential != null)
					{
						char[] chrArray1 = new char[1];
						chrArray1[0] = '\\';
						string[] strArrays2 = pSCredential.UserName.Split(chrArray1);
						if ((int)strArrays2.Length >= 2)
						{
							empty1 = strArrays2[0];
							str1 = strArrays2[1];
						}
						else
						{
							empty1 = string.Empty;
							str1 = strArrays2[0];
						}
						wSManSessionOption.AddDestinationCredentials(new CimCredential(cimPasswordAuthenticationMechanism, empty1, str1, pSCredential.Password));
					}
					if (cimSessionArray == null || (int)cimSessionArray.Length <= 0)
					{
						string[] strArrays3 = strArrays;
						for (int i = 0; i < (int)strArrays3.Length; i++)
						{
							string str3 = strArrays3[i];
							ActivityImplementationContext activityImplementationContext = this.GetPowerShell(context);
							CimActivityImplementationContext cimActivityImplementationContext1 = new CimActivityImplementationContext(activityImplementationContext, str3, pSCredential, str2, nullable3, value, num, pSSessionOption, null, wSManSessionOption, this.ModuleDefinition, uri);
							activityImplementationContexts.Add(cimActivityImplementationContext1);
						}
					}
					else
					{
						CimSession[] cimSessionArray1 = cimSessionArray;
						for (int j = 0; j < (int)cimSessionArray1.Length; j++)
						{
							CimSession cimSession = cimSessionArray1[j];
							ActivityImplementationContext powerShell1 = this.GetPowerShell(context);
							CimActivityImplementationContext cimActivityImplementationContext2 = new CimActivityImplementationContext(powerShell1, cimSession.ComputerName, pSCredential, str2, nullable3, value, num, pSSessionOption, cimSession, wSManSessionOption, this.ModuleDefinition, uri);
							activityImplementationContexts.Add(cimActivityImplementationContext2);
						}
					}
				}
				else
				{
					throw new ArgumentException(Resources.CredentialParameterCannotBeSpecifiedWithPSCertificateThumbPrint);
				}
			}
			return activityImplementationContexts;
		}

		protected bool GetIsComputerNameSpecified(ActivityContext context)
		{
			if (this.PSComputerName.Get(context) == null)
			{
				return false;
			}
			else
			{
				return (int)this.PSComputerName.Get(context).Length > 0;
			}
		}
	}
}