using System;
using System.Collections.Generic;
using System.Management.Automation.Tracing;
using System.Timers;

namespace Microsoft.PowerShell.Workflow
{
	public sealed class PSWorkflowTimer : IDisposable
	{
		private readonly PowerShellTraceSource Tracer;

		private readonly static Tracer StructuredTracer;

		private readonly PSWorkflowInstance _instance;

		private bool disposed;

		private readonly object syncLock;

		private Dictionary<WorkflowTimerType, PSTimer> _timers;

		private readonly object syncElapsedLock;

		private bool _elapsedTimerCalled;

		static PSWorkflowTimer()
		{
			PSWorkflowTimer.StructuredTracer = new Tracer();
		}

		internal PSWorkflowTimer(PSWorkflowInstance instance)
		{
			this.Tracer = PowerShellTraceSourceFactory.GetTraceSource();
			this.syncLock = new object();
			this.syncElapsedLock = new object();
			this._instance = instance;
			this._timers = new Dictionary<WorkflowTimerType, PSTimer>();
		}

		public PSWorkflowTimer(PSWorkflowInstance instance, object deserializedTimers)
		{
			this.Tracer = PowerShellTraceSourceFactory.GetTraceSource();
			this.syncLock = new object();
			this.syncElapsedLock = new object();
			this._instance = instance;
			this._timers = new Dictionary<WorkflowTimerType, PSTimer>();
			if (deserializedTimers != null)
			{
				List<object> objs = (List<object>)deserializedTimers;
				foreach (object obj in objs)
				{
					if (obj == null || obj as Dictionary<string, object> == null)
					{
						continue;
					}
					PSTimer pSTimer = new PSTimer((Dictionary<string, object>)obj, new WorkflowTimerElapsedHandler(this.Timer_WorkflowTimerElapsed));
					this._timers.Add(pSTimer.TimerType, pSTimer);
				}
				return;
			}
			else
			{
				throw new ArgumentNullException("deserializedTimers");
			}
		}

		internal bool CheckIfTimerHasReachedAlready(WorkflowTimerType timerType)
		{
			if (!this.disposed)
			{
				if (!this._timers.ContainsKey(timerType) || !this._timers[WorkflowTimerType.ElapsedTimer].TimerReachedAlready)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (this.disposed || !disposing)
			{
				return;
			}
			else
			{
				lock (this.syncLock)
				{
					if (!this.disposed)
					{
						foreach (PSTimer value in this._timers.Values)
						{
							value.Dispose();
						}
						this._timers.Clear();
						this.disposed = true;
					}
				}
				return;
			}
		}

		public object GetSerializedData()
		{
			if (!this.disposed)
			{
				List<object> objs = new List<object>();
				foreach (PSTimer value in this._timers.Values)
				{
					objs.Add(value.GetSerializedData());
				}
				return objs;
			}
			else
			{
				return null;
			}
		}

		internal void SetupTimer(WorkflowTimerType timerType, TimeSpan interval)
		{
			if (!this.disposed)
			{
				if (!this._timers.ContainsKey(timerType))
				{
					if (timerType != WorkflowTimerType.ElapsedTimer)
					{
						if (timerType == WorkflowTimerType.RunningTimer)
						{
							this._timers.Add(timerType, new PSTimer(timerType, false, false, interval, new WorkflowTimerElapsedHandler(this.Timer_WorkflowTimerElapsed)));
						}
						return;
					}
					else
					{
						this._timers.Add(timerType, new PSTimer(timerType, false, true, interval, new WorkflowTimerElapsedHandler(this.Timer_WorkflowTimerElapsed)));
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

		internal void StartTimer(WorkflowTimerType timerType)
		{
			if (!this.disposed)
			{
				if (this._timers.ContainsKey(timerType))
				{
					this._timers[timerType].Start();
				}
				return;
			}
			else
			{
				return;
			}
		}

		internal void StopTimer(WorkflowTimerType timerType)
		{
			if (!this.disposed)
			{
				if (this._timers.ContainsKey(timerType))
				{
					this._timers[timerType].Stop();
				}
				return;
			}
			else
			{
				return;
			}
		}

		private void TerminateAndRemoveWorkflow(string reason)
		{
			this._elapsedTimerCalled = true;
			if (!this.disposed)
			{
				lock (this.syncElapsedLock)
				{
					if (!this.disposed)
					{
						try
						{
							this._instance.PSWorkflowJob.StopJob(true, Resources.ElapsedTimeReached);
							if (!this._instance.PSWorkflowJob.SynchronousExecution)
							{
								this._instance.Runtime.JobManager.RemoveChildJob(this._instance.PSWorkflowJob);
							}
						}
						catch (Exception exception1)
						{
							Exception exception = exception1;
							this.Tracer.TraceException(exception);
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

		private void TerminateWorkflow(string reason)
		{
			if (!this.disposed)
			{
				if (!this._elapsedTimerCalled)
				{
					lock (this.syncElapsedLock)
					{
						if (!this.disposed)
						{
							if (!this._elapsedTimerCalled)
							{
								try
								{
									this._instance.PSWorkflowJob.StopJob(true, reason);
								}
								catch (Exception exception1)
								{
									Exception exception = exception1;
									this.Tracer.TraceException(exception);
								}
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
			else
			{
				return;
			}
		}

		private void Timer_WorkflowTimerElapsed(PSTimer sender, ElapsedEventArgs e)
		{
			if (!this.disposed)
			{
				PSWorkflowTimer.StructuredTracer.Correlate();
				this.Tracer.WriteMessage(string.Concat("PSWorkflowTimer Elapsed: ", sender.TimerType));
				if (!this.disposed)
				{
					WorkflowTimerType timerType = sender.TimerType;
					switch (timerType)
					{
						case WorkflowTimerType.RunningTimer:
						{
							sender.Stop();
							this.TerminateWorkflow(Resources.RunningTimeReached);
							return;
						}
						case WorkflowTimerType.ElapsedTimer:
						{
							sender.Stop();
							this.TerminateAndRemoveWorkflow(Resources.ElapsedTimeReached);
							return;
						}
						default:
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
			else
			{
				return;
			}
		}
	}
}