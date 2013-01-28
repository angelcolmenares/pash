using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Management.Infrastructure.Native
{
	internal class NativeCimCredentialHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		private SecureString passwordSecureStr;

		private bool credentialIsCretificate;

		internal NativeCimCredentialHandle(IntPtr handle, bool bIsCertificate, SecureString secureStr) : base(true)
		{
            try
            {
                this.passwordSecureStr = null;
                this.credentialIsCretificate = bIsCertificate;
                if (secureStr != null && secureStr.Length > 0)
                {
                    this.passwordSecureStr = secureStr.Copy();
                }
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

		internal SecureString GetSecureString()
		{
			return this.passwordSecureStr;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		protected override unsafe bool ReleaseHandle()
		{
			return true;
		}
	}
}