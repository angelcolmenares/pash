using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace Microsoft.PowerShell.Commands
{
	internal static class SafeNativeMethods
	{
		[DllImport("kernel32.dll", CharSet=CharSet.Auto)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public static extern bool CloseHandle(IntPtr handle);

		internal class PROCESS_INFORMATION
		{
			public IntPtr hProcess;

			public IntPtr hThread;

			public int dwProcessId;

			public int dwThreadId;

			public PROCESS_INFORMATION()
			{
				this.hProcess = IntPtr.Zero;
				this.hThread = IntPtr.Zero;
			}
		}
	}
}