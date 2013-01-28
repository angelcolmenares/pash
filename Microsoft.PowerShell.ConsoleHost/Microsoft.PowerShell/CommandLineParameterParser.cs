using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.PowerShell
{
	internal class CommandLineParameterParser
	{
		private bool serverMode;

		private ConsoleHost parent;

		private ConsoleHostUserInterface ui;

		private bool showHelp;

		private bool showBanner;

		private bool showInitialPrompt;

		private bool noInteractive;

		private string bannerText;

		private string helpText;

		private bool abortStartup;

		private bool skipUserInit;

		private bool? staMode;

		private bool noExit;

		private bool readFromStdin;

		private bool noPrompt;

		private string commandLineCommand;

		private bool wasCommandEncoded;

		private int exitCode;

		private bool dirty;

		private Version ver;

		private Serialization.DataFormat outFormat;

		private Serialization.DataFormat inFormat;

		private Collection<CommandParameter> collectedArgs;

		private string file;

		private string executionPolicy;

		private bool importSystemModules;

		internal bool AbortStartup
		{
			get
			{
				return this.abortStartup;
			}
		}

		internal Collection<CommandParameter> Args
		{
			get
			{
				return this.collectedArgs;
			}
		}

		internal string ExecutionPolicy
		{
			get
			{
				return this.executionPolicy;
			}
		}

		internal int ExitCode
		{
			get
			{
				return this.exitCode;
			}
		}

		internal string File
		{
			get
			{
				return this.file;
			}
		}

		internal bool ImportSystemModules
		{
			get
			{
				return this.importSystemModules;
			}
		}

		internal string InitialCommand
		{
			get
			{
				return this.commandLineCommand;
			}
		}

		internal Serialization.DataFormat InputFormat
		{
			get
			{
				return this.inFormat;
			}
		}

		internal bool NoExit
		{
			get
			{
				return this.noExit;
			}
		}

		internal bool NoPrompt
		{
			get
			{
				return this.noPrompt;
			}
		}

		internal Serialization.DataFormat OutputFormat
		{
			get
			{
				return this.outFormat;
			}
		}

		internal bool ReadFromStdin
		{
			get
			{
				return this.readFromStdin;
			}
		}

		internal bool ServerMode
		{
			get
			{
				return this.serverMode;
			}
		}

		internal bool ShowInitialPrompt
		{
			get
			{
				return this.showInitialPrompt;
			}
		}

		internal bool SkipProfiles
		{
			get
			{
				return this.skipUserInit;
			}
		}

		internal bool StaMode
		{
			get
			{
				if (!this.staMode.HasValue)
				{
					return true;
				}
				else
				{
					return this.staMode.Value;
				}
			}
		}

		internal bool ThrowOnReadAndPrompt
		{
			get
			{
				return this.noInteractive;
			}
		}

		internal bool WasInitialCommandEncoded
		{
			get
			{
				return this.wasCommandEncoded;
			}
		}

		internal CommandLineParameterParser(ConsoleHost p, Version ver, string bannerText, string helpText)
		{
			this.showBanner = true;
			this.staMode = null;
			this.noExit = true;
			this.collectedArgs = new Collection<CommandParameter>();
			this.bannerText = bannerText;
			this.helpText = helpText;
			this.parent = p;
			this.ui = (ConsoleHostUserInterface)p.UI;
			this.ver = ver;
		}

		private bool CollectArgs(string[] args, ref int i)
		{
			bool flag;
			if (this.collectedArgs.Count == 0)
			{
				i = i + 1;
				if (i < (int)args.Length)
				{
					try
					{
						object[] argsConverter = StringToBase64Converter.Base64ToArgsConverter(args[i]);
						if (argsConverter != null)
						{
							object[] objArray = argsConverter;
							for (int num = 0; num < (int)objArray.Length; num++)
							{
								object obj = objArray[num];
								this.collectedArgs.Add(new CommandParameter(null, obj));
							}
						}
						return true;
					}
					catch
					{
						this.ui.WriteErrorLine(CommandLineParameterParserStrings.BadArgsValue);
						this.showHelp = true;
						this.abortStartup = true;
						this.exitCode = -196608;
						flag = false;
					}
					return flag;
				}
				else
				{
					this.ui.WriteErrorLine(CommandLineParameterParserStrings.MissingArgsValue);
					this.showHelp = true;
					this.abortStartup = true;
					this.exitCode = -196608;
					return false;
				}
			}
			else
			{
				this.ui.WriteErrorLine(CommandLineParameterParserStrings.ArgsAlreadySpecified);
				this.showHelp = true;
				this.abortStartup = true;
				this.exitCode = -196608;
				return false;
			}
		}

		private bool MatchSwitch(string switchKey, string match, string smallestUnambiguousMatch)
		{
			if (match.Trim().ToLowerInvariant().IndexOf(switchKey, StringComparison.Ordinal) != 0 || switchKey.Length < smallestUnambiguousMatch.Length)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		internal void Parse(string[] args)
		{
			this.dirty = true;
			this.ParseHelper(args);
		}

		private bool ParseCommand(string[] args, ref int i, bool noexitSeen, bool isEncoded)
		{
			bool flag;
			if (this.commandLineCommand == null)
			{
				i = i + 1;
				if (i < (int)args.Length)
				{
					if (!isEncoded)
					{
						if (args[i] != "-")
						{
							StringBuilder stringBuilder = new StringBuilder();
							while (i < (int)args.Length)
							{
								stringBuilder.Append(string.Concat(args[i], " "));
								i = i + 1;
							}
							if (stringBuilder.Length > 0)
							{
								stringBuilder.Remove(stringBuilder.Length - 1, 1);
							}
							this.commandLineCommand = stringBuilder.ToString();
						}
						else
						{
							this.readFromStdin = true;
							this.noPrompt = true;
							i = i + 1;
							if (i == (int)args.Length)
							{
								if (!this.parent.IsStandardInputRedirected)
								{
									this.ui.WriteErrorLine(CommandLineParameterParserStrings.StdinNotRedirected);
									this.showHelp = true;
									this.abortStartup = true;
									this.exitCode = -196608;
									return false;
								}
							}
							else
							{
								this.ui.WriteErrorLine(CommandLineParameterParserStrings.TooManyParametersToCommand);
								this.showHelp = true;
								this.abortStartup = true;
								this.exitCode = -196608;
								return false;
							}
						}
					}
					else
					{
						try
						{
							this.commandLineCommand = StringToBase64Converter.Base64ToString(args[i]);
							if (!noexitSeen)
							{
								this.noExit = false;
							}
							this.showBanner = false;
							return true;
						}
						catch
						{
							this.ui.WriteErrorLine(CommandLineParameterParserStrings.BadCommandValue);
							this.showHelp = true;
							this.abortStartup = true;
							this.exitCode = -196608;
							flag = false;
						}
						return flag;
					}
					if (!noexitSeen)
					{
						this.noExit = false;
					}
					this.showBanner = false;
					return true;
				}
				else
				{
					this.ui.WriteErrorLine(CommandLineParameterParserStrings.MissingCommandParameter);
					this.showHelp = true;
					this.abortStartup = true;
					this.exitCode = -196608;
					return false;
				}
			}
			else
			{
				this.ui.WriteErrorLine(CommandLineParameterParserStrings.CommandAlreadySpecified);
				this.showHelp = true;
				this.abortStartup = true;
				this.exitCode = -196608;
				return false;
			}
		}

		private void ParseExecutionPolicy(string[] args, ref int i, ref string executionPolicy, string resourceStr)
		{
			i = i + 1;
			if (i < (int)args.Length)
			{
				executionPolicy = args[i];
				return;
			}
			else
			{
				this.ui.WriteErrorLine(resourceStr);
				this.showHelp = true;
				this.abortStartup = true;
				this.exitCode = -196608;
				return;
			}
		}

		private void ParseFormat(string[] args, ref int i, ref Serialization.DataFormat format, string resourceStr)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string[] names = Enum.GetNames(typeof(Serialization.DataFormat));
			for (int num = 0; num < (int)names.Length; num++)
			{
				string str = names[num];
				stringBuilder.Append(str);
				stringBuilder.Append("\r\n");
			}
			i = i + 1;
			if (i < (int)args.Length)
			{
				try
				{
					format = (Serialization.DataFormat)Enum.Parse(typeof(Serialization.DataFormat), args[i], true);
				}
				catch (ArgumentException argumentException)
				{
					this.ui.WriteErrorLine(StringUtil.Format(CommandLineParameterParserStrings.BadFormatParameterValue, args[i], stringBuilder.ToString()));
					this.showHelp = true;
					this.abortStartup = true;
					this.exitCode = -196608;
				}
				return;
			}
			else
			{
				this.ui.WriteErrorLine(StringUtil.Format(resourceStr, stringBuilder.ToString()));
				this.showHelp = true;
				this.abortStartup = true;
				this.exitCode = -196608;
				return;
			}
		}

		private void ParseHelper(string[] args)
		{
			bool flag = false;
			for (int i = 0; i < (int)args.Length; i++)
			{
				string lowerInvariant = args[i].Trim().ToLowerInvariant();
				if (!string.IsNullOrEmpty(lowerInvariant))
				{
					if (SpecialCharacters.IsDash(lowerInvariant[0]) || lowerInvariant[0] == '/')
					{
						lowerInvariant = lowerInvariant.Substring(1);
						if (this.MatchSwitch(lowerInvariant, "help", "h") || this.MatchSwitch(lowerInvariant, "?", "?"))
						{
							this.showHelp = true;
							this.abortStartup = true;
						}
						else
						{
							if (!this.MatchSwitch(lowerInvariant, "noexit", "noe"))
							{
								if (!this.MatchSwitch(lowerInvariant, "importsystemmodules", "imp"))
								{
									if (!this.MatchSwitch(lowerInvariant, "showinitialprompt", "show"))
									{
										if (!this.MatchSwitch(lowerInvariant, "noprofile", "nop"))
										{
											if (!this.MatchSwitch(lowerInvariant, "nologo", "nol"))
											{
												if (!this.MatchSwitch(lowerInvariant, "noninteractive", "noni"))
												{
													if (!this.MatchSwitch(lowerInvariant, "servermode", "s"))
													{
														if (!this.MatchSwitch(lowerInvariant, "command", "c"))
														{
															if (!this.MatchSwitch(lowerInvariant, "windowstyle", "w"))
															{
																if (!this.MatchSwitch(lowerInvariant, "file", "f"))
																{
																	if (this.MatchSwitch(lowerInvariant, "outputformat", "o") || this.MatchSwitch(lowerInvariant, "of", "o"))
																	{
																		this.ParseFormat(args, ref i, ref this.outFormat, CommandLineParameterParserStrings.MissingOutputFormatParameter);
																	}
																	else
																	{
																		if (this.MatchSwitch(lowerInvariant, "inputformat", "i") || this.MatchSwitch(lowerInvariant, "if", "i"))
																		{
																			this.ParseFormat(args, ref i, ref this.inFormat, CommandLineParameterParserStrings.MissingInputFormatParameter);
																		}
																		else
																		{
																			if (this.MatchSwitch(lowerInvariant, "executionpolicy", "ex") || this.MatchSwitch(lowerInvariant, "ep", "ep"))
																			{
																				this.ParseExecutionPolicy(args, ref i, ref this.executionPolicy, CommandLineParameterParserStrings.MissingExecutionPolicyParameter);
																			}
																			else
																			{
																				if (this.MatchSwitch(lowerInvariant, "encodedcommand", "e") || this.MatchSwitch(lowerInvariant, "ec", "e"))
																				{
																					this.wasCommandEncoded = true;
																					if (!this.ParseCommand(args, ref i, flag, true))
																					{
																						break;
																					}
																				}
																				else
																				{
																					if (this.MatchSwitch(lowerInvariant, "encodedarguments", "encodeda") || this.MatchSwitch(lowerInvariant, "ea", "ea"))
																					{
																						if (!this.CollectArgs(args, ref i))
																						{
																							break;
																						}
																					}
																					else
																					{
																						if (!this.MatchSwitch(lowerInvariant, "sta", "s"))
																						{
																							if (!this.MatchSwitch(lowerInvariant, "mta", "mta"))
																							{
																								i--;
																								if (!this.ParseCommand(args, ref i, flag, false))
																								{
																									break;
																								}
																							}
																							else
																							{
																								if (!this.staMode.HasValue)
																								{
																									this.staMode = new bool?(false);
																								}
																								else
																								{
																									this.ui.WriteErrorLine(CommandLineParameterParserStrings.MtaStaMutuallyExclusive);
																									this.showHelp = false;
																									this.showBanner = false;
																									this.abortStartup = true;
																									this.exitCode = -196608;
																									break;
																								}
																							}
																						}
																						else
																						{
																							if (!this.staMode.HasValue)
																							{
																								this.staMode = new bool?(true);
																							}
																							else
																							{
																								this.ui.WriteErrorLine(CommandLineParameterParserStrings.MtaStaMutuallyExclusive);
																								this.showHelp = false;
																								this.showBanner = false;
																								this.abortStartup = true;
																								this.exitCode = -196608;
																								break;
																							}
																						}
																					}
																				}
																			}
																		}
																	}
																}
																else
																{
																	i++;
																	if (i < (int)args.Length)
																	{
																		if (!flag)
																		{
																			this.showBanner = false;
																		}
																		if (!flag)
																		{
																			this.noExit = false;
																		}
																		if (args[i] != "-")
																		{
																			string message = null;
																			try
																			{
																				this.file = Path.GetFullPath(args[i]);
																			}
																			catch (Exception exception1)
																			{
																				Exception exception = exception1;
																				ConsoleHost.CheckForSevereException(exception);
																				message = exception.Message;
																			}
																			if (message == null)
																			{
																				if (Path.GetExtension(this.file).Equals(".ps1", StringComparison.OrdinalIgnoreCase))
																				{
																					if (System.IO.File.Exists(this.file))
																					{
																						i++;
																						Regex regex = new Regex("^.\\w+\\:", RegexOptions.CultureInvariant);
																						string str = null;
																						while (i < (int)args.Length)
																						{
																							string str1 = args[i];
																							if (str == null)
																							{
																								if (string.IsNullOrEmpty(str1) || !SpecialCharacters.IsDash(str1[0]))
																								{
																									this.collectedArgs.Add(new CommandParameter(null, str1));
																								}
																								else
																								{
																									Match match = regex.Match(str1);
																									if (!match.Success)
																									{
																										this.collectedArgs.Add(new CommandParameter(str1));
																									}
																									else
																									{
																										int num = str1.IndexOf(':');
																										if (num != str1.Length - 1)
																										{
																											this.collectedArgs.Add(new CommandParameter(str1.Substring(0, num), str1.Substring(num + 1)));
																										}
																										else
																										{
																											char[] chrArray = new char[1];
																											chrArray[0] = ':';
																											str = str1.TrimEnd(chrArray);
																										}
																									}
																								}
																							}
																							else
																							{
																								this.collectedArgs.Add(new CommandParameter(str, str1));
																								str = null;
																							}
																							i++;
																						}
																						break;
																					}
																					else
																					{
																						object[] objArray = new object[1];
																						objArray[0] = args[i];
																						this.ui.WriteErrorLine(string.Format(CultureInfo.CurrentCulture, CommandLineParameterParserStrings.ArgumentFileDoesNotExist, objArray));
																						this.showHelp = false;
																						this.abortStartup = true;
																						this.exitCode = -196608;
																						break;
																					}
																				}
																				else
																				{
																					object[] objArray1 = new object[1];
																					objArray1[0] = args[i];
																					this.ui.WriteErrorLine(string.Format(CultureInfo.CurrentCulture, CommandLineParameterParserStrings.InvalidFileArgumentExtension, objArray1));
																					this.showHelp = false;
																					this.abortStartup = true;
																					this.exitCode = -196608;
																					break;
																				}
																			}
																			else
																			{
																				object[] objArray2 = new object[2];
																				objArray2[0] = args[i];
																				objArray2[1] = message;
																				this.ui.WriteErrorLine(string.Format(CultureInfo.CurrentCulture, CommandLineParameterParserStrings.InvalidFileArgument, objArray2));
																				this.showHelp = false;
																				this.abortStartup = true;
																				this.exitCode = -196608;
																				break;
																			}
																		}
																		else
																		{
																			this.readFromStdin = true;
																			this.noPrompt = false;
																			break;
																		}
																	}
																	else
																	{
																		this.ui.WriteErrorLine(CommandLineParameterParserStrings.MissingFileArgument);
																		this.showHelp = true;
																		this.abortStartup = true;
																		this.exitCode = -196608;
																		break;
																	}
																}
															}
															else
															{
																i++;
																if (i < (int)args.Length)
																{
																	try
																	{
																		ProcessWindowStyle processWindowStyle = (ProcessWindowStyle)LanguagePrimitives.ConvertTo(args[i], typeof(ProcessWindowStyle), CultureInfo.InvariantCulture);
																		ConsoleControl.SetConsoleMode(processWindowStyle);
																	}
																	catch (PSInvalidCastException pSInvalidCastException1)
																	{
																		PSInvalidCastException pSInvalidCastException = pSInvalidCastException1;
																		object[] message1 = new object[2];
																		message1[0] = args[i];
																		message1[1] = pSInvalidCastException.Message;
																		this.ui.WriteErrorLine(string.Format(CultureInfo.CurrentCulture, CommandLineParameterParserStrings.InvalidWindowStyleArgument, message1));
																		this.showHelp = false;
																		this.showBanner = false;
																		this.abortStartup = true;
																		this.exitCode = -196608;
																		break;
																	}
																}
																else
																{
																	this.ui.WriteErrorLine(CommandLineParameterParserStrings.MissingWindowStyleArgument);
																	this.showHelp = false;
																	this.showBanner = false;
																	this.abortStartup = true;
																	this.exitCode = -196608;
																	break;
																}
															}
														}
														else
														{
															if (!this.ParseCommand(args, ref i, flag, false))
															{
																break;
															}
														}
													}
													else
													{
														this.serverMode = true;
													}
												}
												else
												{
													this.noInteractive = true;
													if (ConsoleHost.DefaultInitialSessionState != null)
													{
														ConsoleHost.DefaultInitialSessionState.WarmUpTabCompletionOnIdle = false;
													}
												}
											}
											else
											{
												this.showBanner = false;
											}
										}
										else
										{
											this.skipUserInit = true;
										}
									}
									else
									{
										this.showInitialPrompt = true;
									}
								}
								else
								{
									this.importSystemModules = true;
								}
							}
							else
							{
								this.noExit = true;
								flag = true;
							}
						}
					}
					else
					{
						i--;
						this.ParseCommand(args, ref i, flag, false);
						break;
					}
				}
			}
			if (this.showHelp)
			{
				this.ShowHelp();
			}
			if (this.showBanner && !this.showHelp)
			{
				this.ShowBanner();
			}
		}

		private void ShowBanner()
		{
			if (!this.showInitialPrompt && !string.IsNullOrEmpty(this.bannerText))
			{
				this.ui.WriteLine(this.bannerText);
				this.ui.WriteLine();
			}
		}

		private void ShowHelp()
		{
			this.ui.WriteLine("");
			if (this.helpText != null)
			{
				this.ui.Write(this.helpText);
			}
			else
			{
				this.ui.WriteLine(CommandLineParameterParserStrings.DefaultHelp);
			}
			this.ui.WriteLine("");
		}
	}
}