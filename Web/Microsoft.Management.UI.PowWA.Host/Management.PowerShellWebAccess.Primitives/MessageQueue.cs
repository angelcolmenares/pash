using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Host;
using System.Threading;
using System.Timers;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	internal sealed class MessageQueue : IDisposable
	{
		internal const int MaxQueueLength = 0x2800;

		internal const int MaximumNumberOfMessagesToFlush = 0x400;

		internal const int MinimumNumberOfMessagesToFlush = 128;

		private List<ClientMessage> messageQueue;

		internal readonly static TimeSpan GetMessagesMaxWaitTime;

		internal readonly static TimeSpan MessagesAvailableDelay;

		private readonly AutoResetEvent messagesAvailable;

		private readonly System.Timers.Timer messagesAvailableTimer;

		private ClientMessage pendingInputMessage;

		private object pendingInputMessageReply;

		private readonly AutoResetEvent pendingInputMessageReplyAvailable;

		internal TimeSpan MessagesAvailableEventTimeout
		{
			get;
			set;
		}

		internal TimeSpan MessagesAvailableTimerInterval
		{
			get
			{
				return TimeSpan.FromMilliseconds(this.messagesAvailableTimer.Interval);
			}
			set
			{
				this.messagesAvailableTimer.Interval = value.TotalMilliseconds;
			}
		}

		public ClientMessage PendingInputMessage
		{
			get
			{
				return this.pendingInputMessage;
			}
		}

		public bool WaitingForUserReply
		{
			get;
			private set;
		}

		static MessageQueue()
		{
			MessageQueue.GetMessagesMaxWaitTime = TimeSpan.FromSeconds(45);
			MessageQueue.MessagesAvailableDelay = TimeSpan.FromSeconds(1);
		}

		public MessageQueue()
		{
			ElapsedEventHandler elapsedEventHandler = null;
			this.messageQueue = new List<ClientMessage>();
			this.messagesAvailable = new AutoResetEvent(false);
			TimeSpan messagesAvailableDelay = MessageQueue.MessagesAvailableDelay;
			System.Timers.Timer timer = new System.Timers.Timer(messagesAvailableDelay.TotalMilliseconds);
			timer.AutoReset = false;
			this.messagesAvailableTimer = timer;
			System.Timers.Timer timer1 = this.messagesAvailableTimer;
			if (elapsedEventHandler == null)
			{
				elapsedEventHandler = (object param0, ElapsedEventArgs param1) => this.messagesAvailable.Set();
			}
			timer1.Elapsed += elapsedEventHandler;
			this.pendingInputMessage = null;
			this.pendingInputMessageReply = null;
			this.pendingInputMessageReplyAvailable = new AutoResetEvent(false);
			this.MessagesAvailableEventTimeout = MessageQueue.GetMessagesMaxWaitTime;
			this.WaitingForUserReply = false;
		}

		private bool CombineWriteLineMessage(WriteLineMessage writeLineMessage)
		{
			WriteMessage item;
			if (writeLineMessage.Value.Length == 0)
			{
				if (this.messageQueue.Count > 0)
				{
					item = this.messageQueue[this.messageQueue.Count - 1] as WriteMessage;
				}
				else
				{
					item = null;
				}
				WriteMessage writeMessage = item;
				if (writeMessage != null)
				{
					this.messageQueue[this.messageQueue.Count - 1] = new WriteLineMessage(writeMessage.ForegroundColor, writeMessage.BackgroundColor, writeMessage.Value);
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		private bool CombineWriteMessage(WriteMessage writeMessage, Size bufferSize)
		{
			WriteMessage item;
			if (this.messageQueue.Count > 0)
			{
				item = this.messageQueue[this.messageQueue.Count - 1] as WriteMessage;
			}
			else
			{
				item = null;
			}
			WriteMessage writeMessage1 = item;
			if (writeMessage1 != null)
			{
				if (writeMessage1.BackgroundColor != writeMessage.BackgroundColor || writeMessage1.ForegroundColor != writeMessage.ForegroundColor || writeMessage1.Value.Length + writeMessage.Value.Length > bufferSize.Width)
				{
					return false;
				}
				else
				{
					writeMessage1.Append(writeMessage.Value);
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		public void CommandCancelled()
		{
			if (this.WaitingForUserReply)
			{
				this.SetInputMessageReply(null);
			}
		}

		public void Dispose()
		{
			this.messagesAvailableTimer.Stop();
			this.messagesAvailableTimer.Dispose();
			this.messagesAvailable.Dispose();
			this.pendingInputMessageReplyAvailable.Dispose();
			GC.SuppressFinalize(this);
		}

		private void FlushMessageQueue(Size bufferSize)
		{
			int num = -1;
			int num1 = 0;
			while (num1 < this.messageQueue.Count)
			{
				if (this.messageQueue[num1].MessageType != ClientMessageType.WriteLine)
				{
					num1++;
				}
				else
				{
					num = num1;
					break;
				}
			}
			if (num >= 0)
			{
				int num2 = num + 1;
				while (num2 < this.messageQueue.Count && num2 - num < 0x400 && this.messageQueue[num2].MessageType == ClientMessageType.WriteLine)
				{
					num2++;
				}
				if (num2 - num >= 128)
				{
					List<ClientMessage> clientMessages = new List<ClientMessage>();
					for (int i = 0; i < num; i++)
					{
						clientMessages.Add(this.messageQueue[i]);
					}
					WriteLineMessage item = (WriteLineMessage)this.messageQueue[num];
					object[] objArray = new object[1];
					objArray[0] = num2 - num;
					clientMessages.Add(new WriteLineMessage(item.ForegroundColor, item.BackgroundColor, string.Format(CultureInfo.CurrentCulture, Resources.ClientMessagesFlushed_Format, objArray)));
					for (int j = num2; j < this.messageQueue.Count; j++)
					{
						clientMessages.Add(this.messageQueue[j]);
					}
					this.messageQueue = clientMessages;
					return;
				}
				else
				{
					throw new InvalidOperationException(Resources.ErrorTooManyClientMessages);
				}
			}
			else
			{
				throw new InvalidOperationException(Resources.ErrorTooManyClientMessages);
			}
		}

		public int GetMessageCount()
		{
			return this.messageQueue.Count;
		}

		public ClientMessage[] GetMessages()
		{
			int maxJsonLength = PowwaSessionManager.Instance.JsonSerializer.MaxJsonLength - 0x1000;
			int jsonSerializationLength = 0;
			int i = 0;
			List<ClientMessage> clientMessages = new List<ClientMessage>();
			for (i = 0; i < this.messageQueue.Count && jsonSerializationLength + this.messageQueue[i].JsonSerializationLength <= maxJsonLength; i++)
			{
				ClientMessage item = this.messageQueue[i];
				clientMessages.Add(item);
				jsonSerializationLength = jsonSerializationLength + item.JsonSerializationLength;
			}
			List<ClientMessage> clientMessages1 = new List<ClientMessage>();
			while (i < this.messageQueue.Count)
			{
				int num = i;
				i = num + 1;
				clientMessages1.Add(this.messageQueue[num]);
			}
			this.messageQueue = clientMessages1;
			return clientMessages.ToArray();
		}

		public object GetUserReply()
		{
			object obj = this.pendingInputMessageReply;
			this.pendingInputMessage = null;
			this.pendingInputMessageReply = null;
			return obj;
		}

		public void Post(ClientMessage message, bool isInputMessage, bool isInternalMessage, Size bufferSize)
		{
			if (message.JsonSerializationLength <= PowwaSessionManager.Instance.JsonSerializer.MaxJsonLength)
			{
				bool flag = false;
				if (message.MessageType != ClientMessageType.Write)
				{
					if (message.MessageType == ClientMessageType.WriteLine)
					{
						flag = this.CombineWriteLineMessage((WriteLineMessage)message);
					}
				}
				else
				{
					flag = this.CombineWriteMessage((WriteMessage)message, bufferSize);
				}
				if (!flag)
				{
					if (this.messageQueue.Count >= 0x2800 && !isInputMessage && message.MessageType != ClientMessageType.CommandCompleted && !isInternalMessage)
					{
						this.messagesAvailable.Set();
						this.FlushMessageQueue(bufferSize);
					}
					this.messageQueue.Add(message);
				}
				if (message.MessageType != ClientMessageType.CommandCompleted)
				{
					if (this.messageQueue.Count != 1)
					{
						if (!this.messagesAvailableTimer.Enabled)
						{
							this.messagesAvailable.Set();
						}
					}
					else
					{
						this.messagesAvailableTimer.Stop();
						this.messagesAvailableTimer.Start();
					}
				}
				else
				{
					this.messagesAvailable.Set();
				}
				if (isInputMessage)
				{
					this.pendingInputMessage = message;
				}
				return;
			}
			else
			{
				this.messagesAvailable.Set();
				throw new InvalidOperationException(Resources.ErrorClientMessageTooLarge);
			}
		}

		public void SetInputMessageReply(object reply)
		{
			this.pendingInputMessageReply = reply;
			this.pendingInputMessageReplyAvailable.Set();
		}

		public void WaitForMessages()
		{
			this.messagesAvailable.WaitOne(this.MessagesAvailableEventTimeout);
		}

		public void WaitForUserReply()
		{
			this.WaitingForUserReply = true;
			try
			{
				this.pendingInputMessageReplyAvailable.WaitOne();
			}
			finally
			{
				this.WaitingForUserReply = false;
			}
		}
	}
}