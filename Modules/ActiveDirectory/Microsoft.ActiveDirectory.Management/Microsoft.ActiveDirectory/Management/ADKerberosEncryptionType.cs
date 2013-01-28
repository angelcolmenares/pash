using System;

namespace Microsoft.ActiveDirectory.Management
{
	[Flags]
	public enum ADKerberosEncryptionType
	{
		None = 0,
		DES = 3,
		RC4 = 4,
		AES128 = 8,
		AES256 = 16
	}
}