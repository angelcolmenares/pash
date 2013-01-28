using Microsoft.PowerShell.Commands.Management;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
	public abstract class ProcessBaseCommand : Cmdlet
	{
		internal ProcessBaseCommand.MatchMode myMode;

		private string[] computerName;

		internal string[] processNames;

		internal int[] processIds;

		private Process[] input;

		private List<Process> _matchingProcesses;

		private Dictionary<int, Process> _keys;

		private Process[] allProcesses;

		internal Process[] AllProcesses
		{
			get
			{
				if (this.allProcesses == null)
				{
					List<Process> processes = new List<Process>();
					if ((int)this.SuppliedComputerName.Length <= 0)
					{
						processes.AddRange(Process.GetProcesses());
					}
					else
					{
						string[] suppliedComputerName = this.SuppliedComputerName;
						for (int i = 0; i < (int)suppliedComputerName.Length; i++)
						{
							string str = suppliedComputerName[i];
							processes.AddRange(Process.GetProcesses(str));
						}
					}
					this.allProcesses = processes.ToArray();
				}
				return this.allProcesses;
			}
		}

		[Parameter(ParameterSetName="InputObject", Mandatory=true, ValueFromPipeline=true)]
		public Process[] InputObject
		{
			get
			{
				return this.input;
			}
			set
			{
				this.myMode = ProcessBaseCommand.MatchMode.ByInput;
				this.input = value;
			}
		}

		protected string[] SuppliedComputerName
		{
			get
			{
				return this.computerName;
			}
			set
			{
				this.computerName = value;
			}
		}

		protected ProcessBaseCommand()
		{
			this.computerName = new string[0];
			this._matchingProcesses = new List<Process>();
			this._keys = new Dictionary<int, Process>();
		}

		private void AddIdempotent(Process process)
		{
			int hashCode = ProcessBaseCommand.SafeGetProcessName(process).GetHashCode() ^ ProcessBaseCommand.SafeGetProcessId(process);
			if (!this._keys.ContainsKey(hashCode))
			{
				this._keys.Add(hashCode, process);
				this._matchingProcesses.Add(process);
			}
		}

		internal List<Process> MatchingProcesses()
		{
			this._matchingProcesses.Clear();
			ProcessBaseCommand.MatchMode matchMode = this.myMode;
			switch (matchMode)
			{
				case ProcessBaseCommand.MatchMode.ById:
				{
					this.RetrieveMatchingProcessesById();
					break;
				}
				case ProcessBaseCommand.MatchMode.ByInput:
				{
					this.RetrieveProcessesByInput();
					break;
				}
				default:
				{
					this.RetrieveMatchingProcessesByProcessName();
					break;
				}
			}
			this._matchingProcesses.Sort(new Comparison<Process>(ProcessBaseCommand.ProcessComparison));
			return this._matchingProcesses;
		}

		private static int ProcessComparison(Process x, Process y)
		{
			int num = string.Compare(ProcessBaseCommand.SafeGetProcessName(x), ProcessBaseCommand.SafeGetProcessName(y), StringComparison.CurrentCultureIgnoreCase);
			if (num == 0)
			{
				return ProcessBaseCommand.SafeGetProcessId(x) - ProcessBaseCommand.SafeGetProcessId(y);
			}
			else
			{
				return num;
			}
		}

		private void RetrieveMatchingProcessesById()
		{
			Process processById;
			if (this.processIds != null)
			{
				int[] numArray = this.processIds;
				for (int i = 0; i < (int)numArray.Length; i++)
				{
					int num = numArray[i];
					try
					{
						if ((int)this.SuppliedComputerName.Length <= 0)
						{
							processById = Process.GetProcessById(num);
							this.AddIdempotent(processById);
						}
						else
						{
							string[] suppliedComputerName = this.SuppliedComputerName;
							for (int j = 0; j < (int)suppliedComputerName.Length; j++)
							{
								string str = suppliedComputerName[j];
								processById = Process.GetProcessById(num, str);
								this.AddIdempotent(processById);
							}
						}
					}
					catch (ArgumentException argumentException)
					{
						this.WriteNonTerminatingError("", num, num, null, ProcessResources.NoProcessFoundForGivenId, "NoProcessFoundForGivenId", ErrorCategory.ObjectNotFound);
					}
				}
				return;
			}
			else
			{
				throw PSTraceSource.NewInvalidOperationException();
			}
		}

		private void RetrieveMatchingProcessesByProcessName()
		{
			if (this.processNames != null)
			{
				string[] strArrays = this.processNames;
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str = strArrays[i];
					WildcardPattern wildcardPattern = new WildcardPattern(str, WildcardOptions.IgnoreCase);
					bool flag = false;
					Process[] allProcesses = this.AllProcesses;
					for (int j = 0; j < (int)allProcesses.Length; j++)
					{
						Process process = allProcesses[j];
						if (wildcardPattern.IsMatch(ProcessBaseCommand.SafeGetProcessName(process)))
						{
							flag = true;
							this.AddIdempotent(process);
						}
					}
					if (!flag && !WildcardPattern.ContainsWildcardCharacters(str))
					{
						this.WriteNonTerminatingError(str, 0, str, null, ProcessResources.NoProcessFoundForGivenName, "NoProcessFoundForGivenName", ErrorCategory.ObjectNotFound);
					}
				}
				return;
			}
			else
			{
				this._matchingProcesses = new List<Process>(this.AllProcesses);
				return;
			}
		}

		private void RetrieveProcessesByInput()
		{
			if (this.InputObject != null)
			{
				Process[] inputObject = this.InputObject;
				for (int i = 0; i < (int)inputObject.Length; i++)
				{
					Process process = inputObject[i];
					ProcessBaseCommand.SafeRefresh(process);
					this.AddIdempotent(process);
				}
				return;
			}
			else
			{
				throw PSTraceSource.NewInvalidOperationException();
			}
		}

		internal static int SafeGetProcessId(Process process)
		{
			int id;
			try
			{
				id = process.Id;
			}
			catch (Win32Exception win32Exception)
			{
				id = -2147483648;
			}
			catch (InvalidOperationException invalidOperationException)
			{
				id = -2147483648;
			}
			return id;
		}

		internal static string SafeGetProcessName(Process process)
		{
			string processName = "";
			try
			{
				if (OSHelper.IsMacOSX)
				{
					int id = SafeGetProcessId(process);
					if (id != -2147483648)
					{
						var startInfo = new ProcessStartInfo("ps", "-p " + id.ToString () + " -xco command") { UseShellExecute = false, CreateNoWindow = false, RedirectStandardError = true, RedirectStandardOutput = true };
						bool headerPassed = false;
						Process p = Process.Start (startInfo);
						DataReceivedEventHandler outputEvent = (object sender, DataReceivedEventArgs e) => {
							if (headerPassed && !string.IsNullOrEmpty (e.Data)) processName = e.Data;
							headerPassed = true;
						};
						DataReceivedEventHandler errorEvent = (object sender, DataReceivedEventArgs e) => {
							// do nothing
						};
						p.OutputDataReceived += outputEvent;
						p.ErrorDataReceived += errorEvent;
						p.BeginOutputReadLine ();
						p.WaitForExit ();
						p.OutputDataReceived -= outputEvent;
						p.ErrorDataReceived -= errorEvent;
						p.Dispose ();
					}
				}
				else {
					processName = process.ProcessName;
				}
			}
			catch (Win32Exception win32Exception)
			{
				processName = "";
			}
			catch (InvalidOperationException invalidOperationException)
			{
				processName = "";
			}
			return processName;
		}

		internal static void SafeRefresh(Process process)
		{
			try
			{
				process.Refresh();
			}
			catch (Win32Exception win32Exception)
			{
			}
			catch (InvalidOperationException invalidOperationException)
			{
			}
		}

		internal static bool TryHasExited(Process process)
		{
			bool hasExited;
			try
			{
				hasExited = process.HasExited;
			}
			catch (Win32Exception win32Exception)
			{
				hasExited = false;
			}
			catch (InvalidOperationException invalidOperationException)
			{
				hasExited = false;
			}
			return hasExited;
		}

		internal void WriteNonTerminatingError(Process process, Exception innerException, string resourceId, string errorId, ErrorCategory category)
		{
			this.WriteNonTerminatingError(ProcessBaseCommand.SafeGetProcessName(process), ProcessBaseCommand.SafeGetProcessId(process), process, innerException, resourceId, errorId, category);
		}

		internal void WriteNonTerminatingError(string processName, int processId, object targetObject, Exception innerException, string resourceId, string errorId, ErrorCategory category)
		{
			object message;
			string str = resourceId;
			object[] objArray = new object[3];
			objArray[0] = processName;
			objArray[1] = processId;
			object[] objArray1 = objArray;
			int num = 2;
			if (innerException == null)
			{
				message = "";
			}
			else
			{
				message = innerException.Message;
			}
			objArray1[num] = message;
			string str1 = StringUtil.Format(str, objArray);
			ProcessCommandException processCommandException = new ProcessCommandException(str1, innerException);
			processCommandException.ProcessName = processName;
			base.WriteError(new ErrorRecord(processCommandException, errorId, category, targetObject));
		}

		internal enum MatchMode
		{
			All,
			ByName,
			ById,
			ByInput
		}
	}
}