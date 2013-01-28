namespace System.Management.Automation.Internal
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;

    internal class PSSafeCryptKey : SafeHandleZeroOrMinusOneIsInvalid
    {
        private static PSSafeCryptKey _zero = new PSSafeCryptKey();

        internal PSSafeCryptKey() : base(true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            return PSCryptoNativeUtils.CryptDestroyKey(base.handle);
        }

        internal static PSSafeCryptKey Zero
        {
            get
            {
                return _zero;
            }
        }
    }
}

