using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class LARGE_INTEGER
	{
		public int lowPart;

		public int highPart;

		public LARGE_INTEGER()
		{
			this.lowPart = 0;
			this.highPart = 0;
		}
	}
}