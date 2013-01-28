using System;
using System.Collections.Generic;
using System.Management.Automation.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.PowerShell.Commands
{
	public sealed class DnsNameProperty
	{
		private List<DnsNameRepresentation> dnsList;

		public List<DnsNameRepresentation> DnsNameList
		{
			get
			{
				return this.dnsList;
			}
		}

		public DnsNameProperty(X509Certificate2 cert)
		{
			this.dnsList = new List<DnsNameRepresentation>();
			if (DownLevelHelper.IsWin8AndAbove() && cert != null)
			{
				this.dnsList = this.GetCertNames(cert.Handle, NativeMethods.AltNameType.CERT_ALT_NAME_DNS_NAME);
			}
		}

		private List<DnsNameRepresentation> GetCertNames(IntPtr certHandle, NativeMethods.AltNameType nameType)
		{
			int num = 0;
			int num1 = 0;
			IntPtr zero = IntPtr.Zero;
			IntPtr intPtr = IntPtr.Zero;
			List<DnsNameRepresentation> dnsNameRepresentations = new List<DnsNameRepresentation>();
			int num2 = NativeMethods.CCGetCertNameList(certHandle, nameType, 0, out num, out zero);
			if (num2 != 0)
			{
				if (num2 == -2146885628)
				{
					num = 0;
				}
				else
				{
					throw Marshal.GetExceptionForHR(num2);
				}
			}
			try
			{
				if (0 < num)
				{
					num2 = NativeMethods.CCGetCertNameList(certHandle, nameType, NativeMethods.CryptDecodeFlags.CRYPT_DECODE_ENABLE_IA5CONVERSION_FLAG, out num1, out intPtr);
					if (num2 == 0)
					{
						if (num == num1)
						{
							for (int i = 0; i < num; i++)
							{
								dnsNameRepresentations.Add(new DnsNameRepresentation(Marshal.PtrToStringUni(Marshal.ReadIntPtr(zero, i * Marshal.SizeOf(zero))), Marshal.PtrToStringUni(Marshal.ReadIntPtr(intPtr, i * Marshal.SizeOf(intPtr)))));
							}
						}
						else
						{
							throw Marshal.GetExceptionForHR(-2147024883);
						}
					}
					else
					{
						throw Marshal.GetExceptionForHR(num2);
					}
				}
			}
			finally
			{
				NativeMethods.CCFreeStringArray(zero);
				NativeMethods.CCFreeStringArray(intPtr);
			}
			return dnsNameRepresentations;
		}
	}
}