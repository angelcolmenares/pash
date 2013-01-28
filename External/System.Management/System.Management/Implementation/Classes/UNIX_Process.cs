using System;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Management.Classes
{
	[Guid("9fa13381-e953-4cc7-a4f0-9849d84e6f20")]
	internal class UNIX_Process : CIM_Process
	{
		public UNIX_Process ()
		{

		}

		protected override void RegisterProperies ()
		{
			base.RegisterProperies ();
			RegisterProperty ("Caption", CimType.String, 0);
			RegisterProperty ("CommandLine", CimType.String, 0); /* */
			RegisterProperty ("CreationClassName", CimType.String, 0);
			RegisterProperty ("CreationDate", CimType.DateTime, 0);
			RegisterProperty ("CSCreationClassName", CimType.String, 0);
			RegisterProperty ("CSName", CimType.String, 0);
			RegisterProperty ("Description", CimType.String, 0);
			RegisterProperty ("ExecutionState", CimType.UInt16, 0);
			RegisterProperty ("ExecutablePath", CimType.String, 0); /* */
			RegisterProperty ("Handle", CimType.String, 0);
			RegisterProperty ("HandleCount", CimType.UInt32, 0); /* */
			RegisterProperty ("InstallDate", CimType.DateTime, 0);
			RegisterProperty ("KernelModeTime", CimType.UInt64, 0);
			RegisterProperty ("MaximumWorkingSetSize", CimType.UInt32, 0); /* */
			RegisterProperty ("MinimumWorkingSetSize", CimType.UInt32, 0); /* */
			RegisterProperty ("Name", CimType.String, 0);
			RegisterProperty ("OSCreationClassName", CimType.String, 0);
			RegisterProperty ("OSName", CimType.String, 0);
			RegisterProperty ("OSVersion", CimType.String, 0);
			RegisterProperty ("OtherOperationCount", CimType.UInt64, 0); /* */
			RegisterProperty ("OtherTransferCount", CimType.UInt64, 0); /* */
			RegisterProperty ("PageFaults", CimType.UInt32, 0); /* */
			RegisterProperty ("PageFileUsage", CimType.UInt32, 0); /* */
			RegisterProperty ("ParentProcessId", CimType.UInt32, 0); /* */
			RegisterProperty ("PeakPageFileUsage", CimType.UInt32, 0); /* */
			RegisterProperty ("PeakVirtualSize", CimType.UInt64, 0); /* */
			RegisterProperty ("PeakWorkingSetSize", CimType.UInt32, 0); /* */
			RegisterProperty ("Priority", CimType.UInt32, 0);
			RegisterProperty ("ProcessId", CimType.SInt32, 0); /* */
			RegisterProperty ("QuotaNonPagedPoolUsage", CimType.UInt32, 0); /* */
			RegisterProperty ("QuotaPagedPoolUsage", CimType.UInt32, 0); /* */
			RegisterProperty ("QuotaPeakNonPagedPoolUsage", CimType.UInt32, 0); /* */
			RegisterProperty ("QuotaPeakPagedPoolUsage", CimType.UInt32, 0); /* */
			RegisterProperty ("ReadOperationCount", CimType.UInt64, 0); /* */
			RegisterProperty ("ReadTransferCount", CimType.UInt64, 0); /* */
			RegisterProperty ("SessionId", CimType.UInt32, 0); /* */
			RegisterProperty ("Status", CimType.String, 0);
			RegisterProperty ("TerminationDate", CimType.DateTime, 0);
			RegisterProperty ("ThreadCount", CimType.UInt32, 0); /* */
			RegisterProperty ("UserModeTime", CimType.UInt64, 0);
			RegisterProperty ("WorkingSetSize", CimType.UInt64, 0);
			RegisterProperty ("VirtualSize", CimType.UInt64, 0); /* */
		}

		protected override QueryParser Parser { 
			get { return new QueryParser<UNIX_Process> (); } 
		}

		protected override IUnixWbemClassHandler Build (object nativeObj)
		{
			var process = (System.Diagnostics.Process)nativeObj;
			var ret = (UNIX_Process)base.Build (nativeObj);
			var id = SafeGetProcessId (process);
			ret.WithProperty ("CommandLine", ret.GetCommandLine(process,id))
				.WithProperty ("ExecutablePath", ret.GetExecutablePath(process, id))
				.WithProperty ("MaximumWorkingSetSize", (OSHelper.IsUnix ? ret.SafeGetMaxWorkingSetSize(id) : (uint)process.MaxWorkingSet.ToInt32 ()))
				.WithProperty ("MinimumWorkingSetSize", (OSHelper.IsUnix ? ret.SafeGetMinWorkingSetSize(id) : (uint)process.MinWorkingSet.ToInt32 ()))
				.WithProperty ("OSVersion", ret.GetOSVersion (process))
				.WithProperty ("OtherOperationCount", (ulong)0)
				.WithProperty ("OtherTransferCount", (ulong)0)
				.WithProperty ("PageFaults", (uint)(OSHelper.IsUnix ? ret.SafeGetMajorPageFaults (id) + ret.SafeGetMinorPageFaults(id) : 0))
				.WithProperty ("PageFileUsage", (uint)(OSHelper.IsUnix ? ret.SafeGetPagedFileUsage(id) : process.PagedMemorySize64))
				.WithProperty ("ParentProcessId", ret.SafeGetParentProcessId(id))
				.WithProperty ("PeakPageFileUsage", (uint)(OSHelper.IsUnix ? ret.SafeGetMaxWorkingSetSize(id) :process.PeakPagedMemorySize64))
				.WithProperty ("PeakVirtualSize", (OSHelper.IsUnix ? ret.SafeGetProcessVirtualSize(id) : (ulong)process.PeakVirtualMemorySize64))
				.WithProperty ("PeakWorkingSetSize", (uint)(OSHelper.IsUnix ? ret.SafeGetMaxWorkingSetSize(id) :process.PeakWorkingSet64))
				.WithProperty ("ProcessId", id)
				.WithProperty ("QuotaNonPagedPoolUsage", (uint)0)
				.WithProperty ("QuotaPagedPoolUsage", (uint)0)
				.WithProperty ("QuotaPeakNonPagedPoolUsage", (uint)0)
				.WithProperty ("QuotaPeakPagedPoolUsage", (uint)0)
				.WithProperty ("ReadOperationCount", (ulong)0)
				.WithProperty ("ReadTransferCount", (ulong)0)
				.WithProperty ("SessionId", (OSHelper.IsUnix ? ret.SafeGetProcessSessionId(id) : (uint)process.SessionId))
				.WithProperty ("ThreadCount", (OSHelper.IsUnix ? ret.SafeGetThreadsCount(id) : (uint)process.Threads.Count))
				.WithProperty ("VirtualSize", (OSHelper.IsUnix ? ret.SafeGetProcessVirtualSize(id) : (ulong)process.VirtualMemorySize64));

			return ret;
		}

		private string GetExecutablePath (System.Diagnostics.Process process, int id)
		{
			string path = "";
			try {
				if (OSHelper.IsUnix)
				{
					path = SafeGetProcessExecutablePath (id);
				}
				else {
					path = process.StartInfo.FileName;
				}
 			} catch (Exception) {
				path = "";
			}
			return path;
		}

		private string GetCommandLine (System.Diagnostics.Process process, int id)
		{
			string cmd = "";
			try {
				if (OSHelper.IsUnix)
				{
					cmd = SafeGetProcessCommandLine (id);
				}
				else {
					cmd = process.StartInfo.FileName;
					if (!string.IsNullOrEmpty (process.StartInfo.Arguments)) {
						cmd += " " + process.StartInfo.Arguments;
					}
				}
			} catch (Exception) {
				cmd = SafeGetProcessName (id);
			}
			return cmd;
		}

		public string CommandLine { get { return GetPropertyAs<string> ("CommandLine"); } }
		public string ExecutablePath { get { return GetPropertyAs<string> ("ExecutablePath"); } }
		public ulong MaximumWorkingSetSize { get { return GetPropertyAs<ulong> ("MaximumWorkingSetSize"); } }
		public uint MinimumWorkingSetSize { get { return GetPropertyAs<uint> ("MinimumWorkingSetSize"); } }
		public string OSVersion { get { return GetPropertyAs<string> ("OSVersion"); } }
		public ulong OtherOperationCount { get { return GetPropertyAs<ulong> ("OtherOperationCount"); } }
		public ulong OtherTransferCount { get { return GetPropertyAs<ulong> ("OtherTransferCount"); } }
		public uint PageFaults { get { return GetPropertyAs<uint> ("PageFaults"); } }
		public uint PageFileUsage { get { return GetPropertyAs<uint> ("PageFileUsage"); } }
		public int ParentProcessId { get { return GetPropertyAs<int> ("ParentProcessId"); } }
		public uint PeakPageFileUsage { get { return GetPropertyAs<uint> ("PeakPageFileUsage"); } }
		public ulong PeakVirtualSize { get { return GetPropertyAs<ulong> ("PeakVirtualSize"); } }
		public uint PeakWorkingSetSize { get { return GetPropertyAs<uint> ("PeakWorkingSetSize"); } }
		public int ProcessId { get { return GetPropertyAs<int> ("ProcessId"); } }
		public uint QuotaNonPagedPoolUsage { get { return GetPropertyAs<uint> ("QuotaNonPagedPoolUsage"); } }
		public uint QuotaPagedPoolUsage { get { return GetPropertyAs<uint> ("QuotaPagedPoolUsage"); } }
		public uint QuotaPeakNonPagedPoolUsage { get { return GetPropertyAs<uint> ("QuotaPeakNonPagedPoolUsage"); } }
		public uint QuotaPeakPagedPoolUsage { get { return GetPropertyAs<uint> ("QuotaPeakPagedPoolUsage"); } }
		public ulong ReadOperationCount { get { return GetPropertyAs<ulong> ("ReadOperationCount"); } }
		public ulong ReadTransferCount { get { return GetPropertyAs<ulong> ("ReadTransferCount"); } }
		public uint SessionId { get { return GetPropertyAs<uint> ("SessionId"); } }
		public uint ThreadCount { get { return GetPropertyAs<uint> ("ThreadCount"); } }
		public ulong VirtualSize { get { return GetPropertyAs<ulong> ("VirtualSize"); } }


	}
}

