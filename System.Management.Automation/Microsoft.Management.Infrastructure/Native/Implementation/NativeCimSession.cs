using System;

namespace Microsoft.Management.Infrastructure.Native
{
	internal struct NativeCimSession
	{
		public string SessionId { get; set; }

		public string ServerName { get;set; }

		public string Protocol { get;set; }

		public IntPtr DestinationOptions { get;set; }
	}
}

