using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;

namespace Microsoft.Management.Infrastructure.Native
{
	internal class SubscriptionDeliveryOptionsHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal SubscriptionDeliveryOptionsHandle (IntPtr handle) : base(true)
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
			_MI_SubscriptionDeliveryOptions* _MISubscriptionDeliveryOptionsPointer = (_MI_SubscriptionDeliveryOptions*)((void*)this.handle);
			this.handle = IntPtr.Zero;
			_MISubscriptionDeliveryOptionsPointer;
			0;
			<Module>.free(_MISubscriptionDeliveryOptionsPointer);
			return (byte)(<Module>.?A0xa71ecfc5.MI_SubscriptionDeliveryOptions_Delete(_MISubscriptionDeliveryOptionsPointer) == 0);
			*/

			this.handle = IntPtr.Zero;
			return true;
		}
	}
}