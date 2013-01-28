using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;

namespace Microsoft.Management.Infrastructure.Native
{
	internal class ApplicationHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal ApplicationHandle(IntPtr handle) : base(true)
		{
            try
            {
                this.handle = handle;
            }
            catch
            {

            }
		}

		[Conditional("DEBUG")]
		internal void AssertValidInternalState()
		{
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		protected override unsafe bool ReleaseHandle()
		{
            /*
			_MI_Application* _MIApplicationPointer = (_MI_Application*)((void*)this.handle);
			this.handle = IntPtr.Zero;
			_MIApplicationPointer;
			0;
			<Module>.free(_MIApplicationPointer);
			return (byte)(<Module>.?A0x21501c0d.MI_Application_Close(_MIApplicationPointer) == 0);
             */
            return true;
		}
	}
}