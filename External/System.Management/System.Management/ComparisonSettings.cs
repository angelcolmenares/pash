using System;

namespace System.Management
{
	[Flags]
	public enum ComparisonSettings
	{
		IncludeAll = 0,
		IgnoreQualifiers = 1,
		IgnoreObjectSource = 2,
		IgnoreDefaultValues = 4,
		IgnoreClass = 8,
		IgnoreCase = 16,
		IgnoreFlavor = 32
	}
}