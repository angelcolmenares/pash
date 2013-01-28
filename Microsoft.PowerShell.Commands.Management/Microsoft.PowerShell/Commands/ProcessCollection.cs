using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Microsoft.PowerShell.Commands
{
	internal class ProcessCollection
	{
		private Process process;

		private ProcessNativeMethods.SafeLocalMemHandle jobObjectHandle;

		internal ProcessCollection(Process process)
		{
			this.process = process;
			IntPtr intPtr = NativeMethods.CreateJobObject(IntPtr.Zero, null);
			this.jobObjectHandle = new ProcessNativeMethods.SafeLocalMemHandle(intPtr, false);
		}

		internal void CheckJobStatus(object stateInfo)
		{
			AutoResetEvent autoResetEvent = (AutoResetEvent)stateInfo;
			JOBOBJECT_BASIC_PROCESS_ID_LIST jOBOBJECTBASICPROCESSIDLIST = new JOBOBJECT_BASIC_PROCESS_ID_LIST();
			int num = Marshal.SizeOf(jOBOBJECTBASICPROCESSIDLIST);
			if (NativeMethods.QueryInformationJobObject(this.jobObjectHandle, 3, ref jOBOBJECTBASICPROCESSIDLIST, num, IntPtr.Zero) && jOBOBJECTBASICPROCESSIDLIST.NumberOfAssignedProcess == 0)
			{
				autoResetEvent.Set();
			}
		}

		internal void Start()
		{
			NativeMethods.AssignProcessToJobObject(this.jobObjectHandle, this.process.Handle);
		}

		internal void WaitOne()
		{
			AutoResetEvent autoResetEvent = new AutoResetEvent(false);
			TimerCallback timerCallback = new TimerCallback(this.CheckJobStatus);
			using (Timer timer = new Timer(timerCallback, autoResetEvent, 0, 0x3e8))
			{
				autoResetEvent.WaitOne();
			}
		}
	}
}