using System;

namespace System.DirectoryServices.Protocols
{
	[Flags]
	public enum ReferralChasingOptions
	{
		None = 0,
		Subordinate = 32,
		External = 64,
		All = 96
	}
}