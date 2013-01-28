using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.PowerShell.Commands
{
	public sealed class SendAsTrustedIssuerProperty
	{
		private readonly static char[] separators;

		static SendAsTrustedIssuerProperty()
		{
			char[] chrArray = new char[2];
			chrArray[0] = '/';
			chrArray[1] = '\\';
			SendAsTrustedIssuerProperty.separators = chrArray;
		}

		public SendAsTrustedIssuerProperty()
		{
		}

		private static string[] GetPathElements(string path)
		{
			string[] strArrays = path.Split(SendAsTrustedIssuerProperty.separators);
			Stack<string> strs = new Stack<string>();
			string[] strArrays1 = strArrays;
			for (int i = 0; i < (int)strArrays1.Length; i++)
			{
				string str = strArrays1[i];
				if (!(str == ".") && !(str == string.Empty))
				{
					if (str != "..")
					{
						strs.Push(str);
					}
					else
					{
						if (strs.Count > 0)
						{
							strs.Pop();
						}
					}
				}
			}
			string[] array = strs.ToArray();
			Array.Reverse(array);
			return array;
		}

		public static bool ReadSendAsTrustedIssuerProperty(X509Certificate2 cert)
		{
			bool flag = false;
			if (DownLevelHelper.IsWin8AndAbove())
			{
				int num = 0;
				if (!NativeMethods.CertGetCertificateContextProperty(cert.Handle, NativeMethods.CertPropertyId.CERT_SEND_AS_TRUSTED_ISSUER_PROP_ID, IntPtr.Zero, ref num))
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					if (lastWin32Error != -2146885628)
					{
						throw new Win32Exception(lastWin32Error);
					}
				}
				else
				{
					flag = true;
				}
			}
			return flag;
		}

		public static void WriteSendAsTrustedIssuerProperty(X509Certificate2 cert, string certPath, bool addProperty)
		{
			IntPtr handle;
			StoreLocation storeLocation;
			if (!DownLevelHelper.IsWin8AndAbove())
			{
				throw Marshal.GetExceptionForHR(-2146893783);
			}
			else
			{
				IntPtr zero = IntPtr.Zero;
				NativeMethods.CRYPT_DATA_BLOB cRYPTDATABLOB = new NativeMethods.CRYPT_DATA_BLOB();
				cRYPTDATABLOB.cbData = 0;
				cRYPTDATABLOB.pbData = IntPtr.Zero;
				X509Certificate x509Certificate2 = null;
				try
				{
					if (certPath != null)
					{
						string[] pathElements = SendAsTrustedIssuerProperty.GetPathElements(certPath);
						bool flag = string.Equals(pathElements[0], "CurrentUser", StringComparison.OrdinalIgnoreCase);
						if (flag)
						{
							storeLocation = StoreLocation.CurrentUser;
						}
						else
						{
							storeLocation = StoreLocation.LocalMachine;
						}
						X509StoreLocation x509StoreLocation = new X509StoreLocation(storeLocation);
						X509NativeStore x509NativeStore = new X509NativeStore(x509StoreLocation, pathElements[1]);
						x509NativeStore.Open(true);
						IntPtr certByName = x509NativeStore.GetCertByName(pathElements[2]);
						if (certByName != IntPtr.Zero)
						{
							x509Certificate2 = new X509Certificate2(certByName);
							x509NativeStore.FreeCert(certByName);
						}
					}
					if (addProperty)
					{
						zero = Marshal.AllocHGlobal(Marshal.SizeOf(cRYPTDATABLOB));
						Marshal.StructureToPtr(cRYPTDATABLOB, zero, false);
					}
					if (x509Certificate2 != null)
					{
						handle = x509Certificate2.Handle;
					}
					else
					{
						handle = cert.Handle;
					}
					if (!NativeMethods.CertSetCertificateContextProperty(handle, NativeMethods.CertPropertyId.CERT_SEND_AS_TRUSTED_ISSUER_PROP_ID, 0, zero))
					{
						throw new Win32Exception(Marshal.GetLastWin32Error());
					}
				}
				finally
				{
					if (zero != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(zero);
					}
				}
				return;
			}
		}
	}
}