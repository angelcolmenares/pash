using System;
using System.Threading;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public class PromptMessageBase : ClientMessage
	{
		private static int id;

		public int Id
		{
			get;
			private set;
		}

		static PromptMessageBase()
		{
		}

		protected PromptMessageBase(ClientMessageType messageType) : base(messageType)
		{
			this.Id = Interlocked.Increment(ref PromptMessageBase.id);
		}
	}
}