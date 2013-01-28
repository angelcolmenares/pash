using System;

namespace Microsoft.Management.Infrastructure.Serialization
{
	[Flags]
	public enum InstanceSerializationOptions : int
	{
		None,
		IncludeClasses
	}
}