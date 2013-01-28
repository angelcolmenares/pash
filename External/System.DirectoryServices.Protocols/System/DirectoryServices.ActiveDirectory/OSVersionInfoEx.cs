using System;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class OSVersionInfoEx
	{
		public int osVersionInfoSize;

		public int majorVersion;

		public int minorVersion;

		public int buildNumber;

		public int platformId;

		public string csdVersion;

		public short servicePackMajor;

		public short servicePackMinor;

		public short suiteMask;

		public byte productType;

		public byte reserved;

		public OSVersionInfoEx()
		{
			this.osVersionInfoSize = Marshal.SizeOf(this);
		}
	}
}