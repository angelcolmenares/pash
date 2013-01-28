using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Management.Infrastructure.Native
{
	internal class NativeCimCredential
	{
		private NativeCimCredential()
		{
		}

		internal static unsafe void CreateCimCredential(string authenticationMechanism, out NativeCimCredentialHandle credentialHandle)
		{
			IntPtr hGlobalUni = Marshal.StringToHGlobalUni(authenticationMechanism);
            credentialHandle = new NativeCimCredentialHandle(hGlobalUni, false, null);
		}

		internal static unsafe void CreateCimCredential(string authenticationMechanism, string domain, string userName, SecureString password, out NativeCimCredentialHandle credentialHandle)
		{
            IntPtr hGlobalUni = Marshal.StringToHGlobalUni(authenticationMechanism);
			IntPtr intPtr = Marshal.StringToHGlobalUni(domain);
			IntPtr hGlobalUni1 = Marshal.StringToHGlobalUni(userName);

			credentialHandle = new NativeCimCredentialHandle(hGlobalUni1, false, password);
		}

		internal static unsafe void CreateCimCredential(string authenticationMechanism, string certificateThumbprint, out NativeCimCredentialHandle credentialHandle)
		{
			IntPtr hGlobalUni = Marshal.StringToHGlobalUni(authenticationMechanism);
			IntPtr intPtr = Marshal.StringToHGlobalUni(certificateThumbprint);
            credentialHandle = new NativeCimCredentialHandle(intPtr, true, null);
		}
	}
}