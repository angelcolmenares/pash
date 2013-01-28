using System;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class NegotiateCallerNameResponse
	{
		public int messageType;

		public string callerName;

		public NegotiateCallerNameResponse()
		{
		}
	}
}