using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.WSMan.Management
{
	[SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
	[TypeLibType((short)0)]
	public enum WSManSessionFlags
	{
		WSManNone = 0,
		WSManFlagUtf8 = 1,
		WSManFlagCredUserNamePassword = 4096,
		WSManFlagSkipCACheck = 8192,
		WSManFlagSkipCNCheck = 16384,
		WSManFlagUseNoAuthentication = 32768,
		WSManFlagUseDigest = 65536,
		WSManFlagUseNegotiate = 131072,
		WSManFlagUseBasic = 262144,
		WSManFlagUseKerberos = 524288,
		WSManFlagNoEncryption = 1048576,
		WSManFlagUseClientCertificate = 2097152,
		WSManFlagEnableSpnServerPort = 4194304,
		WSManFlagUtf16 = 8388608,
		WSManFlagUseCredSsp = 16777216,
		WSManFlagSkipRevocationCheck = 33554432,
		WSManFlagAllowNegotiateImplicitCredentials = 67108864,
		WSManFlagUseSsl = 134217728
	}
}