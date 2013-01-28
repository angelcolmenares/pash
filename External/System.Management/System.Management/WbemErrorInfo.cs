using System;
using System.Runtime.InteropServices;

namespace System.Management
{
	internal class WbemErrorInfo
	{
		public WbemErrorInfo()
		{
		}

		public static IWbemClassObjectFreeThreaded GetErrorInfo()
		{
			IntPtr intPtr;
			IErrorInfo errorInfo = WbemErrorInfo.GetErrorInfo(0);
			if (errorInfo != null)
			{
				IntPtr unknownForObject = Marshal.GetIUnknownForObject(errorInfo);
				Marshal.QueryInterface(unknownForObject, ref IWbemClassObjectFreeThreaded.IID_IWbemClassObject, out intPtr);
				Marshal.Release(unknownForObject);
				if (intPtr != IntPtr.Zero)
				{
					return new IWbemClassObjectFreeThreaded(intPtr);
				}
			}
			return null;
		}

		[DllImport("oleaut32.dll", CharSet=CharSet.None)]
		private static extern IErrorInfo GetErrorInfo(int reserved);
	}
}