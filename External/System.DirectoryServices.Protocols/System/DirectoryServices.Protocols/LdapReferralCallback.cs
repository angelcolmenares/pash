using System;

namespace System.DirectoryServices.Protocols
{
	internal struct LdapReferralCallback
	{
		public int sizeofcallback;

		public QUERYFORCONNECTIONInternal query;

		public NOTIFYOFNEWCONNECTIONInternal notify;

		public DEREFERENCECONNECTIONInternal dereference;

	}
}