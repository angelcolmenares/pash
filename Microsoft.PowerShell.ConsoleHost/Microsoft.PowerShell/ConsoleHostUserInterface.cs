using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Management.Automation.Internal.Host;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Text;

namespace Microsoft.PowerShell
{
	internal class ConsoleHostUserInterface : PSHostUserInterface, IHostUISupportsMultipleChoiceSelection
	{
		internal const string Crlf = "\r\n";

		private const string Tab = "\t";

		private const int maxInputLineLength = 0x2000;

		private const string CustomReadlineCommand = "PSConsoleHostReadLine";

		private const string resBaseName = "ConsoleHostUserInterfaceStrings";

		private const string PromptCommandPrefix = "!";

		private const string PromptEmptyDescriptionsErrorTemplateResource = "PromptEmptyDescriptionsErrorTemplate";

		private const string NullOrEmptyErrorTemplateResource = "NullOrEmptyErrorTemplate";

		private const string NullErrorTemplateResource = "NullErrorTemplate";

		private const string InvalidChoiceHotKeyErrorResource = "InvalidChoiceHotKeyError";

		private const string EmptyChoicesErrorTemplateResource = "EmptyChoicesErrorTemplate";

		private const string InvalidDefaultChoiceErrorTemplateResource = "InvalidDefaultChoiceErrorTemplate";

		private System.Management.Automation.PowerShell commandCompletionPowerShell;

		private ConsoleColor errorForegroundColor;

		private ConsoleColor errorBackgroundColor;

		private ConsoleColor warningForegroundColor;

		private ConsoleColor warningBackgroundColor;

		private ConsoleColor debugForegroundColor;

		private ConsoleColor debugBackgroundColor;

		private ConsoleColor verboseForegroundColor;

		private ConsoleColor verboseBackgroundColor;

		private ConsoleColor progressForegroundColor;

		private ConsoleColor progressBackgroundColor;

		private object instanceLock;

		private bool readFromStdin;

		private bool noPrompt;

		private bool throwOnReadAndPrompt;

		private bool isInteractiveTestToolListening;

		private bool isTestingShiftTab;

		private static string debugFormatString;

		private static string verboseFormatString;

		private static string warningFormatString;

		private ConsoleHostRawUserInterface rawui;

		private ConsoleHost parent;

		[TraceSource("ConsoleHostUserInterface", "Console host's subclass of S.M.A.Host.Console")]
		private static PSTraceSource tracer;

		private ProgressPane progPane;

		private PendingProgress pendingProgress;

		public ConsoleColor DebugBackgroundColor
		{
			get
			{
				return this.debugBackgroundColor;
			}
			set
			{
				this.debugBackgroundColor = value;
			}
		}

		public ConsoleColor DebugForegroundColor
		{
			get
			{
				return this.debugForegroundColor;
			}
			set
			{
				this.debugForegroundColor = value;
			}
		}

		private ConsoleColor DefaultPromptColor
		{
			get
			{
				if (this.PromptColor != ConsoleColor.White)
				{
					return ConsoleColor.Blue;
				}
				else
				{
					return ConsoleColor.Yellow;
				}
			}
		}

		public ConsoleColor ErrorBackgroundColor
		{
			get
			{
				return this.errorBackgroundColor;
			}
			set
			{
				this.errorBackgroundColor = value;
			}
		}

		public ConsoleColor ErrorForegroundColor
		{
			get
			{
				return this.errorForegroundColor;
			}
			set
			{
				this.errorForegroundColor = value;
			}
		}

		internal bool IsCommandCompletionRunning
		{
			get
			{
				if (this.commandCompletionPowerShell == null)
				{
					return false;
				}
				else
				{
					return this.commandCompletionPowerShell.InvocationStateInfo.State == PSInvocationState.Running;
				}
			}
		}

		internal bool NoPrompt
		{
			get
			{
				return this.noPrompt;
			}
			set
			{
				this.noPrompt = value;
			}
		}

		public void SetPrompt (string str)
		{
			ConsoleControl.NativeMethods.SetPrompt (str);
		}

		public ConsoleColor ProgressBackgroundColor
		{
			get
			{
				return this.progressBackgroundColor;
			}
			set
			{
				this.progressBackgroundColor = value;
			}
		}

		public ConsoleColor ProgressForegroundColor
		{
			get
			{
				return this.progressForegroundColor;
			}
			set
			{
				this.progressForegroundColor = value;
			}
		}

		private ConsoleColor PromptColor
		{
			get
			{
				ConsoleColor backgroundColor = this.RawUI.BackgroundColor;
				switch (backgroundColor)
				{
					case ConsoleColor.DarkYellow:
					{
						return ConsoleColor.Black;
					}
					case ConsoleColor.Gray:
					{
						return ConsoleColor.Black;
					}
					case ConsoleColor.DarkGray:
					case ConsoleColor.Blue:
					case ConsoleColor.Red:
					case ConsoleColor.Magenta:
					{
						return ConsoleColor.White;
					}
					case ConsoleColor.Green:
					{
						return ConsoleColor.Black;
					}
					case ConsoleColor.Cyan:
					{
						return ConsoleColor.Black;
					}
					case ConsoleColor.Yellow:
					{
						return ConsoleColor.Black;
					}
					case ConsoleColor.White:
					{
						return ConsoleColor.Black;
					}
					default:
					{
						return ConsoleColor.White;
					}
				}
			}
		}

		public override PSHostRawUserInterface RawUI
		{
			get
			{
				return this.rawui;
			}
		}

		internal bool ReadFromStdin
		{
			get
			{
				return this.readFromStdin;
			}
			set
			{
				this.readFromStdin = value;
			}
		}

		internal bool ThrowOnReadAndPrompt
		{
			set
			{
				this.throwOnReadAndPrompt = value;
			}
		}

		public ConsoleColor VerboseBackgroundColor
		{
			get
			{
				return this.verboseBackgroundColor;
			}
			set
			{
				this.verboseBackgroundColor = value;
			}
		}

		public ConsoleColor VerboseForegroundColor
		{
			get
			{
				return this.verboseForegroundColor;
			}
			set
			{
				this.verboseForegroundColor = value;
			}
		}

		public ConsoleColor WarningBackgroundColor
		{
			get
			{
				return this.warningBackgroundColor;
			}
			set
			{
				this.warningBackgroundColor = value;
			}
		}

		public ConsoleColor WarningForegroundColor
		{
			get
			{
				return this.warningForegroundColor;
			}
			set
			{
				this.warningForegroundColor = value;
			}
		}

		static ConsoleHostUserInterface()
		{
			ConsoleHostUserInterface.tracer = PSTraceSource.GetTracer("ConsoleHostUserInterface", "Console host's subclass of S.M.A.Host.Console");
		}

		internal ConsoleHostUserInterface(ConsoleHost parent)
		{
			this.errorForegroundColor = ConsoleColor.Red;
			this.warningForegroundColor = ConsoleColor.Yellow;
			this.debugForegroundColor = ConsoleColor.Yellow;
			this.verboseForegroundColor = ConsoleColor.Yellow;
			this.progressForegroundColor = ConsoleColor.Yellow;
			this.progressBackgroundColor = ConsoleColor.DarkCyan;
			this.instanceLock = new object();
			this.parent = parent;
			this.rawui = new ConsoleHostRawUserInterface(this);
			ConsoleHostUserInterface.debugFormatString = ConsoleHostUserInterfaceStrings.DebugFormatString;
			ConsoleHostUserInterface.verboseFormatString = ConsoleHostUserInterfaceStrings.VerboseFormatString;
			ConsoleHostUserInterface.warningFormatString = ConsoleHostUserInterfaceStrings.WarningFormatString;
			this.isInteractiveTestToolListening = false;
			this.isTestingShiftTab = false;
			lastForegroundColor = this.rawui.ForegroundColor;
			lastBackgroundColor = this.RawUI.BackgroundColor;
		}

		internal void AddWord(string text, int startIndex, int endIndex, int maxWidthInBufferCells, bool isWhitespace, ref List<ConsoleHostUserInterface.Word> result)
		{
			while (startIndex < endIndex)
			{
				int num = Math.Min(endIndex, startIndex + maxWidthInBufferCells);
				ConsoleHostUserInterface.Word word = new ConsoleHostUserInterface.Word();
				if (isWhitespace)
				{
					word.Flags = ConsoleHostUserInterface.WordFlags.IsWhitespace;
				}
				while (true)
				{
					word.Text = text.Substring(startIndex, num - startIndex);
					word.CellCount = this.RawUI.LengthInBufferCells(word.Text);
					if (word.CellCount <= maxWidthInBufferCells)
					{
						break;
					}
					num--;
				}
				result.Add(word);
				startIndex = num;
			}
		}

		private static bool AtLeastOneHelpMessageIsPresent(Collection<FieldDescription> descriptions)
		{
			bool flag;
			IEnumerator<FieldDescription> enumerator = descriptions.GetEnumerator();
			using (enumerator)
			{
				while (enumerator.MoveNext())
				{
					FieldDescription current = enumerator.Current;
					if (current == null || string.IsNullOrEmpty(current.HelpMessage))
					{
						continue;
					}
					flag = true;
					return flag;
				}
				return false;
			}
			return flag;
		}

		private void BlankAtCursor(Coordinates cursorPosition)
		{
			this.rawui.CursorPosition = cursorPosition;
			this.WriteToConsole(" ", true);
			this.rawui.CursorPosition = cursorPosition;
		}

		internal List<ConsoleHostUserInterface.Word> ChopTextIntoWords(string text, int maxWidthInBufferCells)
		{
			List<ConsoleHostUserInterface.Word> words = new List<ConsoleHostUserInterface.Word>();
			if (!string.IsNullOrEmpty(text))
			{
				if (maxWidthInBufferCells >= 1)
				{
					text = text.Replace('\t', ' ');
					words = new List<ConsoleHostUserInterface.Word>();
					int num = 0;
					int num1 = 0;
					bool flag = false;
					while (num1 < text.Length)
					{
						if (text[num1] != '\n')
						{
							if (text[num1] != ' ')
							{
								if (flag)
								{
									this.AddWord(text, num, num1, maxWidthInBufferCells, flag, ref words);
									num = num1;
								}
								flag = false;
							}
							else
							{
								if (!flag)
								{
									this.AddWord(text, num, num1, maxWidthInBufferCells, flag, ref words);
									num = num1;
								}
								flag = true;
							}
							num1++;
						}
						else
						{
							if (num < num1)
							{
								this.AddWord(text, num, num1, maxWidthInBufferCells, flag, ref words);
							}
							ConsoleHostUserInterface.Word word = new ConsoleHostUserInterface.Word();
							word.Flags = ConsoleHostUserInterface.WordFlags.IsNewline;
							words.Add(word);
							num1++;
							num = num1;
							flag = false;
						}
					}
					if (num != num1)
					{
						this.AddWord(text, num, text.Length, maxWidthInBufferCells, flag, ref words);
					}
					return words;
				}
				else
				{
					return words;
				}
			}
			else
			{
				return words;
			}
		}

		private char GetCharacterUnderCursor(Coordinates cursorPosition)
		{
			Size bufferSize = this.RawUI.BufferSize;
			Rectangle rectangle = new Rectangle(0, cursorPosition.Y, bufferSize.Width - 1, cursorPosition.Y);
			BufferCell[,] bufferContents = this.RawUI.GetBufferContents(rectangle);
			int num = 0;
			int num1 = 0;
			while (num1 <= cursorPosition.X)
			{
				BufferCell bufferCell = bufferContents[0, num];
				if (bufferCell.BufferCellType == BufferCellType.Complete || bufferCell.BufferCellType == BufferCellType.Leading)
				{
					if (num1 != cursorPosition.X)
					{
						num1 = num1 + ConsoleControl.LengthInBufferCells(bufferCell.Character);
					}
					else
					{
						return bufferCell.Character;
					}
				}
				num++;
			}
			return Convert.ToChar(0);
		}

		private IntPtr GetMainWindowHandle()
		{
			Process currentProcess = Process.GetCurrentProcess();
            IntPtr i = IntPtr.Zero;
			for (i = currentProcess.MainWindowHandle; i == IntPtr.Zero && currentProcess != null; i = currentProcess.MainWindowHandle)
			{
				currentProcess = PsUtils.GetParentProcess(currentProcess);
				if (currentProcess == null)
				{
					continue;
				}
			}
			return i;
		}

		private CommandCompletion GetNewCompletionResults(string input)
		{
			CommandCompletion commandCompletion;
			try
			{
				Runspace runspace = this.parent.Runspace;
				if (runspace as LocalRunspace == null || runspace.ExecutionContext.EngineHostInterface.NestedPromptCount <= 0)
				{
                    this.commandCompletionPowerShell = System.Management.Automation.PowerShell.Create();
					this.commandCompletionPowerShell.Runspace = runspace;
				}
				else
				{
                    this.commandCompletionPowerShell = System.Management.Automation.PowerShell.Create(RunspaceMode.CurrentRunspace);
				}
				commandCompletion = CommandCompletion.CompleteInput(input, input.Length, null, this.commandCompletionPowerShell);
			}
			finally
			{
				this.commandCompletionPowerShell = null;
			}
			return commandCompletion;
		}

		private void HandleIncomingProgressRecord(long sourceId, ProgressRecord record)
		{
			if (this.pendingProgress == null)
			{
				this.pendingProgress = new PendingProgress();
			}
			this.pendingProgress.Update(sourceId, record);
			if (this.progPane == null)
			{
				this.progPane = new ProgressPane(this);
			}
			this.progPane.Show(this.pendingProgress);
		}

		internal void HandleThrowOnReadAndPrompt()
		{
			if (!this.throwOnReadAndPrompt)
			{
				return;
			}
			else
			{
				throw PSTraceSource.NewInvalidOperationException("ConsoleHostUserInterfaceStrings", "ReadFailsOnNonInteractiveFlag", new object[0]);
			}
		}

		private void PostRead()
		{
			if (this.progPane != null)
			{
				this.progPane.Show();
			}
		}

		private void PostRead(string value)
		{
			this.PostRead();
			if (this.parent.IsTranscribing)
			{
				try
				{
					this.parent.WriteToTranscript(string.Concat(value, "\r\n"));
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					ConsoleHost.CheckForSevereException(exception);
					this.parent.IsTranscribing = false;
				}
			}
		}

		private void PostWrite()
		{
			if (this.progPane != null)
			{
				this.progPane.Show();
			}
		}

		private void PostWrite(string value)
		{
			this.PostWrite();
			if (this.parent.IsTranscribing)
			{
				try
				{
					this.parent.WriteToTranscript(value);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					ConsoleHost.CheckForSevereException(exception);
					this.parent.IsTranscribing = false;
				}
			}
		}

		private void PreRead()
		{
			if (this.progPane != null)
			{
				this.progPane.Hide();
			}
		}

		private void PreWrite()
		{
			if (this.progPane != null)
			{
				this.progPane.Hide();
			}
		}

		public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
		{
			Dictionary<string, PSObject> strs;
			this.HandleThrowOnReadAndPrompt();
			if (descriptions != null)
			{
				if (descriptions.Count >= 1)
				{
					lock (this.instanceLock)
					{
						Dictionary<string, PSObject> strs1 = new Dictionary<string, PSObject>();
						bool flag = false;
						if (!string.IsNullOrEmpty(caption))
						{
							this.WriteLineToConsole();
							this.WriteToConsole(this.PromptColor, this.RawUI.BackgroundColor, this.WrapToCurrentWindowWidth(caption));
							this.WriteLineToConsole();
						}
						if (!string.IsNullOrEmpty(message))
						{
							this.WriteLineToConsole(this.WrapToCurrentWindowWidth(message));
						}
						if (ConsoleHostUserInterface.AtLeastOneHelpMessageIsPresent(descriptions))
						{
							this.WriteLineToConsole(this.WrapToCurrentWindowWidth(ConsoleHostUserInterfaceStrings.PromptHelp));
						}
						int num = -1;
						foreach (FieldDescription description in descriptions)
						{
							num++;
							if (description != null)
							{
								PSObject pSObject = null;
								string name = description.Name;
								bool flag1 = true;
								if (!string.IsNullOrEmpty(description.ParameterAssemblyFullName))
								{
									Type fieldType = InternalHostUserInterface.GetFieldType(description);
									if (fieldType == null)
									{
										if (!InternalHostUserInterface.IsSecuritySensitiveType(description.ParameterTypeName))
										{
											fieldType = typeof(string);
										}
										else
										{
											string str = StringUtil.Format(ConsoleHostUserInterfaceStrings.PromptTypeLoadErrorTemplate, description.Name, description.ParameterTypeFullName);
											PromptingException promptingException = new PromptingException(str, null, "BadTypeName", ErrorCategory.InvalidType);
											throw promptingException;
										}
									}
									if (fieldType.GetInterface(typeof(IList).FullName) == null)
									{
										string str1 = StringUtil.Format(ConsoleHostUserInterfaceStrings.PromptFieldPromptInputSeparatorTemplate, name);
										object obj = null;
										bool flag2 = false;
										this.PromptForSingleItem(fieldType, str1, name, caption, message, description, flag1, false, out flag2, out flag, out obj);
										if (!flag)
										{
											pSObject = PSObject.AsPSObject(obj);
										}
									}
									else
									{
										ArrayList arrayLists = new ArrayList();
										Type elementType = typeof(object);
										if (fieldType.IsArray)
										{
											elementType = fieldType.GetElementType();
											int arrayRank = fieldType.GetArrayRank();
											if (arrayRank <= 0)
											{
												string str2 = StringUtil.Format(ConsoleHostUserInterfaceStrings.RankZeroArrayErrorTemplate, description.Name);
												object[] objArray = new object[1];
												objArray[0] = num;
												ArgumentException argumentException = PSTraceSource.NewArgumentException(string.Format(CultureInfo.InvariantCulture, "descriptions[{0}].AssemblyFullName", objArray));
												PromptingException promptingException1 = new PromptingException(str2, argumentException, "ZeroRankArray", ErrorCategory.InvalidOperation);
												throw promptingException1;
											}
										}
										StringBuilder stringBuilder = new StringBuilder(name);
										stringBuilder.Append("[");
										while (true)
										{
											object[] count = new object[1];
											count[0] = arrayLists.Count;
											stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0}]: ", count));
											bool flag3 = false;
											object obj1 = null;
											this.PromptForSingleItem(elementType, stringBuilder.ToString(), name, caption, message, description, flag1, true, out flag3, out flag, out obj1);
											if (flag || flag3)
											{
												break;
											}
											if (!flag)
											{
												arrayLists.Add(obj1);
												stringBuilder.Length = name.Length + 1;
											}
										}
										if (!flag)
										{
											object obj2 = null;
											if (!LanguagePrimitives.TryConvertTo(arrayLists, fieldType, out obj2))
											{
												pSObject = PSObject.AsPSObject(arrayLists);
											}
											else
											{
												pSObject = PSObject.AsPSObject(obj2);
											}
										}
									}
									if (!flag)
									{
										strs1.Add(description.Name, PSObject.AsPSObject(pSObject));
									}
									else
									{
										ConsoleHostUserInterface.tracer.WriteLine("Prompt canceled", new object[0]);
										this.WriteLineToConsole();
										strs1.Clear();
										break;
									}
								}
								else
								{
									object[] objArray1 = new object[1];
									objArray1[0] = num;
									string str3 = string.Format(CultureInfo.InvariantCulture, "descriptions[{0}].AssemblyFullName", objArray1);
									object[] objArray2 = new object[1];
									objArray2[0] = str3;
									throw PSTraceSource.NewArgumentException(str3, "ConsoleHostUserInterfaceStrings", "NullOrEmptyErrorTemplate", objArray2);
								}
							}
							else
							{
								object[] objArray3 = new object[1];
								object[] objArray4 = new object[1];
								objArray4[0] = num;
								objArray3[0] = string.Format(CultureInfo.InvariantCulture, "descriptions[{0}]", objArray4);
								throw PSTraceSource.NewArgumentException("descriptions", "ConsoleHostUserInterfaceStrings", "NullErrorTemplate", objArray3);
							}
						}
						strs = strs1;
					}
					return strs;
				}
				else
				{
					object[] objArray5 = new object[1];
					objArray5[0] = "descriptions";
					throw PSTraceSource.NewArgumentException("descriptions", "ConsoleHostUserInterfaceStrings", "PromptEmptyDescriptionsErrorTemplate", objArray5);
				}
			}
			else
			{
				throw PSTraceSource.NewArgumentNullException("descriptions");
			}
		}

		private string PromptCommandMode(string input, FieldDescription desc, out bool inputDone)
		{
			string str = input.Substring(1);
			inputDone = true;
			if (!str.StartsWith("!", StringComparison.OrdinalIgnoreCase))
			{
				if (str.Length != 1)
				{
					if (str.Length != 2 || string.Compare(str, "\"\"", StringComparison.OrdinalIgnoreCase) != 0)
					{
						if (string.Compare(str, "$null", StringComparison.OrdinalIgnoreCase) != 0)
						{
							this.ReportUnrecognizedPromptCommand(input);
							inputDone = false;
							return null;
						}
						else
						{
							return null;
						}
					}
					else
					{
						return string.Empty;
					}
				}
				else
				{
					if (str[0] != '?')
					{
						this.ReportUnrecognizedPromptCommand(input);
					}
					else
					{
						if (!string.IsNullOrEmpty(desc.HelpMessage))
						{
							this.WriteLineToConsole(this.WrapToCurrentWindowWidth(desc.HelpMessage));
						}
						else
						{
							string str1 = StringUtil.Format(ConsoleHostUserInterfaceStrings.PromptNoHelpAvailableErrorTemplate, desc.Name);
							ConsoleHostUserInterface.tracer.TraceWarning(str1, new object[0]);
							this.WriteLineToConsole(this.WrapToCurrentWindowWidth(str1));
						}
					}
					inputDone = false;
					return null;
				}
			}
			else
			{
				return str;
			}
		}

		public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
		{
			ConsoleHostUserInterface.ReadLineResult readLineResult = ConsoleHostUserInterface.ReadLineResult.endedOnEnter;
			int num;
			this.HandleThrowOnReadAndPrompt();
			if (choices != null)
			{
				if (choices.Count != 0)
				{
					if (defaultChoice < -1 || defaultChoice >= choices.Count)
					{
						object[] objArray = new object[2];
						objArray[0] = "defaultChoice";
						objArray[1] = "choice";
						throw PSTraceSource.NewArgumentOutOfRangeException("defaultChoice", defaultChoice, "ConsoleHostUserInterfaceStrings", "InvalidDefaultChoiceErrorTemplate", objArray);
					}
					else
					{
						lock (this.instanceLock)
						{
							if (!string.IsNullOrEmpty(caption))
							{
								this.WriteLineToConsole();
								this.WriteToConsole(this.PromptColor, this.RawUI.BackgroundColor, this.WrapToCurrentWindowWidth(caption));
								this.WriteLineToConsole();
							}
							if (!string.IsNullOrEmpty(message))
							{
								this.WriteLineToConsole(this.WrapToCurrentWindowWidth(message));
							}
							int num1 = defaultChoice;
							string[,] strArrays = null;
							HostUIHelperMethods.BuildHotkeysAndPlainLabels(choices, out strArrays);
							Dictionary<int, bool> nums = new Dictionary<int, bool>();
							if (defaultChoice >= 0)
							{
								nums.Add(defaultChoice, true);
							}
							do
							{
							Label0:
								this.WriteChoicePrompt(strArrays, nums, false);
								string str = this.ReadLine(false, "", out readLineResult, true, true);
								if (readLineResult != ConsoleHostUserInterface.ReadLineResult.endedOnBreak)
								{
									if (str.Length != 0)
									{
										if (str.Trim() != "?")
										{
											num1 = HostUIHelperMethods.DetermineChoicePicked(str.Trim(), choices, strArrays);
										}
										else
										{
											this.ShowChoiceHelp(choices, strArrays);
											goto Label0;
										}
									}
									else
									{
										if (defaultChoice >= 0)
										{
											num1 = defaultChoice;
											break;
										}
										else
										{
											goto Label0;
										}
									}
								}
								else
								{
									string promptCanceledError = ConsoleHostUserInterfaceStrings.PromptCanceledError;
									PromptingException promptingException = new PromptingException(promptCanceledError, null, "PromptForChoiceCanceled", ErrorCategory.OperationStopped);
									throw promptingException;
								}
							}
							while (num1 < 0);
							num = num1;
						}
						return num;
					}
				}
				else
				{
					object[] objArray1 = new object[1];
					objArray1[0] = "choices";
					throw PSTraceSource.NewArgumentException("choices", "ConsoleHostUserInterfaceStrings", "EmptyChoicesErrorTemplate", objArray1);
				}
			}
			else
			{
				throw PSTraceSource.NewArgumentNullException("choices");
			}
		}

		public Collection<int> PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, IEnumerable<int> defaultChoices)
		{
			ConsoleHostUserInterface.ReadLineResult readLineResult = ConsoleHostUserInterface.ReadLineResult.endedOnEnter;
			Collection<int> nums;
			this.HandleThrowOnReadAndPrompt();
			if (choices != null)
			{
				if (choices.Count != 0)
				{
					Dictionary<int, bool> nums1 = new Dictionary<int, bool>();
					if (defaultChoices != null)
					{
						foreach (int defaultChoice in defaultChoices)
						{
							if (defaultChoice < 0 || defaultChoice >= choices.Count)
							{
								object[] objArray = new object[3];
								objArray[0] = "defaultChoice";
								objArray[1] = "choices";
								objArray[2] = defaultChoice;
								throw PSTraceSource.NewArgumentOutOfRangeException("defaultChoice", defaultChoice, "ConsoleHostUserInterfaceStrings", "InvalidDefaultChoiceForMultipleSelection", objArray);
							}
							else
							{
								if (nums1.ContainsKey(defaultChoice))
								{
									continue;
								}
								nums1.Add(defaultChoice, true);
							}
						}
					}
					Collection<int> nums2 = new Collection<int>();
					lock (this.instanceLock)
					{
						if (!string.IsNullOrEmpty(caption))
						{
							this.WriteLineToConsole();
							this.WriteToConsole(this.PromptColor, this.RawUI.BackgroundColor, this.WrapToCurrentWindowWidth(caption));
							this.WriteLineToConsole();
						}
						if (!string.IsNullOrEmpty(message))
						{
							this.WriteLineToConsole(this.WrapToCurrentWindowWidth(message));
						}
						string[,] strArrays = null;
						HostUIHelperMethods.BuildHotkeysAndPlainLabels(choices, out strArrays);
						this.WriteChoicePrompt(strArrays, nums1, true);
						if (nums1.Count > 0)
						{
							this.WriteLineToConsole();
						}
						int num = 0;
						while (true)
						{
							string str = StringUtil.Format(ConsoleHostUserInterfaceStrings.ChoiceMessage, num);
							this.WriteToConsole(this.PromptColor, this.RawUI.BackgroundColor, this.WrapToCurrentWindowWidth(str));
							string str1 = this.ReadLine(false, "", out readLineResult, true, true);
							if (readLineResult == ConsoleHostUserInterface.ReadLineResult.endedOnBreak)
							{
								string promptCanceledError = ConsoleHostUserInterfaceStrings.PromptCanceledError;
								PromptingException promptingException = new PromptingException(promptCanceledError, null, "PromptForChoiceCanceled", ErrorCategory.OperationStopped);
								throw promptingException;
							}
							if (str1.Length == 0)
							{
								break;
							}
							if (str1.Trim() != "?")
							{
								int num1 = HostUIHelperMethods.DetermineChoicePicked(str1.Trim(), choices, strArrays);
								if (num1 >= 0)
								{
									nums2.Add(num1);
									num++;
								}
							}
							else
							{
								this.ShowChoiceHelp(choices, strArrays);
							}
						}
						if (nums2.Count == 0 && nums1.Keys.Count >= 0)
						{
							foreach (int key in nums1.Keys)
							{
								nums2.Add(key);
							}
						}
						nums = nums2;
					}
					return nums;
				}
				else
				{
					object[] objArray1 = new object[1];
					objArray1[0] = "choices";
					throw PSTraceSource.NewArgumentException("choices", "ConsoleHostUserInterfaceStrings", "EmptyChoicesErrorTemplate", objArray1);
				}
			}
			else
			{
				throw PSTraceSource.NewArgumentNullException("choices");
			}
		}

		public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
		{
			return this.PromptForCredential(caption, message, userName, targetName, PSCredentialTypes.Default, PSCredentialUIOptions.Default);
		}

		public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
		{
			if (this.PromptUsingConsole())
			{
				if (!string.IsNullOrEmpty(caption))
				{
					this.WriteLineToConsole();
					this.WriteToConsole(this.PromptColor, this.RawUI.BackgroundColor, this.WrapToCurrentWindowWidth(caption));
					this.WriteLineToConsole();
				}
				if (!string.IsNullOrEmpty(message))
				{
					this.WriteLineToConsole(this.WrapToCurrentWindowWidth(message));
				}
				if (string.IsNullOrEmpty(userName))
				{
					string promptForCredentialUser = ConsoleHostUserInterfaceSecurityResources.PromptForCredential_User;
					do
					{
						this.WriteToConsole(promptForCredentialUser, true);
						userName = this.ReadLine();
						if (userName != null)
						{
							continue;
						}
						return null;
					}
					while (userName.Length == 0);
				}
				string str = StringUtil.Format(ConsoleHostUserInterfaceSecurityResources.PromptForCredential_Password, userName);
				//this.WriteToConsole(str, true);
				//SecureString secureString = this.ReadLineAsSecureString();
				string oldPrompt = ConsoleControl.NativeMethods.Editor.Prompt;
				ConsoleControl.NativeMethods.Editor.Prompt = str;
				ConsoleControl.NativeMethods.Editor.SecureInput = true;
				ConsoleControl.NativeMethods.Editor.SecureChar = new char?('*');
				string pwd = this.ReadLineSafe (false, '*').ToString ().Replace ("\r\n", "");
				SecureString secureString = SecureStringHelper.EasyUnprotect (pwd);
				ConsoleControl.NativeMethods.Editor.SecureInput = false;
				ConsoleControl.NativeMethods.Editor.Prompt = oldPrompt;
				if (secureString != null)
				{
					PSCredential pSCredential = new PSCredential(userName, secureString);
					return pSCredential;
				}
				else
				{
					return null;
				}
			}
			else
			{
				IntPtr mainWindowHandle = this.GetMainWindowHandle();
				return HostUtilities.CredUIPromptForCredential(caption, message, userName, targetName, allowedCredentialTypes, options, mainWindowHandle);
			}
		}

		private string PromptForSingleItem(Type fieldType, string printFieldPrompt, string fieldPrompt, string caption, string message, FieldDescription desc, bool fieldEchoOnPrompt, bool listInput, out bool endListInput, out bool cancelInput, out object convertedObj)
		{
			PSCredential pSCredential;
			cancelInput = false;
			endListInput = false;
			convertedObj = null;
			if (!fieldType.Equals(typeof(SecureString)))
			{
				if (!fieldType.Equals(typeof(PSCredential)))
				{
					string str = null;
					do
					{
						str = this.PromptReadInput(printFieldPrompt, desc, fieldEchoOnPrompt, listInput, out endListInput, out cancelInput);
					}
					while (!cancelInput && !endListInput && this.PromptTryConvertTo(fieldType, desc.IsFromRemoteHost, str, out convertedObj) != ConsoleHostUserInterface.PromptCommonInputErrors.None);
					return str;
				}
				else
				{
					this.WriteLineToConsole(this.WrapToCurrentWindowWidth(fieldPrompt));
					if (this.PromptUsingConsole() || !desc.ModifiedByRemotingProtocol)
					{
						pSCredential = this.PromptForCredential(null, null, null, string.Empty);
					}
					else
					{
						pSCredential = this.PromptForCredential(caption, message, null, string.Empty);
					}
					convertedObj = pSCredential;
					cancelInput = convertedObj == null;
					if (pSCredential != null && pSCredential.Password.Length == 0 && listInput)
					{
						endListInput = true;
					}
				}
			}
			else
			{
				this.WriteToConsole(printFieldPrompt, true);
				SecureString secureString = this.ReadLineAsSecureString();
				convertedObj = secureString;
				cancelInput = convertedObj == null;
				if (secureString != null && secureString.Length == 0 && listInput)
				{
					endListInput = true;
				}
			}
			return null;
		}

		private string PromptReadInput(string fieldPrompt, FieldDescription desc, bool fieldEchoOnPrompt, bool listInput, out bool endListInput, out bool cancelled)
		{
			string str;
			string str1 = null;
			endListInput = false;
			cancelled = false;
			bool flag = false;
			while (!flag)
			{
				//this.WriteToConsole(fieldPrompt, true);
				string oldPrompt = ConsoleControl.NativeMethods.Editor.Prompt;
				ConsoleControl.NativeMethods.Editor.Prompt = fieldPrompt;
				if (!fieldEchoOnPrompt)
				{
					char? nullable = null;
					ConsoleControl.NativeMethods.Editor.SecureInput = true;
					ConsoleControl.NativeMethods.Editor.SecureChar = nullable;
					object obj = this.ReadLineSafe(false, nullable);
					string str2 = obj as string;
					ConsoleControl.NativeMethods.Editor.SecureInput = false;
					str = str2;
				}
				else
				{
					str = this.ReadLine();
				}
				ConsoleControl.NativeMethods.Editor.Prompt = oldPrompt;

				if (str != null)
				{
					if (!str.StartsWith("!", false, CultureInfo.CurrentCulture))
					{
						if (str.Length == 0 && listInput)
						{
							endListInput = true;
						}
						str1 = str;
						break;
					}
					else
					{
						str1 = this.PromptCommandMode(str, desc, out flag);
					}
				}
				else
				{
					cancelled = true;
					break;
				}
			}
			return str1;
		}

		private ConsoleHostUserInterface.PromptCommonInputErrors PromptTryConvertTo(Type fieldType, bool isFromRemoteHost, string inputString, out object convertedObj)
		{
			ConsoleHostUserInterface.PromptCommonInputErrors promptCommonInputError;
			convertedObj = inputString;
			if (!isFromRemoteHost)
			{
				try
				{
					convertedObj = LanguagePrimitives.ConvertTo(inputString, fieldType, CultureInfo.InvariantCulture);
					return ConsoleHostUserInterface.PromptCommonInputErrors.None;
				}
				catch (PSInvalidCastException pSInvalidCastException1)
				{
					PSInvalidCastException pSInvalidCastException = pSInvalidCastException1;
					Exception innerException = pSInvalidCastException.InnerException;
					if (innerException != null)
					{
						if (innerException as OverflowException == null)
						{
							if (innerException as FormatException != null)
							{
								if (inputString.Length > 0)
								{
									string promptParseFormatErrorTemplate = ConsoleHostUserInterfaceStrings.PromptParseFormatErrorTemplate;
									object[] objArray = new object[2];
									objArray[0] = fieldType;
									objArray[1] = inputString;
									this.WriteLineToConsole(this.WrapToCurrentWindowWidth(string.Format(CultureInfo.CurrentCulture, promptParseFormatErrorTemplate, objArray)));
								}
								promptCommonInputError = ConsoleHostUserInterface.PromptCommonInputErrors.Format;
								return promptCommonInputError;
							}
						}
						else
						{
							string promptParseOverflowErrorTemplate = ConsoleHostUserInterfaceStrings.PromptParseOverflowErrorTemplate;
							object[] objArray1 = new object[2];
							objArray1[0] = fieldType;
							objArray1[1] = inputString;
							this.WriteLineToConsole(this.WrapToCurrentWindowWidth(string.Format(CultureInfo.CurrentCulture, promptParseOverflowErrorTemplate, objArray1)));
							promptCommonInputError = ConsoleHostUserInterface.PromptCommonInputErrors.Overflow;
							return promptCommonInputError;
						}
					}
					return ConsoleHostUserInterface.PromptCommonInputErrors.None;
				}
				return promptCommonInputError;
			}
			else
			{
				return ConsoleHostUserInterface.PromptCommonInputErrors.None;
			}
		}

		private bool PromptUsingConsole()
		{
			if (OSHelper.IsUnix) return true;
			RegistryKey registryKey;
			bool flag;
			string registryConfigurationPrefix = Utils.GetRegistryConfigurationPrefix();
			bool flag1 = false;
			try
			{
				registryKey = Registry.LocalMachine.OpenSubKey(registryConfigurationPrefix);
			}
			catch (SecurityException securityException)
			{
				ConsoleHostUserInterface.tracer.TraceError("User doesn't have access to read CredUI registry key.", new object[0]);
				flag = flag1;
				return flag;
			}
			if (registryKey != null)
			{
				try
				{
					object value = registryKey.GetValue("ConsolePrompting");
					if (value != null)
					{
						flag1 = Convert.ToBoolean(value.ToString(), CultureInfo.InvariantCulture);
					}
				}
				catch (SecurityException securityException2)
				{
					SecurityException securityException1 = securityException2;
					ConsoleHostUserInterface.tracer.TraceError(string.Concat("Could not read CredUI registry key: ", securityException1.Message), new object[0]);
					if (registryKey != null)
					{
						registryKey.Close();
					}
					flag = flag1;
					return flag;
				}
				catch (InvalidCastException invalidCastException1)
				{
					InvalidCastException invalidCastException = invalidCastException1;
					ConsoleHostUserInterface.tracer.TraceError(string.Concat("Could not parse CredUI registry key: ", invalidCastException.Message), new object[0]);
					if (registryKey != null)
					{
						registryKey.Close();
					}
					flag = flag1;
					return flag;
				}
				catch (FormatException formatException1)
				{
					FormatException formatException = formatException1;
					ConsoleHostUserInterface.tracer.TraceError(string.Concat("Could not parse CredUI registry key: ", formatException.Message), new object[0]);
					if (registryKey != null)
					{
						registryKey.Close();
					}
					flag = flag1;
					return flag;
				}
				object[] objArray = new object[1];
				objArray[0] = flag1;
				ConsoleHostUserInterface.tracer.WriteLine("DetermineCredUIPolicy: policy == {0}", objArray);
				if (registryKey != null)
				{
					registryKey.Close();
				}
				return flag1;
			}
			else
			{
				return flag1;
			}
			return flag;
		}

		public override string ReadLine()
		{
			ConsoleHostUserInterface.ReadLineResult readLineResult = ConsoleHostUserInterface.ReadLineResult.endedOnEnter;
			this.HandleThrowOnReadAndPrompt();
			return this.ReadLine(false, "", out readLineResult, true, true);
		}

		internal string ReadLine(bool endOnTab, string initialContent, out ConsoleHostUserInterface.ReadLineResult result, bool calledFromPipeline, bool transcribeResult)
		{
			ConsoleHostUserInterface.ReadLineResult readLineResult;
			result = ConsoleHostUserInterface.ReadLineResult.endedOnEnter;
			string str = "";
			if (!this.parent.IsStandardInputRedirected || !this.readFromStdin)
			{
				SafeFileHandle inputHandle = ConsoleControl.GetInputHandle();
				this.PreRead();
				ConsoleControl.ConsoleModes mode = ConsoleControl.GetMode(inputHandle);
				if ((mode & (ConsoleControl.ConsoleModes.ProcessedInput | ConsoleControl.ConsoleModes.LineInput | ConsoleControl.ConsoleModes.EchoInput | ConsoleControl.ConsoleModes.ProcessedOutput | ConsoleControl.ConsoleModes.WrapEndOfLine)) != (ConsoleControl.ConsoleModes.ProcessedInput | ConsoleControl.ConsoleModes.LineInput | ConsoleControl.ConsoleModes.EchoInput | ConsoleControl.ConsoleModes.ProcessedOutput | ConsoleControl.ConsoleModes.WrapEndOfLine) || (mode & ConsoleControl.ConsoleModes.MouseInput) > 0)
				{
					mode = mode & (ConsoleControl.ConsoleModes.ProcessedInput | ConsoleControl.ConsoleModes.LineInput | ConsoleControl.ConsoleModes.EchoInput | ConsoleControl.ConsoleModes.WindowInput | ConsoleControl.ConsoleModes.Insert | ConsoleControl.ConsoleModes.QuickEdit | ConsoleControl.ConsoleModes.Extended | ConsoleControl.ConsoleModes.AutoPosition | ConsoleControl.ConsoleModes.ProcessedOutput | ConsoleControl.ConsoleModes.WrapEndOfLine);
					mode = mode | ConsoleControl.ConsoleModes.ProcessedInput | ConsoleControl.ConsoleModes.LineInput | ConsoleControl.ConsoleModes.EchoInput | ConsoleControl.ConsoleModes.ProcessedOutput | ConsoleControl.ConsoleModes.WrapEndOfLine;
					ConsoleControl.SetMode(inputHandle, mode);
				}
				int num = 0;
				string str1 = null;
				this.rawui.ClearKeyCache();
				while (true)
				{
					str = string.Concat(str, ConsoleControl.ReadConsole(inputHandle, initialContent, 0x2000, endOnTab, out num));
					if (str.Length != 0)
					{
						if (!str.EndsWith("\r\n", StringComparison.CurrentCulture))
						{
							int num1 = str.IndexOf("\t", StringComparison.CurrentCulture);
							if (endOnTab && num1 != -1)
							{
								if ((num & 16) != 0)
								{
									if ((num & 16) > 0)
									{
										result = ConsoleHostUserInterface.ReadLineResult.endedOnShiftTab;
									}
								}
								else
								{
									result = ConsoleHostUserInterface.ReadLineResult.endedOnTab;
								}
								int num2 = this.RawUI.LengthInBufferCells(str.Substring(num1 + 1));
								if (num2 <= 0)
								{
									str1 = string.Concat(str1, str[num1]);
								}
								else
								{
									Coordinates cursorPosition = this.RawUI.CursorPosition;
									char characterUnderCursor = this.GetCharacterUnderCursor(cursorPosition);
									this.Write(new string(' ', num2));
									this.RawUI.CursorPosition = cursorPosition;
									str1 = string.Concat(str[num1], characterUnderCursor, str.Substring(num1 + 1));
								}
								str = str.Remove(num1);
								break;
							}
						}
						else
						{
							result = ConsoleHostUserInterface.ReadLineResult.endedOnEnter;
							str = str.Remove(str.Length - "\r\n".Length);
							break;
						}
					}
					else
					{
						result = ConsoleHostUserInterface.ReadLineResult.endedOnBreak;
						str = null;
						if (!calledFromPipeline)
						{
							break;
						}
						throw new PipelineStoppedException();
					}
				}
				if (!transcribeResult)
				{
					this.PostRead();
				}
				else
				{
					this.PostRead(str);
				}
				if (str1 != null)
				{
					str = string.Concat(str, str1);
				}
				return str;
			}
			else
			{
				str = this.parent.StandardInReader.ReadLine();
				if (!endOnTab || string.IsNullOrEmpty(str) || str.IndexOf("\t", StringComparison.OrdinalIgnoreCase) == -1)
				{
					return str;
				}
				else
				{
					ConsoleHostUserInterface.ReadLineResult readLineResultPointer = (ConsoleHostUserInterface.ReadLineResult)result;
					if (this.isTestingShiftTab)
					{
						readLineResult = ConsoleHostUserInterface.ReadLineResult.endedOnShiftTab;
					}
					else
					{
						readLineResult = ConsoleHostUserInterface.ReadLineResult.endedOnTab;
					}
					readLineResultPointer = readLineResult;
					return str;
				}
			}
		}

		public override SecureString ReadLineAsSecureString()
		{
			object obj;
			this.HandleThrowOnReadAndPrompt();
			string oldPrompt = ConsoleControl.NativeMethods.Editor.Prompt;
			ConsoleControl.NativeMethods.Editor.Prompt = "";
			this.WriteLine ();
			lock (this.instanceLock)
			{
				obj = this.ReadLineSafe(true, new char?('*'));
			}
			ConsoleControl.NativeMethods.Editor.Prompt = oldPrompt;
			SecureString secureString = obj as SecureString;
			return secureString;
		}

		private object ReadLineSafe(bool isSecureString, char? printToken)
		{
			string str;
			this.PreRead();
			if (printToken.HasValue)
			{
				str = printToken.ToString();
			}
			else
			{
				str = null;
			}
			string str1 = str;
			SecureString secureString = new SecureString();
			StringBuilder stringBuilder = new StringBuilder();
			SafeFileHandle inputHandle = ConsoleControl.GetInputHandle();
			ConsoleControl.ConsoleModes mode = ConsoleControl.GetMode(inputHandle);
			bool flag = true;
			try
			{
				ConsoleControl.ConsoleModes consoleMode = ConsoleControl.ConsoleModes.QuickEdit | ConsoleControl.ConsoleModes.Extended;
				ConsoleControl.ConsoleModes consoleMode1 = mode;
				bool flag1 = ConsoleHostUserInterface.shouldUnsetMode(ConsoleControl.ConsoleModes.EchoInput, ref consoleMode1);
				bool flag2 = ConsoleHostUserInterface.shouldUnsetMode(ConsoleControl.ConsoleModes.LineInput, ref consoleMode1);
				bool flag3 = ConsoleHostUserInterface.shouldUnsetMode(ConsoleControl.ConsoleModes.MouseInput, ref consoleMode1);
				bool flag4 = ConsoleHostUserInterface.shouldUnsetMode(ConsoleControl.ConsoleModes.ProcessedInput, ref consoleMode1);
				if ((consoleMode1 & consoleMode) != consoleMode || flag3 || flag1 || flag2 || flag4)
				{
					consoleMode1 = consoleMode1 | consoleMode;
					ConsoleControl.SetMode(inputHandle, consoleMode1);
				}
				else
				{
					flag = false;
				}
				this.rawui.ClearKeyCache();
				Coordinates cursorPosition = this.rawui.CursorPosition;
				while (true)
				{
					int num = 0;
					string str2 = ConsoleControl.ReadConsole(inputHandle, string.Empty, 1, false, out num);
					if (string.IsNullOrEmpty(str2) || 3 == str2[0])
					{
						break;
					}
					if (13 == str2[0])
					{
						goto Label1;
					}
					if (8 != str2[0])
					{
						if (!isSecureString)
						{
							stringBuilder.Append(str2);
							goto Label1;
						}
						else
						{
							foreach(var char1 in str2)
							{
								secureString.AppendChar(str2[0]);
							}
							goto Label1;
						}
						/*
						if (!string.IsNullOrEmpty(str1))
						{
							this.WritePrintToken(str1, ref cursorPosition);
						}
						*/
					}
					else
					{
						if (!isSecureString || secureString.Length <= 0)
						{
							if (stringBuilder.Length > 0)
							{
								stringBuilder.Remove(stringBuilder.Length - 1, 1);
								this.WriteBackSpace(cursorPosition);
							}
						}
						else
						{
							secureString.RemoveAt(secureString.Length - 1);
							this.WriteBackSpace(cursorPosition);
						}
					}
				}
				PipelineStoppedException pipelineStoppedException = new PipelineStoppedException();
				throw pipelineStoppedException;
			}
			finally
			{
				if (flag)
				{
					ConsoleControl.SetMode(inputHandle, mode);
				}
			}
		Label1:
			this.WriteLineToConsole();
			this.PostRead(stringBuilder.ToString());
			if (!isSecureString)
			{
				return stringBuilder;
			}
			else
			{
				return secureString;
			}
		}

		internal string ReadLineWithTabCompletion(Executor exec, bool useUserDefinedCustomReadLine)
		{
			string str;
			bool flag;
			SafeFileHandle activeScreenBufferHandle = ConsoleControl.GetActiveScreenBufferHandle();
			string str1 = null;
			string str2 = "";
			string str3 = "";
			ConsoleHostUserInterface.ReadLineResult readLineResult = ConsoleHostUserInterface.ReadLineResult.endedOnEnter;
			Size bufferSize = this.RawUI.BufferSize;
			Coordinates cursorPosition = this.RawUI.CursorPosition;
			CommandCompletion newCompletionResults = null;
			string str4 = null;
			while (!this.TryInvokeUserDefinedReadLine(out str1, useUserDefinedCustomReadLine))
			{
				str2 = "";
				str1 = this.ReadLine(true, str2, out readLineResult, false, false);
				Coordinates coordinate = this.RawUI.CursorPosition;
				if (str1 == null || readLineResult == ConsoleHostUserInterface.ReadLineResult.endedOnEnter)
				{
					break;
				}
				if (readLineResult != ConsoleHostUserInterface.ReadLineResult.endedOnTab && readLineResult != ConsoleHostUserInterface.ReadLineResult.endedOnShiftTab)
				{
					continue;
				}
				int num = str1.IndexOf("\t", StringComparison.CurrentCulture);
				string empty = string.Empty;
				int length = str1.Length - num - 1;
				if (length > 0)
				{
					if (!this.parent.IsStandardInputRedirected)
					{
						flag = false;
					}
					else
					{
						flag = this.readFromStdin;
					}
					bool flag1 = flag;
					if (!flag1)
					{
						str1 = str1.Remove(str1.Length - 1);
					}
					empty = str1.Substring(num + 1);
				}
				str1 = str1.Remove(num);
				if (str1 != str3 || newCompletionResults == null)
				{
					str4 = str1;
					newCompletionResults = this.GetNewCompletionResults(str1);
				}
				CompletionResult nextResult = newCompletionResults.GetNextResult(readLineResult == ConsoleHostUserInterface.ReadLineResult.endedOnTab);
				if (nextResult == null)
				{
					str = str4;
				}
				else
				{
					str = string.Concat(str4.Substring(0, newCompletionResults.ReplacementIndex), nextResult.CompletionText);
				}
				if (empty != string.Empty)
				{
					str = string.Concat(str, empty);
				}
				if (str.Length > 0x1ffe)
				{
					str = str.Substring(0, 0x1ffe);
				}
				str = this.RemoveNulls(str);
				int x = (cursorPosition.X + str1.Length) / bufferSize.Width;
				cursorPosition.Y = coordinate.Y - x;
				try
				{
					this.RawUI.CursorPosition = cursorPosition;
				}
				catch (PSArgumentOutOfRangeException pSArgumentOutOfRangeException)
				{
					break;
				}
				ConsoleControl.NativeMethods.Editor.Initial = str;
				//this.WriteToConsole(str, false);
				Coordinates cursorPosition1 = this.RawUI.CursorPosition;
				int x1 = (cursorPosition.X + str.Length) / bufferSize.Width;
				cursorPosition.Y = cursorPosition1.Y - x1;
				int y = coordinate.Y * bufferSize.Width + coordinate.X - cursorPosition1.Y * bufferSize.Width + cursorPosition1.X;
				if (y > 0)
				{
					ConsoleControl.FillConsoleOutputCharacter(activeScreenBufferHandle, ' ', y, cursorPosition1);
				}
				if (empty == string.Empty)
				{
					str3 = str;
				}
				else
				{
					str3 = str.Remove(str.Length - empty.Length);
					this.SendLeftArrows(empty.Length);
				}
				str2 = str;
			}
			if (this.parent.IsTranscribing)
			{
				this.parent.WriteToTranscript(string.Concat(str1, "\r\n"));
			}
			return str1;
		}

		private string RemoveNulls(string input)
		{
			if (input.IndexOf('\0') != -1)
			{
				StringBuilder stringBuilder = new StringBuilder();
				string str = input;
				for (int i = 0; i < str.Length; i++)
				{
					char chr = str[i];
					if (chr != 0)
					{
						stringBuilder.Append(chr);
					}
				}
				return stringBuilder.ToString();
			}
			else
			{
				return input;
			}
		}

		private void ReportUnrecognizedPromptCommand(string command)
		{
			string str = StringUtil.Format(ConsoleHostUserInterfaceStrings.PromptUnrecognizedCommandErrorTemplate, command);
			this.WriteLineToConsole(this.WrapToCurrentWindowWidth(str));
		}

		internal void ResetProgress()
		{
			if (this.progPane != null)
			{
				this.progPane.Hide();
				this.progPane = null;
			}
			this.pendingProgress = null;
		}

		private void SendLeftArrows(int length)
		{
			ConsoleControl.INPUT[] nPUTArray = new ConsoleControl.INPUT[length * 2];
			for (int i = 0; i < length; i++)
			{
				ConsoleControl.INPUT zero = new ConsoleControl.INPUT();
				zero.Type = 1;
				zero.Data.Keyboard = new ConsoleControl.KeyboardInput();
				zero.Data.Keyboard.Vk = 37;
				zero.Data.Keyboard.Scan = 0;
				zero.Data.Keyboard.Flags = 0;
				zero.Data.Keyboard.Time = 0;
				zero.Data.Keyboard.ExtraInfo = IntPtr.Zero;
				ConsoleControl.INPUT nPUT = new ConsoleControl.INPUT();
				nPUT.Type = 1;
				nPUT.Data.Keyboard = new ConsoleControl.KeyboardInput();
				nPUT.Data.Keyboard.Vk = 37;
				nPUT.Data.Keyboard.Scan = 0;
				nPUT.Data.Keyboard.Flags = 2;
				nPUT.Data.Keyboard.Time = 0;
				nPUT.Data.Keyboard.ExtraInfo = IntPtr.Zero;
				nPUTArray[2 * i] = zero;
				nPUTArray[2 * i + 1] = nPUT;
			}
			ConsoleControl.MimicKeyPress(nPUTArray);
		}

		private static bool shouldUnsetMode(ConsoleControl.ConsoleModes flagToUnset, ref ConsoleControl.ConsoleModes m)
		{
			if (((ConsoleControl.ConsoleModes)((uint)m) & flagToUnset) <= 0)
			{
				return false;
			}
			else
			{
				m = (ConsoleControl.ConsoleModes)((uint)m) & ~flagToUnset;
				return true;
			}
		}

		private void ShowChoiceHelp(Collection<ChoiceDescription> choices, string[,] hotkeysAndPlainLabels)
		{
			string str;
			for (int i = 0; i < choices.Count; i++)
			{
				if (hotkeysAndPlainLabels[0, i].Length <= 0)
				{
					str = hotkeysAndPlainLabels[1, i];
				}
				else
				{
					str = hotkeysAndPlainLabels[0, i];
				}
				object[] helpMessage = new object[2];
				helpMessage[0] = str;
				helpMessage[1] = choices[i].HelpMessage;
				this.WriteLineToConsole(this.WrapToCurrentWindowWidth(string.Format(CultureInfo.InvariantCulture, "{0} - {1}", helpMessage)));
			}
		}

		private bool TryInvokeUserDefinedReadLine(out string input, bool useUserDefinedCustomReadLine)
		{
			System.Management.Automation.PowerShell powerShell;
			bool flag;
			if (useUserDefinedCustomReadLine)
			{
				LocalRunspace localRunspace = this.parent.LocalRunspace;
				if (localRunspace != null && localRunspace.Engine.Context.EngineIntrinsics.InvokeCommand.GetCommands("PSConsoleHostReadLine", CommandTypes.All, false).Any<CommandInfo>())
				{
					if (localRunspace.ExecutionContext.EngineHostInterface.NestedPromptCount <= 0)
					{
						powerShell = System.Management.Automation.PowerShell.Create();
						powerShell.Runspace = localRunspace;
					}
					else
					{
                        powerShell = System.Management.Automation.PowerShell.Create(RunspaceMode.CurrentRunspace);
					}
					try
					{
						Collection<PSObject> pSObjects = powerShell.AddCommand("PSConsoleHostReadLine").Invoke();
						if (pSObjects.Count != 1)
						{
							input = null;
							return false;
						}
						else
						{
							input = PSObject.Base(pSObjects[0]) as string;
							flag = true;
						}
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						CommandProcessorBase.CheckForSevereException(exception);
						input = null;
						return false;
					}
					return flag;
				}
			}
			input = null;
			return false;
		}

		internal List<string> WrapText(string text, int maxWidthInBufferCells)
		{
			string str;
			List<string> strs = new List<string>();
			List<ConsoleHostUserInterface.Word> words = this.ChopTextIntoWords(text, maxWidthInBufferCells);
			if (words.Count >= 1)
			{
				IEnumerator<ConsoleHostUserInterface.Word> enumerator = words.GetEnumerator();
				bool flag = false;
				int cellCount = 0;
				StringBuilder stringBuilder = new StringBuilder();
				do
				{
					flag = enumerator.MoveNext();
					if (flag)
					{
						if ((enumerator.Current.Flags & ConsoleHostUserInterface.WordFlags.IsNewline) <= 0)
						{
							if (cellCount + enumerator.Current.CellCount > maxWidthInBufferCells)
							{
								if ((enumerator.Current.Flags & ConsoleHostUserInterface.WordFlags.IsWhitespace) != 0)
								{
									int num = maxWidthInBufferCells - cellCount;
									stringBuilder.Append(enumerator.Current.Text.Substring(0, num));
									str = stringBuilder.ToString();
									strs.Add(str);
									string str1 = enumerator.Current.Text.Substring(num);
									stringBuilder = new StringBuilder(str1);
									cellCount = this.RawUI.LengthInBufferCells(str1);
								}
								else
								{
									str = stringBuilder.ToString();
									strs.Add(str);
									stringBuilder = new StringBuilder(enumerator.Current.Text);
									cellCount = enumerator.Current.CellCount;
								}
							}
							else
							{
								stringBuilder.Append(enumerator.Current.Text);
								cellCount = cellCount + enumerator.Current.CellCount;
							}
						}
						else
						{
							str = stringBuilder.ToString();
							strs.Add(str);
							stringBuilder = new StringBuilder();
							cellCount = 0;
						}
					}
					else
					{
						if (stringBuilder.Length <= 0)
						{
							break;
						}
						str = stringBuilder.ToString();
						strs.Add(str);
						break;
					}
				}
				while (flag);
				return strs;
			}
			else
			{
				return strs;
			}
		}

		internal string WrapToCurrentWindowWidth(string text)
		{
			if (OSHelper.IsUnix) return text;
			StringBuilder stringBuilder = new StringBuilder();
			Size windowSize = this.RawUI.WindowSize;
			List<string> strs = this.WrapText(text, windowSize.Width - 1);
			int num = 0;
			foreach (string str in strs)
			{
				stringBuilder.Append(str);
				int num1 = num + 1;
				num = num1;
				if (num1 == strs.Count)
				{
					continue;
				}
				stringBuilder.Append("\r\n");
			}
			return stringBuilder.ToString();
		}

		public override void Write(string value)
		{
			TextWriter consoleTextWriter;
			if (!string.IsNullOrEmpty(value))
			{
				if (!this.parent.IsStandardOutputRedirected || this.parent.IsInteractive)
				{
					consoleTextWriter = this.parent.ConsoleTextWriter;
				}
				else
				{
					consoleTextWriter = this.parent.StandardOutputWriter;
				}
				TextWriter textWriter = consoleTextWriter;
				if (!this.parent.IsRunningAsync)
				{
					textWriter.Write(value);
					return;
				}
				else
				{
					this.parent.OutputSerializer.Serialize(value);
					return;
				}
			}
			else
			{
				return;
			}
		}

		private ConsoleColor lastForegroundColor;
		private ConsoleColor lastBackgroundColor;

		public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			lock (this.instanceLock)
			{
				lastForegroundColor = this.RawUI.ForegroundColor;
				lastBackgroundColor = this.RawUI.BackgroundColor;
				this.RawUI.ForegroundColor = foregroundColor;
				this.RawUI.BackgroundColor = backgroundColor;
				try
				{
					this.Write(value);
				}
				finally
				{
					this.RawUI.ForegroundColor = lastForegroundColor;
					this.RawUI.BackgroundColor = lastBackgroundColor;
				}
			}
		}

		private void WriteBackSpace(Coordinates originalCursorPosition)
		{
			Coordinates cursorPosition = this.rawui.CursorPosition;
			if (cursorPosition != originalCursorPosition)
			{
				if (cursorPosition.X != 0)
				{
					if (cursorPosition.X > 0)
					{
						cursorPosition.X = cursorPosition.X - 1;
						this.BlankAtCursor(cursorPosition);
					}
					return;
				}
				else
				{
					if (cursorPosition.Y > originalCursorPosition.Y)
					{
						Size bufferSize = this.rawui.BufferSize;
						cursorPosition.X = bufferSize.Width - 1;
						cursorPosition.Y = cursorPosition.Y - 1;
						this.BlankAtCursor(cursorPosition);
						return;
					}
					else
					{
						return;
					}
				}
			}
			else
			{
				return;
			}
		}

		private void WriteChoiceHelper(string text, ConsoleColor fg, ConsoleColor bg, ref int lineLen, int lineLenMax)
		{
			string str;
			int num = this.RawUI.LengthInBufferCells(text);
			bool flag = false;
			if (lineLen + num <= lineLenMax)
			{
				lineLen = lineLen + num;
			}
			else
			{
				this.WriteLineToConsole();
				flag = true;
				lineLen = num;
			}
			ConsoleHostUserInterface consoleHostUserInterface = this;
			ConsoleColor consoleColor = fg;
			ConsoleColor consoleColor1 = bg;
			if (flag)
			{
				str = text.TrimEnd(null);
			}
			else
			{
				str = text;
			}
			consoleHostUserInterface.WriteToConsole(consoleColor, consoleColor1, str);
		}

		private void WriteChoicePrompt(string[,] hotkeysAndPlainLabels, Dictionary<int, bool> defaultChoiceKeys, bool shouldEmulateForMultipleChoiceSelection)
		{
			string str;
			ConsoleColor foregroundColor = this.RawUI.ForegroundColor;
			ConsoleColor backgroundColor = this.RawUI.BackgroundColor;
			Size windowSize = this.RawUI.WindowSize;
			int width = windowSize.Width - 1;
			int num = 0;
			string str1 = "[{0}] {1}  ";
			for (int i = 0; i < hotkeysAndPlainLabels.GetLength(1); i++)
			{
				ConsoleColor promptColor = this.PromptColor;
				if (defaultChoiceKeys.ContainsKey(i))
				{
					promptColor = this.DefaultPromptColor;
				}
				object[] objArray = new object[2];
				objArray[0] = hotkeysAndPlainLabels[0, i];
				objArray[1] = hotkeysAndPlainLabels[1, i];
				string str2 = string.Format(CultureInfo.InvariantCulture, str1, objArray);
				this.WriteChoiceHelper(str2, promptColor, backgroundColor, ref num, width);
				if (shouldEmulateForMultipleChoiceSelection)
				{
					this.WriteLineToConsole();
				}
			}
			this.WriteChoiceHelper(ConsoleHostUserInterfaceStrings.PromptForChoiceHelp, foregroundColor, backgroundColor, ref num, width);
			if (shouldEmulateForMultipleChoiceSelection)
			{
				this.WriteLineToConsole();
			}
			string str3 = "";
			if (defaultChoiceKeys.Count > 0)
			{
				string str4 = "";
				StringBuilder stringBuilder = new StringBuilder();
				foreach (int key in defaultChoiceKeys.Keys)
				{
					string str5 = hotkeysAndPlainLabels[0, key];
					if (string.IsNullOrEmpty(str5))
					{
						str5 = hotkeysAndPlainLabels[1, key];
					}
					object[] objArray1 = new object[2];
					objArray1[0] = str4;
					objArray1[1] = str5;
					stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0}{1}", objArray1));
					str4 = ",";
				}
				string str6 = stringBuilder.ToString();
				if (defaultChoiceKeys.Count != 1)
				{
					str3 = StringUtil.Format(ConsoleHostUserInterfaceStrings.DefaultChoicesForMultipleChoices, str6);
				}
				else
				{
					if (shouldEmulateForMultipleChoiceSelection)
					{
						str = StringUtil.Format(ConsoleHostUserInterfaceStrings.DefaultChoiceForMultipleChoices, str6);
					}
					else
					{
						str = StringUtil.Format(ConsoleHostUserInterfaceStrings.DefaultChoicePrompt, str6);
					}
					str3 = str;
				}
			}
			this.WriteChoiceHelper(str3, foregroundColor, backgroundColor, ref num, width);
		}

		public override void WriteDebugLine(string message)
		{
			bool flag = false;
			message = HostUtilities.RemoveGuidFromMessage(message, out flag);
			if (this.parent.ErrorFormat != Serialization.DataFormat.XML)
			{
				this.WriteWrappedLine(this.debugForegroundColor, this.debugBackgroundColor, StringUtil.Format(ConsoleHostUserInterface.debugFormatString, message));
				return;
			}
			else
			{
				this.parent.ErrorSerializer.Serialize(message, "debug");
				return;
			}
		}

		public override void WriteErrorLine(string value)
		{
			TextWriter consoleTextWriter;
			if (!string.IsNullOrEmpty(value))
			{
				if (!this.parent.IsStandardErrorRedirected || this.parent.IsInteractive)
				{
					consoleTextWriter = this.parent.ConsoleTextWriter;
				}
				else
				{
					consoleTextWriter = this.parent.StandardErrorWriter;
				}
				TextWriter textWriter = consoleTextWriter;
				if (this.parent.ErrorFormat != Serialization.DataFormat.XML)
				{
					if (textWriter != this.parent.ConsoleTextWriter)
					{
						this.parent.StandardErrorWriter.Write(string.Concat(value, "\r\n"));
						return;
					}
					else
					{
						this.WriteLine(this.errorForegroundColor, this.errorBackgroundColor, value);
						return;
					}
				}
				else
				{
					this.parent.ErrorSerializer.Serialize(string.Concat(value, "\r\n"));
					return;
				}
			}
			else
			{
				return;
			}
		}

		public override void WriteLine(string value)
		{
			lock (this.instanceLock)
			{
				this.Write(value);
				this.Write("\r\n");
			}
		}

		private void WriteLineToConsole(string text)
		{
			this.WriteToConsole(text, true);
			this.WriteToConsole("\r\n", true);
		}

		private void WriteLineToConsole()
		{
			this.WriteToConsole("\r\n", true);
		}

		private void WritePrintToken(string printToken, ref Coordinates originalCursorPosition)
		{
			Size bufferSize = this.rawui.BufferSize;
			Coordinates cursorPosition = this.rawui.CursorPosition;
			if (cursorPosition.Y >= bufferSize.Height - 1 && cursorPosition.X >= bufferSize.Width - 1 && originalCursorPosition.Y > 0)
			{
				originalCursorPosition.Y = originalCursorPosition.Y - 1;
			}
			this.WriteToConsole(printToken, false);
		}

		public override void WriteProgress(long sourceId, ProgressRecord record)
		{
			bool flag = false;
			if (record != null)
			{
				string str = HostUtilities.RemoveIdentifierInfoFromMessage(record.CurrentOperation, out flag);
				if (flag)
				{
					ProgressRecord progressRecord = new ProgressRecord(record);
					progressRecord.CurrentOperation = str;
					record = progressRecord;
				}
				if (this.parent.ErrorFormat != Serialization.DataFormat.XML)
				{
					lock (this.instanceLock)
					{
						this.HandleIncomingProgressRecord(sourceId, record);
					}
					return;
				}
				else
				{
					PSObject pSObject = new PSObject();
					pSObject.Properties.Add(new PSNoteProperty("SourceId", (object)sourceId));
					pSObject.Properties.Add(new PSNoteProperty("Record", record));
					this.parent.ErrorSerializer.Serialize(pSObject, "progress");
					return;
				}
			}
			else
			{
				return;
			}
		}

		internal void WriteToConsole(string value, bool transcribeResult)
		{
			SafeFileHandle activeScreenBufferHandle = ConsoleControl.GetActiveScreenBufferHandle();
			ConsoleControl.ConsoleModes mode = ConsoleControl.GetMode(activeScreenBufferHandle);
			if ((mode & (ConsoleControl.ConsoleModes.ProcessedInput | ConsoleControl.ConsoleModes.LineInput | ConsoleControl.ConsoleModes.ProcessedOutput | ConsoleControl.ConsoleModes.WrapEndOfLine)) != (ConsoleControl.ConsoleModes.ProcessedInput | ConsoleControl.ConsoleModes.LineInput | ConsoleControl.ConsoleModes.ProcessedOutput | ConsoleControl.ConsoleModes.WrapEndOfLine))
			{
				mode = mode | ConsoleControl.ConsoleModes.ProcessedInput | ConsoleControl.ConsoleModes.LineInput | ConsoleControl.ConsoleModes.ProcessedOutput | ConsoleControl.ConsoleModes.WrapEndOfLine;
				ConsoleControl.SetMode(activeScreenBufferHandle, mode);
			}
			this.PreWrite();
			ConsoleControl.WriteConsole(activeScreenBufferHandle, value);
			if (this.isInteractiveTestToolListening && this.parent.IsStandardOutputRedirected)
			{
				this.parent.StandardOutputWriter.Write(value);
			}
			if (!transcribeResult)
			{
				this.PostWrite();
				return;
			}
			else
			{
				this.PostWrite(value);
				return;
			}
		}

		private void WriteToConsole(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string text)
		{
			ConsoleColor consoleColor = this.RawUI.ForegroundColor;
			ConsoleColor consoleColor1 = this.RawUI.BackgroundColor;
			this.RawUI.ForegroundColor = foregroundColor;
			this.RawUI.BackgroundColor = backgroundColor;
			try
			{
				this.WriteToConsole(text, true);
			}
			finally
			{
				this.RawUI.ForegroundColor = consoleColor;
				this.RawUI.BackgroundColor = consoleColor1;
			}
		}

		public override void WriteVerboseLine(string message)
		{
			bool flag = false;
			message = HostUtilities.RemoveGuidFromMessage(message, out flag);
			if (this.parent.ErrorFormat != Serialization.DataFormat.XML)
			{
				this.WriteWrappedLine(this.verboseForegroundColor, this.verboseBackgroundColor, StringUtil.Format(ConsoleHostUserInterface.verboseFormatString, message));
				return;
			}
			else
			{
				this.parent.ErrorSerializer.Serialize(message, "verbose");
				return;
			}
		}

		public override void WriteWarningLine(string message)
		{
			bool flag = false;
			message = HostUtilities.RemoveGuidFromMessage(message, out flag);
			if (this.parent.ErrorFormat != Serialization.DataFormat.XML)
			{
				this.WriteWrappedLine(this.WarningForegroundColor, this.WarningBackgroundColor, StringUtil.Format(ConsoleHostUserInterface.warningFormatString, message));
				return;
			}
			else
			{
				this.parent.ErrorSerializer.Serialize(message, "warning");
				return;
			}
		}

		internal void WriteWrappedLine(ConsoleColor fg, ConsoleColor bg, string text)
		{
			this.WriteLine(fg, bg, this.WrapToCurrentWindowWidth(text));
		}

		private enum PromptCommonInputErrors
		{
			None,
			Format,
			Overflow
		}

		internal enum ReadLineResult
		{
			endedOnEnter,
			endedOnTab,
			endedOnShiftTab,
			endedOnBreak
		}

		internal struct Word
		{
			internal int CellCount;

			internal string Text;

			internal ConsoleHostUserInterface.WordFlags Flags;

		}

		[Flags]
		internal enum WordFlags
		{
			IsWhitespace = 1,
			IsNewline = 2
		}
	}
}