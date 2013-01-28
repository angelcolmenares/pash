using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Activities
{
	internal static class ActivityUtils
	{
		private static int DefaultMaximumConnectionRedirectionCount;

		static ActivityUtils()
		{
			ActivityUtils.DefaultMaximumConnectionRedirectionCount = 5;
		}

		internal static List<WSManConnectionInfo> GetConnectionInfo(string[] PSComputerName, string[] PSConnectionUri, string PSCertificateThumbprint, string PSConfigurationName, bool? PSUseSsl, int? PSPort, string PSApplicationName, PSCredential PSCredential, AuthenticationMechanism PSAuthentication, bool PSAllowRedirection, PSSessionOption options)
		{
			int defaultMaximumConnectionRedirectionCount;
			List<WSManConnectionInfo> wSManConnectionInfos = new List<WSManConnectionInfo>();
			string[] pSConnectionUri = null;
			bool flag = false;
			if (PSComputerName.IsNullOrEmpty() || !PSConnectionUri.IsNullOrEmpty())
			{
				if (!PSComputerName.IsNullOrEmpty() || PSConnectionUri.IsNullOrEmpty())
				{
					throw new ArgumentException(Resources.CannotSupplyUriAndComputername);
				}
				else
				{
					pSConnectionUri = PSConnectionUri;
				}
			}
			else
			{
				pSConnectionUri = PSComputerName;
				flag = true;
			}
			string[] strArrays = pSConnectionUri;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				if (string.IsNullOrEmpty(str))
				{
					wSManConnectionInfos.Add(null);
				}
				else
				{
					WSManConnectionInfo wSManConnectionInfo = new WSManConnectionInfo();
					if (PSPort.HasValue)
					{
						wSManConnectionInfo.Port = PSPort.Value;
					}
					if (PSUseSsl.HasValue && PSUseSsl.Value)
					{
						wSManConnectionInfo.Scheme = "https";
					}
					if (!string.IsNullOrEmpty(PSConfigurationName))
					{
						wSManConnectionInfo.ShellUri = PSConfigurationName;
					}
					if (!string.IsNullOrEmpty(PSApplicationName))
					{
						wSManConnectionInfo.AppName = PSApplicationName;
					}
					if (!flag)
					{
						wSManConnectionInfo.ConnectionUri = (Uri)LanguagePrimitives.ConvertTo(str, typeof(Uri), CultureInfo.InvariantCulture);
					}
					else
					{
						wSManConnectionInfo.ComputerName = str;
					}
					if (PSCredential != null)
					{
						wSManConnectionInfo.Credential = PSCredential;
					}
					if (!string.IsNullOrEmpty(PSCertificateThumbprint))
					{
						wSManConnectionInfo.CertificateThumbprint = PSCertificateThumbprint;
					}
					if (PSAuthentication != AuthenticationMechanism.Default)
					{
						wSManConnectionInfo.AuthenticationMechanism = PSAuthentication;
					}
					WSManConnectionInfo wSManConnectionInfo1 = wSManConnectionInfo;
					if (PSAllowRedirection)
					{
						defaultMaximumConnectionRedirectionCount = ActivityUtils.DefaultMaximumConnectionRedirectionCount;
					}
					else
					{
						defaultMaximumConnectionRedirectionCount = 0;
					}
					wSManConnectionInfo1.MaximumConnectionRedirectionCount = defaultMaximumConnectionRedirectionCount;
					if (options != null)
					{
						wSManConnectionInfo.SetSessionOptions(options);
					}
					wSManConnectionInfos.Add(wSManConnectionInfo);
				}
			}
			return wSManConnectionInfos;
		}

		internal static bool IsNullOrEmpty(this ICollection c)
		{
			if (c == null)
			{
				return true;
			}
			else
			{
				return c.Count == 0;
			}
		}
	}
}