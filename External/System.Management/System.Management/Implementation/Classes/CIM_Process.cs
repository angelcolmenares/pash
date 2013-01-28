using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Generic;

namespace System.Management.Classes
{
	[Guid("68acf581-18dc-4e0c-920b-dd66dc29dc74")]
	[MetaImplementation(typeof(CIM_Process_MetaImplementation))]
	internal abstract class CIM_Process : CIM_ManagedSystemElement
	{
		public CIM_Process ()
		{

		}

		protected override void RegisterMethods ()
		{
			AddMethod ("Create", new UnixCimMethodInfo { Name = "Create", InProperties =  new List<UnixWbemPropertyInfo>(new [] { new UnixWbemPropertyInfo { Name = "CommandLine", Type = CimType.String, Flavor = 0 }, new UnixWbemPropertyInfo { Name = "CurrentDirectory", Type = CimType.String, Flavor = 0 }}) });
		}

		protected override void RegisterProperies ()
		{
			base.RegisterProperies ();
			RegisterProperty ("Caption", CimType.String, 0);
			RegisterProperty ("CreationClassName", CimType.String, 0);
			RegisterProperty ("CreationDate", CimType.DateTime, 0);
			RegisterProperty ("CSCreationClassName", CimType.String, 0);
			RegisterProperty ("CSName", CimType.String, 0);
			RegisterProperty ("Description", CimType.String, 0);
			RegisterProperty ("ExecutionState", CimType.UInt16, 0);
			RegisterProperty ("Handle", CimType.String, 0);
			RegisterProperty ("InstallDate", CimType.DateTime, 0);
			RegisterProperty ("KernelModeTime", CimType.UInt64, 0);
			RegisterProperty ("Name", CimType.String, 0);
			RegisterProperty ("OSCreationClassName", CimType.String, 0);
			RegisterProperty ("OSName", CimType.String, 0);
			RegisterProperty ("Priority", CimType.UInt32, 0);
			RegisterProperty ("Status", CimType.String, 0);
			RegisterProperty ("TerminationDate", CimType.DateTime, 0);
			RegisterProperty ("UserModeTime", CimType.UInt64, 0);
			RegisterProperty ("WorkingSetSize", CimType.UInt64, 0);
		}

		public override System.Collections.Generic.IEnumerable<object> Get (string strQuery)
		{
			return Get(System.Diagnostics.Process.GetProcesses (), strQuery);
		}

		/* STATUS */

		/*
			 * 
			 * 
			"OK"
			"Error"
			"Degraded"
			"Unknown"
			"Pred Fail"
			"Starting"
			"Stopping"
			"Service"
			"Stressed"
			"NonRecover"
			"No Contact"
			"Lost Comm" 

			**/


		protected override IUnixWbemClassHandler Build (object nativeObj)
		{
			var ret = (CIM_Process)base.Build (nativeObj);
			var process = (System.Diagnostics.Process)nativeObj;
			int processId = SafeGetProcessId (process); 
			var title = process.MainWindowTitle;
			if (string.IsNullOrEmpty (title) || title.Equals ("null", StringComparison.OrdinalIgnoreCase)) title = (OSHelper.IsUnix ? ret.SafeGetProcessName (processId) : process.ProcessName);
			ret.WithProperty ("Caption", title)
					.WithProperty ("CreationClassName", "")
					.WithProperty ("CreationDate", process.StartTime)
					.WithProperty ("CSCreationClassName", "")
					.WithProperty ("CSName", "")
					.WithProperty ("Description", "")
					.WithProperty ("ExecutionState", (ushort)1)
					.WithProperty ("Handle", process.Handle.ToString ())
					.WithProperty ("InstallDate", DateTime.Today)
					.WithProperty ("KernelModeTime", (ulong)process.TotalProcessorTime.Ticks)
					.WithProperty ("Name", (OSHelper.IsUnix ? ret.SafeGetProcessName (processId) : process.ProcessName))
					.WithProperty ("OSCreationClassName", "")
					.WithProperty ("OSName", ret.GetOSName(process))
					.WithProperty ("Priority", (uint)process.BasePriority)
					.WithProperty ("Status", "OK")
					.WithProperty ("TerminationDate", null)
					.WithProperty ("UserModeTime", (ulong)process.UserProcessorTime.Ticks)
					.WithProperty ("WorkingSetSize", (ulong)process.WorkingSet64);

			return ret;
		}

		private OSHelper.OSInformation _osInfo;

		public string GetOSName (Process process)
		{
			if (_osInfo == null) _osInfo = OSHelper.GetComputerDescription();
			return _osInfo.ProductName;
		}

		public string GetOSVersion (Process process)
		{
			if (_osInfo == null) _osInfo = OSHelper.GetComputerDescription();
			var version = _osInfo.Version;
			if (!string.IsNullOrEmpty (_osInfo.BuildNumber)) version += " " + _osInfo.BuildNumber;
			return version;
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

		internal uint SafeGetMaxWorkingSetSize (int id)
		{
			if (cmdInfo == null) FetchProcessInfo (id);
			return cmdInfo.MaxWorkingSetSize;
		}

		internal uint SafeGetMinWorkingSetSize (int id)
		{
			if (cmdInfo == null) FetchProcessInfo (id);
			return cmdInfo.MinWorkingSetSize;
		}
		
		internal uint SafeGetPagedFileUsage (int id)
		{
			if (cmdInfo == null) FetchProcessInfo (id);
			return cmdInfo.PagedFileUsage;
		}

		internal uint SafeGetMajorPageFaults (int id)
		{
			if (cmdInfo == null) FetchProcessInfo (id);
			return cmdInfo.MajorPageFaults;
		}

		internal uint SafeGetMinorPageFaults (int id)
		{
			if (cmdInfo == null) FetchProcessInfo (id);
			return cmdInfo.MinorPageFaults;
		}

		
		internal uint SafeGetThreadsCount (int id)
		{
			if (cmdInfo == null) FetchProcessInfo (id);
			return cmdInfo.ThreadsCount;
		}

		internal ulong SafeGetProcessVirtualSize (int id)
		{
			if (cmdInfo == null) FetchProcessInfo (id);
			return cmdInfo.VirtualMemorySize;
		}

		internal uint SafeGetProcessSessionId (int id)
		{
			if (cmdInfo == null) FetchProcessInfo (id);
			return cmdInfo.SessionId;
		}

		private ProcessCommandInfo cmdInfo;

		internal string SafeGetProcessName (int id)
		{
			if (cmdInfo == null) FetchProcessInfo (id);
			return cmdInfo.ProcessName;
		}

		internal string SafeGetProcessCommandLine (int id)
		{
			if (cmdInfo == null) FetchProcessInfo (id);
			return cmdInfo.CommandLine;
		}

		internal string SafeGetProcessExecutablePath (int id)
		{
			if (cmdInfo == null) FetchProcessInfo (id);
			return cmdInfo.FileName;
		}

		internal string SafeGetProcessArguments (int id)
		{
			if (cmdInfo == null) FetchProcessInfo (id);
			return cmdInfo.Arguments;
		}

		internal int SafeGetParentProcessId (int id)
		{
			if (!OSHelper.IsUnix) return id;
			if (cmdInfo == null) FetchProcessInfo (id);
			return cmdInfo.ParentProcessId;
		}

		internal void FetchProcessInfo(int id)
		{
			cmdInfo = new ProcessCommandInfo();

			try
			{
				if (id != -2147483648)
				{
					var startInfo = new ProcessStartInfo("bash", string.Format ("-c \"ps -p {0} -xo command && ps -p {0} -xco command && ps -p {0} -xco args && ps -p {0} -xo ppid && ps -p {0} -xo rss && ps -p {0} -xo minflt && ps -p {0} -xo majflt && ps -p {0} -xo sess && ps -p {0} -xo vsz && ps -p {0} -M | wc -l\"", id)) { UseShellExecute = false, CreateNoWindow = false, RedirectStandardOutput = true, RedirectStandardError  = true };

					string[]  resultLines = new string[19];
					int i = 0;
					Process p = Process.Start (startInfo);
					DataReceivedEventHandler receiveHandler = (object sender, DataReceivedEventArgs e) => {
						if (i >= resultLines.Length) return;
						if (!string.IsNullOrEmpty (e.Data)) resultLines[i] = e.Data;
						else resultLines[i] = "";
						i++;
					};
					DataReceivedEventHandler errorHandler = (object sender, DataReceivedEventArgs e) => {
					
					};
					p.ErrorDataReceived += errorHandler;
					p.OutputDataReceived += receiveHandler;
					p.BeginOutputReadLine ();
					p.WaitForExit ();
					p.ErrorDataReceived -= errorHandler;
					p.OutputDataReceived -= receiveHandler;
					cmdInfo.CommandLine = resultLines[1];
					string fileName = resultLines[3];
					string args = resultLines[5];
					cmdInfo.ProcessName = fileName;
					cmdInfo.FileName = cmdInfo.CommandLine.Replace (args, fileName);
					args = args.Replace (fileName, "");
					if (args.StartsWith (" ")) args = args.Substring (1);
					cmdInfo.Arguments = args;
					int ppid;
					if (int.TryParse (resultLines[7].Trim (), out ppid))
					{
						cmdInfo.ParentProcessId = ppid;
					}
					uint maxWSize;
					if (uint.TryParse (resultLines[9].Trim (), out maxWSize))
					{
						cmdInfo.MaxWorkingSetSize = maxWSize;
					}
					uint minWSize;
					if (uint.TryParse (resultLines[9].Trim (), out minWSize))
					{
						cmdInfo.MinWorkingSetSize = minWSize;
					}
					uint maxPf;
					if (uint.TryParse (resultLines[11].Trim (), out maxPf))
					{
						cmdInfo.MajorPageFaults = maxPf;
					}
					uint minPf;
					if (uint.TryParse (resultLines[13].Trim (), out minPf))
					{
						cmdInfo.MinorPageFaults = minPf;
					}
					uint sess;
					if (uint.TryParse (resultLines[15].Trim (), out sess))
					{
						cmdInfo.SessionId = sess;
					}
					ulong vmem;
					if (ulong.TryParse (resultLines[17].Trim (), out vmem))
					{
						cmdInfo.VirtualMemorySize = vmem;
					}
					uint thcount;
					if (uint.TryParse (resultLines[18].Trim (), out thcount))
					{
						cmdInfo.ThreadsCount = thcount;
					}
				}
			}
			catch (Win32Exception)
			{

			}
			catch (InvalidOperationException)
			{

			}
		}


		public string Caption { get { return GetPropertyAs<string> ("Caption"); } }
		public string CreationClassName { get { return GetPropertyAs<string> ("CreationClassName"); } }
		public DateTime CreationDate { get { return GetPropertyAs<DateTime> ("CreationDate"); } }
		public string CSCreationClassName { get { return GetPropertyAs<string> ("CSCreationClassName"); } }
		public string CSName { get { return GetPropertyAs<string> ("CSName"); } }
		public string Description { get { return GetPropertyAs<string> ("Description"); } }
		public ushort ExecutionState { get { return GetPropertyAs<ushort> ("ExecutionState"); } }
		public string Handle { get { return GetPropertyAs<string> ("Handle"); } }
		public DateTime InstallDate { get { return GetPropertyAs<DateTime> ("InstallDate"); } }
		public ulong KernelModeTime { get { return GetPropertyAs<ulong> ("KernelModeTime"); } }
		public string Name { get { return GetPropertyAs<string> ("Name"); } }
		public string OSCreationClassName { get { return GetPropertyAs<string> ("Name"); } }
		public string OSName { get { return GetPropertyAs<string> ("OSName"); } }
		public uint Priority { get { return GetPropertyAs<uint> ("Priority"); } }
		public string Status { get { return GetPropertyAs<string> ("Status"); } }
		public DateTime TerminationDate { get { return GetPropertyAs<DateTime> ("TerminationDate"); } }
		public ulong UserModeTime { get { return GetPropertyAs<ulong> ("UserModeTime"); } }
		public ulong WorkingSetSize { get { return GetPropertyAs<ulong> ("WorkingSetSize"); } }

		internal class ProcessCommandInfo
		{
			public string FileName { get; set; }

			public string Arguments { get;set; }

			public string CommandLine { get;set; }

			public string ProcessName { get;set; }

			public int ParentProcessId { get;set; }
			
			public uint MaxWorkingSetSize { get; set; }

			public uint MinWorkingSetSize { get; set; }

			public uint MajorPageFaults { get; set; }
			
			public uint MinorPageFaults { get; set; }

			public uint PagedFileUsage { get;set; }

			public uint SessionId { get; set; }

			public uint ThreadsCount { get; set; }

			public ulong VirtualMemorySize { get; set; }
		}

	}

	
	
	internal class CIM_Process_MetaImplementation : CIM_LogicalDisk
	{
		protected override QueryParser Parser { 
			get { return new QueryParser<CIM_Process_MetaImplementation> (); } 
		}

		protected override bool IsMetaImplementation
		{
			get { return true; }
		}
	}

}