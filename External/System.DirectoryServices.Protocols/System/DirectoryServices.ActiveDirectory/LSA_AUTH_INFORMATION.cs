using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class LSA_AUTH_INFORMATION
	{
		public LARGE_INTEGER LastUpdateTime;

		public int AuthType;

		public int AuthInfoLength;

		public IntPtr AuthInfo;

		public LSA_AUTH_INFORMATION()
		{
		}
	}
}