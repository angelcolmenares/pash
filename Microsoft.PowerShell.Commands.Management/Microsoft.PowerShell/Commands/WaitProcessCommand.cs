using Microsoft.PowerShell.Commands.Management;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Threading;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Wait", "Process", DefaultParameterSetName="Name", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135277")]
	public sealed class WaitProcessCommand : ProcessBaseCommand
	{
		private int timeout;

		private bool timeOutSpecified;

		private bool disposed;

		private List<Process> processList;

		private ManualResetEvent waitHandle;

		private int numberOfProcessesToWaitFor;

		[Alias(new string[] { "PID", "ProcessId" })]
		[Parameter(ParameterSetName="Id", Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
		public int[] Id
		{
			get
			{
				return this.processIds;
			}
			set
			{
				this.myMode = ProcessBaseCommand.MatchMode.ById;
				this.processIds = value;
			}
		}

		[Alias(new string[] { "ProcessName" })]
		[Parameter(ParameterSetName="Name", Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true)]
		public string[] Name
		{
			get
			{
				return this.processNames;
			}
			set
			{
				this.myMode = ProcessBaseCommand.MatchMode.ByName;
				this.processNames = value;
			}
		}

		[Alias(new string[] { "TimeoutSec" })]
		[Parameter(Position=1)]
		[ValidateNotNullOrEmpty]
		[ValidateRange(0, 0x7fff)]
		public int Timeout
		{
			get
			{
				return this.timeout;
			}
			set
			{
				this.timeout = value;
				this.timeOutSpecified = true;
			}
		}

		public WaitProcessCommand()
		{
			this.processList = new List<Process>();
		}

		public void Dispose()
		{
			if (!this.disposed)
			{
				if (this.waitHandle != null)
				{
					this.waitHandle.Close();
					this.waitHandle = null;
				}
				this.disposed = true;
			}
		}

		protected override void EndProcessing()
		{
			this.waitHandle = new ManualResetEvent(false);
			foreach (Process process in this.processList)
			{
				try
				{
					if (!process.HasExited)
					{
						process.EnableRaisingEvents = true;
						process.Exited += new EventHandler(this.myProcess_Exited);
						if (!process.HasExited)
						{
							Interlocked.Increment(ref this.numberOfProcessesToWaitFor);
						}
					}
				}
				catch (Win32Exception win32Exception1)
				{
					Win32Exception win32Exception = win32Exception1;
					base.WriteNonTerminatingError(process, win32Exception, ProcessResources.Process_is_not_terminated, "ProcessNotTerminated", ErrorCategory.CloseError);
				}
			}
			if (this.numberOfProcessesToWaitFor > 0)
			{
				if (!this.timeOutSpecified)
				{
					this.waitHandle.WaitOne();
				}
				else
				{
					this.waitHandle.WaitOne(this.timeout * 0x3e8, false);
				}
			}
			foreach (Process process1 in this.processList)
			{
				try
				{
					if (!process1.HasExited)
					{
						object[] processName = new object[2];
						processName[0] = process1.ProcessName;
						processName[1] = process1.Id;
						string str = StringUtil.Format(ProcessResources.ProcessNotTerminated, processName);
						ErrorRecord errorRecord = new ErrorRecord(new TimeoutException(str), "ProcessNotTerminated", ErrorCategory.CloseError, process1);
						base.WriteError(errorRecord);
					}
				}
				catch (Win32Exception win32Exception3)
				{
					Win32Exception win32Exception2 = win32Exception3;
					base.WriteNonTerminatingError(process1, win32Exception2, ProcessResources.Process_is_not_terminated, "ProcessNotTerminated", ErrorCategory.CloseError);
				}
			}
		}

		private void myProcess_Exited(object sender, EventArgs e)
		{
			if (Interlocked.Decrement(ref this.numberOfProcessesToWaitFor) == 0 && this.waitHandle != null)
			{
				this.waitHandle.Set();
			}
		}

		protected override void ProcessRecord()
		{
			foreach (Process process in base.MatchingProcesses())
			{
				if (process.Id != 0)
				{
					int id = process.Id;
					if (!id.Equals(Process.GetCurrentProcess().Id))
					{
						this.processList.Add(process);
					}
					else
					{
						base.WriteNonTerminatingError(process, null, ProcessResources.WaitOnItself, "WaitOnItself", ErrorCategory.ObjectNotFound);
					}
				}
				else
				{
					base.WriteNonTerminatingError(process, null, ProcessResources.WaitOnIdleProcess, "WaitOnIdleProcess", ErrorCategory.ObjectNotFound);
				}
			}
		}

		protected override void StopProcessing()
		{
			if (this.waitHandle != null)
			{
				this.waitHandle.Set();
			}
		}
	}
}