using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using Microsoft.PowerShell.Commands.Management;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Management.Automation.Remoting;
using System.Threading;

namespace Microsoft.PowerShell.Cmdletization.Cim
{
	internal abstract class CimChildJobBase<T> : StartableJob, IObserver<T>
	{
		private const string CIMJobType = "CimJob";

		private const int MaxRetryDelayMs = 0x3a98;

		private const int MinRetryDelayMs = 100;

		private static long _globalJobNumberCounter;

		private readonly long _myJobNumber;

		private readonly CimJobContext _jobContext;

		private readonly static Random GlobalRandom;

		private readonly Random _random;

		private int _sleepAndRetryDelayRangeMs;

		private int _sleepAndRetryExtraDelayMs;

		private Timer _sleepAndRetryTimer;

		private readonly Lazy<CimCustomOptionsDictionary> _jobSpecificCustomOptions;

		private readonly CancellationTokenSource _cancellationTokenSource;

		private readonly object _jobStateLock;

		private bool _jobHadErrors;

		private bool _jobWasStarted;

		private bool _jobWasStopped;

		private bool _alreadyReachedCompletedState;

		private readonly ConcurrentDictionary<int, ProgressRecord> _activityIdToLastProgressRecord;

		private bool _userWasPromptedForContinuationOfProcessing;

		private bool _userRespondedYesToAtLeastOneShouldProcess;

		internal abstract string Description
		{
			get;
		}

		internal bool DidUserSuppressTheOperation
		{
			get
			{
				bool flag;
				if (!this._userWasPromptedForContinuationOfProcessing)
				{
					flag = false;
				}
				else
				{
					flag = !this._userRespondedYesToAtLeastOneShouldProcess;
				}
				bool flag1 = flag;
				return flag1;
			}
		}

		internal abstract string FailSafeDescription
		{
			get;
		}

		public override bool HasMoreData
		{
			get
			{
				if (base.Results.IsOpen)
				{
					return true;
				}
				else
				{
					return base.Results.Count > 0;
				}
			}
		}

		internal CimJobContext JobContext
		{
			get
			{
				return this._jobContext;
			}
		}

		internal bool JobHadErrors
		{
			get
			{
				bool flag;
				lock (this._jobStateLock)
				{
					flag = this._jobHadErrors;
				}
				return flag;
			}
		}

		public override string Location
		{
			get
			{
                if (this.JobContext != null)
				{
					string computerName = this.JobContext.Session.ComputerName;
					string machineName = computerName;
					if (computerName == null)
					{
						machineName = Environment.MachineName;
					}
					string str = machineName;
					return str;
				}
				else
				{
					return null;
				}
			}
		}

		public override string StatusMessage
		{
			get
			{
				return base.JobStateInfo.State.ToString();
			}
		}

		static CimChildJobBase()
		{
			CimChildJobBase<T>.GlobalRandom = new Random();
		}

		internal CimChildJobBase(CimJobContext jobContext) : base(Job.GetCommandTextFromInvocationInfo(jobContext.CmdletInvocationInfo), " ")
		{
			this._myJobNumber = Interlocked.Increment(ref CimChildJobBase<T>._globalJobNumberCounter);
			this._sleepAndRetryDelayRangeMs = 0x3e8;
			this._cancellationTokenSource = new CancellationTokenSource();
			this._jobStateLock = new object();
			this._activityIdToLastProgressRecord = new ConcurrentDictionary<int, ProgressRecord>();
			this._jobContext = jobContext;
			base.PSJobTypeName = "CimJob";
			base.Name = string.Concat(base.GetType().Name, this._myJobNumber.ToString(CultureInfo.InvariantCulture));
			base.UsesResultsCollection = true;
			lock (CimChildJobBase<T>.GlobalRandom)
			{
				this._random = new Random(CimChildJobBase<T>.GlobalRandom.Next());
			}
			CimChildJobBase<T> cimChildJobBase = this;
			this._jobSpecificCustomOptions = new Lazy<CimCustomOptionsDictionary>(new Func<CimCustomOptionsDictionary>(cimChildJobBase.CalculateJobSpecificCustomOptions));
		}

		internal static void AddShowComputerNameMarker(PSObject pso)
		{
			PSPropertyInfo item = pso.InstanceMembers[RemotingConstants.ShowComputerNameNoteProperty] as PSPropertyInfo;
			if (item == null)
			{
				item = new PSNoteProperty(RemotingConstants.ShowComputerNameNoteProperty, (object)(true));
				pso.InstanceMembers.Add(item);
				return;
			}
			else
			{
				item.Value = true;
				return;
			}
		}

		private CimResponseType BlockingWriteError(ErrorRecord errorRecord)
		{
			Exception obj = null;
			this.ExceptionSafeWrapper(() => this.WriteError(errorRecord, out obj));
			if (obj != null)
			{
				return CimResponseType.NoToAll;
			}
			else
			{
				return CimResponseType.Yes;
			}
		}

		internal abstract CimCustomOptionsDictionary CalculateJobSpecificCustomOptions();

        internal CimOperationOptions CreateOperationOptions()
        {
            CimOperationOptions operationOptions = new CimOperationOptions(false)
            {
                CancellationToken = new CancellationToken?(this._cancellationTokenSource.Token),
                WriteProgress = new Microsoft.Management.Infrastructure.WriteProgressCallback(this.WriteProgressCallback),
                WriteMessage = new Microsoft.Management.Infrastructure.WriteMessageCallback(this.WriteMessageCallback),
                WriteError = new Microsoft.Management.Infrastructure.WriteErrorCallback(this.WriteErrorCallback),
                PromptUser = new Microsoft.Management.Infrastructure.PromptUserCallback(this.PromptUserCallback)
            };
            operationOptions.SetOption("__MI_OPERATIONOPTIONS_IMPROVEDPERF_STREAMING", (uint)1);
            operationOptions.Flags |= this.JobContext.CmdletInvocationContext.CmdletDefinitionContext.SchemaConformanceLevel;
            if (this.JobContext.CmdletInvocationContext.CmdletDefinitionContext.ResourceUri != null)
            {
                operationOptions.ResourceUri = this.JobContext.CmdletInvocationContext.CmdletDefinitionContext.ResourceUri;
            }
            if (((this._jobContext.WarningActionPreference == ActionPreference.SilentlyContinue) || (this._jobContext.WarningActionPreference == ActionPreference.Ignore)) && !this._jobContext.IsRunningInBackground)
            {
                operationOptions.DisableChannel(0);
            }
            else
            {
                operationOptions.EnableChannel(0);
            }
            if (((this._jobContext.VerboseActionPreference == ActionPreference.SilentlyContinue) || (this._jobContext.VerboseActionPreference == ActionPreference.Ignore)) && !this._jobContext.IsRunningInBackground)
            {
                operationOptions.DisableChannel(1);
            }
            else
            {
                operationOptions.EnableChannel(1);
            }
            if (((this._jobContext.DebugActionPreference == ActionPreference.SilentlyContinue) || (this._jobContext.DebugActionPreference == ActionPreference.Ignore)) && !this._jobContext.IsRunningInBackground)
            {
                operationOptions.DisableChannel(2);
            }
            else
            {
                operationOptions.EnableChannel(2);
            }
            switch (this.JobContext.ShouldProcessOptimization)
            {
                case MshCommandRuntime.ShouldProcessPossibleOptimization.AutoYes_CanSkipShouldProcessCall:
                    operationOptions.SetPromptUserRegularMode(CimCallbackMode.Ignore, true);
                    break;

                case MshCommandRuntime.ShouldProcessPossibleOptimization.AutoYes_CanCallShouldProcessAsynchronously:
                    operationOptions.SetPromptUserRegularMode(CimCallbackMode.None, true);
                    break;

                case MshCommandRuntime.ShouldProcessPossibleOptimization.AutoNo_CanCallShouldProcessAsynchronously:
                    operationOptions.SetPromptUserRegularMode(CimCallbackMode.None, false);
                    break;

                default:
                    operationOptions.PromptUserMode = CimCallbackMode.Inquire;
                    break;
            }
            switch (this.JobContext.ErrorActionPreference)
            {
                case ActionPreference.SilentlyContinue:
                case ActionPreference.Continue:
                case ActionPreference.Ignore:
                    operationOptions.WriteErrorMode = CimCallbackMode.None;
                    break;

                default:
                    operationOptions.WriteErrorMode = CimCallbackMode.Inquire;
                    break;
            }
            if (!string.IsNullOrWhiteSpace(this.GetProviderVersionExpectedByJob()))
            {
                CimOperationOptionsHelper.SetCustomOption(operationOptions, "MI_OPERATIONOPTIONS_PROVIDERVERSION", this.GetProviderVersionExpectedByJob());
            }
            if (this.JobContext.CmdletizationModuleVersion != null)
            {
                CimOperationOptionsHelper.SetCustomOption(operationOptions, "MI_OPERATIONOPTIONS_POWERSHELL_MODULEVERSION", this.JobContext.CmdletizationModuleVersion);
            }
            CimOperationOptionsHelper.SetCustomOption(operationOptions, "MI_OPERATIONOPTIONS_POWERSHELL_CMDLETNAME", this.JobContext.CmdletInvocationInfo.MyCommand.Name);
            if (!string.IsNullOrWhiteSpace(this.JobContext.Session.ComputerName))
            {
                CimOperationOptionsHelper.SetCustomOption(operationOptions, "MI_OPERATIONOPTIONS_POWERSHELL_COMPUTERNAME", this.JobContext.Session.ComputerName);
            }
            CimCustomOptionsDictionary jobSpecificCustomOptions = this.GetJobSpecificCustomOptions();
            if (jobSpecificCustomOptions != null)
            {
                jobSpecificCustomOptions.Apply(operationOptions);
            }
            return operationOptions;
        }


		protected override void Dispose(bool disposing)
		{
			bool flag;
			if (disposing)
			{
				lock (this._jobStateLock)
				{
					flag = this._alreadyReachedCompletedState;
				}
				if (!flag)
				{
					this.StopJob();
					base.Finished.WaitOne();
				}
			}
		}

		internal void ExceptionSafeWrapper(Action action)
		{
			try
			{
				try
				{
					action();
				}
				catch (CimJobException cimJobException1)
				{
					CimJobException cimJobException = cimJobException1;
                    this.ReportJobFailure(cimJobException);
				}
				catch (PSInvalidCastException pSInvalidCastException1)
				{
					PSInvalidCastException pSInvalidCastException = pSInvalidCastException1;
                    this.ReportJobFailure(pSInvalidCastException);
				}
				catch (CimException cimException1)
				{
					CimException cimException = cimException1;
					CimJobException cimJobException2 = CimJobException.CreateFromCimException(this.GetDescription(), this.JobContext, cimException);
                    this.ReportJobFailure(cimJobException2);
				}
				catch (PSInvalidOperationException pSInvalidOperationException)
				{
					lock (this._jobStateLock)
					{
						bool flag = false;
						if (this._jobWasStopped)
						{
							flag = true;
						}
						if (this._alreadyReachedCompletedState && this._jobHadErrors)
						{
							flag = true;
						}
						if (!flag)
						{
							throw;
						}
					}
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
                CimJobException cimJobException3 = CimJobException.CreateFromAnyException(this.GetDescription(), this.JobContext, exception);
                this.ReportJobFailure(cimJobException3);
			}
		}

		internal void FinishProgressReporting()
		{
			foreach (ProgressRecord value in this._activityIdToLastProgressRecord.Values)
			{
				if (value.RecordType == ProgressRecordType.Completed)
				{
					continue;
				}
				ProgressRecord progressRecord = new ProgressRecord(value.ActivityId, value.Activity, value.StatusDescription);
				progressRecord.RecordType = ProgressRecordType.Completed;
				progressRecord.PercentComplete = 100;
				progressRecord.SecondsRemaining = 0;
				this.WriteProgress(progressRecord);
			}
		}

		internal abstract IObservable<T> GetCimOperation();

		internal string GetDescription()
		{
			string description;
			try
			{
				description = this.Description;
			}
			catch (Exception exception)
			{
				description = this.FailSafeDescription;
			}
			return description;
		}

		private CimCustomOptionsDictionary GetJobSpecificCustomOptions()
		{
			return this._jobSpecificCustomOptions.Value;
		}

		internal virtual string GetProviderVersionExpectedByJob()
		{
			return this.JobContext.CmdletInvocationContext.CmdletDefinitionContext.CmdletizationClassVersion;
		}

		internal static bool IsShowComputerNameMarkerPresent(CimInstance cimInstance)
		{
			PSObject pSObject = PSObject.AsPSObject(cimInstance);
			PSPropertyInfo item = pSObject.InstanceMembers[RemotingConstants.ShowComputerNameNoteProperty] as PSPropertyInfo;
			if (item != null)
			{
				bool flag = true;
				return flag.Equals(item.Value);
			}
			else
			{
				return false;
			}
		}

		private static bool IsWsManQuotaReached(Exception exception)
		{
			CimException cimException = exception as CimException;
			if (cimException != null)
			{
				if (cimException.NativeErrorCode == NativeErrorCode.ServerLimitsExceeded)
				{
					CimInstance errorData = cimException.ErrorData;
					if (errorData != null)
					{
						CimProperty item = errorData.CimInstanceProperties["error_Code"];
						if (item != null)
						{
							if (item.CimType == CimType.UInt32)
							{
                                uint wsManErrorCode = (uint)item.Value;
								switch (wsManErrorCode)
								{
									case 2144108123:
									case 2144108122:
									case (CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_SHELLS | CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_OPERATIONS | CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_USER):
									case 2144108120:
									case (CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_SYSTEM | CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_SHELLUSERS):
									{
										return true;
									}
									case 2144108119:
									case 2144108118:
									{
										return false;
									}
									default:
									{
                                        if (wsManErrorCode == 2144108060 || wsManErrorCode == (CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_SHELLS | CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_SHELLS_PPQ | CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_USERS_PPQ) || wsManErrorCode == (CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_OPERATIONS | CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_SHELLS_PPQ | CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_PLUGINSHELLS_PPQ) || wsManErrorCode == (CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_SHELLS | CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_OPERATIONS | CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_USER | CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_SHELLS_PPQ | CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_USERS_PPQ | CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_PLUGINSHELLS_PPQ | CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_PLUGINOPERATIONS_PPQ) || wsManErrorCode == (CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_SYSTEM | CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_OPERATIONS_USER_PPQ) || wsManErrorCode == (CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_SYSTEM | CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_OPERATIONS_USER_PPQ | CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_COMMANDS_PER_SHELL_PPQ) || wsManErrorCode == (CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_SYSTEM | CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_MAX_OPERATIONS_USER_PPQ | CimChildJobBase<T>.WsManErrorCode.ERROR_WSMAN_QUOTA_MIN_REQUIREMENT_NOT_AVAILABLE_PPQ))
										{
											return true;
										}
										return false;
									}
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

		public virtual void OnCompleted()
		{
            this.ExceptionSafeWrapper(() => this.SetCompletedJobState(JobState.Completed, null));
		}

		public virtual void OnError(Exception exception)
		{
			this.ExceptionSafeWrapper(() => {
				if (!CimChildJobBase<T>.IsWsManQuotaReached(exception))
				{
                    CimJobException cimJobException = CimJobException.CreateFromAnyException(this.GetDescription(), this.JobContext, exception);
                    this.ReportJobFailure(cimJobException);
					return;
				}
				else
				{
                    this.SleepAndRetry();
					return;
				}
			}
			);
		}

		public abstract void OnNext(T item);

		private CimResponseType PromptUserCallback(string message, CimPromptType promptType)
		{
			Action action = null;
			Action action1 = null;
            string str = this.JobContext.PrependComputerNameToMessage(message);
			Exception obj = null;
			int num = 0;
			this._userWasPromptedForContinuationOfProcessing = true;
			CimPromptType cimPromptType = promptType;
			switch (cimPromptType)
			{
				case CimPromptType.None:
				{
					CimChildJobBase<T> cimChildJobBase = this;
					if (action1 == null)
					{
                        action1 = () => num = (int)this.ShouldProcess(message, null, null);
					}
					cimChildJobBase.ExceptionSafeWrapper(action1);
					break;
				}
				case CimPromptType.Critical:
				{
					CimChildJobBase<T> cimChildJobBase1 = this;
					if (action == null)
					{
						action = () => {
							if (!this.ShouldContinue(str, (string)null, out obj))
							{
								num = (int)CimResponseType.None;
								return;
							}
							else
							{
                                num = (int)CimResponseType.Yes;
								return;
							}
						}
						;
					}
					cimChildJobBase1.ExceptionSafeWrapper(action);
					break;
				}
			}
			if (obj != null)
			{
				num = 2;
			}
			if (num == 1 || num == 3)
			{
				this._userRespondedYesToAtLeastOneShouldProcess = true;
			}
			return (CimResponseType)num;
		}

		internal void ReportJobFailure(IContainsErrorRecord exception)
		{
            TerminatingErrorTracker tracker = TerminatingErrorTracker.GetTracker(this.JobContext.CmdletInvocationInfo);
			bool flag = false;
			bool flag1 = false;
			Exception exceptionIfBrokenSession = null;
			lock (this._jobStateLock)
			{
				if (!this._jobWasStopped)
				{
                    exceptionIfBrokenSession = tracker.GetExceptionIfBrokenSession(this.JobContext.Session, this.JobContext.CmdletInvocationContext.CmdletDefinitionContext.SkipTestConnection, out flag);
				}
			}
			if (exceptionIfBrokenSession == null)
			{
				CimJobException cimJobException = exception as CimJobException;
				if (cimJobException != null && cimJobException.IsTerminatingError)
				{
                    tracker.MarkSessionAsTerminated(this.JobContext.Session, out flag);
					flag1 = true;
				}
			}
			else
			{
				object[] message = new object[1];
				message[0] = exceptionIfBrokenSession.Message;
				string str = string.Format(CultureInfo.InvariantCulture, CmdletizationResources.CimJob_BrokenSession, message);
                exception = CimJobException.CreateWithFullControl(this.JobContext, str, "CimJob_BrokenCimSession", ErrorCategory.ResourceUnavailable, exceptionIfBrokenSession);
				flag1 = true;
			}
			bool flag2 = !flag;
			if (flag2)
			{
				lock (this._jobStateLock)
				{
					if (this._jobWasStopped)
					{
						flag2 = false;
					}
				}
			}
			ErrorRecord errorRecord = exception.ErrorRecord;
			errorRecord.SetInvocationInfo(this.JobContext.CmdletInvocationInfo);
			errorRecord.PreserveInvocationInfoOnce = true;
			if (flag2)
			{
				lock (this._jobStateLock)
				{
					if (!this._alreadyReachedCompletedState)
					{
						if (!flag1)
						{
							this.WriteError(errorRecord);
						}
						else
						{
							base.Error.Add(errorRecord);
							CmdletMethodInvoker<bool> errorReportingDelegate = tracker.GetErrorReportingDelegate(errorRecord);
							base.Results.Add(new PSStreamObject(PSStreamObjectType.ShouldMethod, errorReportingDelegate));
						}
					}
				}
			}
            this.SetCompletedJobState(JobState.Failed, errorRecord.Exception);
		}

		internal void SetCompletedJobState(JobState state, Exception reason)
		{
			lock (this._jobStateLock)
			{
				if (!this._alreadyReachedCompletedState)
				{
					this._alreadyReachedCompletedState = true;
					if (state == JobState.Failed || reason != null)
					{
						this._jobHadErrors = true;
					}
					if (!this._jobWasStopped)
					{
						if (this._jobHadErrors)
						{
							state = JobState.Failed;
						}
					}
					else
					{
						state = JobState.Stopped;
					}
				}
				else
				{
					return;
				}
			}
            this.FinishProgressReporting();
			base.SetJobState(state, reason);
			base.CloseAllStreams();
			this._cancellationTokenSource.Cancel();
		}

		internal CimResponseType ShouldProcess(string target, string action)
		{
			object[] objArray = new object[3];
			objArray[0] = action;
			objArray[1] = target;
			string str = StringUtil.Format(CommandBaseStrings.ShouldProcessMessage, objArray);
            return this.ShouldProcess(str, null, null);
		}

		internal CimResponseType ShouldProcess(string verboseDescription, string verboseWarning, string caption)
		{
			Exception exception = null;
			ShouldProcessReason shouldProcessReason = ShouldProcessReason.None;
			if (!this.JobContext.IsRunningInBackground)
			{
				if (this.JobContext.ShouldProcessOptimization != MshCommandRuntime.ShouldProcessPossibleOptimization.AutoNo_CanCallShouldProcessAsynchronously)
				{
					if (this.JobContext.ShouldProcessOptimization != MshCommandRuntime.ShouldProcessPossibleOptimization.AutoYes_CanCallShouldProcessAsynchronously)
					{
						bool flag = this.ShouldProcess(verboseDescription, verboseWarning, caption, out shouldProcessReason, out exception);
						if (exception == null)
						{
							if (!flag)
							{
								return CimResponseType.None;
							}
							else
							{
								return CimResponseType.Yes;
							}
						}
						else
						{
							return CimResponseType.NoToAll;
						}
					}
					else
					{
						this.NonblockingShouldProcess(verboseDescription, verboseWarning, caption);
						return CimResponseType.Yes;
					}
				}
				else
				{
					this.NonblockingShouldProcess(verboseDescription, verboseWarning, caption);
					return CimResponseType.None;
				}
			}
			else
			{
				return CimResponseType.YesToAll;
			}
		}

		private void SleepAndRetry()
		{
			int num = this._random.Next(0, this._sleepAndRetryDelayRangeMs);
			int num1 = 100 + this._sleepAndRetryExtraDelayMs + num;
			this._sleepAndRetryExtraDelayMs = this._sleepAndRetryDelayRangeMs - num;
			if (this._sleepAndRetryDelayRangeMs < 0x3a98)
			{
				CimChildJobBase<T> cimChildJobBase = this;
				cimChildJobBase._sleepAndRetryDelayRangeMs = cimChildJobBase._sleepAndRetryDelayRangeMs * 2;
			}
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			string cimJobSleepAndRetryVerboseMessage = CmdletizationResources.CimJob_SleepAndRetryVerboseMessage;
			object[] invocationName = new object[3];
			invocationName[0] = this.JobContext.CmdletInvocationInfo.InvocationName;
			object[] objArray = invocationName;
			int num2 = 1;
			string computerName = this.JobContext.Session.ComputerName;
			object obj = computerName;
			if (computerName == null)
			{
				obj = "localhost";
			}
			objArray[num2] = obj;
			invocationName[2] = (double)num1 / 1000;
			string str = string.Format(invariantCulture, cimJobSleepAndRetryVerboseMessage, invocationName);
			this.WriteVerbose(str);
			lock (this._jobStateLock)
			{
				if (!this._jobWasStopped)
				{
					object obj1 = null;
					int num3 = num1;
					int num4 = -1;
					this._sleepAndRetryTimer = new Timer(new TimerCallback(this.SleepAndRetry_OnWakeup), obj1, num3, num4);
				}
				else
				{
					this.SetCompletedJobState(JobState.Stopped, null);
				}
			}
		}

        private void SleepAndRetry_OnWakeup(object state)
        {
            this.ExceptionSafeWrapper(delegate
            {
                lock (this._jobStateLock)
                {
                    if (this._sleepAndRetryTimer != null)
                    {
                        this._sleepAndRetryTimer.Dispose();
                        this._sleepAndRetryTimer = null;
                    }
                    if (this._jobWasStopped)
                    {
                        this.SetCompletedJobState(JobState.Stopped, null);
                        return;
                    }
                }
                this.StartJob();
            });
        }

		internal override void StartJob()
		{
			lock (this._jobStateLock)
			{
				if (!this._jobWasStopped)
				{
					TerminatingErrorTracker tracker = TerminatingErrorTracker.GetTracker(this.JobContext.CmdletInvocationInfo);
					if (!tracker.IsSessionTerminated(this.JobContext.Session))
					{
						if (!this._jobWasStarted)
						{
							this._jobWasStarted = true;
							base.SetJobState(JobState.Running);
						}
					}
					else
					{
						this.SetCompletedJobState(JobState.Failed, new OperationCanceledException());
						return;
					}
				}
				else
				{
					this.SetCompletedJobState(JobState.Stopped, null);
					return;
				}
			}
            ThreadPool.QueueUserWorkItem(param0 => this.ExceptionSafeWrapper(delegate
            {
                IObservable<T> cimOperation = this.GetCimOperation();
                if (cimOperation != null)
                {
                    cimOperation.Subscribe(this);
                }
            }));
		}

		public override void StopJob()
		{
			lock (this._jobStateLock)
			{
				if (this._jobWasStopped || this._alreadyReachedCompletedState)
				{
					return;
				}
				else
				{
					this._jobWasStopped = true;
					if (this._jobWasStarted)
					{
						if (this._sleepAndRetryTimer == null)
						{
							base.SetJobState(JobState.Stopping);
						}
						else
						{
							this._sleepAndRetryTimer.Dispose();
							this._sleepAndRetryTimer = null;
                            this.SetCompletedJobState(JobState.Stopped, null);
						}
					}
					else
					{
                        this.SetCompletedJobState(JobState.Stopped, null);
					}
				}
			}
			this._cancellationTokenSource.Cancel();
		}

		internal override void WriteDebug(string message)
		{
			message = this.JobContext.PrependComputerNameToMessage(message);
			base.WriteDebug(message);
		}

		private CimResponseType WriteErrorCallback(CimInstance cimError)
		{
			lock (this._jobStateLock)
			{
				this._jobHadErrors = true;
			}
			CimException cimException = new CimException(cimError);
            CimJobException cimJobException = CimJobException.CreateFromCimException(this.GetDescription(), this.JobContext, cimException);
			ErrorRecord errorRecord = cimJobException.ErrorRecord;
			ActionPreference errorActionPreference = this.JobContext.ErrorActionPreference;
			switch (errorActionPreference)
			{
				case ActionPreference.Stop:
				case ActionPreference.Inquire:
				{
					return this.BlockingWriteError(errorRecord);
				}
				case ActionPreference.Continue:
				{
					this.WriteError(errorRecord);
					return CimResponseType.Yes;
				}
				default:
				{
					this.WriteError(errorRecord);
					return CimResponseType.Yes;
				}
			}
		}

		private void WriteMessageCallback(uint channel, string message)
		{
			this.ExceptionSafeWrapper(() => {
				CimChildJobBase<T>.MessageChannel messageChannel = (CimChildJobBase<T>.MessageChannel)channel;
				switch (messageChannel)
				{
					case (CimChildJobBase<T>.MessageChannel)CimChildJobBase<T>.MessageChannel.Warning:
					{
						this.WriteWarning(message);
						return;
					}
					case (CimChildJobBase<T>.MessageChannel)CimChildJobBase<T>.MessageChannel.Verbose:
					{
						this.WriteVerbose(message);
						return;
					}
					case (CimChildJobBase<T>.MessageChannel)CimChildJobBase<T>.MessageChannel.Debug:
					{
						this.WriteDebug(message);
						return;
					}
					default:
					{
						return;
					}
				}
			}
			);
		}

		internal override void WriteObject(object outputObject)
		{
			CimInstance baseObject;
			PSObject pSObject = null;
			if (outputObject as PSObject == null)
			{
				baseObject = outputObject as CimInstance;
			}
			else
			{
				pSObject = PSObject.AsPSObject(outputObject);
				baseObject = pSObject.BaseObject as CimInstance;
			}
			if (baseObject != null)
			{
				CimCmdletAdapter.AssociateSessionOfOriginWithInstance(baseObject, this.JobContext.Session);
				CimCustomOptionsDictionary.AssociateCimInstanceWithCustomOptions(baseObject, this.GetJobSpecificCustomOptions());
				if (this.JobContext.ShowComputerName)
				{
					if (pSObject == null)
					{
						pSObject = PSObject.AsPSObject(outputObject);
					}
					CimChildJobBase<T>.AddShowComputerNameMarker(pSObject);
				}
			}
			base.WriteObject(outputObject);
		}

		internal override void WriteProgress(ProgressRecord progressRecord)
		{
			progressRecord.Activity = this.JobContext.PrependComputerNameToMessage(progressRecord.Activity);
			this._activityIdToLastProgressRecord.AddOrUpdate(progressRecord.ActivityId, progressRecord, (int activityId, ProgressRecord oldProgressRecord) => progressRecord);
			base.WriteProgress(progressRecord);
		}

		private void WriteProgressCallback(string activity, string currentOperation, string statusDescription, int percentageCompleted, int secondsRemaining)
		{
			int num;
			int num1;
			if (string.IsNullOrEmpty(activity))
			{
				activity = this.GetDescription();
			}
			if (string.IsNullOrEmpty(statusDescription))
			{
				statusDescription = this.StatusMessage;
			}
			if (secondsRemaining != 0)
			{
				if (secondsRemaining > 0x7fffffff)
				{
					num = 0x7fffffff;
				}
				else
				{
					num = secondsRemaining;
				}
			}
			else
			{
				num = -1;
			}
			if (percentageCompleted != -1)
			{
				if (percentageCompleted > 100)
				{
					num1 = 100;
				}
				else
				{
					num1 = percentageCompleted;
				}
			}
			else
			{
				num1 = -1;
			}
			ProgressRecord progressRecord = new ProgressRecord((int)(this._myJobNumber % (long)0x7fffffff), activity, statusDescription);
			progressRecord.CurrentOperation = currentOperation;
			progressRecord.PercentComplete = num1;
			progressRecord.SecondsRemaining = num;
			progressRecord.RecordType = ProgressRecordType.Processing;
			ProgressRecord progressRecord1 = progressRecord;
			this.ExceptionSafeWrapper(() => this.WriteProgress(progressRecord1));
		}

		internal override void WriteVerbose(string message)
		{
			message = this.JobContext.PrependComputerNameToMessage(message);
			base.WriteVerbose(message);
		}

		internal void WriteVerboseStartOfCimOperation()
		{
            if (this.JobContext.CmdletInvocationContext.CmdletDefinitionContext.ClientSideWriteVerbose)
			{
				object[] description = new object[1];
                description[0] = this.GetDescription();
				string str = string.Format(CultureInfo.CurrentCulture, CmdletizationResources.CimJob_VerboseExecutionMessage, description);
				this.WriteVerbose(str);
			}
		}

		internal override void WriteWarning(string message)
		{
			message = this.JobContext.PrependComputerNameToMessage(message);
			base.WriteWarning(message);
		}

		private enum MessageChannel
		{
			Warning,
			Verbose,
			Debug
		}

		internal class WsManErrorCode
		{
			public const uint ERROR_WSMAN_QUOTA_MAX_SHELLS = 2150859173;
            public const uint ERROR_WSMAN_QUOTA_MAX_OPERATIONS = 2150859174;
            public const uint ERROR_WSMAN_QUOTA_USER = 2150859175;
            public const uint ERROR_WSMAN_QUOTA_SYSTEM = 2150859176;
            public const uint ERROR_WSMAN_QUOTA_MAX_SHELLUSERS = 2150859179;
            public const uint ERROR_WSMAN_QUOTA_MAX_SHELLS_PPQ = 2150859236;
            public const uint ERROR_WSMAN_QUOTA_MAX_USERS_PPQ = 2150859237;
            public const uint ERROR_WSMAN_QUOTA_MAX_PLUGINSHELLS_PPQ = 2150859238;
            public const uint ERROR_WSMAN_QUOTA_MAX_PLUGINOPERATIONS_PPQ = 2150859239;
            public const uint ERROR_WSMAN_QUOTA_MAX_OPERATIONS_USER_PPQ = 2150859240;
            public const uint ERROR_WSMAN_QUOTA_MAX_COMMANDS_PER_SHELL_PPQ = 2150859241;
            public const uint ERROR_WSMAN_QUOTA_MIN_REQUIREMENT_NOT_AVAILABLE_PPQ = 2150859242;
		}
	}
}