namespace Microsoft.Powershell.Commands.GetCounter.PdhNative
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;

    internal sealed class PdhSafeQueryHandle : SafeHandle
    {
        private PdhSafeQueryHandle() : base(IntPtr.Zero, true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            return (PdhHelper.PdhCloseQuery(base.handle) == 0);
        }

        public override bool IsInvalid
        {
            get
            {
                return (base.handle == IntPtr.Zero);
            }
        }
    }
}

