using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;

namespace Microsoft.Management.Infrastructure.Native
{
	internal class InstanceHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal InstanceHandle (IntPtr handle, bool ownsHandle) : base(ownsHandle)
		{
			try {
				this.handle = handle;
			} catch (Exception ex) {

			}
		}

		internal void DangerousSetHandle (IntPtr newHandle)
		{
			try {
				this.handle = newHandle;
			} catch (Exception ex) {
				
			}
		}

		//[Conditional("DEBUG")]
		internal void AssertValidInternalState()
		{

		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		protected override bool ReleaseHandle()
		{
			this.handle = IntPtr.Zero;
			return true; //TODO: (byte)(<Module>.?A0x3f2435d0.MI_Instance_Delete((void*)this.handle) == 0);
		}

		public InstanceHandle Clone ()
		{
			return new InstanceHandle(this.handle, true);
		}
	}
}