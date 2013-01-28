using System;
using System.Runtime;

namespace System.DirectoryServices.Protocols
{
	internal class Luid
	{
		internal int lowPart;

		internal int highPart;

		public int HighPart
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.highPart;
			}
		}

		public int LowPart
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.lowPart;
			}
		}

		internal Luid()
		{
		}
	}
}