using System;

namespace Microsoft.ActiveDirectory.Management
{
	public enum ADClaimValueType : long
	{
		Invalid = 0,
		Int64 = 1,
		UInt64 = 2,
		String = 3,
		FQBN = 4,
		SID = 5,
		Boolean = 6,
		OctetString = 16
	}
}