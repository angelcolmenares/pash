using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Management.Automation.Language;
using System.Management.Automation.Remoting;
using System.Management.Automation.Remoting.Server;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Security;
using System.Management.Automation.Sqm;
using System.Management.Automation.Tracing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace Microsoft.PowerShell
{
	internal sealed class ConsoleHost : PSHost, IDisposable, IHostSupportsInteractiveSession
	{
		internal const uint ExitCodeSuccess = 0;

		internal const uint ExitCodeCtrlBreak = 0xfffe0000;

		internal const uint ExitCodeInitFailure = 0xffff0000;

		internal const uint ExitCodeBadCommandLineParameter = 0xfffd0000;

		private const string resBaseName = "ConsoleHostStrings";

		private static CommandLineParameterParser cpp;

		private bool _isRunspacePushed;

		private PSObject consoleColorProxy;

		internal bool promptDisplayedInNativeCode;

		internal ManualResetEvent runspaceOpenedWaitHandle;

		private bool runspaceIsReady;

		internal int ExitCode;

		private RunspaceRef runspaceRef;

		private GCHandle breakHandlerGcHandle;

		private Thread breakHandlerThread;

		private bool isDisposed;

		internal ConsoleHostUserInterface ui;

		private string savedWindowTitle;

		private Guid id;

		private Version ver;

		private int exitCodeFromRunspace;

		private bool noExit;

		private bool isCtrlCDisabled;

		private bool setShouldExitCalled;

		private Serialization.DataFormat outputFormat;

		private Serialization.DataFormat inputFormat;

		private bool isRunningPromptLoop;

		private bool wasInitialCommandEncoded;

		private RunspaceConfiguration configuration;

		internal object hostGlobalLock;

		private bool shouldEndSession;

		private int beginApplicationNotifyCount;

		private bool isStandardOutputRedirectionDetermined;

		private bool isStandardOutputRedirected;

		private TextWriter standardOutputWriter;

		private bool isStandardErrorRedirectionDetermined;

		private bool isStandardErrorRedirected;

		private TextWriter standardErrorWriter;

		private bool isStandardInputRedirectionDetermined;

		private bool isStandardInputRedirected;

		private TextReader standardInputReader;

		private ConsoleHost.InitializeStandardHandleDelegate initStandardOutDelegate;

		private ConsoleHost.InitializeStandardHandleDelegate initStandardInDelegate;

		private ConsoleHost.InitializeStandardHandleDelegate initStandardErrorDelegate;

		private ConsoleTextWriter consoleWriter;

		private WrappedSerializer outputSerializer;

		private WrappedSerializer errorSerializer;

		private HostUtilities.DebuggerCommandProcessor debuggerCommandProcessor;

		private bool inDebugMode;

		private bool displayDebuggerBanner;

		private DebuggerStopEventArgs debuggerStopEventArgs;

		private static ConsoleHost theConsoleHost;

		private static string exitOnCtrlBreakMessage;

		internal static InitialSessionState DefaultInitialSessionState;

		[TraceSource("ConsoleHost", "ConsoleHost subclass of S.M.A.PSHost")]
		private static PSTraceSource tracer;

		[TraceSource("ConsoleHostRunspaceInit", "Initialization code for ConsoleHost's Runspace")]
		private static PSTraceSource runspaceInitTracer;

		private bool isTranscribing;

		private string transcriptFileName;

		private StreamWriter transcriptionWriter;

		private object transcriptionStateLock;

		internal TextWriter ConsoleTextWriter
		{
			get
			{
				return this.consoleWriter;
			}
		}

		public override CultureInfo CurrentCulture
		{
			get
			{
				CultureInfo culture;
				lock (this.hostGlobalLock)
				{
					culture = NativeCultureResolver.Culture;
				}
				return culture;
			}
		}

		public override CultureInfo CurrentUICulture
		{
			get
			{
				CultureInfo uICulture;
				lock (this.hostGlobalLock)
				{
					uICulture = NativeCultureResolver.UICulture;
				}
				return uICulture;
			}
		}

		internal Serialization.DataFormat ErrorFormat
		{
			get
			{
				Serialization.DataFormat dataFormat = this.outputFormat;
				if (!this.IsInteractive && this.IsStandardErrorRedirected && this.wasInitialCommandEncoded)
				{
					dataFormat = Serialization.DataFormat.XML;
				}
				return dataFormat;
			}
		}

		internal WrappedSerializer ErrorSerializer
		{
			get
			{
				TextWriter standardErrorWriter;
				if (this.errorSerializer == null)
				{
					ConsoleHost wrappedSerializer = this;
					Serialization.DataFormat errorFormat = this.ErrorFormat;
					string str = "Error";
					if (this.IsStandardErrorRedirected)
					{
						standardErrorWriter = this.StandardErrorWriter;
					}
					else
					{
						standardErrorWriter = this.ConsoleTextWriter;
					}
					wrappedSerializer.errorSerializer = new WrappedSerializer(errorFormat, str, standardErrorWriter);
				}
				return this.errorSerializer;
			}
		}

		private bool InDebugMode
		{
			get
			{
				return this.inDebugMode;
			}
		}

		internal Serialization.DataFormat InputFormat
		{
			get
			{
				return this.inputFormat;
			}
		}

		public override Guid InstanceId
		{
			get
			{
				return this.id;
			}
		}

		internal bool IsInteractive
		{
			get
			{
				if (!this.isRunningPromptLoop)
				{
					return false;
				}
				else
				{
					return !this.ui.ReadFromStdin;
				}
			}
		}

		internal bool IsRunningAsync
		{
			get
			{
				if (!this.IsInteractive)
				{
					if (this.OutputFormat != Serialization.DataFormat.Text || this.IsStandardInputRedirected && !this.ui.ReadFromStdin)
					{
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
		}

		public bool IsRunspacePushed
		{
			get
			{
				return this._isRunspacePushed;
			}
		}

		internal bool IsStandardErrorRedirected
		{
			get
			{
				if (this.initStandardErrorDelegate == null)
				{
					this.initStandardErrorDelegate = new ConsoleHost.InitializeStandardHandleDelegate(this.InitializeStandardErrorWriter);
				}
				return this.IsStandardHandleRedirected(-12, ref this.isStandardErrorRedirectionDetermined, ref this.isStandardErrorRedirected, this.initStandardErrorDelegate);
			}
		}

		internal bool IsStandardInputRedirected
		{
			get
			{
				if (this.initStandardInDelegate == null)
				{
					this.initStandardInDelegate = new ConsoleHost.InitializeStandardHandleDelegate(this.InitializeStandardInputReader);
				}
				return this.IsStandardHandleRedirected((long)(ConsoleControl.StandardHandleId.Input | ConsoleControl.StandardHandleId.Error), ref this.isStandardInputRedirectionDetermined, ref this.isStandardInputRedirected, this.initStandardInDelegate);
			}
		}

		internal bool IsStandardOutputRedirected
		{
			get
			{
				if (this.initStandardOutDelegate == null)
				{
					this.initStandardOutDelegate = new ConsoleHost.InitializeStandardHandleDelegate(this.InitializeStandardOutputWriter);
				}
				return this.IsStandardHandleRedirected((long)(ConsoleControl.StandardHandleId.Output | ConsoleControl.StandardHandleId.Error), ref this.isStandardOutputRedirectionDetermined, ref this.isStandardOutputRedirected, this.initStandardOutDelegate);
			}
		}

		internal bool IsTranscribing
		{
			get
			{
				return this.isTranscribing;
			}
			set
			{
				this.isTranscribing = value;
			}
		}

		internal LocalRunspace LocalRunspace
		{
			get
			{
				if (!this._isRunspacePushed)
				{
					if (this.RunspaceRef != null)
					{
						return this.RunspaceRef.Runspace as LocalRunspace;
					}
					else
					{
						return null;
					}
				}
				else
				{
					return this.RunspaceRef.OldRunspace as LocalRunspace;
				}
			}
		}

		public override string Name
		{
			get
			{
				return "ConsoleHost";
			}
		}

		internal Serialization.DataFormat OutputFormat
		{
			get
			{
				return this.outputFormat;
			}
		}

		internal WrappedSerializer OutputSerializer
		{
			get
			{
				TextWriter standardOutputWriter;
				if (this.outputSerializer == null)
				{
					ConsoleHost wrappedSerializer = this;
					Serialization.DataFormat dataFormat = this.outputFormat;
					string str = "Output";
					if (this.IsStandardOutputRedirected)
					{
						standardOutputWriter = this.StandardOutputWriter;
					}
					else
					{
						standardOutputWriter = this.ConsoleTextWriter;
					}
					wrappedSerializer.outputSerializer = new WrappedSerializer(dataFormat, str, standardOutputWriter);
				}
				return this.outputSerializer;
			}
		}

		public override PSObject PrivateData
		{
			get
			{
				if (this.ui != null)
				{
					if (this.consoleColorProxy == null)
					{
						this.consoleColorProxy = PSObject.AsPSObject(new ConsoleHost.ConsoleColorProxy(this.ui));
					}
					return this.consoleColorProxy;
				}
				else
				{
					return null;
				}
			}
		}

		public Runspace Runspace
		{
			get
			{
				if (this.RunspaceRef != null)
				{
					return this.RunspaceRef.Runspace;
				}
				else
				{
					return null;
				}
			}
		}

		internal RunspaceRef RunspaceRef
		{
			get
			{
				if (!this.runspaceIsReady && this.runspaceOpenedWaitHandle != null)
				{
					this.runspaceOpenedWaitHandle.WaitOne();
					if (this.ExitCode != -65536)
					{
						return this.runspaceRef;
					}
				}
				return this.runspaceRef;
			}
		}

		internal bool ShouldEndSession
		{
			get
			{
				bool flag = false;
				lock (this.hostGlobalLock)
				{
					flag = this.shouldEndSession;
				}
				return flag;
			}
			set
			{
				lock (this.hostGlobalLock)
				{
					this.shouldEndSession = value;
				}
			}
		}

		internal static ConsoleHost SingletonInstance
		{
			get
			{
				return ConsoleHost.theConsoleHost;
			}
		}

		internal TextWriter StandardErrorWriter
		{
			get
			{
				if (!this.IsStandardErrorRedirected)
				{
					return null;
				}
				else
				{
					return this.standardErrorWriter;
				}
			}
		}

		internal TextReader StandardInReader
		{
			get
			{
				if (!this.IsStandardInputRedirected)
				{
					return null;
				}
				else
				{
					return this.standardInputReader;
				}
			}
		}

		internal TextWriter StandardOutputWriter
		{
			get
			{
				if (!this.IsStandardOutputRedirected)
				{
					return null;
				}
				else
				{
					return this.standardOutputWriter;
				}
			}
		}

		public override PSHostUserInterface UI
		{
			get
			{
				return this.ui;
			}
		}

		public override Version Version
		{
			get
			{
				return this.ver;
			}
		}

		static ConsoleHost()
		{
			ConsoleHost.tracer = PSTraceSource.GetTracer("ConsoleHost", "ConsoleHost subclass of S.M.A.PSHost");
			ConsoleHost.runspaceInitTracer = PSTraceSource.GetTracer("ConsoleHostRunspaceInit", "Initialization code for ConsoleHost's Runspace", false);
		}

		internal ConsoleHost(RunspaceConfiguration configuration)
		{
			this.savedWindowTitle = "";
			this.id = Guid.NewGuid();
			this.ver = PSVersionInfo.PSVersion;
			this.noExit = true;
			this.hostGlobalLock = new object();
			this.transcriptFileName = string.Empty;
			this.transcriptionStateLock = new object();
			Thread.CurrentThread.CurrentUICulture = this.CurrentUICulture;
			Thread.CurrentThread.CurrentCulture = this.CurrentCulture;
			base.ShouldSetThreadUILanguageToZero = true;
			this.debuggerCommandProcessor = new HostUtilities.DebuggerCommandProcessor();
			this.inDebugMode = false;
			this.displayDebuggerBanner = true;
			this.configuration = configuration;
			this.ui = new ConsoleHostUserInterface(this);
			this.consoleWriter = new ConsoleTextWriter(this.ui);
			ConsoleHost.exitOnCtrlBreakMessage = ConsoleHostStrings.ExitOnCtrlBreakMessage;
			UnhandledExceptionEventHandler unhandledExceptionEventHandler = new UnhandledExceptionEventHandler(this.UnhandledExceptionHandler);
			AppDomain.CurrentDomain.UnhandledException += unhandledExceptionEventHandler;
		}

		private void BindBreakHandler()
		{
			this.breakHandlerGcHandle = GCHandle.Alloc(new ConsoleControl.BreakHandler(ConsoleHost.MyBreakHandler));
			ConsoleControl.AddBreakHandler((ConsoleControl.BreakHandler)this.breakHandlerGcHandle.Target);
		}

		internal static void CheckForSevereException(Exception e)
		{
			if (e as AccessViolationException != null || e as StackOverflowException != null)
			{
				WindowsErrorReporting.FailFast(e);
			}
		}

		private void CreateRunspace(object runspaceCreationArgs)
		{
			try
			{
				try
				{
					RunspaceCreationEventArgs runspaceCreationEventArg = runspaceCreationArgs as RunspaceCreationEventArgs;
					this.DoCreateRunspace(runspaceCreationEventArg.InitialCommand, runspaceCreationEventArg.SkipProfiles, runspaceCreationEventArg.StaMode, runspaceCreationEventArg.ImportSystemModules, runspaceCreationEventArg.InitialCommandArgs);
				}
				catch (ConsoleHost.ConsoleHostStartupException consoleHostStartupException1)
				{
					ConsoleHost.ConsoleHostStartupException consoleHostStartupException = consoleHostStartupException1;
					this.ReportExceptionFallback(consoleHostStartupException.InnerException, consoleHostStartupException.Message);
					this.ExitCode = -65536;
				}
			}
			finally
			{
				this.runspaceOpenedWaitHandle.Set();
			}
		}

		internal static ConsoleHost CreateSingletonInstance(RunspaceConfiguration configuration)
		{
			ConsoleHost.theConsoleHost = new ConsoleHost(configuration);
			return ConsoleHost.theConsoleHost;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool isDisposingNotFinalizing)
		{
			if (!this.isDisposed)
			{
				if (isDisposingNotFinalizing)
				{
					if (this.IsTranscribing)
					{
						this.StopTranscribing();
					}
					if (this.outputSerializer != null)
					{
						this.outputSerializer.End();
					}
					if (this.errorSerializer != null)
					{
						this.errorSerializer.End();
					}
					if (this.runspaceRef != null)
					{
						try
						{
							this.runspaceRef.Runspace.Dispose();
						}
						catch (InvalidRunspaceStateException invalidRunspaceStateException)
						{
						}
					}
					this.runspaceRef = null;
					this.hostGlobalLock = null;
					this.ui = null;
					if (this.runspaceOpenedWaitHandle != null)
					{
						this.runspaceOpenedWaitHandle.Close();
						this.runspaceOpenedWaitHandle = null;
					}
				}
				ConsoleControl.RemoveBreakHandler();
			 	if (this.breakHandlerGcHandle.IsAllocated) this.breakHandlerGcHandle.Free();
			}
			this.isDisposed = true;
		}

		private void DoCreateRunspace(string initialCommand, bool skipProfiles, bool staMode, bool importSystemModules, Collection<CommandParameter> initialCommandArgs)
		{
			ConsoleHost.runspaceInitTracer.WriteLine("Calling RunspaceFactory.CreateRunspace", new object[0]);
			if (ConsoleHost.DefaultInitialSessionState == null)
			{
				this.configuration.ImportSystemModules = importSystemModules;
				this.runspaceRef = new RunspaceRef(RunspaceFactory.CreateRunspace(this, this.configuration));
			}
			else
			{
				this.runspaceRef = new RunspaceRef(RunspaceFactory.CreateRunspace(this, ConsoleHost.DefaultInitialSessionState));
			}
			if (staMode)
			{
				this.runspaceRef.Runspace.ApartmentState = ApartmentState.STA;
				this.runspaceRef.Runspace.ThreadOptions = PSThreadOptions.ReuseThread;
			}
			this.runspaceRef.Runspace.EngineActivityId = EtwActivity.GetActivityId();
			ConsoleHost.runspaceInitTracer.WriteLine("Calling Runspace.Open", new object[0]);
			try
			{
				try
				{
					this.runspaceRef.Runspace.Open();
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					throw new ConsoleHost.ConsoleHostStartupException(ConsoleHostStrings.ShellCannotBeStarted, exception);
				}
			}
			finally
			{
				PSEtwLog.LogOperationalInformation(PSEventId.Perftrack_ConsoleStartupStop, PSOpcode.WinStop, PSTask.PowershellConsoleStartup, PSKeyword.UseAlwaysOperational, new object[0]);
			}
			this.runspaceIsReady = true;
			this.DoRunspaceInitialization(importSystemModules, skipProfiles, initialCommand, initialCommandArgs);
			this.runspaceOpenedWaitHandle.Set();
		}

		private void DoRunspaceInitialization(bool importSystemModules, bool skipProfiles, string initialCommand, Collection<CommandParameter> initialCommandArgs)
		{
			string shellId;
			Exception exception = null;
			Token[] tokenArray = null;
			ParseError[] parseErrorArray = null;
			Exception exception1 = null;
			Token[] tokenArray1 = null;
			ParseError[] parseErrorArray1 = null;
			this.runspaceRef.Runspace.Debugger.DebuggerStop += new EventHandler<DebuggerStopEventArgs>(this.OnExecutionSuspended);
			Executor executor = new Executor(this, false, false);
			if (importSystemModules)
			{
				this.InitializeRunspaceHelper("ImportSystemModules", executor, Executor.ExecutionOptions.None);
			}
			RunspaceConfigurationEntryCollection<ScriptConfigurationEntry> scriptConfigurationEntries = new RunspaceConfigurationEntryCollection<ScriptConfigurationEntry>();
			if (this.configuration != null)
			{
				scriptConfigurationEntries = this.configuration.InitializationScripts;
			}
			if (scriptConfigurationEntries == null || scriptConfigurationEntries.Count == 0)
			{
				ConsoleHost.runspaceInitTracer.WriteLine("There are no built-in scripts to run", new object[0]);
			}
			else
			{
				foreach (ScriptConfigurationEntry scriptConfigurationEntry in scriptConfigurationEntries)
				{
					object[] name = new object[1];
					name[0] = scriptConfigurationEntry.Name;
					ConsoleHost.runspaceInitTracer.WriteLine("Running script: '{0}'", name);
					try
					{
						this.isCtrlCDisabled = true;
						Exception exception2 = this.InitializeRunspaceHelper(scriptConfigurationEntry.Definition, executor, Executor.ExecutionOptions.AddOutputter);
						if (exception2 != null)
						{
							throw new ConsoleHost.ConsoleHostStartupException(ConsoleHostStrings.InitScriptFailed, exception2);
						}
					}
					finally
					{
						this.isCtrlCDisabled = false;
					}
				}
			}
			if (this.configuration == null)
			{
				shellId = "Microsoft.PowerShell";
			}
			else
			{
				shellId = this.configuration.ShellId;
			}
			if (SystemPolicy.GetSystemLockdownPolicy() == SystemEnforcementMode.Enforce)
			{
				this.runspaceRef.Runspace.ExecutionContext.LanguageMode = PSLanguageMode.ConstrainedLanguage;
			}
			string fullProfileFileName = HostUtilities.GetFullProfileFileName(null, false);
			string str = HostUtilities.GetFullProfileFileName(shellId, false);
			string fullProfileFileName1 = HostUtilities.GetFullProfileFileName(null, true);
			string str1 = HostUtilities.GetFullProfileFileName(shellId, true);
			this.runspaceRef.Runspace.SessionStateProxy.SetVariable("PROFILE", HostUtilities.GetDollarProfile(fullProfileFileName, str, fullProfileFileName1, str1));
			if (skipProfiles)
			{
				ConsoleHost.tracer.WriteLine("-noprofile option specified: skipping profiles", new object[0]);
			}
			else
			{
				this.RunProfile(fullProfileFileName, executor);
				this.RunProfile(str, executor);
				this.RunProfile(fullProfileFileName1, executor);
				this.RunProfile(str1, executor);
			}
			if (ConsoleHost.cpp == null || ConsoleHost.cpp.File == null)
			{
				if (!string.IsNullOrEmpty(initialCommand))
				{
					ConsoleHost.tracer.WriteLine("running initial command", new object[0]);
					Pipeline pipeline = executor.CreatePipeline(initialCommand, true);
					if (initialCommandArgs != null)
					{
						foreach (CommandParameter initialCommandArg in initialCommandArgs)
						{
							pipeline.Commands[0].Parameters.Add(initialCommandArg);
						}
					}
					if (!this.IsRunningAsync)
					{
						executor.ExecuteCommandHelper(pipeline, out exception1, Executor.ExecutionOptions.AddOutputter);
					}
					else
					{
						Executor.ExecutionOptions executionOption = Executor.ExecutionOptions.AddOutputter;
						Ast ast = Parser.ParseInput(initialCommand, out tokenArray1, out parseErrorArray1);
						if (AstSearcher.IsUsingDollarInput(ast))
						{
							executionOption = executionOption | Executor.ExecutionOptions.ReadInputObjects;
						}
						executor.ExecuteCommandAsyncHelper(pipeline, out exception1, executionOption);
					}
					if (exception1 != null)
					{
						this.ReportException(exception1, executor);
					}
				}
			}
			else
			{
				string file = ConsoleHost.cpp.File;
				object[] objArray = new object[1];
				objArray[0] = file;
				ConsoleHost.tracer.WriteLine("running -file '{0}'", objArray);
				Pipeline pipeline1 = executor.CreatePipeline();
				Command command = new Command(file, false, false);
				pipeline1.Commands.Add(command);
				if (initialCommandArgs != null)
				{
					foreach (CommandParameter commandParameter in initialCommandArgs)
					{
						command.Parameters.Add(commandParameter);
					}
				}
				if (!this.noExit)
				{
					this.Runspace.ExecutionContext.ScriptCommandProcessorShouldRethrowExit = true;
				}
				if (!this.IsRunningAsync)
				{
					executor.ExecuteCommandHelper(pipeline1, out exception, Executor.ExecutionOptions.AddOutputter);
				}
				else
				{
					Executor.ExecutionOptions executionOption1 = Executor.ExecutionOptions.AddOutputter;
					Ast ast1 = Parser.ParseFile(file, out tokenArray, out parseErrorArray);
					if (AstSearcher.IsUsingDollarInput(ast1))
					{
						executionOption1 = executionOption1 | Executor.ExecutionOptions.ReadInputObjects;
					}
					executor.ExecuteCommandAsyncHelper(pipeline1, out exception, executionOption1);
				}
				if (exception != null)
				{
					this.ReportException(exception, executor);
					return;
				}
			}
		}

		private int DoRunspaceLoop(string initialCommand, bool skipProfiles, Collection<CommandParameter> initialCommandArgs, bool staMode, bool importSystemModules, bool showInitialPrompt)
		{
			bool valueOrDefault;
			this.ExitCode = 0;
			while (!this.ShouldEndSession)
			{
				this.promptDisplayedInNativeCode = showInitialPrompt;
				this.runspaceOpenedWaitHandle = new ManualResetEvent(false);
				RunspaceCreationEventArgs runspaceCreationEventArg = new RunspaceCreationEventArgs(initialCommand, skipProfiles, staMode, importSystemModules, initialCommandArgs);
				ThreadPool.QueueUserWorkItem(new WaitCallback(this.CreateRunspace), runspaceCreationEventArg);
				if (this.noExit || this.ui.ReadFromStdin)
				{
					this.EnterNestedPrompt();
				}
				else
				{
					this.ShouldEndSession = true;
				}
				this.runspaceOpenedWaitHandle.WaitOne();
				if (this.ExitCode == -65536)
				{
					break;
				}
				if (!this.setShouldExitCalled)
				{
					Executor executor = new Executor(this, false, false);
					bool? nullable = executor.ExecuteCommandAndGetResultAsBool("$global:?");
					if (nullable.HasValue)
					{
						valueOrDefault = nullable.GetValueOrDefault();
					}
					else
					{
						valueOrDefault = false;
					}
					bool flag = valueOrDefault;
					if (!flag)
					{
						this.ExitCode = 1;
					}
					else
					{
						this.ExitCode = 0;
					}
				}
				else
				{
					this.ExitCode = this.exitCodeFromRunspace;
				}
				this.runspaceRef.Runspace.Close();
				this.runspaceRef = null;
				if (!staMode)
				{
					continue;
				}
				this.ShouldEndSession = true;
			}
			return this.ExitCode;
		}

		private void EnterDebugMode()
		{
			this.inDebugMode = true;
			try
			{
				try
				{
					this.Runspace.ExecutionContext.EngineHostInterface.EnterNestedPrompt();
				}
				catch (PSNotImplementedException pSNotImplementedException)
				{
					this.WriteDebuggerMessage(ConsoleHostStrings.SessionDoesNotSupportDebugger);
				}
			}
			finally
			{
				this.inDebugMode = false;
			}
		}

		public override void EnterNestedPrompt()
		{
			bool isCommandCompletionRunning;
			Executor currentExecutor = Executor.CurrentExecutor;
			try
			{
				Executor.CurrentExecutor = null;
				if (currentExecutor != null)
				{
					isCommandCompletionRunning = true;
				}
				else
				{
					isCommandCompletionRunning = this.ui.IsCommandCompletionRunning;
				}
				bool flag = isCommandCompletionRunning;
				ConsoleHost.InputLoop.RunNewInputLoop(this, flag);
			}
			finally
			{
				Executor.CurrentExecutor = currentExecutor;
			}
		}

		internal static string EscapeSingleQuotes(string str)
		{
			StringBuilder stringBuilder = new StringBuilder(str.Length * 2);
			for (int i = 0; i < str.Length; i++)
			{
				char chr = str[i];
				if (chr == '\'')
				{
					stringBuilder.Append(chr);
				}
				stringBuilder.Append(chr);
			}
			string str1 = stringBuilder.ToString();
			ConsoleHost.tracer.WriteLine(str1, new object[0]);
			return str1;
		}

		private void ExitDebugMode(DebuggerResumeAction resumeAction)
		{
			this.debuggerStopEventArgs.ResumeAction = resumeAction;
			try
			{
				this.Runspace.ExecutionContext.EngineHostInterface.ExitNestedPrompt();
			}
			catch (ExitNestedPromptException exitNestedPromptException)
			{
			}
		}

		public override void ExitNestedPrompt()
		{
			lock (this.hostGlobalLock)
			{
				ConsoleHost.InputLoop.ExitCurrentLoop();
			}
		}

		~ConsoleHost()
		{
			try
			{
				this.Dispose(false);
			}
			finally
			{
				
			}
		}

		private static void HandleBreak()
		{
			Executor.CancelCurrentExecutor();
			SafeFileHandle inputHandle = ConsoleControl.GetInputHandle();
			ConsoleControl.FlushConsoleInputBuffer(inputHandle);
			ConsoleHost.SingletonInstance.breakHandlerThread = null;
		}

		private void HandleRemoteRunspaceStateChanged(object sender, RunspaceStateEventArgs eventArgs)
		{
			RunspaceState state = eventArgs.RunspaceStateInfo.State;
			RunspaceState runspaceState = state;
			switch (runspaceState)
			{
				case RunspaceState.Closed:
				case RunspaceState.Closing:
				case RunspaceState.Broken:
				case RunspaceState.Disconnected:
				{
					this.PopRunspace();
					return;
				}
				case RunspaceState.Disconnecting:
				{
					return;
				}
				default:
				{
					return;
				}
			}
		}

		private void InitializeRunspace(string initialCommand, bool skipProfiles, Collection<CommandParameter> initialCommandArgs)
		{
			this.runspaceOpenedWaitHandle = new ManualResetEvent(false);
			this.DoCreateRunspace(initialCommand, skipProfiles, false, false, initialCommandArgs);
		}

		private Exception InitializeRunspaceHelper(string command, Executor exec, Executor.ExecutionOptions options)
		{
			ConsoleHost.runspaceInitTracer.WriteLine(string.Concat("running command ", command), new object[0]);
			Exception exception = null;
			if (!this.IsRunningAsync)
			{
				exec.ExecuteCommand(command, out exception, options);
			}
			else
			{
				exec.ExecuteCommandAsync(command, out exception, options);
			}
			if (exception != null)
			{
				this.ReportException(exception, exec);
			}
			return exception;
		}

		[ArchitectureSensitive]
		private void InitializeStandardErrorWriter(IntPtr stdHandle)
		{
			this.standardErrorWriter = Console.Error;
		}

		[ArchitectureSensitive]
		private void InitializeStandardInputReader(IntPtr stdHandle)
		{
			int consoleCP = ConsoleControl.NativeMethods.GetConsoleCP();
			Encoding encoding = Encoding.GetEncoding(consoleCP);
			try
			{
				Stream fileStream = new FileStream(new SafeFileHandle(stdHandle, false), FileAccess.Read);
				this.standardInputReader = TextReader.Synchronized(new StreamReader(fileStream, encoding, false));
			}
			catch (IOException oException)
			{
				this.standardInputReader = TextReader.Synchronized(new StringReader(""));
			}
		}

		[ArchitectureSensitive]
		private void InitializeStandardOutputWriter(IntPtr stdHandle)
		{
			this.standardOutputWriter = Console.Out;
		}

		[ArchitectureSensitive]
		private bool IsStandardHandleRedirected(long handleId, ref bool isHandleRedirectionDetermined, ref bool isHandleRedirected, ConsoleHost.InitializeStandardHandleDelegate handleInit)
		{
			lock (this.hostGlobalLock)
			{
				if (!isHandleRedirectionDetermined)
				{
					isHandleRedirected = false;
					IntPtr stdHandle = ConsoleControl.GetStdHandle(handleId);
					int num = 0;
					bool consoleMode = ConsoleControl.NativeMethods.GetConsoleMode(stdHandle, out num);
					SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, false);
					if (!consoleMode && !safeFileHandle.IsInvalid)
					{
						isHandleRedirected = true;
						handleInit(stdHandle);
					}
					isHandleRedirectionDetermined = true;
				}
			}
            ConsoleHost.tracer.WriteLine(isHandleRedirected);
			return isHandleRedirected;
		}

		internal static bool MyBreakHandler(ConsoleControl.ConsoleBreakSignal signal)
		{
			ConsoleControl.ConsoleBreakSignal consoleBreakSignal = signal;
			switch (consoleBreakSignal)
			{
				case ConsoleControl.ConsoleBreakSignal.CtrlC:
				{
					ConsoleHost.SpinUpBreakHandlerThread(false);
					return true;
				}
				case ConsoleControl.ConsoleBreakSignal.CtrlBreak:
				{
					if (!ConsoleHost.SingletonInstance.IsRunspacePushed)
					{
						PSSQMAPI.LogAllDataSuppressExceptions();
						ConsoleHost.SingletonInstance.shouldEndSession = true;
						SafeFileHandle activeScreenBufferHandle = ConsoleControl.GetActiveScreenBufferHandle();
						ConsoleControl.WriteConsole(activeScreenBufferHandle, string.Concat("\n", ConsoleHost.exitOnCtrlBreakMessage));
						Environment.Exit(-131072);
						return true;
					}
					else
					{
						ConsoleHost.SingletonInstance.PopRunspace();
						ConsoleHost.HandleBreak();
						return true;
					}
				}
				case ConsoleControl.ConsoleBreakSignal.Close:
				case ConsoleControl.ConsoleBreakSignal.Shutdown:
				{
					ConsoleHost.SpinUpBreakHandlerThread(true);
					ConsoleHost.SingletonInstance.runspaceRef.Runspace.Close();
					PSSQMAPI.LogAllDataSuppressExceptions();
					return false;
				}
				case ConsoleControl.ConsoleBreakSignal.CtrlBreak | ConsoleControl.ConsoleBreakSignal.Close:
				/* case 4: */
				{
					ConsoleHost.SpinUpBreakHandlerThread(true);
					PSSQMAPI.LogAllDataSuppressExceptions();
					return false;
				}
				case ConsoleControl.ConsoleBreakSignal.Logoff:
				{
					return true;
				}
				default:
				{
					ConsoleHost.SpinUpBreakHandlerThread(true);
					PSSQMAPI.LogAllDataSuppressExceptions();
					return false;
				}
			}
		}

		public override void NotifyBeginApplication()
		{
			lock (this.hostGlobalLock)
			{
				ConsoleHost consoleHost = this;
				consoleHost.beginApplicationNotifyCount = consoleHost.beginApplicationNotifyCount + 1;
				if (this.beginApplicationNotifyCount == 1)
				{
					this.savedWindowTitle = this.ui.RawUI.WindowTitle;
				}
			}
		}

		public override void NotifyEndApplication()
		{
			lock (this.hostGlobalLock)
			{
				ConsoleHost consoleHost = this;
				consoleHost.beginApplicationNotifyCount = consoleHost.beginApplicationNotifyCount - 1;
				if (this.beginApplicationNotifyCount == 0)
				{
					this.ui.RawUI.WindowTitle = this.savedWindowTitle;
				}
			}
		}

		private void OnExecutionSuspended(object sender, DebuggerStopEventArgs e)
		{
			this.debuggerStopEventArgs = e;
			try
			{
				if (this.displayDebuggerBanner)
				{
					this.WriteDebuggerMessage(ConsoleHostStrings.EnteringDebugger);
					this.WriteDebuggerMessage("");
					this.displayDebuggerBanner = false;
				}
				if (e.Breakpoints.Count > 0)
				{
					string hitBreakpoint = ConsoleHostStrings.HitBreakpoint;
					foreach (Breakpoint breakpoint in e.Breakpoints)
					{
						object[] objArray = new object[1];
						objArray[0] = breakpoint;
						this.WriteDebuggerMessage(string.Format(CultureInfo.CurrentCulture, hitBreakpoint, objArray));
					}
					this.WriteDebuggerMessage("");
				}
				if (e.InvocationInfo != null)
				{
					this.WriteDebuggerMessage(e.InvocationInfo.PositionMessage);
				}
				this.debuggerCommandProcessor.Reset();
				this.EnterDebugMode();
			}
			finally
			{
				this.debuggerStopEventArgs = null;
			}
		}

		public void PopRunspace()
		{
			if (this.runspaceRef == null || !this.runspaceRef.IsRunspaceOverridden)
			{
				return;
			}
			else
			{
				lock (this.hostGlobalLock)
				{
					this.runspaceRef.Revert();
					this._isRunspacePushed = false;
				}
				this.RunspacePopped.SafeInvoke(this, EventArgs.Empty);
				return;
			}
		}

		internal HostUtilities.DebuggerCommand ProcessDebuggerCommand(string command)
		{
			return this.debuggerCommandProcessor.ProcessCommand(this, command, this.debuggerStopEventArgs.InvocationInfo);
		}

		public void PushRunspace(Runspace newRunspace)
		{
			if (this.runspaceRef != null)
			{
				RemoteRunspace remoteRunspace = newRunspace as RemoteRunspace;
				remoteRunspace.StateChanged += new EventHandler<RunspaceStateEventArgs>(this.HandleRemoteRunspaceStateChanged);
				this.runspaceRef.Override(remoteRunspace, this.hostGlobalLock, out this._isRunspacePushed);
				this.RunspacePushed.SafeInvoke(this, EventArgs.Empty);
				return;
			}
			else
			{
				return;
			}
		}

		private void ReportException(Exception e, Executor exec)
		{
			object errorRecord;
			Pipeline pipeline = exec.CreatePipeline();
			IContainsErrorRecord containsErrorRecord = e as IContainsErrorRecord;
			if (containsErrorRecord == null)
			{
				errorRecord = new ErrorRecord(e, "ConsoleHost.ReportException", ErrorCategory.NotSpecified, null);
			}
			else
			{
				errorRecord = containsErrorRecord.ErrorRecord;
			}
			PSObject pSObject = new PSObject(errorRecord);
			PSNoteProperty pSNoteProperty = new PSNoteProperty("writeErrorStream", true);
			pSObject.Properties.Add(pSNoteProperty);
			Exception exception = null;
			pipeline.Input.Write(pSObject);
			if (!this.IsRunningAsync)
			{
				exec.ExecuteCommandHelper(pipeline, out exception, Executor.ExecutionOptions.AddOutputter);
			}
			else
			{
				exec.ExecuteCommandAsyncHelper(pipeline, out exception, Executor.ExecutionOptions.AddOutputter);
			}
			if (exception != null)
			{
				this.ReportExceptionFallback(e, null);
			}
		}

		private void ReportExceptionFallback(Exception e, string header)
		{
			if (!string.IsNullOrEmpty(header))
			{
				Console.Error.WriteLine(header);
			}
			if (e != null)
			{
				ErrorRecord errorRecord = null;
				IContainsErrorRecord containsErrorRecord = e as IContainsErrorRecord;
				if (containsErrorRecord != null)
				{
					errorRecord = containsErrorRecord.ErrorRecord;
				}
				if (e as PSRemotingTransportException == null)
				{
					if (e as TargetInvocationException == null)
					{
						Console.Error.WriteLine(e.Message);
					}
					else
					{
						Console.Error.WriteLine(e.InnerException.Message);
					}
				}
				else
				{
					this.UI.WriteErrorLine(e.Message);
				}
				if (errorRecord != null && errorRecord.InvocationInfo != null)
				{
					Console.Error.WriteLine(errorRecord.InvocationInfo.PositionMessage);
				}
				return;
			}
			else
			{
				return;
			}
		}

		private int Run(CommandLineParameterParser cpp, bool isPrestartWarned)
		{
			int exitCode;
			ConsoleHost.runspaceInitTracer.WriteLine("starting parse of command line parameters", new object[0]);
			if (string.IsNullOrEmpty(cpp.InitialCommand) || !isPrestartWarned)
			{
				if (!cpp.AbortStartup)
				{
					this.outputFormat = cpp.OutputFormat;
					this.inputFormat = cpp.InputFormat;
					this.wasInitialCommandEncoded = cpp.WasInitialCommandEncoded;
					this.ui.ReadFromStdin = cpp.ReadFromStdin;
					this.ui.NoPrompt = cpp.NoPrompt;
					this.ui.ThrowOnReadAndPrompt = cpp.ThrowOnReadAndPrompt;
					this.noExit = cpp.NoExit;
					if (!string.IsNullOrEmpty(cpp.ExecutionPolicy))
					{
						ExecutionPolicy executionPolicy = SecuritySupport.ParseExecutionPolicy(cpp.ExecutionPolicy);
						SecuritySupport.SetExecutionPolicy(ExecutionPolicyScope.Process, executionPolicy, null);
					}
					exitCode = this.DoRunspaceLoop(cpp.InitialCommand, cpp.SkipProfiles, cpp.Args, cpp.StaMode, cpp.ImportSystemModules, cpp.ShowInitialPrompt);
				}
				else
				{
					ConsoleHost.tracer.WriteLine("processing of cmdline args failed, exiting", new object[0]);
					exitCode = cpp.ExitCode;
				}
			}
			else
			{
				object[] initialCommand = new object[1];
				initialCommand[0] = cpp.InitialCommand;
				ConsoleHost.tracer.TraceError("Start up warnings made command \"{0}\" not executed", initialCommand);
				string str = StringUtil.Format(ConsoleHostStrings.InitialCommandNotExecuted, cpp.InitialCommand);
				this.ui.WriteErrorLine(str);
				exitCode = -65536;
			}
			return exitCode;
		}

		private int Run(string bannerText, string helpText, bool isPrestartWarned, string[] args)
		{
			ConsoleHost.cpp = new CommandLineParameterParser(this, this.ver, bannerText, helpText);
			ConsoleHost.cpp.Parse(args);
			return this.Run(ConsoleHost.cpp, isPrestartWarned);
		}

		private void RunProfile(string profileFileName, Executor exec)
		{
			if (!string.IsNullOrEmpty(profileFileName))
			{
				ConsoleHost.runspaceInitTracer.WriteLine(string.Concat("checking profile", profileFileName), new object[0]);
				try
				{
					if (!File.Exists(profileFileName))
					{
						ConsoleHost.runspaceInitTracer.WriteLine("profile file not found", new object[0]);
					}
					else
					{
						this.InitializeRunspaceHelper(string.Concat(". '", ConsoleHost.EscapeSingleQuotes(profileFileName), "'"), exec, Executor.ExecutionOptions.AddOutputter);
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					CommandProcessorBase.CheckForSevereException(exception);
					this.ReportException(exception, exec);
					ConsoleHost.runspaceInitTracer.WriteLine("Could not load profile.", new object[0]);
				}
			}
		}

		public override void SetShouldExit(int exitCode)
		{
			lock (this.hostGlobalLock)
			{
				if (!this.IsRunspacePushed)
				{
					if (!this.inDebugMode)
					{
						this.setShouldExitCalled = true;
						this.exitCodeFromRunspace = exitCode;
						this.ShouldEndSession = true;
					}
					else
					{
						this.ExitDebugMode(DebuggerResumeAction.Continue);
					}
				}
				else
				{
					this.PopRunspace();
				}
			}
		}

		private static void SpinUpBreakHandlerThread(bool shouldEndSession)
		{
			ConsoleHost singletonInstance = ConsoleHost.SingletonInstance;
			lock (singletonInstance.hostGlobalLock)
			{
				if (!singletonInstance.isCtrlCDisabled)
				{
					Thread thread = singletonInstance.breakHandlerThread;
					if (!singletonInstance.ShouldEndSession && shouldEndSession)
					{
						singletonInstance.ShouldEndSession = shouldEndSession;
					}
					if (thread == null)
					{
						singletonInstance.breakHandlerThread = new Thread(new ThreadStart(ConsoleHost.HandleBreak));
						singletonInstance.breakHandlerThread.Name = "ConsoleHost.HandleBreak";
						singletonInstance.breakHandlerThread.Start();
					}
				}
			}
		}

		internal static int Start(RunspaceConfiguration configuration, string bannerText, string helpText, string preStartWarning, string[] args)
		{
			int num = 0;
			Thread.CurrentThread.Name = "ConsoleHost main thread";
			ConsoleHost.theConsoleHost = ConsoleHost.CreateSingletonInstance(configuration);
			ConsoleHost.theConsoleHost.BindBreakHandler();
			PSHost.IsStdOutputRedirected = ConsoleHost.theConsoleHost.IsStandardOutputRedirected;
			if (args == null)
			{
				args = new string[0];
			}
			if (!string.IsNullOrEmpty(preStartWarning))
			{
				ConsoleHost.theConsoleHost.UI.WriteWarningLine(preStartWarning);
			}
			using (ConsoleHost.theConsoleHost)
			{
				ConsoleHost.cpp = new CommandLineParameterParser(ConsoleHost.theConsoleHost, ConsoleHost.theConsoleHost.ver, bannerText, helpText);
				string[] strArrays = new string[args.GetLength(0)];
				args.CopyTo(strArrays, 0);
				ConsoleHost.cpp.Parse(strArrays);
				if (!ConsoleHost.cpp.ServerMode)
				{
					num = ConsoleHost.theConsoleHost.Run(ConsoleHost.cpp, !string.IsNullOrEmpty(preStartWarning));
				}
				else
				{
					OutOfProcessMediator.Run(ConsoleHost.cpp.InitialCommand);
					num = 0;
				}
			}
			return num;
		}

		internal void StartTranscribing(string transcriptFilename, bool shouldAppend)
		{
			lock (this.transcriptionStateLock)
			{
				this.transcriptFileName = transcriptFilename;
				this.transcriptionWriter = new StreamWriter(transcriptFilename, shouldAppend, new UnicodeEncoding());
				this.transcriptionWriter.AutoFlush = true;
				string transcriptPrologue = ConsoleHostStrings.TranscriptPrologue;
				object[] now = new object[5];
				now[0] = DateTime.Now;
				now[1] = Environment.UserDomainName;
				now[2] = Environment.UserName;
				now[3] = Environment.MachineName;
				now[4] = Environment.OSVersion.VersionString;
				string str = StringUtil.Format(transcriptPrologue, now);
				this.transcriptionWriter.WriteLine(str);
				this.isTranscribing = true;
			}
		}

		internal string StopTranscribing()
		{
			string str;
			lock (this.transcriptionStateLock)
			{
				if (this.transcriptionWriter != null)
				{
					try
					{
						this.transcriptionWriter.WriteLine(StringUtil.Format(ConsoleHostStrings.TranscriptEpilogue, DateTime.Now));
					}
					finally
					{
						try
						{
							this.transcriptionWriter.Close();
						}
						finally
						{
							this.transcriptionWriter = null;
							this.isTranscribing = false;
						}
					}
					str = this.transcriptFileName;
				}
				else
				{
					throw new InvalidOperationException(ConsoleHostStrings.HostNotTranscribing);
				}
			}
			return str;
		}

		private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
		{
			this.shouldEndSession = true;
			if (args != null)
			{
			}
			this.ui.WriteLine();
			this.ui.Write(ConsoleColor.Red, this.ui.RawUI.BackgroundColor, ConsoleHostStrings.UnhandledExceptionShutdownMessage);
			this.ui.WriteLine();
		}

		private void WriteDebuggerMessage(string line)
		{
			this.ui.WriteWrappedLine(this.ui.DebugForegroundColor, this.ui.DebugBackgroundColor, line);
		}

		private void WriteErrorLine(string line)
		{
			ConsoleColor consoleColor = ConsoleColor.Red;
			ConsoleColor backgroundColor = this.UI.RawUI.BackgroundColor;
			this.UI.WriteLine(consoleColor, backgroundColor, line);
		}

		internal void WriteToTranscript(string text)
		{
			lock (this.transcriptionStateLock)
			{
				if (this.isTranscribing && this.transcriptionWriter != null)
				{
					this.transcriptionWriter.Write(text);
				}
			}
		}

		internal event EventHandler RunspacePopped;
		internal event EventHandler RunspacePushed;
		public class ConsoleColorProxy
		{
			private ConsoleHostUserInterface ui;

			public ConsoleColor DebugBackgroundColor
			{
				get
				{
					return this.ui.DebugBackgroundColor;
				}
				set
				{
					this.ui.DebugBackgroundColor = value;
				}
			}

			public ConsoleColor DebugForegroundColor
			{
				get
				{
					return this.ui.DebugForegroundColor;
				}
				set
				{
					this.ui.DebugForegroundColor = value;
				}
			}

			public ConsoleColor ErrorBackgroundColor
			{
				get
				{
					return this.ui.ErrorBackgroundColor;
				}
				set
				{
					this.ui.ErrorBackgroundColor = value;
				}
			}

			public ConsoleColor ErrorForegroundColor
			{
				get
				{
					return this.ui.ErrorForegroundColor;
				}
				set
				{
					this.ui.ErrorForegroundColor = value;
				}
			}

			public ConsoleColor ProgressBackgroundColor
			{
				get
				{
					return this.ui.ProgressBackgroundColor;
				}
				set
				{
					this.ui.ProgressBackgroundColor = value;
				}
			}

			public ConsoleColor ProgressForegroundColor
			{
				get
				{
					return this.ui.ProgressForegroundColor;
				}
				set
				{
					this.ui.ProgressForegroundColor = value;
				}
			}

			public ConsoleColor VerboseBackgroundColor
			{
				get
				{
					return this.ui.VerboseBackgroundColor;
				}
				set
				{
					this.ui.VerboseBackgroundColor = value;
				}
			}

			public ConsoleColor VerboseForegroundColor
			{
				get
				{
					return this.ui.VerboseForegroundColor;
				}
				set
				{
					this.ui.VerboseForegroundColor = value;
				}
			}

			public ConsoleColor WarningBackgroundColor
			{
				get
				{
					return this.ui.WarningBackgroundColor;
				}
				set
				{
					this.ui.WarningBackgroundColor = value;
				}
			}

			public ConsoleColor WarningForegroundColor
			{
				get
				{
					return this.ui.WarningForegroundColor;
				}
				set
				{
					this.ui.WarningForegroundColor = value;
				}
			}

			public ConsoleColorProxy(ConsoleHostUserInterface ui)
			{
				if (ui != null)
				{
					this.ui = ui;
					return;
				}
				else
				{
					throw new ArgumentNullException("ui");
				}
			}
		}

		[Serializable]
		private class ConsoleHostStartupException : ApplicationException
		{
			internal ConsoleHostStartupException()
			{
			}

			internal ConsoleHostStartupException(string message) : base(message)
			{
			}

			protected ConsoleHostStartupException(SerializationInfo info, StreamingContext context) : base(info, context)
			{
			}

			internal ConsoleHostStartupException(string message, Exception innerException) : base(message, innerException)
			{
			}
		}

		private delegate void InitializeStandardHandleDelegate(IntPtr handle);

		private class InputLoop
		{
			private ConsoleHost parent;

			private bool isNested;

			private bool shouldExit;

			private Executor exec;

			private Executor promptExec;

			private object syncObject;

			private bool isRunspacePushed;

			private bool runspacePopped;

			private static Stack<ConsoleHost.InputLoop> instanceStack;

			static InputLoop()
			{
				ConsoleHost.InputLoop.instanceStack = new Stack<ConsoleHost.InputLoop>();
			}

			private InputLoop(ConsoleHost parent, bool isNested)
			{
				this.syncObject = new object();
				this.parent = parent;
				this.isNested = isNested;
				this.isRunspacePushed = parent.IsRunspacePushed;
				parent.RunspacePopped += new EventHandler(this.HandleRunspacePopped);
				parent.RunspacePushed += new EventHandler(this.HandleRunspacePushed);
				this.exec = new Executor(parent, isNested, false);
				this.promptExec = new Executor(parent, isNested, true);
			}

			private string EvaluatePrompt()
			{
				Exception exception = null;
				string defaultPrompt = this.promptExec.ExecuteCommandAndGetResultAsString("prompt", out exception);
				if (string.IsNullOrEmpty(defaultPrompt))
				{
					defaultPrompt = ConsoleHostStrings.DefaultPrompt;
				}
				if (!this.isRunspacePushed)
				{
					if (this.runspacePopped)
					{
						this.runspacePopped = false;
					}
				}
				else
				{
					RemoteRunspace runspace = this.parent.Runspace as RemoteRunspace;
					defaultPrompt = HostUtilities.GetRemotePrompt(runspace, defaultPrompt);
				}
				return defaultPrompt;
			}

			private void EvaluateSuggestions(ConsoleHostUserInterface ui)
			{
				try
				{
					ArrayList suggestion = HostUtilities.GetSuggestion(this.parent.Runspace);
					if (suggestion.Count > 0)
					{
						ui.WriteLine();
					}
					bool flag = true;
					foreach (string str in suggestion)
					{
						if (!flag)
						{
							ui.WriteLine();
						}
						ui.WriteLine(str);
						flag = false;
					}
				}
				catch (TerminateException terminateException)
				{
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					CommandProcessorBase.CheckForSevereException(exception);
					ui.WriteErrorLine(exception.Message);
					LocalRunspace runspace = (LocalRunspace)this.parent.Runspace;
					runspace.GetExecutionContext.AppendDollarError(exception);
				}
			}

			internal static void ExitCurrentLoop()
			{
				if (ConsoleHost.InputLoop.instanceStack.Count != 0)
				{
					ConsoleHost.InputLoop inputLoop = ConsoleHost.InputLoop.instanceStack.Peek();
					inputLoop.shouldExit = true;
					return;
				}
				else
				{
					throw PSTraceSource.NewInvalidOperationException("ConsoleHostStrings", "InputExitCurrentLoopOutOfSyncError", new object[0]);
				}
			}

			private void HandleRunspacePopped(object sender, EventArgs eventArgs)
			{
				lock (this.syncObject)
				{
					this.isRunspacePushed = false;
					this.runspacePopped = true;
				}
			}

			private void HandleRunspacePushed(object sender, EventArgs e)
			{
				lock (this.syncObject)
				{
					this.isRunspacePushed = true;
					this.runspacePopped = false;
				}
			}

			private bool IsIncompleteParseException(Exception e)
			{
				if (e as IncompleteParseException == null)
				{
					RemoteException remoteException = e as RemoteException;
					if (remoteException == null || remoteException.ErrorRecord == null)
					{
						return false;
					}
					else
					{
						return remoteException.ErrorRecord.CategoryInfo.Reason == typeof(IncompleteParseException).Name;
					}
				}
				else
				{
					return true;
				}
			}

			internal void Run(bool inputLoopIsNested)
			{
				string str;
				string command;
				Thread thread = null;
				PSHostUserInterface uI = this.parent.UI;
				ConsoleHostUserInterface consoleHostUserInterface = uI as ConsoleHostUserInterface;
				bool flag = false;
				bool flag1 = false;
				StringBuilder stringBuilder = new StringBuilder();
				bool flag2 = false;
				if (!this.parent.promptDisplayedInNativeCode || inputLoopIsNested)
				{
					flag2 = true;
				}
				while (!this.parent.ShouldEndSession && !this.shouldExit)
				{
					try
					{
						this.parent.isRunningPromptLoop = true;
						if (!flag2)
						{
							flag1 = false;
							command = consoleHostUserInterface.ReadLineWithTabCompletion(this.exec, false);
							flag2 = true;
						}
						else
						{
							if (!consoleHostUserInterface.NoPrompt)
							{
								if (!flag)
								{
									if (!flag1)
									{
										this.EvaluateSuggestions(consoleHostUserInterface);
									}
									str = this.EvaluatePrompt();
								}
								else
								{
									str = ">> ";
								}
								consoleHostUserInterface.SetPrompt(str);
							}
							flag1 = false;
							command = consoleHostUserInterface.ReadLineWithTabCompletion(this.exec, true);
						}
						if (command != null)
						{
							if (command.Trim().Length != 0)
							{
								if (flag)
								{
									stringBuilder = new StringBuilder();
									ConsoleHost.tracer.WriteLine("adding line to block", new object[0]);
									stringBuilder.Append("\n");
									stringBuilder.Append(command);
									continue;
								}
							}
							else
							{
								if (!flag)
								{
									if (!this.parent.InDebugMode)
									{
										flag1 = true;
										continue;
									}
								}
								else
								{
									ConsoleHost.tracer.WriteLine("exiting block mode", new object[0]);
									command = stringBuilder.ToString();
									flag = false;
								}
							}
							if (this.parent.InDebugMode)
							{
								HostUtilities.DebuggerCommand debuggerCommand = this.parent.ProcessDebuggerCommand(command.Trim());
								if (!debuggerCommand.ExecutedByDebugger)
								{
									DebuggerResumeAction? resumeAction = debuggerCommand.ResumeAction;
									if (!resumeAction.HasValue)
									{
										command = debuggerCommand.Command;
									}
									else
									{
										DebuggerResumeAction? nullable = debuggerCommand.ResumeAction;
										this.parent.ExitDebugMode(nullable.Value);
										continue;
									}
								}
								else
								{
									continue;
								}
							}
							Exception exception = null;
							if (!this.runspacePopped)
							{
								if (!this.parent.IsRunningAsync)
								{
									this.exec.ExecuteCommand(command, out exception, Executor.ExecutionOptions.AddOutputter | Executor.ExecutionOptions.AddToHistory);
								}
								else
								{
									this.exec.ExecuteCommandAsync(command, out exception, Executor.ExecutionOptions.AddOutputter | Executor.ExecutionOptions.AddToHistory);
								}
								lock (this.parent.hostGlobalLock)
								{
								}
								if (thread != null)
								{
									thread.Join();
								}
								consoleHostUserInterface.ResetProgress();
								if (!this.IsIncompleteParseException(exception))
								{
									if (exception != null)
									{
										this.parent.ReportException(exception, this.exec);
									}
								}
								else
								{
									if (flag)
									{
										stringBuilder.Append(command);
									}
									else
									{
										flag = true;
										stringBuilder = new StringBuilder(command);
									}
								}
							}
							else
							{
								string str1 = StringUtil.Format(ConsoleHostStrings.CommandNotExecuted, command);
								consoleHostUserInterface.WriteErrorLine(str1);
								this.runspacePopped = false;
							}
						}
						else
						{
							flag1 = true;
							ConsoleHost.tracer.WriteLine("line is null", new object[0]);
							if (!consoleHostUserInterface.ReadFromStdin)
							{
								consoleHostUserInterface.WriteLine();
							}
							flag = false;
							if (this.parent.IsStandardInputRedirected)
							{
								if (this.parent.noExit)
								{
									consoleHostUserInterface.ReadFromStdin = false;
								}
								else
								{
									this.parent.ShouldEndSession = true;
									break;
								}
							}
						}
					}
					finally
					{
						this.parent.isRunningPromptLoop = false;
					}
				}
			}

			internal static void RunNewInputLoop(ConsoleHost parent, bool isNested)
			{
				int count = ConsoleHost.InputLoop.instanceStack.Count;
				if (count != 128)
				{
					ConsoleHost.InputLoop inputLoop = new ConsoleHost.InputLoop(parent, isNested);
					ConsoleHost.InputLoop.instanceStack.Push(inputLoop);
					inputLoop.Run(ConsoleHost.InputLoop.instanceStack.Count > 1);
					ConsoleHost.InputLoop.instanceStack.Pop();
					return;
				}
				else
				{
					throw PSTraceSource.NewInvalidOperationException("ConsoleHostStrings", "TooManyNestedPromptsError", new object[0]);
				}
			}
		}
	}
}