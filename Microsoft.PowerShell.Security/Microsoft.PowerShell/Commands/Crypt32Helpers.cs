using System;
using System.Collections.Generic;
using System.Management.Automation.Internal;
using System.Management.Automation.Security;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.PowerShell.Commands
{
	internal static class Crypt32Helpers
	{
		private static object staticLock;

		internal static List<string> storeNames;

		static Crypt32Helpers()
		{
			Crypt32Helpers.staticLock = new object();
			Crypt32Helpers.storeNames = new List<string>();
		}

		internal static bool CertEnumSystemStoreCallBack(string storeName, uint dwFlagsNotUsed, IntPtr notUsed1, IntPtr notUsed2, IntPtr notUsed3)
		{
			Crypt32Helpers.storeNames.Add(storeName);
			return true;
		}

		[ArchitectureSensitive]
		internal static List<string> GetStoreNamesAtLocation(StoreLocation location)
		{
			NativeMethods.CertStoreFlags certStoreFlag = NativeMethods.CertStoreFlags.CERT_SYSTEM_STORE_CURRENT_USER;
			StoreLocation storeLocation = location;
			switch (storeLocation)
			{
				case StoreLocation.CurrentUser:
				{
					certStoreFlag = NativeMethods.CertStoreFlags.CERT_SYSTEM_STORE_CURRENT_USER;
					break;
				}
				case StoreLocation.LocalMachine:
				{
					certStoreFlag = NativeMethods.CertStoreFlags.CERT_SYSTEM_STORE_LOCAL_MACHINE;
					break;
				}
			}
			NativeMethods.CertEnumSystemStoreCallBackProto certEnumSystemStoreCallBackProto = new NativeMethods.CertEnumSystemStoreCallBackProto(Crypt32Helpers.CertEnumSystemStoreCallBack);
			List<string> strs = new List<string>();
			lock (Crypt32Helpers.staticLock)
			{
				Crypt32Helpers.storeNames.Clear();
				NativeMethods.CertEnumSystemStore(certStoreFlag, IntPtr.Zero, IntPtr.Zero, certEnumSystemStoreCallBackProto);
				foreach (string storeName in Crypt32Helpers.storeNames)
				{
					strs.Add(storeName);
				}
			}
			return strs;
		}
	}
}