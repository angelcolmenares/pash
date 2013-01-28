using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class DsNameResult
	{
		public int itemCount;

		public IntPtr items;

		public DsNameResult()
		{
		}
	}
}