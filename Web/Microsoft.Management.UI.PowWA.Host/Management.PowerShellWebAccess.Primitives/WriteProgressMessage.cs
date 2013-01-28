using System;
using System.Management.Automation;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class WriteProgressMessage : ClientMessage
	{
		public PowwaProgressRecord Record
		{
			get;
			private set;
		}

		public long SourceId
		{
			get;
			private set;
		}

		internal WriteProgressMessage(long sourceId, ProgressRecord record) : base((ClientMessageType)108)
		{
			this.SourceId = sourceId;
			this.Record = new PowwaProgressRecord(record);
		}
	}
}