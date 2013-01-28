using System;

namespace Microsoft.Management.Odata.MofParser.ParseTree
{
	[Flags]
	internal enum QualifierScope
	{
		None = 0,
		Association = 1,
		Class = 2,
		Indication = 4,
		Method = 8,
		Parameter = 16,
		Property = 32,
		Qualifier = 64,
		Reference = 128,
		Schema = 256,
		Any = 511
	}
}