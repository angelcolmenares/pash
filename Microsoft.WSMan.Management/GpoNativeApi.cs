using System;
using System.Runtime.InteropServices;

namespace Microsoft.WSMan.Management
{
	public sealed class GpoNativeApi
	{
		private GpoNativeApi()
		{
		}

		[DllImport("Userenv.dll", CharSet=CharSet.Unicode)]
		internal static extern IntPtr EnterCriticalPolicySection(bool bMachine);

		[DllImport("Userenv.dll", CharSet=CharSet.Unicode)]
		internal static extern bool LeaveCriticalPolicySection(IntPtr hSection);
	}
}