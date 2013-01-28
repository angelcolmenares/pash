using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.ActiveDirectory.Management.Provider
{
	[ComVisible(false)]
	[SuppressUnmanagedCodeSecurity]
	internal class UnsafeNativeMethods
	{
		public UnsafeNativeMethods()
		{
		}

		[DllImport("Activeds.dll", CharSet=CharSet.Unicode)]
		internal static extern int ADsBuildVarArrayStr(string[] lppPathNames, int arrayLen, out object varArray);

		[Guid("B1B272A3-3625-11D1-A3A4-00C04FB950DC")]
		[InterfaceType(ComInterfaceType.InterfaceIsDual)]
		internal interface IADsNameTranslate
		{
			int ChaseReferral
			{
				set;
			}

			string Get(int lnFormatType);

			object GetEx(int lnFormatType);

			void Init(int lnInitType, string bstrADsPath);

			void InitEx(int lnInitType, string bstrADsPath, string bstrUserID, string bstrDomain, string bstrPassword);

			int Set(int lnSetType, string bstrADsPath);

			int SetEx(int lnSetType, object pVar);
		}

		[Guid("274fae1f-3626-11d1-a3a4-00c04fb950dc")]
		internal class NameTranslate
		{
			public extern NameTranslate();
		}
	}
}