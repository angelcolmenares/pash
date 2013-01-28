using Microsoft.PowerShell.Commands.Management;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Stop", "Process", DefaultParameterSetName="Id", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=113412")]
	[OutputType(new Type[] { typeof(Process) })]
	public sealed class StopProcessCommand : ProcessBaseCommand
	{
		private const string DomainSeparator = "\\";

		private bool passThru;

		private SwitchParameter force;

		private bool shouldKillCurrentProcess;

		private bool yesToAll;

		private bool noToAll;

		private static uint TOKEN_QUERY;

		[Parameter]
		[ValidateNotNullOrEmpty]
		public SwitchParameter Force
		{
			get
			{
				return this.force;
			}
			set
			{
				this.force = value;
			}
		}

		[Parameter(Position=0, ParameterSetName="Id", Mandatory=true, ValueFromPipelineByPropertyName=true)]
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

		[Parameter(Position=0, ParameterSetName="InputObject", Mandatory=true, ValueFromPipeline=true)]
		public Process[] InputObject
		{
			get
			{
				return base.InputObject;
			}
			set
			{
				base.InputObject = value;
			}
		}

		[Alias(new string[] { "ProcessName" })]
		[Parameter(ParameterSetName="Name", Mandatory=true, ValueFromPipelineByPropertyName=true)]
		public string[] Name
		{
			get
			{
				return this.processNames;
			}
			set
			{
				this.processNames = value;
				this.myMode = ProcessBaseCommand.MatchMode.ByName;
			}
		}

		[Parameter]
		public SwitchParameter PassThru
		{
			get
			{
				return this.passThru;
			}
			set
			{
				this.passThru = value;
			}
		}

		static StopProcessCommand()
		{
			StopProcessCommand.TOKEN_QUERY = 8;
		}

		public StopProcessCommand()
		{
		}

		[DllImport("kernel32.dll", CharSet=CharSet.None)]
		private static extern bool CloseHandle(IntPtr hObject);

		private bool DoStopProcess(Process process)
		{
			Exception exception = null;
			try
			{
				if (!process.HasExited)
				{
					process.Kill();
				}
			}
			catch (Win32Exception win32Exception1)
			{
				Win32Exception win32Exception = win32Exception1;
				exception = win32Exception;
			}
			catch (InvalidOperationException invalidOperationException1)
			{
				InvalidOperationException invalidOperationException = invalidOperationException1;
				exception = invalidOperationException;
			}
			if (exception == null)
			{
				return true;
			}
			else
			{
				if (!ProcessBaseCommand.TryHasExited(process))
				{
					base.WriteNonTerminatingError(process, exception, ProcessResources.CouldNotStopProcess, "CouldNotStopProcess", ErrorCategory.CloseError);
				}
				return false;
			}
		}

		protected override void EndProcessing()
		{
			if (this.shouldKillCurrentProcess)
			{
				this.DoStopProcess(Process.GetCurrentProcess());
			}
		}

		private string GetProcessOwnerId(Process process)
		{
			string empty = string.Empty;
			IntPtr zero = IntPtr.Zero;
			try
			{
				try
				{
					StopProcessCommand.OpenProcessToken(process.Handle, StopProcessCommand.TOKEN_QUERY, out zero);
					WindowsIdentity windowsIdentity = new WindowsIdentity(zero);
					empty = windowsIdentity.Name;
				}
				catch (IdentityNotMappedException identityNotMappedException)
				{
				}
				catch (ArgumentException argumentException)
				{
				}
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					StopProcessCommand.CloseHandle(zero);
				}
			}
			return empty;
		}

		private ManagementObjectCollection GetWmiQueryResults(string queryString)
		{
			ManagementObjectCollection managementObjectCollections = null;
			try
			{
				string str = "\\root\\cimv2";
				ManagementScope managementScope = new ManagementScope(str);
				ObjectQuery objectQuery = new ObjectQuery(queryString);
				ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(managementScope, objectQuery);
				managementObjectCollections = managementObjectSearcher.Get();
			}
			catch (ManagementException managementException1)
			{
				ManagementException managementException = managementException1;
				ErrorRecord errorRecord = new ErrorRecord(managementException, "GetWMIManagementException", ErrorCategory.InvalidOperation, null);
				base.WriteError(errorRecord);
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				ErrorRecord errorRecord1 = new ErrorRecord(cOMException, "GetWMICOMException", ErrorCategory.InvalidOperation, null);
				base.WriteError(errorRecord1);
			}
			return managementObjectCollections;
		}

		[DllImport("advapi32.dll", CharSet=CharSet.None)]
		private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

		protected override void ProcessRecord()
		{
			if (this.myMode == ProcessBaseCommand.MatchMode.All || this.myMode == ProcessBaseCommand.MatchMode.ByName && this.processNames == null)
			{
				throw PSTraceSource.NewInvalidOperationException();
			}
			else
			{
				foreach (Process process in base.MatchingProcesses())
				{
					if (!base.ShouldProcess(StringUtil.Format(ProcessResources.ProcessNameForConfirmation, ProcessBaseCommand.SafeGetProcessName(process), ProcessBaseCommand.SafeGetProcessId(process))))
					{
						continue;
					}
					try
					{
						bool hasExited = process.HasExited;
						if (!hasExited)
						{
							if (Process.GetCurrentProcess().Id != ProcessBaseCommand.SafeGetProcessId(process))
							{
								if (!this.Force)
								{
									string processOwnerId = this.GetProcessOwnerId(process);
									if (processOwnerId.Contains("\\"))
									{
										processOwnerId = processOwnerId.Substring(processOwnerId.LastIndexOf("\\", StringComparison.CurrentCultureIgnoreCase) + 1);
									}
									if (!string.Equals(processOwnerId, Environment.UserName, StringComparison.CurrentCultureIgnoreCase))
									{
										string str = StringUtil.Format(ProcessResources.ConfirmStopProcess, ProcessBaseCommand.SafeGetProcessName(process), ProcessBaseCommand.SafeGetProcessId(process));
										if (!base.ShouldContinue(str, null, ref this.yesToAll, ref this.noToAll))
										{
											continue;
										}
									}
								}
								if (string.Equals(ProcessBaseCommand.SafeGetProcessName(process), "SVCHOST", StringComparison.CurrentCultureIgnoreCase))
								{
									string str1 = string.Concat("Select * From Win32_Service Where ProcessId=", ProcessBaseCommand.SafeGetProcessId(process), " And State !='Stopped' ");
									ManagementObjectCollection wmiQueryResults = this.GetWmiQueryResults(str1);
									if (wmiQueryResults != null)
									{
										foreach (ManagementObject wmiQueryResult in wmiQueryResults)
										{
											using (ServiceController serviceController = new ServiceController(wmiQueryResult["Name"].ToString()))
											{
												try
												{
													serviceController.Stop();
													serviceController.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan((long)0x1312d00));
												}
												catch (Win32Exception win32Exception)
												{
													continue;
												}
												catch (InvalidOperationException invalidOperationException)
												{
													continue;
												}
												catch (System.ServiceProcess.TimeoutException timeoutException)
												{
													continue;
												}
											}
										}
									}
								}
								if (!this.DoStopProcess(process))
								{
									continue;
								}
							}
							else
							{
								this.shouldKillCurrentProcess = true;
								continue;
							}
						}
					}
					catch (Win32Exception win32Exception2)
					{
						Win32Exception win32Exception1 = win32Exception2;
						if (!ProcessBaseCommand.TryHasExited(process))
						{
							base.WriteNonTerminatingError(process, win32Exception1, ProcessResources.CouldNotStopProcess, "CouldNotStopProcess", ErrorCategory.CloseError);
							continue;
						}
					}
					catch (InvalidOperationException invalidOperationException2)
					{
						InvalidOperationException invalidOperationException1 = invalidOperationException2;
						if (!ProcessBaseCommand.TryHasExited(process))
						{
							base.WriteNonTerminatingError(process, invalidOperationException1, ProcessResources.CouldNotStopProcess, "CouldNotStopProcess", ErrorCategory.CloseError);
							continue;
						}
					}
					if (!this.PassThru)
					{
						continue;
					}
					base.WriteObject(process);
				}
				return;
			}
		}
	}
}