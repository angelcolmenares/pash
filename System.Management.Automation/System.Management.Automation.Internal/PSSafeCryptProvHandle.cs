namespace System.Management.Automation.Internal
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;

    internal class PSSafeCryptProvHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal PSSafeCryptProvHandle() : base(true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            return PSCryptoNativeUtils.CryptReleaseContext(base.handle, 0);
        }
    }
}

