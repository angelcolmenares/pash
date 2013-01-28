using System;

namespace Microsoft.Management.Infrastructure
{
	[Flags]
	public enum CimFlags : long
	{
		None = 0,
		Class = 1,
		Method = 2,
		Property = 4,
		Parameter = 8,
		Association = 16,
		Indication = 32,
		Reference = 64,
		Any = 127,
		EnableOverride = 128,
		DisableOverride = 256,
		Restricted = 512,
		ToSubclass = 1024,
		Translatable = 2048,
		Key = 4096,
		In = 8192,
		Out = 16384,
		Required = 32768,
		Static = 65536,
		Abstract = 131072,
		Terminal = 262144,
		Expensive = 524288,
		Stream = 1048576,
		ReadOnly = 2097152,
		NotModified = 33554432,
		NullValue = 536870912,
		Borrow = 1073741824,
		Adopt = 2147483648
	}
}