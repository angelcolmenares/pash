using System;

namespace System.DirectoryServices.Protocols
{
	internal class LdapPartialAsyncResult : LdapAsyncResult
	{
		internal LdapConnection con;

		internal int messageID;

		internal bool partialCallback;

		internal ResultsStatus resultStatus;

		internal TimeSpan requestTimeout;

		internal SearchResponse response;

		internal Exception exception;

		internal DateTime startTime;

		public LdapPartialAsyncResult(int messageID, AsyncCallback callbackRoutine, object state, bool partialResults, LdapConnection con, bool partialCallback, TimeSpan requestTimeout) : base(callbackRoutine, state, partialResults)
		{
			this.messageID = -1;
			this.messageID = messageID;
			this.con = con;
			this.partialResults = true;
			this.partialCallback = partialCallback;
			this.requestTimeout = requestTimeout;
			this.startTime = DateTime.Now;
		}
	}
}