using System;

namespace Microsoft.Management.Infrastructure.Serialization
{
	[Flags]
	public enum ClassSerializationOptions : int
	{
		None,
		IncludeParentClasses
	}
}