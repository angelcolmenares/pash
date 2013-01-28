using System;

namespace System.DirectoryServices.Protocols
{
	internal class LdapRequestState
	{
		internal DirectoryResponse response;

		internal LdapAsyncResult ldapAsync;

		internal Exception exception;

		internal bool abortCalled;

		public LdapRequestState()
		{
		}
	}
}