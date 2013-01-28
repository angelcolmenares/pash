using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.PowerShell.Commands
{
	public sealed class EnhancedKeyUsageProperty
	{
		private List<EnhancedKeyUsageRepresentation> ekuList;

		public List<EnhancedKeyUsageRepresentation> EnhancedKeyUsageList
		{
			get
			{
				return this.ekuList;
			}
		}

		public EnhancedKeyUsageProperty(X509Certificate2 cert)
		{
			EnhancedKeyUsageRepresentation enhancedKeyUsageRepresentation;
			this.ekuList = new List<EnhancedKeyUsageRepresentation>();
			if (DownLevelHelper.IsWin8AndAbove())
			{
				Collection<string> certEKU = SecuritySupport.GetCertEKU(cert);
				foreach (string str in certEKU)
				{
					if (string.IsNullOrEmpty(str))
					{
						continue;
					}
					IntPtr hGlobalAnsi = Marshal.StringToHGlobalAnsi(str);
					IntPtr intPtr = NativeMethods.CryptFindOIDInfo(1, hGlobalAnsi, 0);
					if (intPtr == IntPtr.Zero)
					{
						enhancedKeyUsageRepresentation = new EnhancedKeyUsageRepresentation(null, str);
					}
					else
					{
						NativeMethods.CRYPT_OID_INFO structure = (NativeMethods.CRYPT_OID_INFO)Marshal.PtrToStructure(intPtr, typeof(NativeMethods.CRYPT_OID_INFO));
						enhancedKeyUsageRepresentation = new EnhancedKeyUsageRepresentation(structure.pwszName, str);
					}
					this.ekuList.Add(enhancedKeyUsageRepresentation);
				}
			}
		}
	}
}