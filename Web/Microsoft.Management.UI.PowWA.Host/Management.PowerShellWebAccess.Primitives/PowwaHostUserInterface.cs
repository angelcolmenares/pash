using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	internal class PowwaHostUserInterface : PSHostUserInterface, IMessageCreated
	{
		internal const ConsoleColor ErrorForegroundColor = ConsoleColor.Red;

		internal const ConsoleColor ErrorBackgroundColor = ConsoleColor.Black;

		internal const ConsoleColor WarningForegroundColor = ConsoleColor.Yellow;

		internal const ConsoleColor WarningBackgroundColor = ConsoleColor.Black;

		internal const ConsoleColor DebugForegroundColor = ConsoleColor.Yellow;

		internal const ConsoleColor DebugBackgroundColor = ConsoleColor.Black;

		internal const ConsoleColor VerboseForegroundColor = ConsoleColor.Yellow;

		internal const ConsoleColor VerboseBackgroundColor = ConsoleColor.Black;

		internal const ConsoleColor InputBackgroundColor = ConsoleColor.DarkBlue;

		internal const ConsoleColor InputForegroundColor = ConsoleColor.Yellow;

		private readonly PowwaHostRawUserInterface rawUi;

		public override PSHostRawUserInterface RawUI
		{
			get
			{
				return this.rawUi;
			}
		}

		public PowwaHostUserInterface(ClientInfo clientInfo)
		{
			this.rawUi = new PowwaHostRawUserInterface(clientInfo);
		}

		private static Type GetFieldType(FieldDescription description, out bool isList)
		{
			Type elementType;
			Type type = null;
			if (!LanguagePrimitives.TryConvertTo<Type>(description.ParameterAssemblyFullName, out type))
			{
				if (description.ParameterTypeName.Equals(typeof(PSCredential).Name, StringComparison.OrdinalIgnoreCase) || description.ParameterTypeName.Equals(typeof(SecureString).Name, StringComparison.OrdinalIgnoreCase))
				{
					object[] parameterTypeFullName = new object[1];
					parameterTypeFullName[0] = description.ParameterTypeFullName;
					string str = string.Format(CultureInfo.CurrentCulture, Resources.PromptTypeConversionError_Format, parameterTypeFullName);
					throw new PromptingException(str);
				}
				else
				{
					isList = false;
					return typeof(string);
				}
			}
			else
			{
				isList = type.GetInterface(typeof(IList).FullName) != null;
				if (isList)
				{
					if (type.IsArray)
					{
						elementType = type.GetElementType();
					}
					else
					{
						elementType = typeof(object);
					}
					type = elementType;
				}
				return type;
			}
		}

		private void OnMessageCreated(MessageCreatedEventArgs e)
		{
			EventHandler<MessageCreatedEventArgs> eventHandler = this.MessageCreated;
			if (eventHandler != null)
			{
				eventHandler(this, e);
			}
		}

		public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
		{
			bool flag = false;
			Dictionary<string, PSObject> reply;
			PowwaEvents.PowwaEVENT_DEBUG_LOG0("Prompt(): Enter");
			try
			{
				if (descriptions != null)
				{
					if (descriptions.Count != 0)
					{
						PromptFieldDescription[] promptFieldDescriptionArray = new PromptFieldDescription[descriptions.Count];
						int num = 0;
						while (num < descriptions.Count)
						{
							if (descriptions[num] != null)
							{
								Type fieldType = PowwaHostUserInterface.GetFieldType(descriptions[num], out flag);
								PromptFieldType promptFieldType = PromptFieldType.String;
								if (fieldType != typeof(SecureString))
								{
									if (fieldType == typeof(PSCredential))
									{
										promptFieldType = PromptFieldType.Credential;
									}
								}
								else
								{
									promptFieldType = PromptFieldType.SecureString;
								}
								PromptFieldDescription promptFieldDescription = new PromptFieldDescription();
								promptFieldDescription.Name = descriptions[num].Name;
								promptFieldDescription.Label = descriptions[num].Label;
								promptFieldDescription.HelpMessage = descriptions[num].HelpMessage;
								promptFieldDescription.PromptFieldType = promptFieldType;
								promptFieldDescription.PromptFieldTypeIsList = flag;
								promptFieldDescriptionArray[num] = promptFieldDescription;
								num++;
							}
							else
							{
								object[] objArray = new object[1];
								objArray[0] = num;
								string str = string.Format(CultureInfo.InvariantCulture, "descriptions[{0}]", objArray);
								PowwaEvents.PowwaEVENT_DEBUG_LOG1("Prompt(): Invalid Description", str, "null");
								throw new ArgumentNullException(str);
							}
						}
						MessageCreatedEventArgs messageCreatedEventArg = new MessageCreatedEventArgs(new PromptMessage(caption, message, promptFieldDescriptionArray), true);
						this.OnMessageCreated(messageCreatedEventArg);
						reply = (Dictionary<string, PSObject>)messageCreatedEventArg.Reply;
					}
					else
					{
						int count = descriptions.Count;
						PowwaEvents.PowwaEVENT_DEBUG_LOG1("Prompt(): Invalid Argument", "Descriptions.Count", count.ToString(CultureInfo.InvariantCulture));
						throw new ArgumentException("descriptions cannot be an empty array", "descriptions");
					}
				}
				else
				{
					PowwaEvents.PowwaEVENT_DEBUG_LOG1("Prompt(): Invalid Argument", "Descriptions", "null");
					throw new ArgumentNullException("descriptions");
				}
			}
			finally
			{
				PowwaEvents.PowwaEVENT_DEBUG_LOG0("Prompt(): Exit");
			}
			return reply;
		}

		public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
		{
			int reply;
			PowwaEvents.PowwaEVENT_DEBUG_LOG0("PromptForChoice(): Enter");
			try
			{
				MessageCreatedEventArgs messageCreatedEventArg = new MessageCreatedEventArgs(new PromptForChoiceMessage(caption, message, choices, defaultChoice), true);
				this.OnMessageCreated(messageCreatedEventArg);
				reply = (int)messageCreatedEventArg.Reply;
			}
			finally
			{
				PowwaEvents.PowwaEVENT_DEBUG_LOG0("PromptForChoice(): Exit");
			}
			return reply;
		}

		public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
		{
			return this.PromptForCredential(caption, message, userName, targetName, PSCredentialTypes.Default, PSCredentialUIOptions.Default);
		}

		public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
		{
			PSCredential reply;
			string str;
			PowwaEvents.PowwaEVENT_DEBUG_LOG0("PromptForCredential(): Enter");
			try
			{
				if (string.IsNullOrEmpty(targetName))
				{
					bool flag = (allowedCredentialTypes & PSCredentialTypes.Domain) == PSCredentialTypes.Domain;
					string str1 = "PromptForCredential()";
					string str2 = "domainCredentials";
					if (flag)
					{
						str = "true";
					}
					else
					{
						str = "false";
					}
					PowwaEvents.PowwaEVENT_DEBUG_LOG1(str1, str2, str);
					MessageCreatedEventArgs messageCreatedEventArg = new MessageCreatedEventArgs(new PromptForCredentialMessage(caption, message, userName, flag), true);
					this.OnMessageCreated(messageCreatedEventArg);
					reply = (PSCredential)messageCreatedEventArg.Reply;
				}
				else
				{
					PowwaEvents.PowwaEVENT_DEBUG_LOG1("PromptForCredential(): Invalid Argument", "targetName", "null");
					throw new NotSupportedException(Resources.PromptForCredentialTargetNameNotSupported);
				}
			}
			finally
			{
				PowwaEvents.PowwaEVENT_DEBUG_LOG0("PromptForCredential(): Exit");
			}
			return reply;
		}

		public override string ReadLine()
		{
			MessageCreatedEventArgs messageCreatedEventArg = new MessageCreatedEventArgs(new ReadLineMessage(), true);
			this.OnMessageCreated(messageCreatedEventArg);
			return (string)messageCreatedEventArg.Reply;
		}

		public override SecureString ReadLineAsSecureString()
		{
			MessageCreatedEventArgs messageCreatedEventArg = new MessageCreatedEventArgs(new ReadLineAsSecureStringMessage(), true);
			this.OnMessageCreated(messageCreatedEventArg);
			return (SecureString)messageCreatedEventArg.Reply;
		}

		public override void Write(string value)
		{
			this.OnMessageCreated(new MessageCreatedEventArgs(new WriteMessage(this.RawUI.ForegroundColor, this.RawUI.BackgroundColor, value), false));
		}

		public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			this.OnMessageCreated(new MessageCreatedEventArgs(new WriteMessage(foregroundColor, backgroundColor, value), false));
		}

		public override void WriteDebugLine(string message)
		{
			object[] objArray = new object[1];
			objArray[0] = message;
			this.OnMessageCreated(new MessageCreatedEventArgs(new WriteLineMessage(ConsoleColor.Yellow, ConsoleColor.Black, string.Format(CultureInfo.CurrentCulture, Resources.DebugLineFormatString, objArray)), false));
		}

		public override void WriteErrorLine(string value)
		{
			this.OnMessageCreated(new MessageCreatedEventArgs(new WriteLineMessage(ConsoleColor.Red, ConsoleColor.Black, value), false));
		}

		public override void WriteLine(string value)
		{
			this.OnMessageCreated(new MessageCreatedEventArgs(new WriteLineMessage(this.RawUI.ForegroundColor, this.RawUI.BackgroundColor, value), false));
		}

		public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			this.OnMessageCreated(new MessageCreatedEventArgs(new WriteLineMessage(foregroundColor, backgroundColor, value), false));
		}

		public override void WriteProgress(long sourceId, ProgressRecord record)
		{
			this.OnMessageCreated(new MessageCreatedEventArgs(new WriteProgressMessage(sourceId, record), false));
		}

		public override void WriteVerboseLine(string message)
		{
			object[] objArray = new object[1];
			objArray[0] = message;
			this.OnMessageCreated(new MessageCreatedEventArgs(new WriteLineMessage(ConsoleColor.Yellow, ConsoleColor.Black, string.Format(CultureInfo.CurrentCulture, Resources.VerboseLineFormatString, objArray)), false));
		}

		public override void WriteWarningLine(string message)
		{
			object[] objArray = new object[1];
			objArray[0] = message;
			this.OnMessageCreated(new MessageCreatedEventArgs(new WriteLineMessage(ConsoleColor.Yellow, ConsoleColor.Black, string.Format(CultureInfo.CurrentCulture, Resources.WarningLineFormatString, objArray)), false));
		}

		public event EventHandler<MessageCreatedEventArgs> MessageCreated;
	}
}