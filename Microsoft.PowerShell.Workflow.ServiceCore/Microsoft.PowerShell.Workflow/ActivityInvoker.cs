using Microsoft.PowerShell.Activities;
using System;
using System.Management.Automation;
using System.Management.Automation.Remoting;
using System.Management.Automation.Runspaces;
using System.Management.Automation.Tracing;

namespace Microsoft.PowerShell.Workflow
{
	internal class ActivityInvoker
	{
		private readonly object _syncObject;

		private bool _invoked;

		private bool _cancelled;

		private readonly PowerShellTraceSource _tracer;

		internal ConnectionAsyncResult AsyncResult
		{
			get;
			set;
		}

		internal PSDataCollection<PSObject> Input
		{
			get;
			set;
		}

		internal bool IsCancelled
		{
			get
			{
				bool flag;
				lock (this._syncObject)
				{
					flag = this._cancelled;
				}
				return flag;
			}
		}

		internal PSDataCollection<PSObject> Output
		{
			get;
			set;
		}

		internal PSActivityEnvironment Policy
		{
			get;
			set;
		}

		internal System.Management.Automation.PowerShell PowerShell
		{
			get;
			set;
		}

		public ActivityInvoker()
		{
			this._syncObject = new object();
			this._tracer = PowerShellTraceSourceFactory.GetTraceSource();
		}

		internal void InvokePowerShell(Runspace runspace)
		{
			IAsyncResult asyncResult;
			IAsyncResult asyncResult1 = null;
			if (!this._cancelled)
			{
				Exception exception = null;
				try
				{
					try
					{
						lock (this._syncObject)
						{
							if (!this._cancelled)
							{
								this._tracer.WriteMessage("State of runspace passed to invoker ", runspace.RunspaceStateInfo.State.ToString());
								this.PowerShell.Runspace = runspace;
								if (this.Input != null && this.Input.EnumeratorNeverBlocks && this.Input.IsOpen)
								{
									this.Input.Complete();
								}
								this._tracer.WriteMessage("BEGIN invocation of command out of proc");
								if (this.Output == null)
								{
									asyncResult = this.PowerShell.BeginInvoke<PSObject>(this.Input);
								}
								else
								{
									asyncResult = this.PowerShell.BeginInvoke<PSObject, PSObject>(this.Input, this.Output);
								}
								asyncResult1 = asyncResult;
								this._invoked = true;
							}
							else
							{
								return;
							}
						}
						this.PowerShell.EndInvoke(asyncResult1);
						this._tracer.WriteMessage("END invocation of command out of proc");
					}
					catch (Exception exception2)
					{
						Exception exception1 = exception2;
						this._tracer.WriteMessage("Running powershell in activity host threw exception");
						this._tracer.TraceException(exception1);
						exception = exception1;
						if (exception1 as PSRemotingTransportException != null)
						{
							throw;
						}
					}
				}
				finally
				{
					this.AsyncResult.SetAsCompleted(exception);
				}
				return;
			}
			else
			{
				return;
			}
		}

		internal void StopPowerShell()
		{
			bool flag = false;
			lock (this._syncObject)
			{
				this._cancelled = true;
				if (this._invoked)
				{
				}
			}
			if (flag)
			{
				this.PowerShell.Stop();
			}
			this.AsyncResult.SetAsCompleted(null);
		}
	}
}