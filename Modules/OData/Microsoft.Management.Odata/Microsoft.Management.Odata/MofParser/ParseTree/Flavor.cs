using System;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	[Flags]
	internal enum Flavor
	{
		None = 0,
		EnableOverride = 1,
		DisableOverride = 2,
		Restricted = 4,
		ToSubclass = 8,
		Translatable = 16
	}
}