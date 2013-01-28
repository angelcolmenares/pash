using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;

namespace Microsoft.Management.Infrastructure.Native
{
	internal class ClassHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal ClassHandle (IntPtr handle, bool ownsHandle) : base(ownsHandle) 
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
		protected override bool ReleaseHandle()
		{
			this.handle = IntPtr.Zero;
			return true; //TODO: (byte)(<Module>.?A0x37ea71e9.MI_Class_Delete((void*)this.handle) == 0);
		}

		public Microsoft.Management.Infrastructure.Native.ClassHandle Clone ()
		{
			return new ClassHandle(this.handle, true);
		}
	}
}