using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.CimCmdlets;
using Microsoft.Management.Infrastructure.Options;
using Microsoft.PowerShell.Commands;
using Microsoft.PowerShell.Workflow;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Sqm;
using System.Management.Automation.Tracing;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Microsoft.PowerShell.Activities
{
	public abstract class PSActivity : PipelineEnabledActivity
	{
		private const string PSComputerName = "PSComputerName";

		internal const int CommandRunInProc = 0;

		internal const int RunInProcNoRunspace = 1;

		internal const int CommandRunOutOfProc = 2;

		internal const int CommandRunRemotely = 3;

		internal const int CimCommandRunInProc = 4;

		internal const int CleanupActivity = 5;

		private const string RunspaceInitScript = "\r\n            Get-Variable -Exclude input | Remove-Variable 2> $Null;$error.Clear();$input | Foreach-Object {$nvp=$_}; foreach($k in $nvp.keys){set-variable -name $k -value $nvp[$k]}\r\n        ";

		private const string MessagePattern = "^([\\d\\w]{8}\\-[\\d\\w]{4}\\-[\\d\\w]{4}\\-[\\d\\w]{4}\\-[\\d\\w]{12}:\\[.*\\]:).*";

		private const string LocalHost = "localhost";

		public readonly static string PSBookmarkPrefix;

		public readonly static string PSSuspendBookmarkPrefix;

		public readonly static string PSPersistBookmarkPrefix;

		private Variable<PSActivityContext> psActivityContextImplementationVariable;

		private Delay cancelTimer;

		private readonly PowerShellTraceSource _tracer;

		private readonly static Tracer _structuredTracer;

		private Variable<NoPersistHandle> noPersistHandle;

		private Variable<bool> bookmarking;

		private TerminateWorkflow terminateActivity;

		private static HashSet<string> _commonCommandParameters;

		internal readonly static InitialSessionState Iss;

		private readonly static ConcurrentDictionary<Guid, RunCommandsArguments> ArgsTableForRunspaces;

		private readonly static char[] Delimiter;

		private readonly static ConcurrentDictionary<Guid, RunCommandsArguments> ArgsTable;

		protected override bool CanInduceIdle
		{
			get
			{
				return true;
			}
		}

		[BehaviorCategory]
		[DefaultValue(null)]
		public InArgument<bool?> Debug
		{
			get;
			set;
		}

		protected string DefiningModule
		{
			get
			{
				return string.Empty;
			}
		}

		[BehaviorCategory]
		[DefaultValue(null)]
		public InArgument<ActionPreference?> ErrorAction
		{
			get;
			set;
		}

		[BehaviorCategory]
		[DefaultValue(null)]
		public InArgument<bool?> MergeErrorToOutput
		{
			get;
			set;
		}

		protected Variable<Dictionary<string, object>> ParameterDefaults
		{
			get;
			set;
		}

		[BehaviorCategory]
		[DefaultValue(null)]
		public InArgument<uint?> PSActionRetryCount
		{
			get;
			set;
		}

		[BehaviorCategory]
		[DefaultValue(null)]
		public InArgument<uint?> PSActionRetryIntervalSec
		{
			get;
			set;
		}

		[BehaviorCategory]
		[DefaultValue(null)]
		public InArgument<uint?> PSActionRunningTimeoutSec
		{
			get;
			set;
		}

		public virtual string PSCommandName
		{
			get
			{
				return string.Empty;
			}
		}

		[DefaultValue(null)]
		[InputAndOutputCategory]
		public InOutArgument<PSDataCollection<DebugRecord>> PSDebug
		{
			get;
			set;
		}

		protected virtual string PSDefiningModule
		{
			get
			{
				return null;
			}
		}

		[BehaviorCategory]
		[DefaultValue(null)]
		public InArgument<bool?> PSDisableSerialization
		{
			get;
			set;
		}

		[DefaultValue(null)]
		[InputAndOutputCategory]
		public InOutArgument<PSDataCollection<ErrorRecord>> PSError
		{
			get;
			set;
		}

		[BehaviorCategory]
		[DefaultValue(null)]
		public InArgument<bool?> PSPersist
		{
			get;
			set;
		}

		[DefaultValue(null)]
		[InputAndOutputCategory]
		public InOutArgument<PSDataCollection<ProgressRecord>> PSProgress
		{
			get;
			set;
		}

		[DefaultValue(null)]
		public InArgument<string> PSProgressMessage
		{
			get;
			set;
		}

		[BehaviorCategory]
		[DefaultValue(null)]
		public InArgument<string[]> PSRequiredModules
		{
			get;
			set;
		}

		[DefaultValue(null)]
		[InputAndOutputCategory]
		public InOutArgument<PSDataCollection<VerboseRecord>> PSVerbose
		{
			get;
			set;
		}

		[DefaultValue(null)]
		[InputAndOutputCategory]
		public InOutArgument<PSDataCollection<WarningRecord>> PSWarning
		{
			get;
			set;
		}

		protected PowerShellTraceSource Tracer
		{
			get
			{
				return this._tracer;
			}
		}

		protected virtual bool UpdatePreferenceVariable
		{
			get
			{
				return true;
			}
		}

		[BehaviorCategory]
		[DefaultValue(null)]
		public InArgument<bool?> Verbose
		{
			get;
			set;
		}

		[BehaviorCategory]
		[DefaultValue(null)]
		public InArgument<ActionPreference?> WarningAction
		{
			get;
			set;
		}

		static PSActivity()
		{
			PSActivity.PSBookmarkPrefix = "Microsoft_PowerShell_Workflow_Bookmark_";
			PSActivity.PSSuspendBookmarkPrefix = "Microsoft_PowerShell_Workflow_Bookmark_Suspend_";
			PSActivity.PSPersistBookmarkPrefix = "Microsoft_PowerShell_Workflow_Bookmark_PSPersist_";
			PSActivity._structuredTracer = new Tracer();
			HashSet<string> strs = new HashSet<string>();
			strs.Add("Verbose");
			strs.Add("Debug");
			strs.Add("ErrorAction");
			strs.Add("WarningAction");
			strs.Add("ErrorVariable");
			strs.Add("WarningVariable");
			strs.Add("OutVariable");
			strs.Add("OutBuffer");
			PSActivity._commonCommandParameters = strs;
			PSActivity.Iss = InitialSessionState.CreateDefault();
			PSActivity.ArgsTableForRunspaces = new ConcurrentDictionary<Guid, RunCommandsArguments>();
			char[] chrArray = new char[1];
			chrArray[0] = ':';
			PSActivity.Delimiter = chrArray;
			PSActivity.ArgsTable = new ConcurrentDictionary<Guid, RunCommandsArguments>();
		}

		protected PSActivity()
		{
			this.psActivityContextImplementationVariable = new Variable<PSActivityContext>("psActivityContextImplementationVariable");
			this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
			this.noPersistHandle = new Variable<NoPersistHandle>("NoPersistHandle");
			this.bookmarking = new Variable<bool>("Bookmarking");
			TerminateWorkflow terminateWorkflow = new TerminateWorkflow();
			terminateWorkflow.Reason = Resources.RunningTimeExceeded;
			this.terminateActivity = terminateWorkflow;
			//base();
			Delay delay = new Delay();
			ParameterExpression parameterExpression = Expression.Parameter(typeof(ActivityContext), "context");
			Expression[] expressionArray = new Expression[3];
			expressionArray[0] = Expression.Constant(0, typeof(int));
			expressionArray[1] = Expression.Constant(0, typeof(int));
			Expression[] expressionArray1 = new Expression[1];
			expressionArray1[0] = Expression.Property(Expression.Constant(this, typeof(PSActivity)), (MethodInfo)MethodBase.GetMethodFromHandle(get_PSActionRunningTimeoutSec));
			expressionArray[2] = Expression.Convert(Expression.Call(parameterExpression, (MethodInfo)MethodBase.GetMethodFromHandle(GetValue<int?>), expressionArray1), typeof(int));
			ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
			parameterExpressionArray[0] = parameterExpression;
			//TODO: REVIEW: (ConstructorInfo)MethodBase.GetMethodFromHandle
			delay.Duration = new InArgument<TimeSpan>(Expression.Lambda<Func<ActivityContext, TimeSpan>>(Expression.New(typeof(TimeSpan).GetConstructor(new System.Type[] { typeof(int), typeof(int), typeof(int) }), expressionArray), parameterExpressionArray));
			this.cancelTimer = delay;
		}

		protected virtual void ActivityEndPersistence(NativeActivityContext context)
		{
			string str;
			Guid guid;
			bool hasValue;
			bool flag;
			bool? activityPersistFlag = this.GetActivityPersistFlag(context);
			bool? nullable = null;
			if (this.PSPersist.Expression == null)
			{
				bool? nullable1 = this.PSPersist.Get(context);
				if (nullable1.HasValue)
				{
					bool? nullable2 = this.PSPersist.Get(context);
					nullable = new bool?(nullable2.Value);
				}
			}
			bool hostPersistFlag = this.GetHostPersistFlag(context);
			if (!activityPersistFlag.HasValue)
			{
				if (!hostPersistFlag)
				{
					bool? nullable3 = nullable;
					if (!nullable3.GetValueOrDefault())
					{
						flag = false;
					}
					else
					{
						flag = nullable3.HasValue;
					}
					if (flag)
					{
						guid = Guid.NewGuid();
						str = string.Concat(PSActivity.PSPersistBookmarkPrefix, guid.ToString().Replace("-", "_"));
						context.CreateBookmark(str, new BookmarkCallback(this.BookmarkResumed));
						return;
					}
				}
				else
				{
					guid = Guid.NewGuid();
					str = string.Concat(PSActivity.PSPersistBookmarkPrefix, guid.ToString().Replace("-", "_"));
					context.CreateBookmark(str, new BookmarkCallback(this.BookmarkResumed));
					return;
				}
			}
			bool? nullable4 = activityPersistFlag;
			if (!nullable4.GetValueOrDefault())
			{
				hasValue = false;
			}
			else
			{
				hasValue = nullable4.HasValue;
			}
			if (!hasValue)
			{
				return;
			}
			guid = Guid.NewGuid();
			str = string.Concat(PSActivity.PSPersistBookmarkPrefix, guid.ToString().Replace("-", "_"));
			context.CreateBookmark(str, new BookmarkCallback(this.BookmarkResumed));
		}

		private static void ActivityHostManagerCallback(IAsyncResult asyncResult)
		{
			object asyncState = asyncResult.AsyncState;
			RunCommandsArguments runCommandsArgument = asyncState as RunCommandsArguments;
			PSWorkflowHost workflowHost = runCommandsArgument.WorkflowHost;
			PSActivityContext pSActivityContext = runCommandsArgument.PSActivityContext;
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			using (traceSource)
			{
				traceSource.WriteMessage("Executing callback for Executing command out of proc");
				bool flag = false;
				try
				{
					try
					{
						((PSOutOfProcessActivityController)workflowHost.PSActivityHostController).EndInvokePowerShell(asyncResult);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						flag = PSActivity.HandleRunOneCommandException(runCommandsArgument, exception);
						if (flag)
						{
							PSActivity.BeginActionRetry(runCommandsArgument);
						}
					}
				}
				finally
				{
					PSActivity.RemoveHandlersFromStreams(runCommandsArgument.ImplementationContext.PowerShellInstance, runCommandsArgument);
					PSActivity.RunOneCommandFinally(runCommandsArgument, flag);
					traceSource.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity: Finished running command.", new object[0]));
					PSActivity.DecrementRunningCountAndCheckForEnd(pSActivityContext);
				}
			}
		}

		private static void AddHandlersToStreams(System.Management.Automation.PowerShell commandToRun, RunCommandsArguments args)
		{
			if (commandToRun == null || args == null)
			{
				return;
			}
			else
			{
				bool mergeErrorToOutput = args.PSActivityContext.MergeErrorToOutput;
				if (mergeErrorToOutput)
				{
					commandToRun.Streams.Error.DataAdded += new EventHandler<DataAddedEventArgs>(PSActivity.HandleErrorDataAdded);
				}
				if (args.PSActivityContext.Output != null)
				{
					args.PSActivityContext.Output.DataAdding += new EventHandler<DataAddingEventArgs>(PSActivity.HandleOutputDataAdding);
				}
				commandToRun.Streams.Error.DataAdding += new EventHandler<DataAddingEventArgs>(PSActivity.HandleErrorDataAdding);
				commandToRun.Streams.Progress.DataAdding += new EventHandler<DataAddingEventArgs>(PSActivity.HandleProgressDataAdding);
				commandToRun.Streams.Verbose.DataAdding += new EventHandler<DataAddingEventArgs>(PSActivity.HandleInformationalRecordDataAdding);
				commandToRun.Streams.Warning.DataAdding += new EventHandler<DataAddingEventArgs>(PSActivity.HandleInformationalRecordDataAdding);
				commandToRun.Streams.Debug.DataAdding += new EventHandler<DataAddingEventArgs>(PSActivity.HandleInformationalRecordDataAdding);
				PSActivity.ArgsTable.TryAdd(commandToRun.InstanceId, args);
				return;
			}
		}

		internal static void AddIdentifierInfoToErrorRecord(ErrorRecord errorRecord, string computerName, Guid jobInstanceId)
		{
			RemotingErrorRecord remotingErrorRecord = errorRecord as RemotingErrorRecord;
			if (remotingErrorRecord == null)
			{
				if (errorRecord.ErrorDetails != null)
				{
					errorRecord.ErrorDetails.RecommendedAction = PSActivity.AddIdentifierInfoToString(jobInstanceId, computerName, errorRecord.ErrorDetails.RecommendedAction);
					return;
				}
				else
				{
					errorRecord.ErrorDetails = new ErrorDetails(string.Empty);
					errorRecord.ErrorDetails.RecommendedAction = PSActivity.AddIdentifierInfoToString(jobInstanceId, computerName, errorRecord.ErrorDetails.RecommendedAction);
					return;
				}
			}
			else
			{
				return;
			}
		}

		internal static void AddIdentifierInfoToOutput(PSObject psObject, Guid jobInstanceId, string computerName)
		{
			if (psObject.Properties["PSComputerName"] == null)
			{
				psObject.Properties.Add(new PSNoteProperty("PSComputerName", computerName));
			}
			else
			{
				PSNoteProperty item = psObject.Properties["PSComputerName"] as PSNoteProperty;
				if (item != null)
				{
					try
					{
						item.Value = computerName;
					}
					catch (SetValueException setValueException)
					{
					}
				}
			}
			if (psObject.Properties["PSShowComputerName"] != null)
			{
				psObject.Properties.Remove("PSShowComputerName");
			}
			psObject.Properties.Add(new PSNoteProperty("PSShowComputerName", (object)((bool)1)));
			if (psObject.Properties["PSSourceJobInstanceId"] != null)
			{
				psObject.Properties.Remove("PSSourceJobInstanceId");
			}
			psObject.Properties.Add(new PSNoteProperty("PSSourceJobInstanceId", (object)jobInstanceId));
		}

		internal static string AddIdentifierInfoToString(Guid instanceId, string computerName, string message)
		{
			Guid guid;
			string str = null;
			string str1 = null;
			string str2;
			if (PSActivity.StringContainsIdentifierInfo(message, out guid, out str, out str1))
			{
				str2 = str1;
			}
			else
			{
				str2 = message;
			}
			string str3 = str2;
			StringBuilder stringBuilder = new StringBuilder(instanceId.ToString());
			stringBuilder.Append(":[");
			stringBuilder.Append(computerName);
			stringBuilder.Append("]:");
			stringBuilder.Append(str3);
			return stringBuilder.ToString();
		}

		private static void BeginActionRetry(RunCommandsArguments args)
		{
			PSActivityContext pSActivityContext = args.PSActivityContext;
			Interlocked.Increment(ref pSActivityContext.CommandsRunningCount);
			PSActivity.BeginRunOneCommand(args);
		}

		private static void BeginExecuteOneCommand(RunCommandsArguments args)
		{
			PSActivityContext pSActivityContext = args.PSActivityContext;
			PSWorkflowHost workflowHost = args.WorkflowHost;
			ActivityImplementationContext implementationContext = args.ImplementationContext;
			PSDataCollection<PSObject> input = args.Input;
			PSDataCollection<PSObject> output = args.Output;
			int commandExecutionType = args.CommandExecutionType;
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			using (traceSource)
			{
				PowerShell powerShellInstance = implementationContext.PowerShellInstance;
				object[] objArray = new object[1];
				objArray[0] = powerShellInstance;
				traceSource.WriteMessage(string.Format(CultureInfo.InvariantCulture, "BEGIN BeginExecuteOneCommand {0}.", objArray));
				int num = commandExecutionType;
				switch (num)
				{
					case 0:
					{
						powerShellInstance.Runspace.ThreadOptions = PSThreadOptions.UseCurrentThread;
						ThreadPool.QueueUserWorkItem(new WaitCallback(PSActivity.InitializeRunspaceAndExecuteCommandWorker), args);
						break;
					}
					case 1:
					{
						ThreadPool.QueueUserWorkItem(new WaitCallback(PSActivity.ExecuteOneRunspaceFreeCommandWorker), args);
						break;
					}
					case 2:
					{
						if (!PSActivity.CheckForCancel(pSActivityContext))
						{
							PSResumableActivityHostController pSActivityHostController = workflowHost.PSActivityHostController as PSResumableActivityHostController;
							if (pSActivityHostController == null)
							{
								PSOutOfProcessActivityController pSOutOfProcessActivityController = workflowHost.PSActivityHostController as PSOutOfProcessActivityController;
								if (pSOutOfProcessActivityController == null)
								{
									break;
								}
								PSActivity.AddHandlersToStreams(powerShellInstance, args);
								IAsyncResult asyncResult = pSOutOfProcessActivityController.BeginInvokePowerShell(powerShellInstance, input, output, implementationContext.PSActivityEnvironment, new AsyncCallback(PSActivity.ActivityHostManagerCallback), args);
								pSActivityContext.AsyncResults.Enqueue(asyncResult);
								break;
							}
							else
							{
								PowerShellStreams<PSObject, PSObject> powerShellStream = new PowerShellStreams<PSObject, PSObject>();
								if (!pSActivityHostController.SupportDisconnectedPSStreams)
								{
									powerShellStream.InputStream = input;
									powerShellStream.OutputStream = output;
									powerShellStream.DebugStream = powerShellInstance.Streams.Debug;
									powerShellStream.ErrorStream = powerShellInstance.Streams.Error;
									powerShellStream.ProgressStream = powerShellInstance.Streams.Progress;
									powerShellStream.VerboseStream = powerShellInstance.Streams.Verbose;
									powerShellStream.WarningStream = powerShellInstance.Streams.Warning;
								}
								else
								{
									powerShellStream.InputStream = input;
									powerShellStream.OutputStream = new PSDataCollection<PSObject>();
									powerShellStream.ErrorStream = new PSDataCollection<ErrorRecord>();
									powerShellStream.DebugStream = new PSDataCollection<DebugRecord>();
									powerShellStream.ProgressStream = new PSDataCollection<ProgressRecord>();
									powerShellStream.VerboseStream = new PSDataCollection<VerboseRecord>();
									powerShellStream.WarningStream = new PSDataCollection<WarningRecord>();
								}
								pSActivityHostController.StartResumablePSCommand(args.PSActivityContext.JobInstanceId, (Bookmark)args.PSActivityContext.AsyncState, powerShellInstance, powerShellStream, implementationContext.PSActivityEnvironment, (PSActivity)pSActivityContext.ActivityObject);
								break;
							}
						}
						else
						{
							return;
						}
					}
					case 3:
					{
						PSActivity.ArgsTableForRunspaces.TryAdd(powerShellInstance.Runspace.InstanceId, args);
						PSActivity.InitializeRunspaceAndExecuteCommandWorker(args);
						break;
					}
					case 4:
					{
						ThreadPool.QueueUserWorkItem(new WaitCallback(PSActivity.InitializeRunspaceAndExecuteCommandWorker), args);
						break;
					}
					case 5:
					{
						PSActivity.ExecuteCleanupActivity(args);
						break;
					}
				}
				traceSource.WriteMessage("END BeginExecuteOneCommand");
			}
		}

		private static void BeginImportRequiredModules(RunCommandsArguments args)
		{
			Runspace runspace = args.ImplementationContext.PowerShellInstance.Runspace;
			PSActivityEnvironment pSActivityEnvironment = args.ImplementationContext.PSActivityEnvironment;
			System.Management.Automation.PowerShell powerShell = System.Management.Automation.PowerShell.Create();
			if (pSActivityEnvironment == null)
			{
				powerShell.AddCommand("Import-Module").AddParameter("Name", args.ActivityParameters.PSRequiredModules).AddParameter("ErrorAction", ActionPreference.Stop);
			}
			else
			{
				powerShell.AddCommand("Import-Module").AddParameter("Name", pSActivityEnvironment.Modules).AddParameter("ErrorAction", ActionPreference.Stop);
			}
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			using (traceSource)
			{
				traceSource.WriteMessage("Importing modules in runspace ", runspace.InstanceId);
				powerShell.Runspace = runspace;
				args.HelperCommand = powerShell;
				PSActivity.BeginInvokeOnPowershellCommand(powerShell, null, null, new AsyncCallback(PSActivity.ImportRequiredModulesCallback), args);
			}
		}

		private static void BeginInvokeOnPowershellCommand(System.Management.Automation.PowerShell ps, PSDataCollection<object> varsInput, PSInvocationSettings settings, AsyncCallback callback, RunCommandsArguments args)
		{
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			using (traceSource)
			{
				try
				{
					ps.BeginInvoke<object>(varsInput, settings, callback, args);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					bool flag = PSActivity.ProcessException(args, exception);
					if (!flag)
					{
						PSActivity.ReleaseResourcesAndCheckForEnd(ps, args, false, false);
					}
					else
					{
						PSActivity.BeginActionRetry(args);
					}
					traceSource.TraceException(exception);
				}
			}
		}

		private static void BeginPowerShellInvocation(object state)
		{
			RunCommandsArguments runCommandsArgument = state as RunCommandsArguments;
			ActivityImplementationContext implementationContext = runCommandsArgument.ImplementationContext;
			PSDataCollection<PSObject> output = runCommandsArgument.Output;
			PSDataCollection<PSObject> input = runCommandsArgument.Input;
			PowerShell powerShellInstance = implementationContext.PowerShellInstance;
			PSWorkflowHost workflowHost = runCommandsArgument.WorkflowHost;
			PSActivityContext pSActivityContext = runCommandsArgument.PSActivityContext;
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			using (traceSource)
			{
				PSDataCollection<PSObject> pSObjects = output;
				PSDataCollection<PSObject> pSObjects1 = pSObjects;
				if (pSObjects == null)
				{
					pSObjects1 = new PSDataCollection<PSObject>();
				}
				PSDataCollection<PSObject> pSObjects2 = pSObjects1;
				bool flag = false;
				try
				{
					if (!PSActivity.CheckForCancel(pSActivityContext))
					{
						PSActivity.AddHandlersToStreams(powerShellInstance, runCommandsArgument);
						if (!PSActivity.CheckForCancel(pSActivityContext))
						{
							powerShellInstance.BeginInvoke<PSObject, PSObject>(input, pSObjects2, null, new AsyncCallback(PSActivity.PowerShellInvocationCallback), runCommandsArgument);
						}
						else
						{
							return;
						}
					}
					else
					{
						return;
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					flag = true;
					bool flag1 = PSActivity.ProcessException(runCommandsArgument, exception);
					if (!flag1)
					{
						PSActivity.ReleaseResourcesAndCheckForEnd(powerShellInstance, runCommandsArgument, true, false);
					}
					else
					{
						PSActivity.BeginActionRetry(runCommandsArgument);
					}
					traceSource.TraceException(exception);
				}
				traceSource.WriteMessage("Completed BeginInvoke call on PowerShell");
				if (!flag && runCommandsArgument.CommandExecutionType == 3)
				{
					workflowHost.RemoteRunspaceProvider.ReadyForDisconnect(powerShellInstance.Runspace);
				}
			}
		}

		internal static void BeginRunOneCommand(RunCommandsArguments args)
		{
			PSActivityContext pSActivityContext = args.PSActivityContext;
			ActivityImplementationContext implementationContext = args.ImplementationContext;
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			using (traceSource)
			{
				PowerShell powerShellInstance = implementationContext.PowerShellInstance;
				object[] objArray = new object[1];
				objArray[0] = powerShellInstance;
				traceSource.WriteMessage(string.Format(CultureInfo.InvariantCulture, "Begining action to run command {0}.", objArray));
				if (!PSActivity.CheckForCancel(pSActivityContext))
				{
					PSActivity.InitializeOneCommand(args);
					if (args.CommandExecutionType != 3 && args.CommandExecutionType != 0 && args.CommandExecutionType != 4)
					{
						PSActivity.BeginExecuteOneCommand(args);
					}
				}
			}
		}

		private static void BeginRunspaceInitializeSetup(RunCommandsArguments args)
		{
			PSActivityEnvironment pSActivityEnvironment = args.ImplementationContext.PSActivityEnvironment;
			Runspace runspace = args.ImplementationContext.PowerShellInstance.Runspace;
			string[] pSRequiredModules = args.ActivityParameters.PSRequiredModules;
			PSActivityContext pSActivityContext = args.PSActivityContext;
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			using (traceSource)
			{
				traceSource.WriteMessage("BEGIN BeginRunspaceInitializeSetup");
				if (args.CommandExecutionType != 4)
				{
					if (runspace.ConnectionInfo == null)
					{
						try
						{
							PSActivity.SetVariablesInRunspaceUsingProxy(pSActivityEnvironment, runspace);
						}
						catch (Exception exception1)
						{
							Exception exception = exception1;
							bool flag = PSActivity.HandleRunOneCommandException(args, exception);
							if (!flag)
							{
								traceSource.WriteMessage("Setting variables for command failed, returning");
								PSActivity.RunOneCommandFinally(args, false);
							}
							else
							{
								traceSource.WriteMessage("Setting variables for command failed, attempting retry");
								PSActivity.CloseRunspace(args.ImplementationContext.PowerShellInstance.Runspace, args.CommandExecutionType, args.WorkflowHost, pSActivityContext);
								PSActivity.BeginActionRetry(args);
							}
							PSActivity.DecrementRunningCountAndCheckForEnd(pSActivityContext);
							return;
						}
					}
					else
					{
						PSActivity.BeginSetVariablesInRemoteRunspace(args);
						return;
					}
				}
				if ((int)pSRequiredModules.Length <= 0)
				{
					PSActivity.BeginPowerShellInvocation(args);
				}
				else
				{
					PSActivity.BeginImportRequiredModules(args);
				}
				traceSource.WriteMessage("END BeginRunspaceInitializeSetup");
			}
		}

		private static void BeginSetVariablesInRemoteRunspace(RunCommandsArguments args)
		{
			Runspace runspace = args.ImplementationContext.PowerShellInstance.Runspace;
			PSActivityEnvironment pSActivityEnvironment = args.ImplementationContext.PSActivityEnvironment;
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			using (traceSource)
			{
				traceSource.WriteMessage("BEGIN BeginSetVariablesInRemoteRunspace");
				PowerShell powerShell = PowerShell.Create();
				powerShell.Runspace = runspace;
				powerShell.AddScript("\r\n            Get-Variable -Exclude input | Remove-Variable 2> $Null;$error.Clear();$input | Foreach-Object {$nvp=$_}; foreach($k in $nvp.keys){set-variable -name $k -value $nvp[$k]}\r\n        ");
				Dictionary<string, object> variablesToSetInRunspace = PSActivity.GetVariablesToSetInRunspace(pSActivityEnvironment);
				PSDataCollection<object> objs = new PSDataCollection<object>();
				objs.Add(variablesToSetInRunspace);
				PSDataCollection<object> objs1 = objs;
				objs1.Complete();
				args.HelperCommand = powerShell;
				args.HelperCommandInput = objs1;
				PSActivity.BeginInvokeOnPowershellCommand(powerShell, objs1, null, new AsyncCallback(PSActivity.SetVariablesCallback), args);
				traceSource.WriteMessage("END BeginSetVariablesInRemoteRunspace");
			}
		}

		private void BookmarkResumed(NativeActivityContext context, Bookmark bookmark, object value)
		{
		}

		protected override void CacheMetadata(NativeActivityMetadata metadata)
		{
			base.CacheMetadata(metadata);
			metadata.AddImplementationVariable(this.bookmarking);
			metadata.AddImplementationVariable(this.noPersistHandle);
			metadata.AddImplementationChild(this.cancelTimer);
			metadata.AddImplementationChild(this.terminateActivity);
			NativeActivityMetadata nativeActivityMetadataPointer = metadata;
			nativeActivityMetadataPointer.AddDefaultExtensionProvider<PSWorkflowInstanceExtension>(() => new PSWorkflowInstanceExtension());
			this.ParameterDefaults = new Variable<Dictionary<string, object>>();
			metadata.AddImplementationVariable(this.ParameterDefaults);
			string[] strArrays = new string[1];
			strArrays[0] = "not";
			this.Tracer.WriteMessage(base.GetType().Name, "CacheMetadata", Guid.Empty, "Adding PowerShell specific extensions to metadata, CommonParameters are {0} available.", strArrays);
			metadata.AddImplementationVariable(this.psActivityContextImplementationVariable);
		}

		protected override void Cancel(NativeActivityContext context)
		{
			try
			{
				if (this.bookmarking.Get(context))
				{
					NoPersistHandle noPersistHandle = this.noPersistHandle.Get(context);
					noPersistHandle.Enter(context);
				}
				PSActivityContext item = null;
				HostParameterDefaults extension = context.GetExtension<HostParameterDefaults>();
				if (extension != null && extension.AsyncExecutionCollection != null)
				{
					Dictionary<string, PSActivityContext> asyncExecutionCollection = extension.AsyncExecutionCollection;
					if (asyncExecutionCollection.ContainsKey(context.ActivityInstanceId))
					{
						item = asyncExecutionCollection[context.ActivityInstanceId];
						asyncExecutionCollection.Remove(context.ActivityInstanceId);
					}
				}
				if (item == null)
				{
					item = this.psActivityContextImplementationVariable.Get(context);
				}
				item.IsCanceled = true;
				this.psActivityContextImplementationVariable.Set(context, item);
				this.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity: Executing cancel request.", new object[0]));
				if (item.commandQueue != null && !item.commandQueue.IsEmpty)
				{
					ActivityImplementationContext[] array = item.commandQueue.ToArray();
					for (int i = 0; i < (int)array.Length; i++)
					{
						ActivityImplementationContext activityImplementationContext = array[i];
						RunCommandsArguments runCommandsArgument = null;
						PSActivity.ArgsTable.TryGetValue(activityImplementationContext.PowerShellInstance.InstanceId, out runCommandsArgument);
						if (runCommandsArgument != null)
						{
							PSActivity.RemoveHandlersFromStreams(activityImplementationContext.PowerShellInstance, runCommandsArgument);
						}
					}
				}
				if (item.runningCommands != null && item.runningCommands.Count > 0)
				{
					lock (item.runningCommands)
					{
						foreach (PowerShell key in item.runningCommands.Keys)
						{
							RunCommandsArguments runCommandsArgument1 = null;
							PSActivity.ArgsTable.TryGetValue(key.InstanceId, out runCommandsArgument1);
							if (runCommandsArgument1 == null)
							{
								continue;
							}
							PSActivity.RemoveHandlersFromStreams(key, runCommandsArgument1);
						}
					}
				}
				item.Cancel();
			}
			finally
			{
				context.MarkCanceled();
			}
		}

		private static bool CheckForCancel(PSActivityContext psActivityContext)
		{
			bool isCanceled = psActivityContext.IsCanceled;
			if (isCanceled)
			{
				PSActivity.RaiseTerminalCallback(psActivityContext);
			}
			return isCanceled;
		}

		private static void CleanupActivityCallback(object state)
		{
			RunCommandsArguments runCommandsArgument = state as RunCommandsArguments;
			PSActivity.DecrementRunningCountAndCheckForEnd(runCommandsArgument.PSActivityContext);
		}

		private static void CloseRunspace(Runspace runspace, int commandType = 0, PSWorkflowHost workflowHost = null, PSActivityContext psActivityContext = null)
		{
			int num = commandType;
			switch (num)
			{
				case 0:
				{
					workflowHost.LocalRunspaceProvider.ReleaseRunspace(runspace);
					return;
				}
				case 1:
				case 2:
				{
					return;
				}
				case 3:
				{
					PSActivity.UnregisterAndReleaseRunspace(runspace, workflowHost, psActivityContext);
					return;
				}
				case 4:
				{
					workflowHost.LocalRunspaceProvider.ReleaseRunspace(runspace);
					return;
				}
				default:
				{
					return;
				}
			}
		}

		internal static void CloseRunspaceAndDisposeCommand(System.Management.Automation.PowerShell currentCommand, PSWorkflowHost WorkflowHost, PSActivityContext psActivityContext, int commandType)
		{
			if (!currentCommand.IsRunspaceOwner && (currentCommand.Runspace.RunspaceStateInfo.State == RunspaceState.Opened || currentCommand.Runspace.RunspaceStateInfo.State == RunspaceState.Disconnected))
			{
				PSActivity.CloseRunspace(currentCommand.Runspace, commandType, WorkflowHost, psActivityContext);
			}
			currentCommand.Dispose();
		}

		private static void ConnectionManagerCallback(IAsyncResult asyncResult)
		{
			object asyncState = asyncResult.AsyncState;
			RunCommandsArguments runCommandsArgument = asyncState as RunCommandsArguments;
			ActivityImplementationContext implementationContext = runCommandsArgument.ImplementationContext;
			PowerShell powerShellInstance = implementationContext.PowerShellInstance;
			PSWorkflowHost workflowHost = runCommandsArgument.WorkflowHost;
			PSActivityContext pSActivityContext = runCommandsArgument.PSActivityContext;
			//TODO: REIVEW:powerShellInstance.Runspace.ConnectionInfo;
			string[] pSComputerName = implementationContext.PSComputerName;
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			using (traceSource)
			{
				traceSource.WriteMessage("Executing callback for GetRunspace for computer ", powerShellInstance.Runspace.ConnectionInfo.ComputerName);
				Runspace runspace = null;
				try
				{
					runspace = workflowHost.RemoteRunspaceProvider.EndGetRunspace(asyncResult);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					traceSource.WriteMessage("Error in connecting to remote computer ", powerShellInstance.Runspace.ConnectionInfo.ComputerName);
					if (pSComputerName == null || (int)pSComputerName.Length <= 1)
					{
						ErrorRecord errorRecord = new ErrorRecord(exception, "ConnectionAttemptFailed", ErrorCategory.OpenError, pSComputerName);
						lock (pSActivityContext.exceptions)
						{
							pSActivityContext.exceptions.Add(new RuntimeException(exception.Message, exception, errorRecord));
						}
					}
					else
					{
						PSActivity.WriteError(exception, "ConnectionAttemptFailed", ErrorCategory.InvalidResult, pSComputerName, pSActivityContext);
					}
					PSActivity.RunOneCommandFinally(runCommandsArgument, false);
					PSActivity.DecrementRunningCountAndCheckForEnd(pSActivityContext);
					return;
				}
				Guid instanceId = runspace.InstanceId;
				traceSource.WriteMessage("Runspace successfully obtained with guid ", instanceId.ToString());
				if (pSActivityContext.IsCanceled)
				{
					PSActivity.CloseRunspace(runspace, 3, workflowHost, pSActivityContext);
					if (PSActivity.CheckForCancel(pSActivityContext))
					{
						return;
					}
				}
				powerShellInstance.Runspace = runspace;
				pSActivityContext.HandleRunspaceStateChanged = new EventHandler<RunspaceStateEventArgs>(PSActivity.HandleRunspaceStateChanged);
				powerShellInstance.Runspace.StateChanged += pSActivityContext.HandleRunspaceStateChanged;
				PSActivity.BeginExecuteOneCommand(runCommandsArgument);
				traceSource.WriteMessage("Returning from callback for GetRunspace for computer ", powerShellInstance.Runspace.ConnectionInfo.ComputerName);
			}
		}

		private PSDataCollection<PSObject> CreateOutputStream(NativeActivityContext context)
		{
			PSDataCollection<PSObject> pSObjects = new PSDataCollection<PSObject>();
			pSObjects.IsAutoGenerated = true;
			pSObjects.EnumeratorNeverBlocks = true;
			if (this.GetDisableSerialization(context))
			{
				pSObjects.SerializeInput = false;
			}
			else
			{
				pSObjects.SerializeInput = true;
			}
			base.Result.Set(context, pSObjects);
			object[] activityInstanceId = new object[1];
			activityInstanceId[0] = context.ActivityInstanceId;
			this.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: No OutputStream was passed in; creating a new stream.", activityInstanceId));
			return pSObjects;
		}

		private static void DecrementRunningCountAndCheckForEnd(PSActivityContext psActivityContext)
		{
			Interlocked.Decrement(ref psActivityContext.CommandsRunningCount);
			if (psActivityContext.CommandsRunningCount == 0)
			{
				PSActivity.RaiseTerminalCallback(psActivityContext);
				return;
			}
			else
			{
				return;
			}
		}

		private static void DisposeRunspaceInPowerShell(System.Management.Automation.PowerShell commandToRun, bool setToNull = true)
		{
			commandToRun.Runspace.Dispose();
			commandToRun.Runspace.Close();
			if (setToNull)
			{
				commandToRun.Runspace = null;
			}
		}

		protected override void Execute(NativeActivityContext context)
		{
			object obj;
			WaitCallback waitCallback;
			bool value;
			object[] activityInstanceId = new object[1];
			activityInstanceId[0] = context.ActivityInstanceId;
			this.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: Beginning execution.", activityInstanceId));
			string displayName = base.DisplayName;
			if (string.IsNullOrEmpty(displayName))
			{
				displayName = base.GetType().Name;
			}
			if (PSActivity._structuredTracer.IsEnabled)
			{
				PSActivity._structuredTracer.ActivityExecutionStarted(displayName, base.GetType().FullName);
			}
			PSSQMAPI.IncrementWorkflowActivityPresent(base.GetType().FullName);
			bool? activityPersistFlag = this.GetActivityPersistFlag(context);
			if (!activityPersistFlag.HasValue || !activityPersistFlag.HasValue)
			{
				value = false;
			}
			else
			{
				value = activityPersistFlag.Value;
			}
			bool flag = value;
			if (!flag)
			{
				NoPersistHandle noPersistHandle = this.noPersistHandle.Get(context);
				noPersistHandle.Enter(context);
			}
			this.bookmarking.Set(context, flag);
			Dictionary<string, object> strs = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			HostParameterDefaults extension = context.GetExtension<HostParameterDefaults>();
			if (extension != null)
			{
				Dictionary<string, object> parameters = extension.Parameters;
				foreach (KeyValuePair<string, object> parameter in parameters)
				{
					strs[parameter.Key] = parameter.Value;
				}
				if (strs.ContainsKey("PSComputerName") && strs["PSComputerName"] as string != null)
				{
					object[] item = new object[1];
					item[0] = (string)strs["PSComputerName"];
					strs["PSComputerName"] = item;
				}
			}
			if (!base.UseDefaultInput && strs.ContainsKey("Input"))
			{
				strs.Remove("Input");
			}
			context.SetValue<Dictionary<string, object>>(this.ParameterDefaults, strs);
			PSActivityContext pSActivityContext = new PSActivityContext();
			pSActivityContext.runningCommands = new Dictionary<PowerShell, RetryCount>();
			pSActivityContext.commandQueue = new ConcurrentQueue<ActivityImplementationContext>();
			pSActivityContext.IsCanceled = false;
			pSActivityContext.HostExtension = extension;
			GenericCimCmdletActivity genericCimCmdletActivity = this as GenericCimCmdletActivity;
			if (genericCimCmdletActivity != null)
			{
				pSActivityContext.TypeImplementingCmdlet = genericCimCmdletActivity.TypeImplementingCmdlet;
			}
			foreach (PSActivityArgumentInfo activityArgument in this.GetActivityArguments())
			{
				Argument argument = activityArgument.Value;
				this.PopulateParameterFromDefault(argument, context, activityArgument.Name, strs);
				if (argument.Get(context) == null)
				{
					continue;
				}
				object[] name = new object[3];
				name[0] = context.ActivityInstanceId;
				name[1] = activityArgument.Name;
				name[2] = argument.Get(context);
				this.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: Using parameter {1}, with value '{2}'.", name));
			}
			PSDataCollection<PSObject> pSObjects = base.Input.Get(context);
			if (pSObjects == null || pSObjects.Count != 0)
			{
				bool flag1 = false;
				PSDataCollection<PSObject> pSObjects1 = base.Result.Get(context);
				if (pSObjects1 != null)
				{
					if (this.GetDisableSerialization(context))
					{
						pSObjects1.SerializeInput = false;
					}
					else
					{
						pSObjects1.SerializeInput = true;
					}
				}
				if ((pSObjects1 == null || !pSObjects1.IsOpen) && base.Result.Expression != null)
				{
					if (pSObjects1 != null)
					{
						flag1 = true;
					}
					else
					{
						pSObjects1 = this.CreateOutputStream(context);
					}
				}
				else
				{
					if ((this.ParameterDefaults == null || !strs.ContainsKey("Result") || strs["Result"] != base.Result.Get(context)) && pSObjects1 != null)
					{
						bool flag2 = false;
						bool? appendOutput = base.AppendOutput;
						if (appendOutput.HasValue)
						{
							bool? nullable = base.AppendOutput;
							if (nullable.Value)
							{
								flag2 = true;
							}
						}
						if (!flag2)
						{
							flag1 = true;
						}
					}
				}
				pSActivityContext.errors = this.PSError.Get(context);
				if (this.PSError.Expression != null && (pSActivityContext.errors == null || pSActivityContext.errors.IsAutoGenerated))
				{
					pSActivityContext.errors = new PSDataCollection<ErrorRecord>();
					pSActivityContext.errors.IsAutoGenerated = true;
					this.PSError.Set(context, pSActivityContext.errors);
					object[] objArray = new object[1];
					objArray[0] = context.ActivityInstanceId;
					this.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: No ErrorStream was passed in; creating a new stream.", objArray));
				}
				bool? nullable1 = this.MergeErrorToOutput.Get(context);
				if (nullable1.HasValue)
				{
					bool? nullable2 = this.MergeErrorToOutput.Get(context);
					if (nullable2.GetValueOrDefault(false) && pSObjects1 != null && pSActivityContext.errors != null)
					{
						if (this.ParameterDefaults != null && strs.ContainsKey("PSError") && strs["PSError"] == this.PSError.Get(context))
						{
							pSActivityContext.errors = new PSDataCollection<ErrorRecord>();
							pSActivityContext.errors.IsAutoGenerated = true;
							this.PSError.Set(context, pSActivityContext.errors);
							object[] activityInstanceId1 = new object[1];
							activityInstanceId1[0] = context.ActivityInstanceId;
							this.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: Merge error to the output stream and current error stream is the host default; creating a new stream.", activityInstanceId1));
						}
						pSActivityContext.MergeErrorToOutput = true;
					}
				}
				PSDataCollection<ProgressRecord> progressRecords = this.PSProgress.Get(context);
				if (this.PSProgress.Expression != null && (progressRecords == null || progressRecords.IsAutoGenerated))
				{
					progressRecords = new PSDataCollection<ProgressRecord>();
					progressRecords.IsAutoGenerated = true;
					this.PSProgress.Set(context, progressRecords);
					object[] objArray1 = new object[1];
					objArray1[0] = context.ActivityInstanceId;
					this.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: No ProgressStream was passed in; creating a new stream.", objArray1));
				}
				pSActivityContext.progress = progressRecords;
				this.WriteProgressRecord(context, progressRecords, Resources.RunningString, ProgressRecordType.Processing);
				PSDataCollection<VerboseRecord> verboseRecords = this.PSVerbose.Get(context);
				if (this.PSVerbose.Expression != null && (verboseRecords == null || verboseRecords.IsAutoGenerated))
				{
					verboseRecords = new PSDataCollection<VerboseRecord>();
					verboseRecords.IsAutoGenerated = true;
					this.PSVerbose.Set(context, verboseRecords);
					object[] activityInstanceId2 = new object[1];
					activityInstanceId2[0] = context.ActivityInstanceId;
					this.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: No VerboseStream was passed in; creating a new stream.", activityInstanceId2));
				}
				PSDataCollection<DebugRecord> debugRecords = this.PSDebug.Get(context);
				if (this.PSDebug.Expression != null && (debugRecords == null || debugRecords.IsAutoGenerated))
				{
					debugRecords = new PSDataCollection<DebugRecord>();
					debugRecords.IsAutoGenerated = true;
					this.PSDebug.Set(context, debugRecords);
					object[] objArray2 = new object[1];
					objArray2[0] = context.ActivityInstanceId;
					this.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: No DebugStream was passed in; creating a new stream.", objArray2));
				}
				PSDataCollection<WarningRecord> warningRecords = this.PSWarning.Get(context);
				if (this.PSWarning.Expression != null && (warningRecords == null || warningRecords.IsAutoGenerated))
				{
					warningRecords = new PSDataCollection<WarningRecord>();
					warningRecords.IsAutoGenerated = true;
					this.PSWarning.Set(context, warningRecords);
					object[] activityInstanceId3 = new object[1];
					activityInstanceId3[0] = context.ActivityInstanceId;
					this.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: No WarningStream was passed in; creating a new stream.", activityInstanceId3));
				}
				pSObjects = base.Input.Get(context);
				if (pSObjects != null)
				{
					bool flag3 = false;
					if (this.ParameterDefaults != null && strs.ContainsKey("Input") && strs["Input"] == base.Input.Get(context))
					{
						flag3 = true;
					}
					if (flag3)
					{
						PSDataCollection<PSObject> pSObjects2 = new PSDataCollection<PSObject>(pSObjects);
						pSObjects2.IsAutoGenerated = true;
						base.Input.Set(context, pSObjects2);
						pSObjects.Clear();
						pSObjects = base.Input.Get(context);
					}
				}
				List<ActivityImplementationContext> tasks = this.GetTasks(context);
				foreach (ActivityImplementationContext task in tasks)
				{
					bool flag4 = false;
					PropertyInfo[] properties = base.GetType().GetProperties();
					for (int i = 0; i < (int)properties.Length; i++)
					{
						PropertyInfo propertyInfo = properties[i];
						if (typeof(Argument).IsAssignableFrom(propertyInfo.PropertyType))
						{
							Argument value1 = (Argument)propertyInfo.GetValue(this, null);
							if (value1 != null && (value1.ArgumentType.IsAssignableFrom(typeof(ScriptBlock)) || value1.ArgumentType.IsAssignableFrom(typeof(ScriptBlock[]))))
							{
								flag4 = true;
								break;
							}
						}
					}
					if (flag4)
					{
						this.PopulateRunspaceFromContext(task, pSActivityContext, context);
					}
					pSActivityContext.commandQueue.Enqueue(task);
				}
				uint? nullable3 = this.PSActionRunningTimeoutSec.Get(context);
				activityInstanceId = new object[2];
				activityInstanceId[0] = context.ActivityInstanceId;
				activityInstanceId[1] = nullable3;
				this.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: Max running time: {1}.", activityInstanceId));
				if (nullable3.HasValue)
				{
					ActivityInstance activityInstance = context.ScheduleActivity(this.cancelTimer, new CompletionCallback(this.MaxRunTimeElapsed));
					pSActivityContext.runningCancelTimer = activityInstance;
				}
				activityInstanceId = new object[1];
				activityInstanceId[0] = context.ActivityInstanceId;
				this.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: Invoking command.", activityInstanceId));
				PSActivity.OnActivityCreated(this, new ActivityCreatedEventArgs(null));
				uint? nullable4 = null;
				uint? nullable5 = null;
				IImplementsConnectionRetry implementsConnectionRetry = this as IImplementsConnectionRetry;
				if (implementsConnectionRetry != null)
				{
					nullable4 = implementsConnectionRetry.PSConnectionRetryCount.Get(context);
					nullable5 = implementsConnectionRetry.PSConnectionRetryIntervalSec.Get(context);
				}
				List<string> strs1 = new List<string>();
				if (!string.IsNullOrEmpty(this.PSDefiningModule))
				{
					strs1.Add(this.PSDefiningModule);
				}
				string[] strArrays = this.PSRequiredModules.Get(context);
				if (strArrays != null)
				{
					strs1.AddRange(strArrays);
				}
				Action<object> activateDelegate = null;
				if (extension != null && extension.ActivateDelegate != null)
				{
					activateDelegate = extension.ActivateDelegate;
				}
				Guid empty = Guid.Empty;
				Guid guid = Guid.NewGuid();
				Bookmark bookmark = context.CreateBookmark(string.Concat(PSActivity.PSBookmarkPrefix, guid.ToString()), new BookmarkCallback(this.OnResumeBookmark));
				if (activateDelegate == null)
				{
					PSWorkflowInstanceExtension pSWorkflowInstanceExtension = context.GetExtension<PSWorkflowInstanceExtension>();
					BookmarkContext bookmarkContext = new BookmarkContext();
					bookmarkContext.CurrentBookmark = bookmark;
					bookmarkContext.BookmarkResumingExtension = pSWorkflowInstanceExtension;
					BookmarkContext bookmarkContext1 = bookmarkContext;
					waitCallback = new WaitCallback(PSActivity.OnComplete);
					obj = bookmarkContext1;
				}
				else
				{
					obj = bookmark;
					waitCallback = new WaitCallback(activateDelegate.Invoke);
					empty = extension.JobInstanceId;
					if (extension != null && extension.AsyncExecutionCollection != null)
					{
						Dictionary<string, PSActivityContext> asyncExecutionCollection = extension.AsyncExecutionCollection;
						if (asyncExecutionCollection != null)
						{
							if (asyncExecutionCollection.ContainsKey(context.ActivityInstanceId))
							{
								asyncExecutionCollection.Remove(context.ActivityInstanceId);
							}
							asyncExecutionCollection.Add(context.ActivityInstanceId, pSActivityContext);
						}
					}
				}
				this.psActivityContextImplementationVariable.Set(context, pSActivityContext);
				pSActivityContext.Callback = waitCallback;
				pSActivityContext.AsyncState = obj;
				pSActivityContext.JobInstanceId = empty;
				pSActivityContext.ActivityParams = new ActivityParameters(nullable4, nullable5, this.PSActionRetryCount.Get(context), this.PSActionRetryIntervalSec.Get(context), strs1.ToArray());
				pSActivityContext.Input = pSObjects;
				if (flag1)
				{
					pSObjects1 = this.CreateOutputStream(context);
				}
				pSActivityContext.Output = pSObjects1;
				pSActivityContext.WorkflowHost = PSActivity.GetWorkflowHost(extension);
				pSActivityContext.RunInProc = this.GetRunInProc(context);
				pSActivityContext.ParameterDefaults = strs;
				pSActivityContext.ActivityType = base.GetType();
				PSActivity pSActivity = this;
				pSActivityContext.PrepareSession = new PrepareSessionDelegate(pSActivity.PrepareSession);
				pSActivityContext.ActivityObject = this;
				if (PSActivity.IsActivityInlineScript(this) && this.RunWithCustomRemoting(context))
				{
					pSActivityContext.RunWithCustomRemoting = true;
				}
				context.SetValue<Dictionary<string, object>>(this.ParameterDefaults, strs);
				pSActivityContext.Execute();
				if (PSActivity._structuredTracer.IsEnabled)
				{
					PSActivity._structuredTracer.ActivityExecutionFinished(displayName);
				}
				return;
			}
			else
			{
				object[] objArray3 = new object[1];
				objArray3[0] = context.ActivityInstanceId;
				this.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: Execution skipped due to supplied (but empty) pipeline input.", objArray3));
				return;
			}
		}

		private static void ExecuteCleanupActivity(RunCommandsArguments args)
		{
			PSCleanupActivity activityObject = args.ActivityObject as PSCleanupActivity;
			if (activityObject != null)
			{
				activityObject.DoCleanup(args, new WaitCallback(PSActivity.CleanupActivityCallback));
				return;
			}
			else
			{
				throw new ArgumentNullException("args");
			}
		}

		private static void ExecuteOneRunspaceFreeCommandWorker(object state)
		{
			RunCommandsArguments runCommandsArgument = state as RunCommandsArguments;
			PSActivityContext pSActivityContext = runCommandsArgument.PSActivityContext;
			ActivityImplementationContext implementationContext = runCommandsArgument.ImplementationContext;
			PSDataCollection<PSObject> input = runCommandsArgument.Input;
			PSDataCollection<PSObject> output = runCommandsArgument.Output;
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			using (traceSource)
			{
				bool flag = false;
				try
				{
					try
					{
						traceSource.WriteMessage("Running WMI/CIM generic activity on ThreadPool thread");
						PSActivity.RunDirectExecutionActivity(implementationContext.PowerShellInstance, input, output, pSActivityContext, implementationContext);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						PowerShellTraceSource powerShellTraceSource = PowerShellTraceSourceFactory.GetTraceSource();
						powerShellTraceSource.TraceException(exception);
						flag = PSActivity.HandleRunOneCommandException(runCommandsArgument, exception);
						if (flag)
						{
							PSActivity.BeginActionRetry(runCommandsArgument);
						}
					}
				}
				finally
				{
					implementationContext.CleanUp();
					PSActivity.RunOneCommandFinally(runCommandsArgument, flag);
					traceSource.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity: Finished running command.", new object[0]));
					PSActivity.DecrementRunningCountAndCheckForEnd(pSActivityContext);
				}
			}
		}

		protected IEnumerable<PSActivityArgumentInfo> GetActivityArguments()
		{
			for (Type i = base.GetType(); i != null; i = null)
			{
				if (i.IsAbstract)
				{
					PropertyInfo[] properties = i.GetProperties();
					for (int j = 0; j < (int)properties.Length; j++)
					{
						PropertyInfo propertyInfo = properties[j];
						if (typeof(Argument).IsAssignableFrom(propertyInfo.PropertyType))
						{
							Argument value = (Argument)propertyInfo.GetValue(this, null);
							PSActivityArgumentInfo pSActivityArgumentInfo = new PSActivityArgumentInfo();
							pSActivityArgumentInfo.Name = propertyInfo.Name;
							pSActivityArgumentInfo.Value = value;
							yield return pSActivityArgumentInfo;
						}
					}
				}
				i = i.BaseType;
				if (typeof(PSActivity).IsAssignableFrom(i))
				{
					continue;
				}
			}
		}

		private bool? GetActivityPersistFlag(NativeActivityContext context)
		{
			bool? nullable;
			object value;
			bool flag = false;
			if (this.PSPersist.Expression != null)
			{
				bool? nullable1 = this.PSPersist.Get(context);
				if (!nullable1.HasValue)
				{
					foreach (PropertyDescriptor property in context.DataContext.GetProperties())
					{
						if (!string.Equals(property.DisplayName, "PSPersistPreference", StringComparison.OrdinalIgnoreCase))
						{
							continue;
						}
						value = property.GetValue(context.DataContext);
						if (value == null || !LanguagePrimitives.TryConvertTo<bool>(value, CultureInfo.InvariantCulture, out flag))
						{
							continue;
						}
						nullable = new bool?(flag);
					}
					return nullable;
				}
				bool? nullable2 = this.PSPersist.Get(context);
				nullable = new bool?(nullable2.Value);
				return nullable;
			}
			foreach (PropertyDescriptor property in context.DataContext.GetProperties())
			{
				if (!string.Equals(property.DisplayName, "PSPersistPreference", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				value = property.GetValue(context.DataContext);
				if (value == null || !LanguagePrimitives.TryConvertTo<bool>(value, CultureInfo.InvariantCulture, out flag))
				{
					continue;
				}
				nullable = new bool?(flag);
			}
			return nullable;
		}

		private static RunCommandsArguments GetArgsForCommand(Guid powerShellId, out string computerName, out Guid jobInstanceId)
		{
			RunCommandsArguments runCommandsArgument = null;
			ActivityImplementationContext implementationContext;
			Guid empty;
			PSActivity.ArgsTable.TryGetValue(powerShellId, out runCommandsArgument);
			computerName = "localhost";
			Guid guidPointer = jobInstanceId;
			if (runCommandsArgument == null)
			{
				empty = Guid.Empty;
			}
			else
			{
				empty = runCommandsArgument.PSActivityContext.JobInstanceId;
			}
			guidPointer = empty;
			if (runCommandsArgument != null)
			{
				if (runCommandsArgument.PSActivityContext.HostExtension != null && runCommandsArgument.PSActivityContext.HostExtension.Parameters != null && runCommandsArgument.PSActivityContext.HostExtension.Parameters.ContainsKey("PSComputerName"))
				{
					string[] item = (string[])runCommandsArgument.PSActivityContext.HostExtension.Parameters["PSComputerName"];
					if ((int)item.Length == 1)
					{
						computerName = item[0];
						return runCommandsArgument;
					}
				}
				int commandExecutionType = runCommandsArgument.CommandExecutionType;
				if (commandExecutionType == 0 || commandExecutionType == 2 || commandExecutionType == 3)
				{
					computerName = PSActivity.GetComputerNameFromCommand(runCommandsArgument.ImplementationContext.PowerShellInstance);
					if (runCommandsArgument != null)
					{
						implementationContext = runCommandsArgument.ImplementationContext;
						if (implementationContext != null && implementationContext.PSRemotingBehavior == RemotingBehavior.Custom && implementationContext.PSComputerName != null && (int)implementationContext.PSComputerName.Length != 0)
						{
							computerName = implementationContext.PSComputerName[0];
						}
					}
					return runCommandsArgument;
				}
				else if (commandExecutionType == 1)
				{
					if (runCommandsArgument != null)
					{
						implementationContext = runCommandsArgument.ImplementationContext;
						if (implementationContext != null && implementationContext.PSRemotingBehavior == RemotingBehavior.Custom && implementationContext.PSComputerName != null && (int)implementationContext.PSComputerName.Length != 0)
						{
							computerName = implementationContext.PSComputerName[0];
						}
					}
					return runCommandsArgument;
				}
			}
			if (runCommandsArgument != null)
			{
				implementationContext = runCommandsArgument.ImplementationContext;
				if (implementationContext != null && implementationContext.PSRemotingBehavior == RemotingBehavior.Custom && implementationContext.PSComputerName != null && (int)implementationContext.PSComputerName.Length != 0)
				{
					computerName = implementationContext.PSComputerName[0];
				}
			}
			return runCommandsArgument;
		}

		private static bool GetComputerNameAndJobIdForCommand(Guid powerShellId, out string computerName, out Guid jobInstanceId)
		{
			RunCommandsArguments argsForCommand = PSActivity.GetArgsForCommand(powerShellId, out computerName, out jobInstanceId);
			return argsForCommand != null;
		}

		private static string GetComputerNameFromCommand(System.Management.Automation.PowerShell commandToRun)
		{
			Runspace runspace = commandToRun.Runspace;
			if (runspace.ConnectionInfo == null)
			{
				return "localhost";
			}
			else
			{
				return runspace.ConnectionInfo.ComputerName;
			}
		}

		private bool GetDisableSerialization(NativeActivityContext context)
		{
			bool flag = false;
			bool flag1;
			bool? nullable = this.PSDisableSerialization.Get(context);
			bool valueOrDefault = nullable.GetValueOrDefault(false);
			if (!valueOrDefault)
			{
				IEnumerator enumerator = context.DataContext.GetProperties().GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						PropertyDescriptor current = (PropertyDescriptor)enumerator.Current;
						if (!string.Equals(current.DisplayName, "PSDisableSerializationPreference", StringComparison.OrdinalIgnoreCase))
						{
							continue;
						}
						object value = current.GetValue(context.DataContext);
						if (value == null || !LanguagePrimitives.TryConvertTo<bool>(value, CultureInfo.InvariantCulture, out flag))
						{
							continue;
						}
						flag1 = flag;
						return flag1;
					}
					goto Label0;
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
				return flag1;
			}
			else
			{
				return valueOrDefault;
			}
		Label0:
			if (!PSSessionConfigurationData.IsServerManager)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		private bool GetHostPersistFlag(NativeActivityContext context)
		{
			Func<bool> hostPersistenceDelegate = null;
			HostParameterDefaults extension = context.GetExtension<HostParameterDefaults>();
			if (extension != null && extension != null && extension.HostPersistenceDelegate != null)
			{
				hostPersistenceDelegate = extension.HostPersistenceDelegate;
			}
			if (hostPersistenceDelegate != null)
			{
				bool flag = hostPersistenceDelegate();
				return flag;
			}
			else
			{
				return false;
			}
		}

		protected virtual List<ActivityImplementationContext> GetImplementation(NativeActivityContext context)
		{
			ActivityImplementationContext powerShell = this.GetPowerShell(context);
			this.UpdateImplementationContextForLocalExecution(powerShell, context);
			List<ActivityImplementationContext> activityImplementationContexts = new List<ActivityImplementationContext>();
			activityImplementationContexts.Add(powerShell);
			return activityImplementationContexts;
		}

		protected abstract ActivityImplementationContext GetPowerShell(NativeActivityContext context);

		protected bool GetRunInProc(ActivityContext context)
		{
			bool flag = false;
			bool flag1;
			HostParameterDefaults extension = context.GetExtension<HostParameterDefaults>();
			if (this as PSGeneratedCIMActivity == null)
			{
				IEnumerator enumerator = context.DataContext.GetProperties().GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						PropertyDescriptor current = (PropertyDescriptor)enumerator.Current;
						if (!string.Equals(current.DisplayName, "PSRunInProcessPreference", StringComparison.OrdinalIgnoreCase))
						{
							continue;
						}
						object value = current.GetValue(context.DataContext);
						if (value == null || !LanguagePrimitives.TryConvertTo<bool>(value, CultureInfo.InvariantCulture, out flag))
						{
							continue;
						}
						flag1 = flag;
						return flag1;
					}
					this.psActivityContextImplementationVariable.Get(context);
					PSWorkflowHost workflowHost = PSActivity.GetWorkflowHost(extension);
					return workflowHost.PSActivityHostController.RunInActivityController(this);
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
				return flag1;
			}
			else
			{
				return true;
			}
		}

		private List<ActivityImplementationContext> GetTasks(NativeActivityContext context)
		{
			List<ActivityImplementationContext> implementation = this.GetImplementation(context);
			if (!PSActivity.IsActivityInlineScript(this) || !this.RunWithCustomRemoting(context))
			{
				foreach (ActivityImplementationContext activityImplementationContext in implementation)
				{
					this.PopulateActivityImplementationContext(activityImplementationContext, context, -1);
				}
				return implementation;
			}
			else
			{
				for (int i = 0; i < implementation.Count; i++)
				{
					this.PopulateActivityImplementationContext(implementation[i], context, i);
				}
				return implementation;
			}
		}

		private static Dictionary<string, object> GetVariablesToSetInRunspace(PSActivityEnvironment activityEnvironment)
		{
			InitialSessionStateEntryCollection<SessionStateVariableEntry> variables = PSActivity.Iss.Variables;
			Func<SessionStateVariableEntry, string> func = (SessionStateVariableEntry entry) => entry.Name;
			Dictionary<string, object> dictionary = variables.ToDictionary<SessionStateVariableEntry, string, object>(func, (SessionStateVariableEntry entry) => entry.Value);
			if (activityEnvironment != null && activityEnvironment.Variables != null)
			{
				foreach (string key in activityEnvironment.Variables.Keys)
				{
					object item = activityEnvironment.Variables[key];
					if (item == null)
					{
						continue;
					}
					if (!dictionary.ContainsKey(key))
					{
						dictionary.Add(key, item);
					}
					else
					{
						dictionary[key] = item;
					}
				}
			}
			if (dictionary.ContainsKey("OutputEncoding"))
			{
				dictionary.Remove("OutputEncoding");
			}
			return dictionary;
		}

		internal static PSWorkflowHost GetWorkflowHost(HostParameterDefaults defaults)
		{
			PSWorkflowHost instance = null;
			if (defaults != null && defaults.Runtime != null)
			{
				PSWorkflowHost runtime = defaults.Runtime;
				Interlocked.CompareExchange<PSWorkflowHost>(ref instance, runtime, null);
			}
			if (instance == null)
			{
				instance = DefaultWorkflowHost.Instance;
			}
			return instance;
		}

		private static void HandleErrorDataAdded(object sender, DataAddedEventArgs e)
		{
			RunCommandsArguments runCommandsArgument = null;
			PSActivity.ArgsTable.TryGetValue(e.PowerShellInstanceId, out runCommandsArgument);
			if (runCommandsArgument != null)
			{
				bool mergeErrorToOutput = runCommandsArgument.PSActivityContext.MergeErrorToOutput;
				if (mergeErrorToOutput)
				{
					PSActivity.MergeError_DataAdded(sender, e, runCommandsArgument.PSActivityContext.errors);
				}
				return;
			}
			else
			{
				return;
			}
		}

		private static void HandleErrorDataAdding(object sender, DataAddingEventArgs e)
		{
			string str = null;
			Guid guid;
			HostSettingCommandMetadata hostCommandMetadata;
			PSDataCollection<PSObject> output;
			ErrorRecord itemAdded = (ErrorRecord)e.ItemAdded;
			if (itemAdded != null)
			{
				RunCommandsArguments argsForCommand = PSActivity.GetArgsForCommand(e.PowerShellInstanceId, out str, out guid);
				if (argsForCommand != null)
				{
					PSActivity.AddIdentifierInfoToErrorRecord(itemAdded, str, argsForCommand.PSActivityContext.JobInstanceId);
					bool hostExtension = argsForCommand.PSActivityContext.HostExtension != null;
					bool mergeErrorToOutput = argsForCommand.PSActivityContext.MergeErrorToOutput;
					if (mergeErrorToOutput || hostExtension)
					{
						if (hostExtension)
						{
							hostCommandMetadata = argsForCommand.PSActivityContext.HostExtension.HostCommandMetadata;
						}
						else
						{
							hostCommandMetadata = null;
						}
						HostSettingCommandMetadata hostSettingCommandMetadatum = hostCommandMetadata;
						if (mergeErrorToOutput)
						{
							output = argsForCommand.PSActivityContext.Output;
						}
						else
						{
							output = null;
						}
						PSDataCollection<PSObject> pSObjects = output;
						PSActivity.PowerShellInvocation_ErrorAdding(sender, e, hostSettingCommandMetadatum, pSObjects);
						return;
					}
					else
					{
						return;
					}
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		private static bool HandleFailure(int attempts, uint? retryCount, uint? retryInterval, ActivityImplementationContext implementationContext, string errorId, Exception e, PSActivityContext psActivityContext)
		{
			bool flag = false;
			if ((long)attempts <= (ulong)retryCount.GetValueOrDefault(0))
			{
				if (e != null)
				{
					PSActivity.WriteError(e, errorId, ErrorCategory.InvalidResult, implementationContext.PowerShellInstance.Runspace.ConnectionInfo, psActivityContext);
				}
				if (!psActivityContext.IsCanceled)
				{
					flag = true;
				}
				for (int i = 0; (long)i < (ulong)retryInterval.GetValueOrDefault(1) && !PSActivity.CheckForCancel(psActivityContext); i++)
				{
					Thread.Sleep(0x3e8);
				}
			}
			else
			{
				if (implementationContext.PSComputerName == null || (int)implementationContext.PSComputerName.Length <= 1)
				{
					if (e != null)
					{
						lock (psActivityContext.exceptions)
						{
							psActivityContext.exceptions.Add(e);
						}
					}
				}
				else
				{
					if (e != null)
					{
						PSActivity.WriteError(e, errorId, ErrorCategory.InvalidResult, implementationContext.PowerShellInstance.Runspace.ConnectionInfo, psActivityContext);
					}
				}
			}
			return flag;
		}

		private static void HandleInformationalRecordDataAdding(object sender, DataAddingEventArgs e)
		{
			string str = null;
			Guid guid;
			InformationalRecord itemAdded = (InformationalRecord)e.ItemAdded;
			if (itemAdded != null)
			{
				if (PSActivity.GetComputerNameAndJobIdForCommand(e.PowerShellInstanceId, out str, out guid))
				{
					itemAdded.Message = PSActivity.AddIdentifierInfoToString(guid, str, itemAdded.Message);
				}
				return;
			}
			else
			{
				return;
			}
		}

		private static void HandleOutputDataAdding(object sender, DataAddingEventArgs e)
		{
			string str = null;
			Guid guid;
			PSObject itemAdded = (PSObject)e.ItemAdded;
			if (itemAdded != null)
			{
				RunCommandsArguments argsForCommand = PSActivity.GetArgsForCommand(e.PowerShellInstanceId, out str, out guid);
				if (argsForCommand != null)
				{
					PSActivity.AddIdentifierInfoToOutput(itemAdded, guid, str);
					return;
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		private static void HandleProgressDataAdding(object sender, DataAddingEventArgs e)
		{
			string str = null;
			Guid guid;
			ProgressRecord itemAdded = (ProgressRecord)e.ItemAdded;
			if (itemAdded != null)
			{
				if (PSActivity.GetComputerNameAndJobIdForCommand(e.PowerShellInstanceId, out str, out guid))
				{
					itemAdded.CurrentOperation = PSActivity.AddIdentifierInfoToString(guid, str, itemAdded.CurrentOperation);
				}
				return;
			}
			else
			{
				return;
			}
		}

		private static bool HandleRunOneCommandException(RunCommandsArguments args, Exception e)
		{
			bool flag = false;
			PSActivityContext pSActivityContext = args.PSActivityContext;
			ActivityImplementationContext implementationContext = args.ImplementationContext;
			ActivityParameters activityParameters = args.ActivityParameters;
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			using (traceSource)
			{
				PowerShell powerShellInstance = implementationContext.PowerShellInstance;
				object[] objArray = new object[1];
				objArray[0] = powerShellInstance;
				traceSource.WriteMessage(string.Format(CultureInfo.InvariantCulture, "Exception handling for command {0}.", objArray));
				object[] message = new object[1];
				message[0] = e.Message;
				traceSource.WriteMessage(string.Format(CultureInfo.InvariantCulture, "Got exception running command: {0}.", message));
				int actionAttempts = 0x7fffffff;
				if (!pSActivityContext.IsCanceled)
				{
					if (pSActivityContext.runningCommands.ContainsKey(powerShellInstance))
					{
						actionAttempts = pSActivityContext.runningCommands[powerShellInstance].ActionAttempts;
					}
					flag = PSActivity.HandleFailure(actionAttempts, activityParameters.ActionRetryCount, activityParameters.ActionRetryInterval, implementationContext, "ActivityActionFailed", e, pSActivityContext);
				}
			}
			return flag;
		}

		private static void HandleRunspaceStateChanged(object sender, RunspaceStateEventArgs eventArgs)
		{
			RunCommandsArguments runCommandsArgument = null;
			if (eventArgs.RunspaceStateInfo.State == RunspaceState.Opened || eventArgs.RunspaceStateInfo.State == RunspaceState.Disconnected)
			{
				Runspace runspace = sender as Runspace;
				PSActivity.ArgsTableForRunspaces.TryGetValue(runspace.InstanceId, out runCommandsArgument);
				if (runCommandsArgument != null)
				{
					PowerShell powerShellInstance = runCommandsArgument.ImplementationContext.PowerShellInstance;
					PSWorkflowHost workflowHost = runCommandsArgument.PSActivityContext.WorkflowHost;
					if (eventArgs.RunspaceStateInfo.State != RunspaceState.Opened || powerShellInstance.InvocationStateInfo.State != PSInvocationState.Disconnected)
					{
						if (eventArgs.RunspaceStateInfo.State == RunspaceState.Disconnected && !workflowHost.RemoteRunspaceProvider.IsDisconnectedByRunspaceProvider(runspace))
						{
							PSActivity.ArgsTableForRunspaces.TryRemove(runspace.InstanceId, out runCommandsArgument);
							object[] computerName = new object[1];
							computerName[0] = runspace.ConnectionInfo.ComputerName;
							RuntimeException runtimeException = new RuntimeException(string.Format(CultureInfo.CurrentCulture, Resources.ActivityFailedDueToRunspaceDisconnect, computerName), eventArgs.RunspaceStateInfo.Reason);
							PSActivity.RunspaceDisconnectedCallback(runCommandsArgument, runtimeException);
						}
						return;
					}
					else
					{
						powerShellInstance.ConnectAsync();
						return;
					}
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		private static void ImportRequiredModulesCallback(IAsyncResult asyncResult)
		{
			object asyncState = asyncResult.AsyncState;
			RunCommandsArguments runCommandsArgument = asyncState as RunCommandsArguments;
			PowerShell helperCommand = runCommandsArgument.HelperCommand;
			PSActivityContext pSActivityContext = runCommandsArgument.PSActivityContext;
			ActivityParameters activityParameters = runCommandsArgument.ActivityParameters;
			Type activityType = runCommandsArgument.ActivityType;
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			using (traceSource)
			{
				traceSource.WriteMessage("Executing callback for importing required modules");
				try
				{
					try
					{
						helperCommand.EndInvoke(asyncResult);
					}
					catch (Exception exception2)
					{
						Exception exception = exception2;
						string str = "";
						string[] pSRequiredModules = activityParameters.PSRequiredModules;
						for (int i = 0; i < (int)pSRequiredModules.Length; i++)
						{
							string str1 = pSRequiredModules[i];
							str = string.Concat(str, str1, ", ");
						}
						char[] chrArray = new char[2];
						chrArray[0] = ',';
						chrArray[1] = ' ';
						str = str.TrimEnd(chrArray);
						object[] name = new object[2];
						name[0] = str;
						name[1] = activityType.Name;
						string str2 = string.Format(CultureInfo.InvariantCulture, Resources.DependModuleImportFailed, name);
						Exception exception1 = new Exception(str2, exception);
						traceSource.TraceException(exception1);
						bool flag = PSActivity.HandleRunOneCommandException(runCommandsArgument, exception1);
						if (!flag)
						{
							PSActivity.RunOneCommandFinally(runCommandsArgument, false);
						}
						else
						{
							traceSource.WriteMessage("Runspace initialization failed, attempting retry");
							PSActivity.CloseRunspace(runCommandsArgument.ImplementationContext.PowerShellInstance.Runspace, runCommandsArgument.CommandExecutionType, runCommandsArgument.WorkflowHost, pSActivityContext);
							PSActivity.BeginActionRetry(runCommandsArgument);
						}
						PSActivity.DecrementRunningCountAndCheckForEnd(pSActivityContext);
						return;
					}
				}
				finally
				{
					helperCommand.Dispose();
					runCommandsArgument.HelperCommand = null;
				}
				if (!PSActivity.CheckForCancel(pSActivityContext))
				{
					PSActivity.BeginPowerShellInvocation(runCommandsArgument);
				}
			}
		}

		private static void InitializeActivityEnvironmentAndAddRequiredModules(ActivityImplementationContext implementationContext, ActivityParameters activityParameters)
		{
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			using (traceSource)
			{
				if (implementationContext.PSActivityEnvironment == null)
				{
					implementationContext.PSActivityEnvironment = new PSActivityEnvironment();
				}
				PSActivityEnvironment pSActivityEnvironment = implementationContext.PSActivityEnvironment;
				string[] pSRequiredModules = activityParameters.PSRequiredModules;
				string[] strArrays = pSRequiredModules;
				if (pSRequiredModules == null)
				{
					strArrays = new string[0];
				}
				string[] strArrays1 = strArrays;
				for (int i = 0; i < (int)strArrays1.Length; i++)
				{
					string str = strArrays1[i];
					traceSource.WriteMessage(string.Concat("Adding dependent module to policy: ", str));
					pSActivityEnvironment.Modules.Add(str);
				}
			}
		}

		private static void InitializeCmdletInstanceParameters(Command command, PSObject wrappedCmdlet, bool isGenericCim, PSActivityContext psActivityContext, CimSessionOptions cimSessionOptions, ActivityImplementationContext implementationContext)
		{
			PSActivity.PSActivity variable = new PSActivity.PSActivity();
			variable.cimSessionOptions = cimSessionOptions;
			bool flag = false;
			foreach (CommandParameter parameter in command.Parameters)
			{
				if (PSActivity._commonCommandParameters.Contains(parameter.Name))
				{
					continue;
				}
				if (parameter.Name.Equals("CimSession"))
				{
					flag = true;
				}
				if (wrappedCmdlet.Properties[parameter.Name] == null)
				{
					wrappedCmdlet.Properties.Add(new PSNoteProperty(parameter.Name, parameter.Value));
				}
				else
				{
					wrappedCmdlet.Properties[parameter.Name].Value = parameter.Value;
				}
			}
			string[] item = null;
			variable.cimActivityImplementationContext = implementationContext as CimActivityImplementationContext;
			if (variable.cimActivityImplementationContext == null || string.IsNullOrEmpty(variable.cimActivityImplementationContext.ComputerName))
			{
				if (psActivityContext.ParameterDefaults.ContainsKey("PSComputerName"))
				{
					item = psActivityContext.ParameterDefaults["PSComputerName"] as string[];
				}
			}
			else
			{
				string[] computerName = new string[1];
				computerName[0] = variable.cimActivityImplementationContext.ComputerName;
				item = computerName;
			}
			if (item != null && (int)item.Length > 0)
			{
				if (!isGenericCim || wrappedCmdlet.Properties["CimSession"] == null)
				{
					if (wrappedCmdlet.Properties["ComputerName"] == null)
					{
						wrappedCmdlet.Properties.Add(new PSNoteProperty("ComputerName", item));
					}
				}
				else
				{
					if (!flag)
					{
						if (variable.cimActivityImplementationContext != null)
						{
							bool value = false;
							bool? pSUseSsl = variable.cimActivityImplementationContext.PSUseSsl;
							if (pSUseSsl.HasValue)
							{
								bool? nullable = variable.cimActivityImplementationContext.PSUseSsl;
								value = nullable.Value;
							}
							uint num = 0;
							uint? pSPort = variable.cimActivityImplementationContext.PSPort;
							if (pSPort.HasValue)
							{
								uint? pSPort1 = variable.cimActivityImplementationContext.PSPort;
								num = pSPort1.Value;
							}
							AuthenticationMechanism authenticationMechanism = AuthenticationMechanism.Default;
							AuthenticationMechanism? pSAuthentication = variable.cimActivityImplementationContext.PSAuthentication;
							if (pSAuthentication.HasValue)
							{
								AuthenticationMechanism? pSAuthentication1 = variable.cimActivityImplementationContext.PSAuthentication;
								authenticationMechanism = pSAuthentication1.Value;
							}
							List<CimSession> cimSessions = item.ToList<string>().ConvertAll<CimSession>((string computer) => CimConnectionManager.GetGlobalCimConnectionManager().GetSession(computer, LambdaVar9.PSCredential, LambdaVar9.PSCertificateThumbprint, authenticationMechanism, cimSessionOptions, value, num, LambdaVar9.PSSessionOption));
							wrappedCmdlet.Properties["CimSession"].Value = cimSessions.ToArray<CimSession>();
							variable.cimActivityImplementationContext.Session = cimSessions[0];
							return;
						}
						else
						{
							throw new ArgumentException(Resources.InvalidImplementationContext);
						}
					}
				}
			}
		}

		private static void InitializeOneCommand(RunCommandsArguments args)
		{
			ActivityParameters activityParameters = args.ActivityParameters;
			PSActivityContext pSActivityContext = args.PSActivityContext;
			PSWorkflowHost workflowHost = args.WorkflowHost;
			//TODO: REIVEW: args.ParameterDefaults;
			Type activityType = args.ActivityType;
			PrepareSessionDelegate @delegate = args.Delegate;
			object activityObject = args.ActivityObject;
			ActivityImplementationContext implementationContext = args.ImplementationContext;
			int commandExecutionType = args.CommandExecutionType;
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			using (traceSource)
			{
				PowerShell powerShellInstance = implementationContext.PowerShellInstance;
				object[] objArray = new object[1];
				objArray[0] = powerShellInstance;
				traceSource.WriteMessage(string.Format(CultureInfo.InvariantCulture, "Beginning initialization for command '{0}'.", objArray));
				lock (pSActivityContext.runningCommands)
				{
					if (!PSActivity.CheckForCancel(pSActivityContext))
					{
						if (!pSActivityContext.runningCommands.ContainsKey(powerShellInstance))
						{
							pSActivityContext.runningCommands[powerShellInstance] = new RetryCount();
						}
					}
					else
					{
						return;
					}
				}
				if (!PSActivity.CheckForCancel(pSActivityContext))
				{
					RetryCount item = pSActivityContext.runningCommands[powerShellInstance];
					item.ActionAttempts = item.ActionAttempts + 1;
					if (commandExecutionType != 5)
					{
						PSActivity.UpdatePowerShell(implementationContext, pSActivityContext, activityType, @delegate, activityObject);
					}
					int num = commandExecutionType;
					switch (num)
					{
						case 0:
						{
							PSActivity.InitializeActivityEnvironmentAndAddRequiredModules(implementationContext, activityParameters);
							workflowHost.LocalRunspaceProvider.BeginGetRunspace(null, 0, 0, new AsyncCallback(PSActivity.LocalRunspaceProviderCallback), args);
							goto Label1;
						}
						case 1:
						case 5:
						{
						Label1:
							break;
						}
						case 2:
						{
							PSActivity.InitializeActivityEnvironmentAndAddRequiredModules(implementationContext, activityParameters);
							goto Label1;
						}
						case 3:
						{
							PSActivity.DisposeRunspaceInPowerShell(powerShellInstance, false);
							PSActivity.InitializeActivityEnvironmentAndAddRequiredModules(implementationContext, activityParameters);
							WSManConnectionInfo connectionInfo = powerShellInstance.Runspace.ConnectionInfo as WSManConnectionInfo;
							uint? connectionRetryCount = activityParameters.ConnectionRetryCount;
							uint? connectionRetryInterval = activityParameters.ConnectionRetryInterval;
							workflowHost.RemoteRunspaceProvider.BeginGetRunspace(connectionInfo, connectionRetryCount.GetValueOrDefault(0), connectionRetryInterval.GetValueOrDefault(0), new AsyncCallback(PSActivity.ConnectionManagerCallback), args);
							goto Label1;
						}
						case 4:
						{
							workflowHost.LocalRunspaceProvider.BeginGetRunspace(null, 0, 0, new AsyncCallback(PSActivity.LocalRunspaceProviderCallback), args);
							goto Label1;
						}
						default:
						{
							goto Label1;
						}
					}
				}
			}
		}

		private static void InitializeRunspaceAndExecuteCommandWorker(object state)
		{
			RunCommandsArguments runCommandsArgument = state as RunCommandsArguments;
			//TODO: REVIEW: runCommandsArgument.ImplementationContext.PowerShellInstance;
			PSActivityContext pSActivityContext = runCommandsArgument.PSActivityContext;
			try
			{
				if (!PSActivity.CheckForCancel(pSActivityContext))
				{
					PSActivity.BeginRunspaceInitializeSetup(runCommandsArgument);
				}
			}
			catch (PipelineStoppedException pipelineStoppedException)
			{
			}
		}

		internal static bool IsActivityInlineScript(Activity activity)
		{
			return string.Equals(activity.GetType().FullName, "Microsoft.PowerShell.Activities.InlineScript", StringComparison.OrdinalIgnoreCase);
		}

		private static void LocalRunspaceProviderCallback(IAsyncResult asyncResult)
		{
			object asyncState = asyncResult.AsyncState;
			RunCommandsArguments runCommandsArgument = asyncState as RunCommandsArguments;
			ActivityImplementationContext implementationContext = runCommandsArgument.ImplementationContext;
			PowerShell powerShellInstance = implementationContext.PowerShellInstance;
			PSWorkflowHost workflowHost = runCommandsArgument.WorkflowHost;
			PSActivityContext pSActivityContext = runCommandsArgument.PSActivityContext;
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			using (traceSource)
			{
				traceSource.WriteMessage("Executing callback for LocalRunspaceProvider");
				Runspace runspace = null;
				try
				{
					runspace = workflowHost.LocalRunspaceProvider.EndGetRunspace(asyncResult);
					if (runspace.ConnectionInfo == null)
					{
						if (pSActivityContext.UserVariables.Count != 0)
						{
							foreach (KeyValuePair<string, object> userVariable in pSActivityContext.UserVariables)
							{
								runspace.SessionStateProxy.SetVariable(userVariable.Key, userVariable.Value);
							}
						}
						PSActivity.SetCurrentDirectory(pSActivityContext, runspace);
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					lock (pSActivityContext.exceptions)
					{
						pSActivityContext.exceptions.Add(exception);
					}
					PSActivity.RunOneCommandFinally(runCommandsArgument, false);
					PSActivity.DecrementRunningCountAndCheckForEnd(pSActivityContext);
					return;
				}
				Guid instanceId = runspace.InstanceId;
				traceSource.WriteMessage("Local Runspace successfully obtained with guid ", instanceId.ToString());
				if (pSActivityContext.IsCanceled)
				{
					PSActivity.CloseRunspace(runspace, 0, workflowHost, pSActivityContext);
					if (PSActivity.CheckForCancel(pSActivityContext))
					{
						return;
					}
				}
				powerShellInstance.Runspace = runspace;
				PSActivity.OnActivityCreated(runCommandsArgument.ActivityObject, new ActivityCreatedEventArgs(powerShellInstance));
				if (runCommandsArgument.CommandExecutionType == 4)
				{
					CimActivityImplementationContext session = implementationContext as CimActivityImplementationContext;
					runspace.SessionStateProxy.InvokeCommand.InvokeScript(false, session.ModuleScriptBlock, null, new object[0]);
					if (session.Session == null && !string.IsNullOrEmpty(session.ComputerName) && !string.Equals(session.ComputerName, "localhost", StringComparison.OrdinalIgnoreCase))
					{
						bool value = false;
						bool? pSUseSsl = session.PSUseSsl;
						if (pSUseSsl.HasValue)
						{
							bool? nullable = session.PSUseSsl;
							value = nullable.Value;
						}
						uint num = 0;
						uint? pSPort = session.PSPort;
						if (pSPort.HasValue)
						{
							uint? pSPort1 = session.PSPort;
							num = pSPort1.Value;
						}
						AuthenticationMechanism authenticationMechanism = AuthenticationMechanism.Default;
						AuthenticationMechanism? pSAuthentication = session.PSAuthentication;
						if (pSAuthentication.HasValue)
						{
							AuthenticationMechanism? pSAuthentication1 = session.PSAuthentication;
							authenticationMechanism = pSAuthentication1.Value;
						}
						session.Session = CimConnectionManager.GetGlobalCimConnectionManager().GetSession(session.ComputerName, session.PSCredential, session.PSCertificateThumbprint, authenticationMechanism, session.SessionOptions, value, num, session.PSSessionOption);
						if (session.Session != null)
						{
							powerShellInstance.AddParameter("CimSession", session.Session);
						}
						else
						{
							throw new InvalidOperationException();
						}
					}
				}
				PSActivity.BeginExecuteOneCommand(runCommandsArgument);
				traceSource.WriteMessage("Returning from callback for GetRunspace for LocalRunspaceProvider");
			}
		}

		private void MaxRunTimeElapsed(NativeActivityContext context, ActivityInstance instance)
		{
			if (instance.State == ActivityInstanceState.Canceled)
			{
				return;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = this.PSActionRunningTimeoutSec.Get(context);
				string str = string.Format(CultureInfo.CurrentCulture, Resources.RunningTimeExceeded, objArray);
				throw new TimeoutException(str);
			}
		}

		private static void MergeError_DataAdded(object sender, DataAddedEventArgs e, PSDataCollection<ErrorRecord> errors)
		{
			if (errors != null)
			{
				errors.RemoveAt(0);
			}
		}

		private static void OnActivityCreated(object sender, ActivityCreatedEventArgs e)
		{
			if (PSActivity.ActivityCreated != null)
			{
				PSActivity.ActivityCreated(sender, e);
			}
		}

		private static void OnComplete(object state)
		{
			PSActivity._structuredTracer.Correlate();
			BookmarkContext bookmarkContext = state as BookmarkContext;
			PSWorkflowInstanceExtension bookmarkResumingExtension = bookmarkContext.BookmarkResumingExtension;
			Bookmark currentBookmark = bookmarkContext.CurrentBookmark;
			ThreadPool.QueueUserWorkItem((object o) => bookmarkResumingExtension.BeginResumeBookmark(currentBookmark, null, (IAsyncResult ar) => bookmarkResumingExtension.EndResumeBookmark(ar), null));
		}

		private void OnResumeBookmark(NativeActivityContext context, Bookmark bookmark, object value)
		{
			PSActivity._structuredTracer.Correlate();
			if (!this.bookmarking.Get(context))
			{
				NoPersistHandle noPersistHandle = this.noPersistHandle.Get(context);
				noPersistHandle.Exit(context);
			}
			ActivityOnResumeAction activityOnResumeAction = ActivityOnResumeAction.Resume;
			if (value != null && (ActivityOnResumeAction)value.GetType() == typeof(ActivityOnResumeAction))
			{
				activityOnResumeAction = (ActivityOnResumeAction)value;
			}
			if (activityOnResumeAction != ActivityOnResumeAction.Restart)
			{
				PSResumableActivityContext pSResumableActivityContext = null;
				if (value != null && value.GetType() == typeof(PSResumableActivityContext))
				{
					pSResumableActivityContext = (PSResumableActivityContext)value;
				}
				if (pSResumableActivityContext == null)
				{
					PSActivityContext item = null;
					PSDataCollection<ProgressRecord> progressRecords = null;
					try
					{
						if (this.bookmarking.Get(context))
						{
							HostParameterDefaults extension = context.GetExtension<HostParameterDefaults>();
							if (extension != null && extension != null && extension.AsyncExecutionCollection != null)
							{
								Dictionary<string, PSActivityContext> asyncExecutionCollection = extension.AsyncExecutionCollection;
								if (asyncExecutionCollection != null && asyncExecutionCollection.ContainsKey(context.ActivityInstanceId))
								{
									item = asyncExecutionCollection[context.ActivityInstanceId];
									asyncExecutionCollection.Remove(context.ActivityInstanceId);
								}
							}
							if (item != null)
							{
								progressRecords = item.progress;
								if (base.Result.Expression != null)
								{
									base.Result.Set(context, item.Output);
								}
							}
						}
						else
						{
							progressRecords = this.PSProgress.Get(context);
							item = this.psActivityContextImplementationVariable.Get(context);
							this.psActivityContextImplementationVariable.Set(context, null);
							HostParameterDefaults hostParameterDefault = context.GetExtension<HostParameterDefaults>();
							if (hostParameterDefault != null && hostParameterDefault != null && hostParameterDefault.AsyncExecutionCollection != null)
							{
								Dictionary<string, PSActivityContext> strs = hostParameterDefault.AsyncExecutionCollection;
								if (strs != null && strs.ContainsKey(context.ActivityInstanceId))
								{
									strs.Remove(context.ActivityInstanceId);
								}
							}
						}
						if (item.runningCancelTimer != null)
						{
							context.CancelChild(item.runningCancelTimer);
						}
						if (item.exceptions.Count <= 0)
						{
							this.ActivityEndPersistence(context);
							if (!item.Failed)
							{
								this.WriteProgressRecord(context, progressRecords, Resources.CompletedString, ProgressRecordType.Completed);
							}
							else
							{
								this.WriteProgressRecord(context, progressRecords, Resources.FailedString, ProgressRecordType.Completed);
							}
						}
						else
						{
							this.WriteProgressRecord(context, progressRecords, Resources.FailedString, ProgressRecordType.Completed);
							this.Tracer.WriteMessage("PSActivity", "OnResumeBookmark", context.WorkflowInstanceId, "We are about to rethrow the exception in order to preserve the stack trace writing it into the logs.", new string[0]);
							this.Tracer.TraceException(item.exceptions[0]);
							throw item.exceptions[0];
						}
					}
					finally
					{
						if (item != null)
						{
							item.Dispose();
						}
					}
					return;
				}
				else
				{
					HostParameterDefaults extension1 = context.GetExtension<HostParameterDefaults>();
					if (extension1 != null)
					{
						if (pSResumableActivityContext.SupportDisconnectedStreams && pSResumableActivityContext.Streams != null)
						{
							this.PopulateSteamsData(pSResumableActivityContext, context, extension1);
						}
						PSDataCollection<ProgressRecord> item1 = null;
						if (this.PSProgress.Expression == null)
						{
							if (extension1.Parameters["PSProgress"] != null && extension1.Parameters["PSProgress"].GetType() == typeof(PSDataCollection<ProgressRecord>))
							{
								item1 = extension1.Parameters["PSProgress"] as PSDataCollection<ProgressRecord>;
							}
						}
						else
						{
							item1 = this.PSProgress.Get(context);
						}
						if (pSResumableActivityContext.Error == null)
						{
							this.ActivityEndPersistence(context);
							if (!pSResumableActivityContext.Failed)
							{
								this.WriteProgressRecord(context, item1, Resources.CompletedString, ProgressRecordType.Completed);
							}
							else
							{
								this.WriteProgressRecord(context, item1, Resources.FailedString, ProgressRecordType.Completed);
								return;
							}
						}
						else
						{
							this.WriteProgressRecord(context, item1, Resources.FailedString, ProgressRecordType.Completed);
							this.Tracer.WriteMessage("PSActivity", "OnResumeBookmark", context.WorkflowInstanceId, "We are about to rethrow the exception in order to preserve the stack trace writing it into the logs.", new string[0]);
							this.Tracer.TraceException(pSResumableActivityContext.Error);
							throw pSResumableActivityContext.Error;
						}
					}
					return;
				}
			}
			else
			{
				this.Execute(context);
				return;
			}
		}

		private void PopulateActivityImplementationContext(ActivityImplementationContext implementationContext, NativeActivityContext context, int index)
		{
			foreach (PSActivityArgumentInfo activityArgument in this.GetActivityArguments())
			{
				PropertyInfo property = implementationContext.GetType().GetProperty(activityArgument.Name);
				if (property != null)
				{
					if (!string.Equals(activityArgument.Name, "PSComputerName", StringComparison.OrdinalIgnoreCase) || index == -1)
					{
						property.SetValue(implementationContext, activityArgument.Value.Get(context), null);
					}
					else
					{
						PSActivity.PopulatePSComputerName(implementationContext, context, activityArgument, index);
					}
				}
				else
				{
					throw new Exception(string.Concat("Could not find corresponding task context field for activity argument: ", activityArgument.Name));
				}
			}
		}

		private void PopulateParameterFromDefault(Argument argument, NativeActivityContext context, string argumentName, Dictionary<string, object> parameterDefaults)
		{
			if (argument != null && argument.Expression == null && argument.Direction != ArgumentDirection.Out && this.ParameterDefaults != null && parameterDefaults.ContainsKey(argumentName))
			{
				object[] activityInstanceId = new object[2];
				activityInstanceId[0] = context.ActivityInstanceId;
				activityInstanceId[1] = argumentName;
				this.Tracer.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity ID={0}: Using default {1} value.", activityInstanceId));
				object item = parameterDefaults[argumentName];
				if ((argument.ArgumentType == typeof(bool) || argument.ArgumentType == typeof(bool?)) && item as SwitchParameter != null)
				{
					SwitchParameter switchParameter = (SwitchParameter)item;
					item = switchParameter.ToBool();
				}
				if (argument.ArgumentType.IsGenericType && argument.ArgumentType.GetGenericTypeDefinition() == typeof(Nullable<>) && item as Nullable == null)
				{
					item = LanguagePrimitives.ConvertTo(item, argument.ArgumentType, CultureInfo.InvariantCulture);
				}
				if (argument.ArgumentType.IsAssignableFrom(typeof(PSCredential)) && item.GetType().IsAssignableFrom(typeof(PSObject)))
				{
					item = LanguagePrimitives.ConvertTo(item, typeof(PSCredential), CultureInfo.InvariantCulture);
				}
				argument.Set(context, item);
			}
		}

		private static void PopulatePSComputerName(ActivityImplementationContext implementationContext, NativeActivityContext context, PSActivityArgumentInfo field, int index)
		{
			PropertyInfo property = implementationContext.GetType().GetProperty(field.Name);
			string[] strArrays = (string[])field.Value.Get(context);
			string[] strArrays1 = new string[1];
			strArrays1[0] = strArrays[index];
			property.SetValue(implementationContext, strArrays1, null);
		}

		private void PopulateRunspaceFromContext(ActivityImplementationContext implementationContext, PSActivityContext activityContext, NativeActivityContext context)
		{
			if (implementationContext.PowerShellInstance != null)
			{
				PropertyDescriptorCollection properties = context.DataContext.GetProperties();
				foreach (PropertyDescriptor property in properties)
				{
					string name = property.Name;
					object value = property.GetValue(context.DataContext);
					if (value == null)
					{
						continue;
					}
					object item = value;
					PSDataCollection<PSObject> pSObjects = value as PSDataCollection<PSObject>;
					if (pSObjects != null && pSObjects.Count == 1)
					{
						item = pSObjects[0];
					}
					activityContext.UserVariables.Add(name, item);
				}
			}
		}

		private void PopulateSteamsData(PSResumableActivityContext arguments, NativeActivityContext context, HostParameterDefaults hostValues)
		{
			if (arguments.Streams.OutputStream != null)
			{
				if (base.Result.Expression == null)
				{
					if (hostValues.Parameters["Result"] != null && hostValues.Parameters["Result"].GetType() == typeof(PSDataCollection<PSObject>))
					{
						PSDataCollection<PSObject> item = hostValues.Parameters["Result"] as PSDataCollection<PSObject>;
						if (item != arguments.Streams.OutputStream && item != null && item.IsOpen)
						{
							foreach (PSObject outputStream in arguments.Streams.OutputStream)
							{
								item.Add(outputStream);
							}
						}
					}
				}
				else
				{
					base.Result.Set(context, arguments.Streams.OutputStream);
				}
			}
			if (arguments.Streams.InputStream != null)
			{
				if (base.Input.Expression == null)
				{
					if (hostValues.Parameters["Input"] != null && hostValues.Parameters["Input"].GetType() == typeof(PSDataCollection<PSObject>))
					{
						hostValues.Parameters["Input"] = arguments.Streams.InputStream;
					}
				}
				else
				{
					base.Input.Set(context, arguments.Streams.InputStream);
				}
			}
			if (arguments.Streams.ErrorStream != null)
			{
				if (this.PSError.Expression == null)
				{
					if (hostValues.Parameters["PSError"] != null && hostValues.Parameters["PSError"].GetType() == typeof(PSDataCollection<ErrorRecord>))
					{
						PSDataCollection<ErrorRecord> errorRecords = hostValues.Parameters["PSError"] as PSDataCollection<ErrorRecord>;
						if (errorRecords != arguments.Streams.ErrorStream && errorRecords != null && errorRecords.IsOpen)
						{
							foreach (ErrorRecord errorStream in arguments.Streams.ErrorStream)
							{
								errorRecords.Add(errorStream);
							}
						}
					}
				}
				else
				{
					this.PSError.Set(context, arguments.Streams.ErrorStream);
				}
			}
			if (arguments.Streams.WarningStream != null)
			{
				if (this.PSWarning.Expression == null)
				{
					if (hostValues.Parameters["PSWarning"] != null && hostValues.Parameters["PSWarning"].GetType() == typeof(PSDataCollection<WarningRecord>))
					{
						PSDataCollection<WarningRecord> warningRecords = hostValues.Parameters["PSWarning"] as PSDataCollection<WarningRecord>;
						if (warningRecords != arguments.Streams.WarningStream && warningRecords != null && warningRecords.IsOpen)
						{
							foreach (WarningRecord warningStream in arguments.Streams.WarningStream)
							{
								warningRecords.Add(warningStream);
							}
						}
					}
				}
				else
				{
					this.PSWarning.Set(context, arguments.Streams.WarningStream);
				}
			}
			if (arguments.Streams.ProgressStream != null)
			{
				if (this.PSProgress.Expression == null)
				{
					if (hostValues.Parameters["PSProgress"] != null && hostValues.Parameters["PSProgress"].GetType() == typeof(PSDataCollection<ProgressRecord>))
					{
						PSDataCollection<ProgressRecord> progressRecords = hostValues.Parameters["PSProgress"] as PSDataCollection<ProgressRecord>;
						if (progressRecords != arguments.Streams.ProgressStream && progressRecords != null && progressRecords.IsOpen)
						{
							foreach (ProgressRecord progressStream in arguments.Streams.ProgressStream)
							{
								progressRecords.Add(progressStream);
							}
						}
					}
				}
				else
				{
					this.PSProgress.Set(context, arguments.Streams.ProgressStream);
				}
			}
			if (arguments.Streams.VerboseStream != null)
			{
				if (this.PSVerbose.Expression == null)
				{
					if (hostValues.Parameters["PSVerbose"] != null && hostValues.Parameters["PSVerbose"].GetType() == typeof(PSDataCollection<VerboseRecord>))
					{
						PSDataCollection<VerboseRecord> verboseRecords = hostValues.Parameters["PSVerbose"] as PSDataCollection<VerboseRecord>;
						if (verboseRecords != arguments.Streams.VerboseStream && verboseRecords != null && verboseRecords.IsOpen)
						{
							foreach (VerboseRecord verboseStream in arguments.Streams.VerboseStream)
							{
								verboseRecords.Add(verboseStream);
							}
						}
					}
				}
				else
				{
					this.PSVerbose.Set(context, arguments.Streams.VerboseStream);
				}
			}
			if (arguments.Streams.DebugStream != null)
			{
				if (this.PSDebug.Expression == null)
				{
					if (hostValues.Parameters["PSDebug"] != null && hostValues.Parameters["PSDebug"].GetType() == typeof(PSDataCollection<DebugRecord>))
					{
						PSDataCollection<DebugRecord> debugRecords = hostValues.Parameters["PSDebug"] as PSDataCollection<DebugRecord>;
						if (debugRecords != arguments.Streams.DebugStream && debugRecords != null && debugRecords.IsOpen)
						{
							foreach (DebugRecord debugStream in arguments.Streams.DebugStream)
							{
								debugRecords.Add(debugStream);
							}
						}
					}
				}
				else
				{
					this.PSDebug.Set(context, arguments.Streams.DebugStream);
					return;
				}
			}
		}

		private static void PowerShellInvocation_ErrorAdding(object sender, DataAddingEventArgs e, HostSettingCommandMetadata commandMetadata, PSDataCollection<PSObject> output)
		{
			ErrorRecord itemAdded = e.ItemAdded as ErrorRecord;
			if (itemAdded != null)
			{
				if (commandMetadata != null)
				{
					ScriptPosition scriptPosition = new ScriptPosition(commandMetadata.CommandName, commandMetadata.StartLineNumber, commandMetadata.StartColumnNumber, null);
					ScriptPosition scriptPosition1 = new ScriptPosition(commandMetadata.CommandName, commandMetadata.EndLineNumber, commandMetadata.EndColumnNumber, null);
					ScriptExtent scriptExtent = new ScriptExtent(scriptPosition, scriptPosition1);
					if (itemAdded.InvocationInfo != null)
					{
						itemAdded.InvocationInfo.DisplayScriptPosition = scriptExtent;
					}
				}
				if (output != null)
				{
					output.Add(PSObject.AsPSObject(itemAdded));
				}
			}
		}

		private static void PowerShellInvocationCallback(IAsyncResult asyncResult)
		{
			object asyncState = asyncResult.AsyncState;
			RunCommandsArguments runCommandsArgument = asyncState as RunCommandsArguments;
			PSActivityContext pSActivityContext = runCommandsArgument.PSActivityContext;
			ActivityImplementationContext implementationContext = runCommandsArgument.ImplementationContext;
			PowerShell powerShellInstance = implementationContext.PowerShellInstance;
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			using (traceSource)
			{
				traceSource.WriteMessage("Executing callback for Executing command using PowerShell - either inproc or remote");
				bool flag = false;
				try
				{
					try
					{
						if (!PSActivity.CheckForCancel(pSActivityContext))
						{
							powerShellInstance.EndInvoke(asyncResult);
							implementationContext.CleanUp();
							if (powerShellInstance.HadErrors)
							{
								traceSource.WriteMessage(string.Format(CultureInfo.InvariantCulture, "Errors occurred executing the command.", new object[0]));
								pSActivityContext.Failed = true;
							}
						}
						else
						{
							return;
						}
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						flag = PSActivity.ProcessException(runCommandsArgument, exception);
					}
				}
				finally
				{
					if (!flag)
					{
						PSActivity.ReleaseResourcesAndCheckForEnd(powerShellInstance, runCommandsArgument, true, false);
					}
					else
					{
						Interlocked.Decrement(ref pSActivityContext.CommandsRunningCount);
						PSActivity.BeginActionRetry(runCommandsArgument);
					}
				}
			}
		}

		protected virtual void PrepareSession(ActivityImplementationContext implementationContext)
		{
		}

		private static bool ProcessException(RunCommandsArguments args, Exception e)
		{
			string str = null;
			Guid guid;
			IContainsErrorRecord containsErrorRecord;
			ErrorRecord errorRecord;
			string str1 = null;
			Guid guid1;
			bool flag = false;
			PSActivityContext pSActivityContext = args.PSActivityContext;
			ActivityImplementationContext implementationContext = args.ImplementationContext;
			PowerShell powerShellInstance = implementationContext.PowerShellInstance;
			uint? connectionRetryCount = args.ActivityParameters.ConnectionRetryCount;
			if (!connectionRetryCount.HasValue)
			{
				uint? connectionRetryInterval = args.ActivityParameters.ConnectionRetryInterval;
				if (!connectionRetryInterval.HasValue)
				{
					containsErrorRecord = e as IContainsErrorRecord;
					if (containsErrorRecord != null)
					{
						errorRecord = containsErrorRecord.ErrorRecord;
						if (PSActivity.GetComputerNameAndJobIdForCommand(powerShellInstance.InstanceId, out str1, out guid1))
						{
							PSActivity.AddIdentifierInfoToErrorRecord(errorRecord, str1, guid1);
						}
					}
					flag = PSActivity.HandleRunOneCommandException(args, e);
					return flag;
				}
			}
			if (e.InnerException != null && e.InnerException as IContainsErrorRecord != null)
			{
				IContainsErrorRecord containsErrorRecord1 = e as IContainsErrorRecord;
				if (containsErrorRecord1.ErrorRecord.FullyQualifiedErrorId.StartsWith("CimJob_BrokenCimSession", StringComparison.OrdinalIgnoreCase))
				{
					int actionAttempts = 0x7fffffff;
					if (!pSActivityContext.IsCanceled)
					{
						if (pSActivityContext.runningCommands.ContainsKey(powerShellInstance))
						{
							actionAttempts = pSActivityContext.runningCommands[powerShellInstance].ActionAttempts;
						}
						flag = PSActivity.HandleFailure(actionAttempts, args.ActivityParameters.ConnectionRetryCount, args.ActivityParameters.ConnectionRetryInterval, implementationContext, "ActivityActionFailed", null, pSActivityContext);
					}
					if (!flag)
					{
						ErrorRecord errorRecord1 = containsErrorRecord1.ErrorRecord;
						if (PSActivity.GetComputerNameAndJobIdForCommand(powerShellInstance.InstanceId, out str, out guid))
						{
							PSActivity.AddIdentifierInfoToErrorRecord(errorRecord1, str, guid);
						}
						if (implementationContext.PSComputerName == null || (int)implementationContext.PSComputerName.Length <= 1)
						{
							lock (pSActivityContext.exceptions)
							{
								pSActivityContext.exceptions.Add(e);
							}
						}
						else
						{
							PSActivity.WriteError(e, "ActivityActionFailed", ErrorCategory.InvalidResult, implementationContext.PowerShellInstance.Runspace.ConnectionInfo, pSActivityContext);
						}
					}
					return flag;
				}
			}
			containsErrorRecord = e as IContainsErrorRecord;
			if (containsErrorRecord != null)
			{
				errorRecord = containsErrorRecord.ErrorRecord;
				if (PSActivity.GetComputerNameAndJobIdForCommand(powerShellInstance.InstanceId, out str1, out guid1))
				{
					PSActivity.AddIdentifierInfoToErrorRecord(errorRecord, str1, guid1);
				}
			}
			flag = PSActivity.HandleRunOneCommandException(args, e);
			return flag;
		}

		private static void RaiseTerminalCallback(PSActivityContext psActivityContext)
		{
			lock (psActivityContext.SyncRoot)
			{
				if (psActivityContext.AllCommandsStarted || psActivityContext.commandQueue.Count == 0)
				{
					ThreadPool.QueueUserWorkItem(psActivityContext.Callback, psActivityContext.AsyncState);
				}
			}
		}

		private static void ReleaseResourcesAndCheckForEnd(System.Management.Automation.PowerShell ps, RunCommandsArguments args, bool removeHandlersFromStreams, bool attemptRetry)
		{
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			using (traceSource)
			{
				PSActivityContext pSActivityContext = args.PSActivityContext;
				if (removeHandlersFromStreams)
				{
					PSActivity.RemoveHandlersFromStreams(ps, args);
				}
				PSActivity.RunOneCommandFinally(args, attemptRetry);
				traceSource.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity: Finished running command.", new object[0]));
				PSActivity.DecrementRunningCountAndCheckForEnd(pSActivityContext);
			}
		}

		private static void RemoveHandlersFromStreams(System.Management.Automation.PowerShell commandToRun, RunCommandsArguments args)
		{
			RunCommandsArguments runCommandsArgument = null;
			if (commandToRun == null || args == null)
			{
				return;
			}
			else
			{
				bool mergeErrorToOutput = args.PSActivityContext.MergeErrorToOutput;
				if (mergeErrorToOutput)
				{
					commandToRun.Streams.Error.DataAdded -= new EventHandler<DataAddedEventArgs>(PSActivity.HandleErrorDataAdded);
				}
				if (args.PSActivityContext.Output != null)
				{
					args.PSActivityContext.Output.DataAdding -= new EventHandler<DataAddingEventArgs>(PSActivity.HandleOutputDataAdding);
				}
				commandToRun.Streams.Error.DataAdding -= new EventHandler<DataAddingEventArgs>(PSActivity.HandleErrorDataAdding);
				commandToRun.Streams.Progress.DataAdding -= new EventHandler<DataAddingEventArgs>(PSActivity.HandleProgressDataAdding);
				commandToRun.Streams.Verbose.DataAdding -= new EventHandler<DataAddingEventArgs>(PSActivity.HandleInformationalRecordDataAdding);
				commandToRun.Streams.Warning.DataAdding -= new EventHandler<DataAddingEventArgs>(PSActivity.HandleInformationalRecordDataAdding);
				commandToRun.Streams.Debug.DataAdding -= new EventHandler<DataAddingEventArgs>(PSActivity.HandleInformationalRecordDataAdding);
				PSActivity.ArgsTable.TryRemove(commandToRun.InstanceId, out runCommandsArgument);
				return;
			}
		}

		private static void RunDirectExecutionActivity(System.Management.Automation.PowerShell commandToRun, PSDataCollection<PSObject> input, PSDataCollection<PSObject> output, PSActivityContext psActivityContext, ActivityImplementationContext implementationContext)
		{
			PSDataCollection<PSObject> pSObjects;
			Type type;
			CimSessionOptions sessionOptions;
			Command item = commandToRun.Commands.Commands[0];
			string commandText = item.CommandText;
			Cmdlet invokeWmiMethod = null;
			bool flag = false;
			if (!string.Equals(commandText, "Get-WMIObject", StringComparison.OrdinalIgnoreCase))
			{
				if (string.Equals(commandText, "Invoke-WMIMethod", StringComparison.OrdinalIgnoreCase))
				{
					invokeWmiMethod = new InvokeWmiMethod();
					flag = true;
				}
			}
			else
			{
				invokeWmiMethod = new GetWmiObjectCommand();
			}
			if (!PSActivity.CheckForCancel(psActivityContext))
			{
				if (output != null)
				{
					pSObjects = output;
				}
				else
				{
					pSObjects = new PSDataCollection<PSObject>();
				}
				ActivityImplementationContext activityImplementationContext = implementationContext;
				if (invokeWmiMethod != null)
				{
					type = invokeWmiMethod.GetType();
				}
				else
				{
					type = psActivityContext.TypeImplementingCmdlet;
				}
				DirectExecutionActivitiesCommandRuntime directExecutionActivitiesCommandRuntime = new DirectExecutionActivitiesCommandRuntime(pSObjects, activityImplementationContext, type);
				directExecutionActivitiesCommandRuntime.Error = commandToRun.Streams.Error;
				directExecutionActivitiesCommandRuntime.Warning = commandToRun.Streams.Warning;
				directExecutionActivitiesCommandRuntime.Progress = commandToRun.Streams.Progress;
				directExecutionActivitiesCommandRuntime.Verbose = commandToRun.Streams.Verbose;
				directExecutionActivitiesCommandRuntime.Debug = commandToRun.Streams.Debug;
				if (invokeWmiMethod == null)
				{
					CimActivityImplementationContext cimActivityImplementationContext = implementationContext as CimActivityImplementationContext;
					if (cimActivityImplementationContext != null)
					{
						sessionOptions = cimActivityImplementationContext.SessionOptions;
					}
					else
					{
						sessionOptions = null;
					}
					CimSessionOptions cimSessionOption = sessionOptions;
					if (psActivityContext.TypeImplementingCmdlet != null)
					{
						if (input == null || input.Count <= 0 && !input.IsOpen)
						{
							CimBaseCommand cimBaseCommand = (CimBaseCommand)Activator.CreateInstance(psActivityContext.TypeImplementingCmdlet);
							using (cimBaseCommand)
							{
								cimBaseCommand.CommandRuntime = directExecutionActivitiesCommandRuntime;
								PSObject pSObject = PSObject.AsPSObject(cimBaseCommand);
								PSActivity.InitializeCmdletInstanceParameters(item, pSObject, true, psActivityContext, cimSessionOption, implementationContext);
								cimBaseCommand.Invoke().GetEnumerator().MoveNext();
							}
						}
						else
						{
							if (psActivityContext.TypeImplementingCmdlet.GetProperty("InputObject") != null)
							{
								foreach (PSObject pSObject1 in input)
								{
									try
									{
										CimBaseCommand cimBaseCommand1 = (CimBaseCommand)Activator.CreateInstance(psActivityContext.TypeImplementingCmdlet);
										using (cimBaseCommand1)
										{
											cimBaseCommand1.CommandRuntime = directExecutionActivitiesCommandRuntime;
											CimInstance cimInstance = LanguagePrimitives.ConvertTo<CimInstance>(pSObject1);
											PSObject pSObject2 = PSObject.AsPSObject(cimBaseCommand1);
											PSActivity.InitializeCmdletInstanceParameters(item, pSObject2, true, psActivityContext, cimSessionOption, implementationContext);
											PSPropertyInfo pSPropertyInfo = pSObject2.Properties["InputObject"];
											pSPropertyInfo.Value = cimInstance;
											cimBaseCommand1.Invoke().GetEnumerator().MoveNext();
										}
									}
									catch (PSInvalidCastException pSInvalidCastException1)
									{
										PSInvalidCastException pSInvalidCastException = pSInvalidCastException1;
										if (pSInvalidCastException.ErrorRecord != null)
										{
											directExecutionActivitiesCommandRuntime.Error.Add(pSInvalidCastException.ErrorRecord);
										}
									}
									if (!PSActivity.CheckForCancel(psActivityContext))
									{
										continue;
									}
									return;
								}
							}
							else
							{
								object[] objArray = new object[1];
								objArray[0] = commandText;
								throw new NotImplementedException(string.Format(CultureInfo.CurrentCulture, Resources.CmdletDoesNotImplementInputObjectProperty, objArray));
							}
						}
					}
					else
					{
						throw new InvalidOperationException(commandText);
					}
				}
				else
				{
					invokeWmiMethod.CommandRuntime = directExecutionActivitiesCommandRuntime;
					PSObject pSObject3 = PSObject.AsPSObject(invokeWmiMethod);
					PSActivity.InitializeCmdletInstanceParameters(item, pSObject3, false, psActivityContext, null, implementationContext);
					if (!flag || input == null || input.Count <= 0 && !input.IsOpen)
					{
						invokeWmiMethod.Invoke().GetEnumerator().MoveNext();
						return;
					}
					else
					{
						InvokeWmiMethod invokeWmiMethod1 = invokeWmiMethod as InvokeWmiMethod;
						foreach (PSObject pSObject4 in input)
						{
							try
							{
								ManagementObject managementObject = LanguagePrimitives.ConvertTo<ManagementObject>(pSObject4);
								invokeWmiMethod1.InputObject = managementObject;
								invokeWmiMethod1.Invoke().GetEnumerator().MoveNext();
							}
							catch (PSInvalidCastException pSInvalidCastException3)
							{
								PSInvalidCastException pSInvalidCastException2 = pSInvalidCastException3;
								if (pSInvalidCastException2.ErrorRecord != null)
								{
									directExecutionActivitiesCommandRuntime.Error.Add(pSInvalidCastException2.ErrorRecord);
								}
							}
							if (!PSActivity.CheckForCancel(psActivityContext))
							{
								continue;
							}
							return;
						}
					}
				}
				return;
			}
			else
			{
				return;
			}
		}

		private static void RunOneCommandFinally(RunCommandsArguments args, bool attemptRetry)
		{
			if (!attemptRetry)
			{
				PSActivityContext pSActivityContext = args.PSActivityContext;
				ActivityImplementationContext implementationContext = args.ImplementationContext;
				PSWorkflowHost workflowHost = args.WorkflowHost;
				PowerShell powerShellInstance = implementationContext.PowerShellInstance;
				lock (pSActivityContext.runningCommands)
				{
					pSActivityContext.runningCommands.Remove(powerShellInstance);
				}
				if (!pSActivityContext.IsCanceled && args.CommandExecutionType != 1)
				{
					PSActivity.CloseRunspaceAndDisposeCommand(powerShellInstance, workflowHost, pSActivityContext, args.CommandExecutionType);
				}
				return;
			}
			else
			{
				return;
			}
		}

		private static void RunspaceDisconnectedCallback(RunCommandsArguments args, Exception runspaceDisconnectedException)
		{
			PSActivityContext pSActivityContext = args.PSActivityContext;
			ActivityImplementationContext implementationContext = args.ImplementationContext;
			PowerShell powerShellInstance = implementationContext.PowerShellInstance;
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			using (traceSource)
			{
				traceSource.WriteMessage("Executing callback when remote runspace got disconnected");
				bool flag = false;
				try
				{
					try
					{
						if (!PSActivity.CheckForCancel(pSActivityContext))
						{
							implementationContext.CleanUp();
							if (args.HelperCommand != null)
							{
								args.HelperCommand.Dispose();
								args.HelperCommand = null;
							}
							traceSource.WriteMessage(string.Format(CultureInfo.InvariantCulture, "Runspace disconnected is treated as an errors in executing the command.", new object[0]));
							pSActivityContext.Failed = true;
							throw runspaceDisconnectedException;
						}
						else
						{
							return;
						}
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						flag = PSActivity.HandleRunOneCommandException(args, exception);
						if (flag)
						{
							PSActivity.BeginActionRetry(args);
						}
					}
				}
				finally
				{
					PSActivity.RemoveHandlersFromStreams(powerShellInstance, args);
					PSActivity.RunOneCommandFinally(args, flag);
					traceSource.WriteMessage(string.Format(CultureInfo.InvariantCulture, "PowerShell activity: Finished running command.", new object[0]));
					PSActivity.DecrementRunningCountAndCheckForEnd(pSActivityContext);
				}
			}
		}

		internal bool RunWithCustomRemoting(ActivityContext context)
		{
			if (typeof(PSRemotingActivity).IsAssignableFrom(base.GetType()))
			{
				PSRemotingActivity pSRemotingActivity = (PSRemotingActivity)this;
				if (pSRemotingActivity.PSRemotingBehavior.Get(context) == RemotingBehavior.Custom)
				{
					return true;
				}
			}
			return false;
		}

		private static void SetCurrentDirectory(PSActivityContext psActivityContext, Runspace runspace)
		{
			if (psActivityContext.ParameterDefaults != null && psActivityContext.ParameterDefaults.ContainsKey("PSCurrentDirectory"))
			{
				string item = psActivityContext.ParameterDefaults["PSCurrentDirectory"] as string;
				if (item != null)
				{
					runspace.SessionStateProxy.Path.SetLocation(item);
				}
			}
		}

		private static void SetVariablesCallback(IAsyncResult asyncResult)
		{
			object asyncState = asyncResult.AsyncState;
			RunCommandsArguments runCommandsArgument = asyncState as RunCommandsArguments;
			PowerShell helperCommand = runCommandsArgument.HelperCommand;
			PSActivityContext pSActivityContext = runCommandsArgument.PSActivityContext;
			PSActivityEnvironment pSActivityEnvironment = runCommandsArgument.ImplementationContext.PSActivityEnvironment;
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			using (traceSource)
			{
				traceSource.WriteMessage("Executing callback for setting variables in remote runspace");
				try
				{
					try
					{
						helperCommand.EndInvoke(asyncResult);
					}
					catch (Exception exception2)
					{
						traceSource.WriteMessage("Setting varibles in remote runspace failed using script, trying with proxy");
						try
						{
							PSActivity.SetVariablesInRunspaceUsingProxy(pSActivityEnvironment, helperCommand.Runspace);
						}
						catch (Exception exception1)
						{
							Exception exception = exception1;
							bool flag = PSActivity.HandleRunOneCommandException(runCommandsArgument, exception);
							if (!flag)
							{
								PSActivity.RunOneCommandFinally(runCommandsArgument, false);
							}
							else
							{
								traceSource.WriteMessage("Runspace initialization failed, attempting retry");
								PSActivity.CloseRunspace(runCommandsArgument.ImplementationContext.PowerShellInstance.Runspace, runCommandsArgument.CommandExecutionType, runCommandsArgument.WorkflowHost, pSActivityContext);
								PSActivity.BeginActionRetry(runCommandsArgument);
							}
							PSActivity.DecrementRunningCountAndCheckForEnd(pSActivityContext);
							return;
						}
					}
				}
				finally
				{
					helperCommand.Dispose();
					runCommandsArgument.HelperCommand = null;
					runCommandsArgument.HelperCommandInput.Dispose();
					runCommandsArgument.HelperCommandInput = null;
				}
				if (!PSActivity.CheckForCancel(pSActivityContext))
				{
					if ((pSActivityEnvironment == null || pSActivityEnvironment.Modules == null || pSActivityEnvironment.Modules.Count <= 0) && (runCommandsArgument.ActivityParameters == null || runCommandsArgument.ActivityParameters.PSRequiredModules == null || (int)runCommandsArgument.ActivityParameters.PSRequiredModules.Length <= 0))
					{
						PSActivity.BeginPowerShellInvocation(runCommandsArgument);
					}
					else
					{
						PSActivity.BeginImportRequiredModules(runCommandsArgument);
					}
				}
			}
		}

		private static void SetVariablesInRunspaceUsingProxy(PSActivityEnvironment activityEnvironment, Runspace runspace)
		{
			PowerShellTraceSource traceSource = PowerShellTraceSourceFactory.GetTraceSource();
			using (traceSource)
			{
				traceSource.WriteMessage("BEGIN SetVariablesInRunspaceUsingProxy");
				Dictionary<string, object> variablesToSetInRunspace = PSActivity.GetVariablesToSetInRunspace(activityEnvironment);
				foreach (string key in variablesToSetInRunspace.Keys)
				{
					object item = variablesToSetInRunspace[key];
					if (item == null)
					{
						continue;
					}
					try
					{
						runspace.SessionStateProxy.PSVariable.Set(key, item);
					}
					catch (PSNotSupportedException pSNotSupportedException)
					{
						traceSource.WriteMessage("SetVariablesInRunspaceUsingProxy: Copying the workflow variables to a RemoteSessionStateProxy is not supported.");
						return;
					}
				}
				traceSource.WriteMessage("END SetVariablesInRunspaceUsingProxy");
			}
		}

		private static bool StringContainsIdentifierInfo(string message, out Guid jobInstanceId, out string computerName, out string originalString)
		{
			jobInstanceId = Guid.Empty;
			computerName = string.Empty;
			originalString = string.Empty;
			if (!string.IsNullOrEmpty(message))
			{
				if (Regex.IsMatch(message, "^([\\d\\w]{8}\\-[\\d\\w]{4}\\-[\\d\\w]{4}\\-[\\d\\w]{4}\\-[\\d\\w]{12}:\\[.*\\]:).*"))
				{
					string[] strArrays = message.Split(PSActivity.Delimiter, 3);
					if ((int)strArrays.Length == 3)
					{
						if (!Guid.TryParse(strArrays[0], out jobInstanceId))
						{
							jobInstanceId = Guid.Empty;
						}
						computerName = strArrays[1];
						originalString = strArrays[2].Trim();
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
			else
			{
				return false;
			}
		}

		private static void UnregisterAndReleaseRunspace(Runspace runspace, PSWorkflowHost workflowHost, PSActivityContext psActivityContext)
		{
			RunCommandsArguments runCommandsArgument = null;
			PSActivity.ArgsTableForRunspaces.TryRemove(runspace.InstanceId, out runCommandsArgument);
			if (psActivityContext.HandleRunspaceStateChanged != null)
			{
				runspace.StateChanged -= psActivityContext.HandleRunspaceStateChanged;
			}
			workflowHost.RemoteRunspaceProvider.ReleaseRunspace(runspace);
		}

		protected internal void UpdateImplementationContextForLocalExecution(ActivityImplementationContext implementationContext, ActivityContext context)
		{
		}

		private static void UpdatePowerShell(ActivityImplementationContext implementationContext, PSActivityContext psActivityContext, Type ActivityType, PrepareSessionDelegate PrepareSession, object activityObject)
		{
			try
			{
				PrepareSession(implementationContext);
				PowerShell powerShellInstance = implementationContext.PowerShellInstance;
				if (implementationContext.PSError != null)
				{
					powerShellInstance.Streams.Error = implementationContext.PSError;
				}
				if (implementationContext.PSProgress != null)
				{
					powerShellInstance.Streams.Progress = implementationContext.PSProgress;
				}
				if (implementationContext.PSVerbose != null)
				{
					powerShellInstance.Streams.Verbose = implementationContext.PSVerbose;
				}
				if (implementationContext.PSDebug != null)
				{
					powerShellInstance.Streams.Debug = implementationContext.PSDebug;
				}
				if (implementationContext.PSWarning != null)
				{
					powerShellInstance.Streams.Warning = implementationContext.PSWarning;
				}
				PSActivity pSActivity = activityObject as PSActivity;
				if (pSActivity.UpdatePreferenceVariable)
				{
					PSActivity.UpdatePreferenceVariables(implementationContext);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				lock (psActivityContext.exceptions)
				{
					psActivityContext.exceptions.Add(exception);
				}
			}
		}

		private static void UpdatePreferenceVariables(ActivityImplementationContext implementationContext)
		{
			Command item = implementationContext.PowerShellInstance.Commands.Commands[0];
			bool? verbose = implementationContext.Verbose;
			if (verbose.HasValue)
			{
				item.Parameters.Add("Verbose", implementationContext.Verbose);
			}
			bool? debug = implementationContext.Debug;
			if (debug.HasValue)
			{
				item.Parameters.Add("Debug", implementationContext.Debug);
			}
			bool? whatIf = implementationContext.WhatIf;
			if (whatIf.HasValue)
			{
				item.Parameters.Add("WhatIf", implementationContext.WhatIf);
			}
			ActionPreference? errorAction = implementationContext.ErrorAction;
			if (errorAction.HasValue)
			{
				item.Parameters.Add("ErrorAction", implementationContext.ErrorAction);
			}
			ActionPreference? warningAction = implementationContext.WarningAction;
			if (warningAction.HasValue)
			{
				item.Parameters.Add("WarningAction", implementationContext.WarningAction);
			}
		}

		private static void WriteError(Exception exception, string errorId, ErrorCategory errorCategory, object originalTarget, PSActivityContext psActivityContext)
		{
			if (psActivityContext.errors == null)
			{
				lock (psActivityContext.exceptions)
				{
					psActivityContext.exceptions.Add(exception);
				}
			}
			else
			{
				ErrorRecord errorRecord = new ErrorRecord(exception, errorId, errorCategory, originalTarget);
				lock (psActivityContext.errors)
				{
					psActivityContext.errors.Add(errorRecord);
				}
			}
		}

		protected void WriteProgressRecord(NativeActivityContext context, PSDataCollection<ProgressRecord> progress, string statusDescription, ProgressRecordType type)
		{
			string displayName;
			int num = 0;
			if (progress != null)
			{
				string str = null;
				if (this.PSProgressMessage != null)
				{
					str = this.PSProgressMessage.Get(context);
					if (this.PSProgressMessage.Expression != null && string.IsNullOrEmpty(str))
					{
						return;
					}
				}
				if (str != null)
				{
					displayName = string.Concat(base.DisplayName, ": ", str);
				}
				else
				{
					displayName = base.DisplayName;
					if (string.IsNullOrEmpty(displayName))
					{
						displayName = base.GetType().Name;
					}
				}
				ProgressRecord progressRecord = new ProgressRecord(0, displayName, statusDescription);
				progressRecord.RecordType = type;
				string str1 = string.Concat(base.Id, ":");
				HostParameterDefaults extension = context.GetExtension<HostParameterDefaults>();
				if (extension != null)
				{
					HostSettingCommandMetadata hostCommandMetadata = extension.HostCommandMetadata;
					if (hostCommandMetadata != null)
					{
						object[] commandName = new object[3];
						commandName[0] = hostCommandMetadata.CommandName;
						commandName[1] = hostCommandMetadata.StartLineNumber;
						commandName[2] = hostCommandMetadata.StartColumnNumber;
						str1 = string.Concat(str1, string.Format(CultureInfo.CurrentCulture, Resources.ProgressPositionMessage, commandName));
					}
				}
				progressRecord.CurrentOperation = str1;
				foreach (PropertyDescriptor property in context.DataContext.GetProperties())
				{
					if (!string.Equals(property.DisplayName, "PSParentActivityID", StringComparison.OrdinalIgnoreCase))
					{
						if (!string.Equals(property.DisplayName, "ProgressPreference", StringComparison.OrdinalIgnoreCase))
						{
							continue;
						}
						string value = property.GetValue(context.DataContext) as string;
						if (string.IsNullOrEmpty(value) || !string.Equals(value, "SilentlyContinue", StringComparison.OrdinalIgnoreCase) && !string.Equals(value, "Ignore", StringComparison.OrdinalIgnoreCase))
						{
							continue;
						}
						return;
					}
					else
					{
						object obj = property.GetValue(context.DataContext);
						if (obj == null || !LanguagePrimitives.TryConvertTo<int>(obj, CultureInfo.InvariantCulture, out num))
						{
							continue;
						}
						progressRecord.ParentActivityId = num;
					}
				}
				progress.Add(progressRecord);
				return;
			}
			else
			{
				return;
			}
		}

		internal static event ActivityCreatedEventHandler ActivityCreated;
	}
}