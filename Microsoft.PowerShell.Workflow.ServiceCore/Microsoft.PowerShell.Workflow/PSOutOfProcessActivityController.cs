using Microsoft.PowerShell.Activities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.PerformanceData;
using System.Management.Automation.Tracing;
using System.Threading;

namespace Microsoft.PowerShell.Workflow
{
	internal sealed class PSOutOfProcessActivityController : PSActivityHostController
	{
		private const int Servicing = 1;

		private const int NotServicing = 0;

		private const int MinActivityHosts = 1;

		private readonly Collection<ActivityHostProcess> _hostProcesses;

		private readonly ConcurrentQueue<ActivityInvoker> _requests;

		private int _isServicing;

		private int _busyHosts;

		private readonly Tracer _structuredTracer;

		private readonly PSWorkflowConfigurationProvider _configuration;

		private readonly static PSPerfCountersMgr PerfCountersMgr;

		private readonly ConcurrentQueue<ActivityInvoker> _failedRequests;

		static PSOutOfProcessActivityController()
		{
			PSOutOfProcessActivityController.PerfCountersMgr = PSPerfCountersMgr.Instance;
		}

		internal PSOutOfProcessActivityController(PSWorkflowRuntime runtime) : base(runtime)
		{
			this._hostProcesses = new Collection<ActivityHostProcess>();
			this._requests = new ConcurrentQueue<ActivityInvoker>();
			this._structuredTracer = new Tracer();
			this._failedRequests = new ConcurrentQueue<ActivityInvoker>();
			if (runtime != null)
			{
				this._configuration = runtime.Configuration;
				this.InitializeActivityHostProcesses();
				return;
			}
			else
			{
				throw new ArgumentNullException("runtime");
			}
		}

		internal IAsyncResult BeginInvokePowerShell(System.Management.Automation.PowerShell command, PSDataCollection<PSObject> input, PSDataCollection<PSObject> output, PSActivityEnvironment policy, AsyncCallback callback, object state)
		{
			if (command != null)
			{
				ConnectionAsyncResult connectionAsyncResult = new ConnectionAsyncResult(state, callback, command.InstanceId);
				this._structuredTracer.OutOfProcessRunspaceStarted(command.ToString());
				ActivityInvoker activityInvoker = new ActivityInvoker();
				activityInvoker.Input = input;
				activityInvoker.Output = output;
				activityInvoker.Policy = policy;
				activityInvoker.PowerShell = command;
				activityInvoker.AsyncResult = connectionAsyncResult;
				ActivityInvoker activityInvoker1 = activityInvoker;
				connectionAsyncResult.Invoker = activityInvoker1;
				this._requests.Enqueue(activityInvoker1);
				PSOutOfProcessActivityController.PerfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 19, (long)1, true);
				PSOutOfProcessActivityController.PerfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 20, (long)1, true);
				this.CheckAndStartServicingThread();
				return connectionAsyncResult;
			}
			else
			{
				throw new ArgumentNullException("command");
			}
		}

		internal void CancelInvokePowerShell(IAsyncResult asyncResult)
		{
			ConnectionAsyncResult connectionAsyncResult = asyncResult as ConnectionAsyncResult;
			if (connectionAsyncResult != null)
			{
				connectionAsyncResult.Invoker.StopPowerShell();
			}
		}

		private void CheckAndStartServicingThread()
		{
			if (Interlocked.CompareExchange(ref this._isServicing, 1, 0) == 0)
			{
				ThreadPool.QueueUserWorkItem(new WaitCallback(this.ServiceRequests));
			}
		}

		private ActivityHostProcess CreateNewActivityHostProcess()
		{
			ActivityHostProcess activityHostProcess = new ActivityHostProcess(this._configuration.ActivityProcessIdleTimeoutSec, this._configuration.LanguageMode);
			activityHostProcess.ProcessCrashed += new EventHandler<ActivityHostCrashedEventArgs>(this.ProcessCrashed);
			activityHostProcess.Finished += new EventHandler(this.ProcessFinished);
			activityHostProcess.OnProcessIdle += new EventHandler(this.ProcessIdle);
			PSOutOfProcessActivityController.PerfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 23, (long)1, true);
			return activityHostProcess;
		}

		private void DecrementHostCountAndStartThreads()
		{
			Interlocked.Decrement(ref this._busyHosts);
			PSOutOfProcessActivityController.PerfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 16, (long)-1, true);
			this.CheckAndStartServicingThread();
		}

		internal void EndInvokePowerShell(IAsyncResult asyncResult)
		{
			ConnectionAsyncResult connectionAsyncResult = asyncResult as ConnectionAsyncResult;
			if (connectionAsyncResult != null)
			{
				connectionAsyncResult.EndInvoke();
				return;
			}
			else
			{
				throw new PSInvalidOperationException(Resources.AsyncResultNotValid);
			}
		}

		private void InitializeActivityHostProcesses()
		{
			for (int i = 0; i < 1; i++)
			{
				ActivityHostProcess activityHostProcess = this.CreateNewActivityHostProcess();
				this._hostProcesses.Add(activityHostProcess);
			}
		}

		private void ProcessCrashed(object sender, ActivityHostCrashedEventArgs e)
		{
			ActivityHostProcess activityHostProcess = sender as ActivityHostProcess;
			activityHostProcess.MarkForRemoval = true;
			if (e.FailureOnSetup)
			{
				this._failedRequests.Enqueue(e.Invoker);
				PSOutOfProcessActivityController.PerfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 17, (long)1, true);
				PSOutOfProcessActivityController.PerfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 18, (long)1, true);
			}
			this.DecrementHostCountAndStartThreads();
		}

		private void ProcessFinished(object sender, EventArgs e)
		{
			this.DecrementHostCountAndStartThreads();
		}

		private void ProcessIdle(object sender, EventArgs e)
		{
			this.CheckAndStartServicingThread();
		}

		internal void Reset()
		{
			foreach (ActivityHostProcess _hostProcess in this._hostProcesses)
			{
				_hostProcess.Dispose();
			}
			this._hostProcesses.Clear();
			this.InitializeActivityHostProcesses();
		}

		private void RunInProcess(ActivityInvoker invoker, ActivityHostProcess process)
		{
			if (!invoker.IsCancelled)
			{
				process.Busy = true;
				Interlocked.Increment(ref this._busyHosts);
				PSOutOfProcessActivityController.PerfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 16, (long)1, true);
				Tuple<ActivityHostProcess, ActivityInvoker> tuple = new Tuple<ActivityHostProcess, ActivityInvoker>(process, invoker);
				ThreadPool.QueueUserWorkItem(new WaitCallback(PSOutOfProcessActivityController.RunPowerShellInActivityHostWorker), tuple);
				return;
			}
			else
			{
				return;
			}
		}

		internal void RunPowerShellInActivityHost(System.Management.Automation.PowerShell powershell, PSDataCollection<PSObject> input, PSDataCollection<PSObject> output, PSActivityEnvironment policy, ConnectionAsyncResult asyncResult)
		{
			ActivityInvoker activityInvoker = new ActivityInvoker();
			activityInvoker.Input = input;
			activityInvoker.Output = output;
			activityInvoker.Policy = policy;
			activityInvoker.PowerShell = powershell;
			activityInvoker.AsyncResult = asyncResult;
			ActivityInvoker activityInvoker1 = activityInvoker;
			this._requests.Enqueue(activityInvoker1);
			this.CheckAndStartServicingThread();
		}

		private static void RunPowerShellInActivityHostWorker(object state)
		{
			Tuple<ActivityHostProcess, ActivityInvoker> tuple = state as Tuple<ActivityHostProcess, ActivityInvoker>;
			tuple.Item1.PrepareAndRun(tuple.Item2);
		}

		private void SafelyDisposeProcess(ActivityHostProcess process)
		{
			process.Finished -= new EventHandler(this.ProcessFinished);
			process.ProcessCrashed -= new EventHandler<ActivityHostCrashedEventArgs>(this.ProcessCrashed);
			process.OnProcessIdle -= new EventHandler(this.ProcessIdle);
			process.Dispose();
			this._hostProcesses.Remove(process);
			PSOutOfProcessActivityController.PerfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 23, (long)-1, true);
		}

		private void ServiceRequests(object state)
		{
			ActivityInvoker activityInvoker = null;
			bool flag = false;
			while (Interlocked.CompareExchange(ref this._busyHosts, this._configuration.MaxActivityProcesses, this._configuration.MaxActivityProcesses) < this._configuration.MaxActivityProcesses)
			{
				Collection<ActivityHostProcess> activityHostProcesses = this._hostProcesses;
				List<ActivityHostProcess> list = activityHostProcesses.Where<ActivityHostProcess>((ActivityHostProcess process) => process.MarkForRemoval).ToList<ActivityHostProcess>();
				foreach (ActivityHostProcess activityHostProcess in list)
				{
					this.SafelyDisposeProcess(activityHostProcess);
				}
				if (this._failedRequests.Count <= 0)
				{
					this._requests.TryDequeue(out activityInvoker);
					flag = false;
				}
				else
				{
					this._failedRequests.TryDequeue(out activityInvoker);
					flag = true;
				}
				if (activityInvoker == null)
				{
					break;
				}
				if (activityInvoker.IsCancelled)
				{
					continue;
				}
				if (!flag)
				{
					PSOutOfProcessActivityController.PerfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 20, (long)-1, true);
				}
				else
				{
					PSOutOfProcessActivityController.PerfCountersMgr.UpdateCounterByValue(PSWorkflowPerformanceCounterSetInfo.CounterSetId, 18, (long)-1, true);
				}
				bool flag1 = false;
				Collection<ActivityHostProcess> activityHostProcesses1 = this._hostProcesses;
				IEnumerator<ActivityHostProcess> enumerator = activityHostProcesses1.Where<ActivityHostProcess>((ActivityHostProcess process) => !process.Busy).GetEnumerator();
				using (enumerator)
				{
					if (enumerator.MoveNext())
					{
						ActivityHostProcess current = enumerator.Current;
						flag1 = true;
						this.RunInProcess(activityInvoker, current);
					}
				}
				if (flag1)
				{
					continue;
				}
				ActivityHostProcess activityHostProcess1 = this.CreateNewActivityHostProcess();
				this._hostProcesses.Add(activityHostProcess1);
				this.RunInProcess(activityInvoker, activityHostProcess1);
			}
			Interlocked.CompareExchange(ref this._isServicing, 0, 1);
			if ((this._failedRequests.Count > 0 || this._requests.Count > 0) && Interlocked.CompareExchange(ref this._busyHosts, this._configuration.MaxActivityProcesses, this._configuration.MaxActivityProcesses) < this._configuration.MaxActivityProcesses)
			{
				this.CheckAndStartServicingThread();
			}
		}
	}
}