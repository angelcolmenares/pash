using System;

namespace Microsoft.ActiveDirectory.Management
{
	[Flags]
	internal enum GroupTypeFlag
	{
		SecurityGroup = -2147483648,
		System = 1,
		Global = 2,
		DomainLocal = 4,
		Universal = 8,
		App_Basic = 16,
		App_Query = 32
	}
}