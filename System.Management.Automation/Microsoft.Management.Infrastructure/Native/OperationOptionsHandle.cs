using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;

namespace Microsoft.Management.Infrastructure.Native
{
	internal class OperationOptionsHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal OperationOptionsHandle (IntPtr handle) : base(true)
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
			_MI_OperationOptions* _MIOperationOptionsPointer = (_MI_OperationOptions*)((void*)this.handle);
			this.handle = IntPtr.Zero;
			<Module>.?A0x59b3b7f8.MI_OperationOptions_Delete(_MIOperationOptionsPointer);
			_MIOperationOptionsPointer;
			0;
			<Module>.free(_MIOperationOptionsPointer);
			*/
			return true;
		}
	}
}