using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;

namespace Microsoft.Management.Infrastructure.Native
{
	internal class DestinationOptionsHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal DestinationOptionsHandle (IntPtr handle) : base(true)
		{
			try {
				this.handle = handle;
			} catch (Exception ex) {

			}
		}

		internal void DangerousSetHandle (IntPtr handle)
		{
			try {
				this.handle = handle;
			} catch (Exception ex) {
				
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
			_MI_DestinationOptions* _MIDestinationOptionsPointer = (_MI_DestinationOptions*)((void*)this.handle);
			this.handle = IntPtr.Zero;
			<Module>.?A0x5fa54d02.MI_DestinationOptions_Delete(_MIDestinationOptionsPointer);
			_MIDestinationOptionsPointer;
			0;
			<Module>.free(_MIDestinationOptionsPointer);
			*/
			return true;
		}
	}
}