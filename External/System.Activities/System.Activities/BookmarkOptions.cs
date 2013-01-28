using System;

namespace System.Activities
{
	[Flags]
	public enum BookmarkOptions
	{
		None = 0,
		MultipleResume = 1,
		NonBlocking = 2,
	}
}
