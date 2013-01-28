using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class NegotiateCallerNameRequest
	{
		public int messageType;

		public LUID logonId;

		public NegotiateCallerNameRequest()
		{
		}
	}
}