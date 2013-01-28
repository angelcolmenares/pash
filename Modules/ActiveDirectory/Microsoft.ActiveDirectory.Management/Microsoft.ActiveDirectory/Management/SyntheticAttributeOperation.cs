using System;

namespace Microsoft.ActiveDirectory.Management
{
	[Flags]
	internal enum SyntheticAttributeOperation
	{
		Read = 1,
		Write = 2
	}
}