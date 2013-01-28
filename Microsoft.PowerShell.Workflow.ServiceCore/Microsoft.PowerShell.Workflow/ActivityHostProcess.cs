using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.PerformanceData;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Tracing;
using System.Threading;
using System.Timers;

namespace Microsoft.PowerShell.Workflow
{
	internal class ActivityHostProcess : IDisposable
	{
		private const string ActivityHostShellUri = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell.Workflow.ActivityHost";

		private const int WSManLocalPort = 0xb799;

		private const int TimeOut = 0x493e0;

		private const string SetVariableFunction = "function Set-Variable\r\n        {\r\n            [CmdletBinding()]\r\n            param(\r\n        \r\n                [Parameter(Position=0)]\r\n                [string[]]\r\n                $Name,\r\n        \r\n                [Parameter(Position=1)]\r\n                [object[]]\r\n                $Value        \r\n            )\r\n    \r\n            for($i=0; $i -lt $Name.Count; $i++)\r\n            {\r\n                microsoft.powershell.utility\\set-variable -name $Name[$i] -value $Value[$i] -scope global\r\n            }\r\n\r\n            Set-StrictMode -Off\r\n        }";

		private static PSPerfCountersMgr _perfCountersMgr;

		private Runspace _runspace;

		private readonly static WSManConnectionInfo ActivityHostConnectionInfo;

		private readonly static string[] ActivitiesTypesFiles;

		private readonly static TypeTable ActivitiesTypeTable;

		private bool _busy;

		private readonly object _syncObject;

		private readonly PowerShellProcessInstance _processInstance;

		private readonly PowerShellTraceSource _tracer;

		private ActivityInvoker _currentInvoker;

		private readonly System.Timers.Timer _timer;

		private readonly bool _useJobIPCProcess;

		private readonly PSLanguageMode? _languageMode;

		private bool _isDisposed;

		private bool _markForRemoval;

		internal static string ActivityHostConfiguration
		{
			get
			{
				return "http://schemas.microsoft.com/powershell/Microsoft.PowerShell.Workflow.ActivityHost";
			}
		}

		internal bool Busy
		{
			get
			{
				bool flag;
				lock (this._syncObject)
				{
					flag = this._busy;
				}
				return flag;
			}
			set
			{
				if (!value)
				{
					this.ResetBusy();
					return;
				}
				else
				{
					this.SetBusy();
					return;
				}
			}
		}

		internal bool MarkForRemoval
		{
			get
			{
				return this._markForRemoval;
			}
			set
			{
				this._markForRemoval = value;
			}
		}

		static ActivityHostProcess()
		{
			ActivityHostProcess._perfCountersMgr = PSPerfCountersMgr.Instance;
			string[] strArrays = new string[1];
			strArrays[0] = "%windir%\\system32\\windowspowershell\\v1.0\\modules\\psworkflow\\PSWorkflow.types.ps1xml";
			ActivityHostProcess.ActivitiesTypesFiles = strArrays;
			WSManConnectionInfo wSManConnectionInfo = new WSManConnectionInfo();
			wSManConnectionInfo.Port = 0xb799;
			wSManConnectionInfo.ShellUri = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell.Workflow.ActivityHost";
			ActivityHostProcess.ActivityHostConnectionInfo = wSManConnectionInfo;
			List<string> defaultTypeFiles = TypeTable.GetDefaultTypeFiles();
			defaultTypeFiles.AddRange(ActivityHostProcess.ActivitiesTypesFiles.Select<string, string>(new Func<string, string>(Environment.ExpandEnvironmentVariables)));
			ActivityHostProcess.ActivitiesTypeTable = new TypeTable(defaultTypeFiles);
		}

		internal ActivityHostProcess(int activityHostTimeoutSec, PSLanguageMode? languageMode)
		{
			object obj;
			this._syncObject = new object();
			this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
			this._languageMode = languageMode;
			this._useJobIPCProcess = true;
			this._tracer.WriteMessage("BEGIN Creating new PowerShell process instance");
			this._processInstance = new PowerShellProcessInstance();
			this._tracer.WriteMessage("END Creating new PowerShell process instance ");
			this._runspace = this.CreateRunspace();
			Guid instanceId = this._runspace.InstanceId;
			this._tracer.WriteMessage("New runspace created ", instanceId.ToString());
			Timer timer = new Timer();
			timer.AutoReset = false;
			timer.Interval = 300000;
			this._timer = timer;
			this._timer.Elapsed += new ElapsedEventHandler(this.TimerElapsed);
			Timer timer1 = this._timer;
			if (activityHostTimeoutSec > 0)
			{
				obj = activityHostTimeoutSec * 0x3e8;
			}
			else
			{
				obj = 0x493e0;
			}
			timer1.Interval = (double)obj;
			ActivityHostProcess._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 21, (long)1, true);
		}

		private void CloseAndDisposeRunspace()
		{
			try
			{
				this._runspace.Close();
				this._runspace.Dispose();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				this._tracer.TraceException(exception);
			}
		}

		private Runspace CreateRunspace()
		{
			if (this._useJobIPCProcess)
			{
				return RunspaceFactory.CreateOutOfProcessRunspace(ActivityHostProcess.ActivitiesTypeTable, this._processInstance);
			}
			else
			{
				return RunspaceFactory.CreateRunspace(ActivityHostProcess.ActivityHostConnectionInfo, null, ActivityHostProcess.ActivitiesTypeTable);
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!this._isDisposed)
				{
					lock (this._syncObject)
					{
						if (!this._isDisposed)
						{
							this._isDisposed = true;
						}
						else
						{
							return;
						}
					}
					this.CloseAndDisposeRunspace();
					this._processInstance.Dispose();
					this._currentInvoker = null;
					this._tracer.Dispose();
					ActivityHostProcess._perfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 22, (long)1, true);
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

		private Runspace GetRunspace(bool createNew)
		{
			if (this._runspace.RunspaceStateInfo.State != RunspaceState.BeforeOpen)
			{
				if (this._useJobIPCProcess)
				{
					this.CloseAndDisposeRunspace();
					this._runspace = this.CreateRunspace();
					Guid instanceId = this._runspace.InstanceId;
					this._tracer.WriteMessage("New runspace created ", instanceId.ToString());
					this.OpenRunspace(this._runspace);
				}
			}
			else
			{
				this.OpenRunspace(this._runspace);
			}
			return this._runspace;
		}

		private void HandleTransportError(PSRemotingTransportException transportException, bool onSetup)
		{
			this._tracer.TraceException(transportException);
			if (this.ProcessCrashed != null)
			{
				ActivityHostCrashedEventArgs activityHostCrashedEventArg = new ActivityHostCrashedEventArgs();
				activityHostCrashedEventArg.FailureOnSetup = onSetup;
				activityHostCrashedEventArg.Invoker = this._currentInvoker;
				ActivityHostCrashedEventArgs activityHostCrashedEventArg1 = activityHostCrashedEventArg;
				this.ProcessCrashed(this, activityHostCrashedEventArg1);
			}
		}

		private void ImportModulesFromPolicy(Runspace runspace, ICollection<string> modules)
		{
			if (modules.Count > 0)
			{
				PowerShell powerShell = PowerShell.Create();
				using (powerShell)
				{
					powerShell.Runspace = runspace;
					powerShell.AddCommand("Import-Module").AddArgument(modules).AddParameter("ErrorAction", ActionPreference.Stop).AddParameter("Force");
					powerShell.Invoke();
				}
				return;
			}
			else
			{
				return;
			}
		}

		private void OpenRunspace(Runspace runspace)
		{
			try
			{
				Guid instanceId = this._runspace.InstanceId;
				this._tracer.WriteMessage("Opening runspace ", instanceId.ToString());
				runspace.Open();
				Guid guid = this._runspace.InstanceId;
				this._tracer.WriteMessage("Runspace opened successfully ", guid.ToString());
				PSLanguageMode? nullable = this._languageMode;
				if (nullable.HasValue)
				{
					PSLanguageMode? nullable1 = this._languageMode;
					if (nullable1.HasValue)
					{
						PowerShell powerShell = PowerShell.Create();
						using (powerShell)
						{
							powerShell.Runspace = runspace;
							PSLanguageMode? nullable2 = this._languageMode;
							string str = string.Concat("$ExecutionContext.SessionState.LanguageMode = '", nullable2.Value.ToString(), "'");
							powerShell.AddScript(str);
							powerShell.Invoke();
						}
					}
				}
			}
			catch (PSRemotingTransportRedirectException pSRemotingTransportRedirectException)
			{
				Guid instanceId1 = this._runspace.InstanceId;
				this._tracer.WriteMessage("Opening runspace threw  PSRemotingTransportRedirectException", instanceId1.ToString());
			}
			catch (PSRemotingTransportException pSRemotingTransportException1)
			{
				PSRemotingTransportException pSRemotingTransportException = pSRemotingTransportException1;
				Guid guid1 = this._runspace.InstanceId;
				this._tracer.WriteMessage("Opening runspace threw  PSRemotingTransportException", guid1.ToString());
				this._tracer.TraceException(pSRemotingTransportException);
				throw;
			}
			catch (PSRemotingDataStructureException pSRemotingDataStructureException)
			{
				Guid instanceId2 = this._runspace.InstanceId;
				this._tracer.WriteMessage("Opening runspace threw  PSRemotingDataStructureException", instanceId2.ToString());
			}
		}

		internal void PrepareAndRun(ActivityInvoker invoker)
		{
			Runspace runspace;
			bool flag = false;
			try
			{
				try
				{
					this._currentInvoker = invoker;
					for (int i = 1; i <= 10 && !invoker.IsCancelled; i++)
					{
						runspace = this.GetRunspace(true);
						if (runspace.RunspaceStateInfo.State == RunspaceState.Opened)
						{
							break;
						}
						Thread.Sleep(i * 200);
					}
					if (!invoker.IsCancelled)
					{
						if (invoker.Policy.Variables.Count > 0)
						{
							Guid instanceId = this._runspace.InstanceId;
							this._tracer.WriteMessage("BEGIN Setting up variables in runspace ", instanceId.ToString());
							this.SetVariablesFromPolicy(runspace, invoker.Policy.Variables);
							Guid guid = this._runspace.InstanceId;
							this._tracer.WriteMessage("END Setting up variables in runspace ", guid.ToString());
						}
						if (invoker.Policy.Modules.Count > 0)
						{
							Guid instanceId1 = this._runspace.InstanceId;
							this._tracer.WriteMessage("BEGIN Setting up runspace from policy ", instanceId1.ToString());
							this.ImportModulesFromPolicy(runspace, invoker.Policy.Modules);
							Guid guid1 = this._runspace.InstanceId;
							this._tracer.WriteMessage("END Setting up runspace from policy ", guid1.ToString());
						}
						flag = true;
						invoker.InvokePowerShell(runspace);
					}
					else
					{
						return;
					}
				}
				catch (PSRemotingTransportException pSRemotingTransportException1)
				{
					PSRemotingTransportException pSRemotingTransportException = pSRemotingTransportException1;
					this.HandleTransportError(pSRemotingTransportException, !flag);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this._tracer.TraceException(exception);
					invoker.AsyncResult.SetAsCompleted(exception);
				}
			}
			finally
			{
				this._currentInvoker = null;
				this.ResetBusy();
			}
		}

		internal void RaiseProcessFinishedEvent()
		{
			if (this.Finished != null)
			{
				this.Finished(this, new EventArgs());
			}
		}

		private void ResetBusy()
		{
			lock (this._syncObject)
			{
				if (this._busy && !this._markForRemoval)
				{
					this._busy = false;
					this._timer.Enabled = true;
					this.RaiseProcessFinishedEvent();
				}
			}
		}

		private void SetBusy()
		{
			lock (this._syncObject)
			{
				this._busy = true;
				this._timer.Stop();
			}
		}

		private void SetVariablesFromPolicy(Runspace runspace, IDictionary<string, object> variables)
		{
			PowerShell powerShell = PowerShell.Create();
			using (powerShell)
			{
				powerShell.Runspace = runspace;
				powerShell.AddScript("function Set-Variable\r\n        {\r\n            [CmdletBinding()]\r\n            param(\r\n        \r\n                [Parameter(Position=0)]\r\n                [string[]]\r\n                $Name,\r\n        \r\n                [Parameter(Position=1)]\r\n                [object[]]\r\n                $Value        \r\n            )\r\n    \r\n            for($i=0; $i -lt $Name.Count; $i++)\r\n            {\r\n                microsoft.powershell.utility\\set-variable -name $Name[$i] -value $Value[$i] -scope global\r\n            }\r\n\r\n            Set-StrictMode -Off\r\n        }");
				powerShell.Invoke();
				powerShell.Commands.Clear();
				powerShell.AddCommand("Set-Variable").AddParameter("Name", variables.Keys).AddParameter("Value", variables.Values);
				powerShell.Invoke();
			}
		}

		private void TimerElapsed(object sender, ElapsedEventArgs e)
		{
			lock (this._syncObject)
			{
				if (!this._busy)
				{
					this._busy = true;
					this._markForRemoval = true;
				}
				else
				{
					return;
				}
			}
			if (this.OnProcessIdle != null)
			{
				this.OnProcessIdle(this, new EventArgs());
			}
		}

		internal event EventHandler Finished;
		internal event EventHandler OnProcessIdle;
		internal event EventHandler<ActivityHostCrashedEventArgs> ProcessCrashed;
	}
}