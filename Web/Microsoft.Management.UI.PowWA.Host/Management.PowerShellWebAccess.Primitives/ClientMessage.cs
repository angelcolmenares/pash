using System;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public abstract class ClientMessage
	{
		private int jsonSerializationLength;

		internal int JsonSerializationLength
		{
			get
			{
				if (this.jsonSerializationLength == 0)
				{
					this.jsonSerializationLength = PowwaSessionManager.Instance.JsonSerializer.Serialize(this).Length;
				}
				return this.jsonSerializationLength;
			}
		}

		public ClientMessageType MessageType
		{
			get;
			private set;
		}

		protected ClientMessage(ClientMessageType type)
		{
			this.MessageType = type;
		}

		protected void ResetJsonSerializationLength()
		{
			this.jsonSerializationLength = 0;
		}
	}
}