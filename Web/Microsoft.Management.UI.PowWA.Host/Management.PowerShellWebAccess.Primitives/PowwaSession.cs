using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public sealed class PowwaSession : IDisposable
	{
		private readonly System.Management.Automation.PowerShell executingCommandPowerShell;

		private Runspace Runspace 
		{
			get { return Host.Runspace; }
			set { Host.SetRunspace (value); } 
		}

		private readonly PowwaHost host;

		private readonly MessageQueue messageQueue;

		private readonly object clientRequestLock;

		private readonly object cancelCommandLock;

		private readonly object sessionStateLock;

		public readonly static int ErrorBadNetPath;

		public readonly static int ErrorLogOnFailure;

		public readonly static int ErrorBadNetName;

		private readonly Microsoft.PowerShell.Executor exec;

		private readonly static RNGCryptoServiceProvider CryptoServiceProvider;

		public string AuthenticatedUserSid
		{
			get;
			internal set;
		}

		internal PowwaHost Host
		{
			get
			{
				return this.host;
			}
		}

		public string Id
		{
			get;
			private set;
		}

		public bool InUse
		{
			get;
			set;
		}

		internal TimeSpan MessagesAvailableEventTimeout
		{
			get
			{
				return this.messageQueue.MessagesAvailableEventTimeout;
			}
			set
			{
				this.messageQueue.MessagesAvailableEventTimeout = value;
			}
		}

		public string Name
		{
			get;
			internal set;
		}

		public string SessionKey
		{
			get;
			private set;
		}

		internal PowwaSession.SessionState State
		{
			get;
			private set;
		}

		public string UserName
		{
			get;
			private set;
		}

		internal bool WaitingForUserReply
		{
			get
			{
				return this.messageQueue.WaitingForUserReply;
			}
		}

		static PowwaSession()
		{
			PowwaSession.ErrorBadNetPath = 53;
			PowwaSession.ErrorLogOnFailure = 0x52e;
			PowwaSession.ErrorBadNetName = 67;
			PowwaSession.CryptoServiceProvider = new RNGCryptoServiceProvider();
		}

		internal PowwaSession(string sessionId, string sessionUser, RunspaceConnectionInfo connectionInfo, ClientInfo clientInfo)
		{
			EventHandler<MessageCreatedEventArgs> eventHandler = null;
			EventHandler<MessageCreatedEventArgs> eventHandler1 = null;
			EventHandler<MessageCreatedEventArgs> eventHandler2 = null;
			if (sessionId != null)
			{
				if (connectionInfo != null)
				{
					if (clientInfo != null)
					{
						this.Id = sessionId;
						this.Name = string.Empty;
						this.UserName = sessionUser;
						this.host = new PowwaHost(clientInfo);
						this.host.UI.RawUI.WindowTitle = connectionInfo.ComputerName;
						PowwaHost powwaHost = this.host;
						if (eventHandler == null)
						{
							eventHandler = (object sender, MessageCreatedEventArgs e) => this.OnClientMessageCreated(e);
						}
						powwaHost.MessageCreated += eventHandler;
						IMessageCreated uI = (IMessageCreated)this.host.UI;
						if (eventHandler1 == null)
						{
							eventHandler1 = (object sender, MessageCreatedEventArgs e) => this.OnClientMessageCreated(e);
						}
						uI.MessageCreated += eventHandler1;
						IMessageCreated rawUI = (IMessageCreated)this.host.UI.RawUI;
						if (eventHandler2 == null)
						{
							eventHandler2 = (object sender, MessageCreatedEventArgs e) => this.OnClientMessageCreated(e);
						}
						rawUI.MessageCreated += eventHandler2;
						this.Runspace = RunspaceFactory.CreateRunspace(connectionInfo, this.host, PowwaSession.TypeTableLoader.Instance.LoadDefaultTypeFiles());
						this.Runspace.StateChanged += new EventHandler<RunspaceStateEventArgs>(this.OnRunspaceStateChanged);
						this.executingCommandPowerShell = System.Management.Automation.PowerShell.Create();
						this.executingCommandPowerShell.Runspace = this.Runspace;
						this.executingCommandPowerShell.InvocationStateChanged += new EventHandler<PSInvocationStateChangedEventArgs>(this.OnExecutingCommandInvocationStateChanged);
						this.messageQueue = new MessageQueue();
						this.clientRequestLock = new object();
						this.cancelCommandLock = new object();
						this.sessionStateLock = new object();
						this.SessionKey = PowwaSession.CreateSessionKey();
						this.State = PowwaSession.SessionState.Available;
						try {
							this.Runspace.Open ();
						}
						catch(Exception ex)
						{
							var stackTrace = ex.StackTrace;
							System.Diagnostics.Debug.WriteLine (stackTrace);
						}
						this.exec = new Microsoft.PowerShell.Executor(this.host, false, false);
						this.exec.OutputHandler += HandleOutputHandler;
						if (this.messageQueue.GetMessageCount() > 0)
						{
							this.PostClientMessage(new CommandCompletedMessage(this.GetPowerShellPrompt()), false);
						}
						return;
					}
					else
					{
						throw new ArgumentNullException("clientInfo");
					}
				}
				else
				{
					throw new ArgumentNullException("connectionInfo");
				}
			}
			else
			{
				throw new ArgumentNullException("sessionId");
			}
		}

		private void HandleOutputHandler (object sender, PipelineStateEventArgs e)
		{
			OnExecutingCommandInvocationStateChanged(this.executingCommandPowerShell, new PSInvocationStateChangedEventArgs(new PSInvocationStateInfo(e.PipelineStateInfo)));
		}

		public void CancelCommand()
		{
			PowwaEvents.PowwaEVENT_DEBUG_LOG0("CancelCommand(): Enter");
			try
			{
				lock (this.cancelCommandLock)
				{
					lock (this.sessionStateLock)
					{
						PowwaSession.SessionState state = this.State;
						switch (state)
						{
							case PowwaSession.SessionState.Available:
							case PowwaSession.SessionState.CancellingCommand:
							{
								return;
							}
							case PowwaSession.SessionState.ExecutingCommand:
							{
								this.State = PowwaSession.SessionState.CancellingCommand;
								this.executingCommandPowerShell.BeginStop(null, null);
								this.messageQueue.CommandCancelled();
								return;
							}
						}
						throw new InvalidOperationException();
					}
				}
			}
			finally
			{
				PowwaEvents.PowwaEVENT_DEBUG_LOG0("CancelCommand(): Exit");
			}
		}

		public void Close()
		{
			lock (this.clientRequestLock)
			{
				lock (this.sessionStateLock)
				{
					if (this.State != PowwaSession.SessionState.Closed)
					{
						this.Runspace.CloseAsync();
						this.messageQueue.Dispose();
						this.State = PowwaSession.SessionState.Closed;
					}
				}
			}
		}

		private static string CreateSessionKey()
		{
			byte[] numArray = new byte[16];
			PowwaSession.CryptoServiceProvider.GetNonZeroBytes(numArray);
			return Convert.ToBase64String(numArray);
		}

		//[DllImport("credui", CharSet=CharSet.Unicode)]
		private static uint CredUIParseUserName (string pszUserName, StringBuilder pszUser, int ulUserMaxChars, StringBuilder pszDomain, int ulDomainMaxChars)
		{
			if (pszUserName.Length > ulUserMaxChars) {
				return 1;
			}
			return 0;
		}

		public void Dispose()
		{
			this.Close();
			GC.SuppressFinalize(this);
		}

		public ClientMessage[] ExecuteCommand(string command)
		{
			ClientMessage[] clientMessages;
			if (command != null)
			{
				lock (this.clientRequestLock)
				{
					lock (this.sessionStateLock)
					{
						if (this.State == PowwaSession.SessionState.Available)
						{
							PSCommand commands = null;
							try
							{
								commands = ScriptBlock.Create(command).GetPowerShell(new object[0]).Commands;
							}
							catch (ScriptBlockToPowerShellNotSupportedException scriptBlockToPowerShellNotSupportedException)
							{

							}
							catch (RuntimeException runtimeException)
							{

							}
							if (commands == null)
							{
								commands = new PSCommand();
								commands.AddScript(command);
							}
							commands.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
							Command outputCmd = new Command("Out-Default", false, false);
							commands.AddCommand (outputCmd);
							this.executingCommandPowerShell.Commands = commands;
							this.executingCommandPowerShell.HistoryString = command;

							try
							{
								PSInvocationSettings pSInvocationSetting = new PSInvocationSettings();
								pSInvocationSetting.AddToHistory = true;
								/*Exception ex;
								this.State = PowwaSession.SessionState.ExecutingCommand;
								this.exec.ExecuteCommandAsync(command, out ex, Microsoft.PowerShell.Executor.ExecutionOptions.AddOutputter | Microsoft.PowerShell.Executor.ExecutionOptions.AddToHistory);
								if (ex != null) throw ex;
								*/
								this.executingCommandPowerShell.BeginInvoke<object>(null, pSInvocationSetting, null, null);
							}
							catch (Exception exception1)
							{
								Exception exception = exception1;
								this.WriteException(exception);
								if (exception as InvalidRunspaceStateException != null)
								{
									this.PostClientMessage(new SessionTerminatedMessage(), false);
								}
							}
						}
						else
						{
							PowwaEvents.PowwaEVENT_DEBUG_LOG1("ExecuteCommand(): Invalid Session State", "SessionState", this.State.ToString());
							throw new InvalidOperationException("The session is not available");
						}
					}
					clientMessages = this.GetClientMessages();
				}
				return clientMessages;
			}
			else
			{
				throw new ArgumentNullException("command");
			}
		}

		public ClientConfiguration GetClientConfiguration()
		{
			ClientConfiguration clientConfiguration;
			string computerName;
			PowwaEvents.PowwaEVENT_DEBUG_LOG0("GetClientConfiguration(): Enter");
			try
			{
				lock (this.clientRequestLock)
				{
					if (this.State == PowwaSession.SessionState.Available)
					{
						ClientConfiguration bufferSize = new ClientConfiguration();
						bufferSize.BufferSize = this.host.UI.RawUI.BufferSize;
						bufferSize.WindowSize = this.host.UI.RawUI.WindowSize;
						bufferSize.ForegroundColor = HtmlHelper.ToHtmlColor(this.host.UI.RawUI.ForegroundColor);
						bufferSize.BackgroundColor = HtmlHelper.ToHtmlColor(this.host.UI.RawUI.BackgroundColor);
						bufferSize.WindowTitle = this.host.UI.RawUI.WindowTitle;
						bufferSize.Prompt = this.GetPowerShellPrompt();
						ClientConfiguration clientConfiguration1 = bufferSize;
						if (this.Runspace.ConnectionInfo != null)
						{
							computerName = this.Runspace.ConnectionInfo.ComputerName;
						}
						else
						{
							computerName = string.Empty;
						}
						clientConfiguration1.ComputerName = computerName;
						bufferSize.InputForegroundColor = HtmlHelper.ToHtmlColor(ConsoleColor.Yellow);
						bufferSize.InputBackgroundColor = HtmlHelper.ToHtmlColor(ConsoleColor.DarkBlue);
						bufferSize.ErrorForegroundColor = HtmlHelper.ToHtmlColor(ConsoleColor.Red);
						bufferSize.ErrorBackgroundColor = HtmlHelper.ToHtmlColor(ConsoleColor.Black);
						bufferSize.WarningForegroundColor = HtmlHelper.ToHtmlColor(ConsoleColor.Yellow);
						bufferSize.WarningBackgroundColor = HtmlHelper.ToHtmlColor(ConsoleColor.Black);
						bufferSize.StartupMessages = this.GetClientMessages();
						clientConfiguration = bufferSize;
					}
					else
					{
						PowwaEvents.PowwaEVENT_DEBUG_LOG1("GetClientConfiguration(): Invalid Session State", "SessionState", this.State.ToString());
						throw new InvalidOperationException("The session is not available");
					}
				}
			}
			finally
			{
				PowwaEvents.PowwaEVENT_DEBUG_LOG0("GetClientConfiguration(): Exit");
			}
			return clientConfiguration;
		}

		public ClientMessage[] GetClientMessages()
		{
			ClientMessage[] messages;
			lock (this.clientRequestLock)
			{
				bool flag = false;
				lock (this.sessionStateLock)
				{
					PowwaSession.SessionState state = this.State;
					switch (state)
					{
						case PowwaSession.SessionState.Available:
						{
							break;
						}
						case PowwaSession.SessionState.ExecutingCommand:
						case PowwaSession.SessionState.CancellingCommand:
						{
							flag = true;
							break;
						}
						case PowwaSession.SessionState.Closed:
						{
							throw PowwaException.CreateInvalidSessionException();
						}
						default:
						{
							break;
						}
					}
				}
				if (flag)
				{
					this.messageQueue.WaitForMessages();
				}
				lock (this.sessionStateLock)
				{
					if (this.State != PowwaSession.SessionState.Closed)
					{
						messages = this.messageQueue.GetMessages();
					}
					else
					{
						PowwaEvents.PowwaEVENT_DEBUG_LOG1("GetClientMessages() Invalid Session State", "SessionState", this.State.ToString());
						throw PowwaException.CreateInvalidSessionException();
					}
				}
			}
			return messages;
		}

		private string GetPowerShellPrompt()
		{
			string item;
			try
			{
				System.Management.Automation.PowerShell powerShell = System.Management.Automation.PowerShell.Create();
				using (powerShell)
				{
					powerShell.Runspace = this.Runspace;
					Collection<string> strs = powerShell.AddCommand("prompt").Invoke<string>();
					if (strs.Count == 0 || powerShell.Streams.Error.Count > 0)
					{
						item = "PS> ";
					}
					else
					{
						item = strs[0];
					}
				}
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.WriteLine ("Error getting prompt: " + ex.Message);
				System.Diagnostics.Debug.WriteLine ("\t" + ex.StackTrace);
				item = "PS> ";
			}
			return item;
		}

		public PowwaSessionStatusInfo GetSessionStatus()
		{
			PowwaSessionStatusInfo powwaSessionStatusInfo;
			PowwaEvents.PowwaEVENT_DEBUG_LOG0("GetSessionStatus(): Enter");
			try
			{
				lock (this.clientRequestLock)
				{
					lock (this.sessionStateLock)
					{
						PowwaSession.SessionState state = this.State;
						switch (state)
						{
							case PowwaSession.SessionState.Available:
							{
								powwaSessionStatusInfo = new PowwaSessionStatusInfo(PowwaSessionStatus.Available, null);
								return powwaSessionStatusInfo;
							}
							case PowwaSession.SessionState.ExecutingCommand:
							{
								if (!this.messageQueue.WaitingForUserReply)
								{
									powwaSessionStatusInfo = new PowwaSessionStatusInfo(PowwaSessionStatus.Executing, null);
									return powwaSessionStatusInfo;
								}
								else
								{
									powwaSessionStatusInfo = new PowwaSessionStatusInfo(PowwaSessionStatus.Prompting, this.messageQueue.PendingInputMessage);
									return powwaSessionStatusInfo;
								}
							}
							case PowwaSession.SessionState.CancellingCommand:
							{
								powwaSessionStatusInfo = new PowwaSessionStatusInfo(PowwaSessionStatus.Cancelling, null);
								return powwaSessionStatusInfo;
							}
							case PowwaSession.SessionState.Closed:
							{
								powwaSessionStatusInfo = new PowwaSessionStatusInfo(PowwaSessionStatus.Closed, null);
								return powwaSessionStatusInfo;
							}
						}
						throw new InvalidOperationException();
					}
				}
			}
			finally
			{
				PowwaEvents.PowwaEVENT_DEBUG_LOG0("GetSessionStatus(): Exit");
			}
			return powwaSessionStatusInfo;
		}

		public string[] GetTabCompletion(string commandLine)
		{
			string[] strArrays;
			if (commandLine != null)
			{
				lock (this.clientRequestLock)
				{
					if (this.State == PowwaSession.SessionState.Available)
					{
						this.State = PowwaSession.SessionState.ExecutingCommand;
						System.Management.Automation.PowerShell powerShell = null;
						try
						{
							try
							{
								powerShell = System.Management.Automation.PowerShell.Create();
								powerShell.Runspace = this.Runspace;
								CommandCompletion commandCompletion = CommandCompletion.CompleteInput(commandLine, commandLine.Length, null, powerShell);
								string str = commandLine.Substring(0, commandCompletion.ReplacementIndex);
								string[] strArrays1 = new string[commandCompletion.CompletionMatches.Count];
								for (int i = 0; i < commandCompletion.CompletionMatches.Count; i++)
								{
									strArrays1[i] = string.Concat(str, commandCompletion.CompletionMatches[i].CompletionText);
								}
								strArrays = strArrays1;
							}
							catch
							{
								strArrays = new string[0];
							}
						}
						finally
						{
							if (powerShell != null)
							{
								powerShell.Dispose();
							}
							this.State = PowwaSession.SessionState.Available;
						}
					}
					else
					{
						PowwaEvents.PowwaEVENT_DEBUG_LOG1("GetTabCompletion(): Invalid Session State", "SessionState", this.State.ToString());
						throw new InvalidOperationException("The session is not available");
					}
				}
				return strArrays;
			}
			else
			{
				throw new ArgumentNullException("commandLine");
			}
		}

		private void OnClientMessageCreated(MessageCreatedEventArgs e)
		{
			e.Reply = this.PostClientMessage(e.Message, e.IsInputMessage);
		}

		private void OnExecutingCommandInvocationStateChanged(object sender, PSInvocationStateChangedEventArgs e)
		{
			bool flag = false;
			lock (this.sessionStateLock)
			{
				if (this.State != PowwaSession.SessionState.Closed)
				{
					PSInvocationState state = e.InvocationStateInfo.State;
					if (state == PSInvocationState.Completed || state == PSInvocationState.Failed || state == PSInvocationState.Stopped)
					{
						if (state == PSInvocationState.Failed)
						{
							this.WriteException(e.InvocationStateInfo.Reason);
						}
						flag = true;
						if (state == PSInvocationState.Completed)
						{
							//Ouput Stuff...

						}
					}
				}
				else
				{
					return;
				}
			}
			if (flag)
			{
				string powerShellPrompt = this.GetPowerShellPrompt();
				lock (this.sessionStateLock)
				{
					if (this.State != PowwaSession.SessionState.Closed)
					{
						this.PostClientMessage(new CommandCompletedMessage(powerShellPrompt), false);
						this.State = PowwaSession.SessionState.Available;
					}
				}
			}
		}

		private void OnRunspaceStateChanged(object sender, RunspaceStateEventArgs e)
		{
			lock (this.sessionStateLock)
			{
				if (e.RunspaceStateInfo.State == RunspaceState.Broken)
				{
					PowwaEvents.PowwaEVENT_PSREXECUTION_FAILURE(this.Name, e.RunspaceStateInfo.Reason.Message);
					this.PostClientMessage(new SessionTerminatedMessage(), false);
				}
			}
		}

		private object PostClientMessage(ClientMessage message, bool isInputMessage)
		{
			object userReply;
			lock (this.sessionStateLock)
			{
				this.messageQueue.Post(message, isInputMessage, false, this.host.UI.RawUI.BufferSize);
			}
			if (!isInputMessage)
			{
				return null;
			}
			else
			{
				this.messageQueue.WaitForUserReply();
				lock (this.sessionStateLock)
				{
					userReply = this.messageQueue.GetUserReply();
				}
				return userReply;
			}
		}

		private static object PromptReplyObjectToObject(object reply, PromptFieldDescription description)
		{
			PromptFieldType promptFieldType = description.PromptFieldType;
			switch (promptFieldType)
			{
				case PromptFieldType.String:
				{
					if (reply as string != null)
					{
						return reply;
					}
					else
					{
						throw new ArgumentException("Expected a string", "reply");
					}
				}
				case PromptFieldType.SecureString:
				{
					object[] objArray = reply as object[];
					if (objArray != null)
					{
						return PowwaSession.ToSecureString(objArray);
					}
					else
					{
						throw new ArgumentException("Expected an object[]", "reply");
					}
				}
				case PromptFieldType.Credential:
				{
					Dictionary<string, object> strs = reply as Dictionary<string, object>;
					if (strs == null || !strs.ContainsKey("username") || !strs.ContainsKey("password"))
					{
						throw new ArgumentException("Expected an object with username and password properties", "reply");
					}
					else
					{
						string item = strs["username"] as string;
						if (item != null)
						{
							object[] item1 = strs["password"] as object[];
							if (item1 != null)
							{
								return new PSCredential(item, PowwaSession.ToSecureString(item1));
							}
							else
							{
								throw new ArgumentException("The password should be an object[]", "reply");
							}
						}
						else
						{
							throw new ArgumentException("The username should be a string", "reply");
						}
					}
				}
			}
			throw new ArgumentException("Unknown reply type", "reply");
		}

		private static PSObject PromptReplyObjectToPsObject(object reply, PromptFieldDescription description)
		{
			if (description.PromptFieldTypeIsList)
			{
				object[] objArray = reply as object[];
				if (objArray != null)
				{
					ArrayList arrayLists = new ArrayList();
					object[] objArray1 = objArray;
					for (int i = 0; i < (int)objArray1.Length; i++)
					{
						object obj = objArray1[i];
						arrayLists.Add(PowwaSession.PromptReplyObjectToObject(obj, description));
					}
					return PSObject.AsPSObject(arrayLists);
				}
				else
				{
					throw new ArgumentException("Expected multiple values as an object[]", "reply");
				}
			}
			else
			{
				return PSObject.AsPSObject(PowwaSession.PromptReplyObjectToObject(reply, description));
			}
		}

		public ClientMessage[] SetPromptForChoiceReply(int reply)
		{
			ClientMessage[] clientMessages;
			PowwaEvents.PowwaEVENT_DEBUG_LOG1("SetPromptForChoiceReply(): Enter", "reply", reply.ToString(CultureInfo.InvariantCulture));
			try
			{
				lock (this.clientRequestLock)
				{
					lock (this.sessionStateLock)
					{
						PromptForChoiceMessage promptForChoiceMessage = this.ValidateSessionStateForMessageReply<PromptForChoiceMessage>();
						if (reply < 0 || reply >= promptForChoiceMessage.Choices.Count)
						{
							int count = promptForChoiceMessage.Choices.Count;
							PowwaEvents.PowwaEVENT_DEBUG_LOG2("SetPromptForChoiceReply(): Choice not within the valid range", "reply", reply.ToString(CultureInfo.InvariantCulture), "message.Choices.Count", count.ToString(CultureInfo.InvariantCulture));
							throw new ArgumentException("The choice is not within the valid range", "reply");
						}
						else
						{
							this.messageQueue.SetInputMessageReply(reply);
						}
					}
					clientMessages = this.GetClientMessages();
				}
			}
			finally
			{
				PowwaEvents.PowwaEVENT_DEBUG_LOG0("SetPromptForChoiceReply(): Exit");
			}
			return clientMessages;
		}

		public ClientMessage[] SetPromptForCredentialReply(string userName, char[] password)
		{
			ClientMessage[] clientMessages;
			PowwaEvents.PowwaEVENT_DEBUG_LOG0("SetPromptForCredentialReply(): Enter");
			try
			{
				lock (this.clientRequestLock)
				{
					lock (this.sessionStateLock)
					{
						PromptForCredentialMessage promptForCredentialMessage = this.ValidateSessionStateForMessageReply<PromptForCredentialMessage>();
						if (promptForCredentialMessage.DomainCredentials)
						{
							StringBuilder stringBuilder = new StringBuilder(0x202);
							StringBuilder stringBuilder1 = new StringBuilder(0x152);
							uint num = PowwaSession.CredUIParseUserName(userName, stringBuilder, stringBuilder.Capacity, stringBuilder1, stringBuilder1.Capacity);
							if (num != 0)
							{
								PowwaEvents.PowwaEVENT_DEBUG_LOG2("SetPromptForCredentialReply(): Invalid UserName", "userName", userName, "errorCode", num.ToString(CultureInfo.InvariantCulture));
								throw PowwaException.CreateValidationErrorException(Resources.InvalidUserNameInDomainCredentials);
							}
						}
						PSCredential pSCredential = new PSCredential(userName, PowwaSession.ToSecureString(password));
						this.messageQueue.SetInputMessageReply(pSCredential);
					}
					clientMessages = this.GetClientMessages();
				}
			}
			finally
			{
				PowwaEvents.PowwaEVENT_DEBUG_LOG0("SetPromptForCredentialReply(): Exit");
			}
			return clientMessages;
		}

		public ClientMessage[] SetPromptReply(object[] reply)
		{
			ClientMessage[] clientMessages;
			PowwaEvents.PowwaEVENT_DEBUG_LOG0("SetPromptReply(): Enter");
			try
			{
				lock (this.clientRequestLock)
				{
					lock (this.sessionStateLock)
					{
						PromptMessage promptMessage = this.ValidateSessionStateForMessageReply<PromptMessage>();
						if ((int)reply.Length == (int)promptMessage.Descriptions.Length)
						{
							Dictionary<string, PSObject> strs = new Dictionary<string, PSObject>();
							for (int i = 0; i < (int)reply.Length; i++)
							{
								strs.Add(promptMessage.Descriptions[i].Name, PowwaSession.PromptReplyObjectToPsObject(reply[i], promptMessage.Descriptions[i]));
							}
							this.messageQueue.SetInputMessageReply(strs);
						}
						else
						{
							int length = (int)reply.Length;
							int num = (int)promptMessage.Descriptions.Length;
							PowwaEvents.PowwaEVENT_DEBUG_LOG2("SetPromptReply(): Number of items in the reply does not match prompt message", "reply.Length", length.ToString(CultureInfo.InvariantCulture), "message.Descriptions.Length", num.ToString(CultureInfo.InvariantCulture));
							throw new ArgumentException("The number of items in the reply does not match the prompt message", "reply");
						}
					}
					clientMessages = this.GetClientMessages();
				}
			}
			finally
			{
				PowwaEvents.PowwaEVENT_DEBUG_LOG0("SetPromptReply(): Exit");
			}
			return clientMessages;
		}

		public ClientMessage[] SetReadLineAsSecureStringReply(char[] reply)
		{
			ClientMessage[] clientMessages;
			PowwaEvents.PowwaEVENT_DEBUG_LOG0("SetReadLineAsSecureStringReply(): Enter");
			try
			{
				lock (this.clientRequestLock)
				{
					lock (this.sessionStateLock)
					{
						this.ValidateSessionStateForMessageReply<ReadLineAsSecureStringMessage>();
						SecureString secureString = PowwaSession.ToSecureString(reply);
						this.messageQueue.SetInputMessageReply(secureString);
					}
					clientMessages = this.GetClientMessages();
				}
			}
			finally
			{
				PowwaEvents.PowwaEVENT_DEBUG_LOG0("SetReadLineAsSecureStringReply(): Exit");
			}
			return clientMessages;
		}

		public ClientMessage[] SetReadLineReply(string reply)
		{
			ClientMessage[] clientMessages;
			PowwaEvents.PowwaEVENT_DEBUG_LOG0("SetReadLineReply(): Enter");
			try
			{
				lock (this.clientRequestLock)
				{
					lock (this.sessionStateLock)
					{
						this.ValidateSessionStateForMessageReply<ReadLineMessage>();
						this.messageQueue.SetInputMessageReply(reply);
					}
					clientMessages = this.GetClientMessages();
				}
			}
			finally
			{
				PowwaEvents.PowwaEVENT_DEBUG_LOG0("SetReadLineReply(): Exit");
			}
			return clientMessages;
		}

		private static SecureString ToSecureString(char[] array)
		{
			SecureString secureString = new SecureString();
			for (int i = 0; i < (int)array.Length; i++)
			{
				secureString.AppendChar(array[i]);
				array[i] = '\0';
			}
			return secureString;
		}

		private static SecureString ToSecureString(object[] array)
		{
			char[] chr = new char[(int)array.Length];
			for (int i = 0; i < (int)array.Length; i++)
			{
				chr[i] = Convert.ToChar(array[i], CultureInfo.InvariantCulture);
				array[i] = (char)0;
			}
			return PowwaSession.ToSecureString(chr);
		}

		private T ValidateSessionStateForMessageReply<T>()
		where T : ClientMessage
		{
			/* this.State != PowwaSession.SessionState.ExecutingCommand || */ /* TODO: The command is reset to Available... */
			if (!this.WaitingForUserReply)
			{
				PowwaEvents.PowwaEVENT_DEBUG_LOG1("ValidateSessionStateForMessageReply(): Session not waiting for reply", "SessionState", this.State.ToString());
				throw new InvalidOperationException("The session is not waiting for a reply");
			}
			else
			{
				T pendingInputMessage = (T)(this.messageQueue.PendingInputMessage as T);
				if (pendingInputMessage != null)
				{
					return pendingInputMessage;
				}
				else
				{
					throw new InvalidOperationException("Invalid reply type");
				}
			}
		}

		private void WriteErrorLine(string line)
		{
			this.messageQueue.Post(new WriteLineMessage(ConsoleColor.Red, ConsoleColor.Black, line), false, true, this.host.UI.RawUI.BufferSize);
		}

		private void WriteException(Exception exception)
		{
			string str;
			PSRemotingTransportException pSRemotingTransportException = exception as PSRemotingTransportException;
			if (pSRemotingTransportException == null || pSRemotingTransportException.InnerException as TargetInvocationException == null || pSRemotingTransportException.InnerException.InnerException == null)
			{
				IContainsErrorRecord containsErrorRecord = exception as IContainsErrorRecord;
				if (containsErrorRecord != null)
				{
					ErrorRecord errorRecord = containsErrorRecord.ErrorRecord;
					if (errorRecord.InvocationInfo == null || errorRecord.InvocationInfo.PositionMessage == null)
					{
						str = errorRecord.ToString();
					}
					else
					{
						str = errorRecord.InvocationInfo.PositionMessage;
					}
					string str1 = str;
					char[] chrArray = new char[2];
					chrArray[0] = '\n';
					chrArray[1] = '\r';
					str1 = str1.TrimEnd(chrArray);
					this.WriteErrorLine(str1);
					object[] categoryInfo = new object[1];
					categoryInfo[0] = errorRecord.CategoryInfo;
					this.WriteErrorLine(string.Format(CultureInfo.CurrentCulture, "    + CategoryInfo          : {0} ", categoryInfo));
					object[] fullyQualifiedErrorId = new object[1];
					fullyQualifiedErrorId[0] = errorRecord.FullyQualifiedErrorId;
					this.WriteErrorLine(string.Format(CultureInfo.CurrentCulture, "    + FullyQualifiedErrorId : {0} ", fullyQualifiedErrorId));
					this.WriteErrorLine("");
					return;
				}
			}
			else
			{
				exception = pSRemotingTransportException.InnerException;
			}
			if (exception as TargetInvocationException == null)
			{
				this.host.UI.WriteErrorLine(exception.Message);
				return;
			}
			else
			{
				this.host.UI.WriteErrorLine(exception.InnerException.Message);
				return;
			}
		}

		internal enum SessionState
		{
			Available,
			ExecutingCommand,
			CancellingCommand,
			Closed
		}

		private class TypeTableLoader
		{
			private static volatile PowwaSession.TypeTableLoader instance;

			private static object lockObject;

			public static PowwaSession.TypeTableLoader Instance
			{
				get
				{
					if (PowwaSession.TypeTableLoader.instance == null)
					{
						lock (PowwaSession.TypeTableLoader.lockObject)
						{
							if (PowwaSession.TypeTableLoader.instance == null)
							{
								PowwaSession.TypeTableLoader.instance = new PowwaSession.TypeTableLoader();
							}
						}
					}
					return PowwaSession.TypeTableLoader.instance;
				}
			}

			static TypeTableLoader()
			{
				PowwaSession.TypeTableLoader.lockObject = new object();
			}

			private TypeTableLoader()
			{
				RunspaceFactory.CreateRunspace().Close();
			}

			public TypeTable LoadDefaultTypeFiles()
			{
				return TypeTable.LoadDefaultTypeFiles();
			}
		}
	}
}