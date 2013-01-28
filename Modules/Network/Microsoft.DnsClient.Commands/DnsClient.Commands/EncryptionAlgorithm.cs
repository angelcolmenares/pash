using System;

namespace Microsoft.DnsClient.Commands
{
	public enum EncryptionAlgorithm : byte
	{
		RSA_MD5,
		Diffie_Hellman,
		DSA,
		Elliptic_Curve,
		RSA_SHA1
	}
}