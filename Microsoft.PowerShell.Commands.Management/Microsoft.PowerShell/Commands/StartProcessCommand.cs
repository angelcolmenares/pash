using Microsoft.PowerShell.Commands.Management;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Start", "Process", DefaultParameterSetName="Default", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135261")]
	[OutputType(new Type[] { typeof(Process) })]
	public sealed class StartProcessCommand : PSCmdlet
	{
		private ManualResetEvent waithandle;

		private bool IsDefaultSetParameterSpecified;

		private string _path;

		private string[] _argumentlist;

		private PSCredential _credential;

		private string _workingdirectory;

		private SwitchParameter _loaduserprofile;

		private SwitchParameter _nonewwindow;

		private SwitchParameter _passthru;

		private string _redirectstandarderror;

		private string _redirectstandardinput;

		private string _redirectstandardoutput;

		private string _verb;

		private SwitchParameter _wait;

		private ProcessWindowStyle _windowstyle;

		private bool _windowstyleSpecified;

		private SwitchParameter _UseNewEnvironment;

		[Alias(new string[] { "Args" })]
		[Parameter(Position=1)]
		[ValidateNotNullOrEmpty]
		public string[] ArgumentList
		{
			get
			{
				return this._argumentlist;
			}
			set
			{
				this._argumentlist = value;
			}
		}

		[Alias(new string[] { "RunAs" })]
		[Credential]
		[Parameter(ParameterSetName="Default")]
		[ValidateNotNullOrEmpty]
		public PSCredential Credential
		{
			get
			{
				return this._credential;
			}
			set
			{
				this._credential = value;
				this.IsDefaultSetParameterSpecified = true;
			}
		}

		[Alias(new string[] { "PSPath" })]
		[Parameter(Mandatory=true, Position=0)]
		[ValidateNotNullOrEmpty]
		public string FilePath
		{
			get
			{
				return this._path;
			}
			set
			{
				this._path = value;
			}
		}

		[Alias(new string[] { "Lup" })]
		[Parameter(ParameterSetName="Default")]
		public SwitchParameter LoadUserProfile
		{
			get
			{
				return this._loaduserprofile;
			}
			set
			{
				this._loaduserprofile = value;
				this.IsDefaultSetParameterSpecified = true;
			}
		}

		[Alias(new string[] { "nnw" })]
		[Parameter(ParameterSetName="Default")]
		public SwitchParameter NoNewWindow
		{
			get
			{
				return this._nonewwindow;
			}
			set
			{
				this._nonewwindow = value;
				this.IsDefaultSetParameterSpecified = true;
			}
		}

		[Parameter]
		public SwitchParameter PassThru
		{
			get
			{
				return this._passthru;
			}
			set
			{
				this._passthru = value;
			}
		}

		[Alias(new string[] { "RSE" })]
		[Parameter(ParameterSetName="Default")]
		[ValidateNotNullOrEmpty]
		public string RedirectStandardError
		{
			get
			{
				return this._redirectstandarderror;
			}
			set
			{
				this._redirectstandarderror = value;
				this.IsDefaultSetParameterSpecified = true;
			}
		}

		[Alias(new string[] { "RSI" })]
		[Parameter(ParameterSetName="Default")]
		[ValidateNotNullOrEmpty]
		public string RedirectStandardInput
		{
			get
			{
				return this._redirectstandardinput;
			}
			set
			{
				this._redirectstandardinput = value;
				this.IsDefaultSetParameterSpecified = true;
			}
		}

		[Alias(new string[] { "RSO" })]
		[Parameter(ParameterSetName="Default")]
		[ValidateNotNullOrEmpty]
		public string RedirectStandardOutput
		{
			get
			{
				return this._redirectstandardoutput;
			}
			set
			{
				this._redirectstandardoutput = value;
				this.IsDefaultSetParameterSpecified = true;
			}
		}

		[Parameter(ParameterSetName="Default")]
		public SwitchParameter UseNewEnvironment
		{
			get
			{
				return this._UseNewEnvironment;
			}
			set
			{
				this._UseNewEnvironment = value;
				this.IsDefaultSetParameterSpecified = true;
			}
		}

		[Parameter(ParameterSetName="UseShellExecute")]
		[ValidateNotNullOrEmpty]
		public string Verb
		{
			get
			{
				return this._verb;
			}
			set
			{
				this._verb = value;
			}
		}

		[Parameter]
		public SwitchParameter Wait
		{
			get
			{
				return this._wait;
			}
			set
			{
				this._wait = value;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public ProcessWindowStyle WindowStyle
		{
			get
			{
				return this._windowstyle;
			}
			set
			{
				this._windowstyle = value;
				this._windowstyleSpecified = true;
			}
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public string WorkingDirectory
		{
			get
			{
				return this._workingdirectory;
			}
			set
			{
				this._workingdirectory = value;
			}
		}

		public StartProcessCommand()
		{
			this._loaduserprofile = SwitchParameter.Present;
		}

		protected override void BeginProcessing()
		{
			string str;
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.ErrorDialog = false;
			try
			{
				CommandInfo commandInfo = CommandDiscovery.LookupCommandInfo(this._path, CommandTypes.ExternalScript | CommandTypes.Application, SearchResolutionOptions.None, CommandOrigin.Internal, base.Context);
				processStartInfo.FileName = commandInfo.Definition;
			}
			catch (CommandNotFoundException commandNotFoundException)
			{
				processStartInfo.FileName = this._path;
			}
			if (this._argumentlist != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				string[] strArrays = this._argumentlist;
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str1 = strArrays[i];
					stringBuilder.Append(str1);
					stringBuilder.Append(' ');
				}
				processStartInfo.Arguments = stringBuilder.ToString();
			}
			if (this._workingdirectory == null)
			{
				processStartInfo.WorkingDirectory = this.ResolveFilePath(base.SessionState.Path.CurrentFileSystemLocation.Path);
			}
			else
			{
				this._workingdirectory = this.ResolveFilePath(this._workingdirectory);
				if (Directory.Exists(this._workingdirectory))
				{
					processStartInfo.WorkingDirectory = this._workingdirectory;
				}
				else
				{
					str = StringUtil.Format(ProcessResources.InvalidInput, "WorkingDirectory");
					ErrorRecord errorRecord = new ErrorRecord(new DirectoryNotFoundException(str), "DirectoryNotFoundException", ErrorCategory.InvalidOperation, null);
					base.WriteError(errorRecord);
					return;
				}
			}
			if (!base.ParameterSetName.Equals("Default"))
			{
				if (base.ParameterSetName.Equals("UseShellExecute"))
				{
					processStartInfo.UseShellExecute = true;
					if (this._verb != null)
					{
						processStartInfo.Verb = this._verb;
					}
					processStartInfo.WindowStyle = this._windowstyle;
				}
			}
			else
			{
				if (this.IsDefaultSetParameterSpecified)
				{
					processStartInfo.UseShellExecute = false;
				}
				if (this._UseNewEnvironment)
				{
					processStartInfo.EnvironmentVariables.Clear();
					this.LoadEnvironmentVariable(processStartInfo, Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine));
					this.LoadEnvironmentVariable(processStartInfo, Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User));
				}
				if (!this._nonewwindow || !this._windowstyleSpecified)
				{
					if (this._nonewwindow)
					{
						processStartInfo.CreateNoWindow = this._nonewwindow;
					}
					processStartInfo.WindowStyle = this._windowstyle;
					processStartInfo.LoadUserProfile = this._loaduserprofile;
					if (this._credential != null)
					{
						NetworkCredential networkCredential = this._credential.GetNetworkCredential();
						processStartInfo.UserName = networkCredential.UserName;
						if (!string.IsNullOrEmpty(networkCredential.Domain))
						{
							processStartInfo.Domain = networkCredential.Domain;
						}
						else
						{
							processStartInfo.Domain = ".";
						}
						processStartInfo.Password = this._credential.Password;
					}
					if (this._redirectstandardinput != null)
					{
						this._redirectstandardinput = this.ResolveFilePath(this._redirectstandardinput);
						if (!File.Exists(this._redirectstandardinput))
						{
							str = StringUtil.Format(ProcessResources.InvalidInput, string.Concat("RedirectStandardInput '", this.RedirectStandardInput, "'"));
							ErrorRecord errorRecord1 = new ErrorRecord(new FileNotFoundException(str), "FileNotFoundException", ErrorCategory.InvalidOperation, null);
							base.WriteError(errorRecord1);
							return;
						}
					}
					if (this._redirectstandardinput != null && this._redirectstandardoutput != null)
					{
						this._redirectstandardinput = this.ResolveFilePath(this._redirectstandardinput);
						this._redirectstandardoutput = this.ResolveFilePath(this._redirectstandardoutput);
						if (this._redirectstandardinput.Equals(this._redirectstandardoutput, StringComparison.CurrentCultureIgnoreCase))
						{
							str = StringUtil.Format(ProcessResources.DuplicateEntry, "RedirectStandardInput", "RedirectStandardOutput");
							ErrorRecord errorRecord2 = new ErrorRecord(new InvalidOperationException(str), "InvalidOperationException", ErrorCategory.InvalidOperation, null);
							base.WriteError(errorRecord2);
							return;
						}
					}
					if (this._redirectstandardinput != null && this._redirectstandarderror != null)
					{
						this._redirectstandardinput = this.ResolveFilePath(this._redirectstandardinput);
						this._redirectstandarderror = this.ResolveFilePath(this._redirectstandarderror);
						if (this._redirectstandardinput.Equals(this._redirectstandarderror, StringComparison.CurrentCultureIgnoreCase))
						{
							str = StringUtil.Format(ProcessResources.DuplicateEntry, "RedirectStandardInput", "RedirectStandardError");
							ErrorRecord errorRecord3 = new ErrorRecord(new InvalidOperationException(str), "InvalidOperationException", ErrorCategory.InvalidOperation, null);
							base.WriteError(errorRecord3);
							return;
						}
					}
					if (this._redirectstandardoutput != null && this._redirectstandarderror != null)
					{
						this._redirectstandarderror = this.ResolveFilePath(this._redirectstandarderror);
						this._redirectstandardoutput = this.ResolveFilePath(this._redirectstandardoutput);
						if (this._redirectstandardoutput.Equals(this._redirectstandarderror, StringComparison.CurrentCultureIgnoreCase))
						{
							str = StringUtil.Format(ProcessResources.DuplicateEntry, "RedirectStandardOutput", "RedirectStandardError");
							ErrorRecord errorRecord4 = new ErrorRecord(new InvalidOperationException(str), "InvalidOperationException", ErrorCategory.InvalidOperation, null);
							base.WriteError(errorRecord4);
							return;
						}
					}
				}
				else
				{
					str = StringUtil.Format(ProcessResources.ContradictParametersSpecified, "-NoNewWindow", "-WindowStyle");
					ErrorRecord errorRecord5 = new ErrorRecord(new InvalidOperationException(str), "InvalidOperationException", ErrorCategory.InvalidOperation, null);
					base.WriteError(errorRecord5);
					return;
				}
			}
			Process process = this.start(processStartInfo);
			if (this._passthru.IsPresent)
			{
				if (process == null)
				{
					str = StringUtil.Format(ProcessResources.CannotStarttheProcess, new object[0]);
					ErrorRecord errorRecord6 = new ErrorRecord(new InvalidOperationException(str), "InvalidOperationException", ErrorCategory.InvalidOperation, null);
					base.ThrowTerminatingError(errorRecord6);
				}
				else
				{
					base.WriteObject(process);
				}
			}
			if (this._wait.IsPresent)
			{
				if (process == null)
				{
					str = StringUtil.Format(ProcessResources.CannotStarttheProcess, new object[0]);
					ErrorRecord errorRecord7 = new ErrorRecord(new InvalidOperationException(str), "InvalidOperationException", ErrorCategory.InvalidOperation, null);
					base.ThrowTerminatingError(errorRecord7);
				}
				else
				{
					process.EnableRaisingEvents = true;
					if (!process.HasExited)
					{
						ProcessCollection processCollection = new ProcessCollection(process);
						processCollection.Start();
						processCollection.WaitOne();
						return;
					}
				}
			}
		}

		private static StringBuilder BuildCommandLine(string executableFileName, string arguments)
		{
			bool flag;
			StringBuilder stringBuilder = new StringBuilder();
			string str = executableFileName.Trim();
			if (!str.StartsWith("\"", StringComparison.Ordinal))
			{
				flag = false;
			}
			else
			{
				flag = str.EndsWith("\"", StringComparison.Ordinal);
			}
			bool flag1 = flag;
			if (!flag1)
			{
				stringBuilder.Append("\"");
			}
			stringBuilder.Append(str);
			if (!flag1)
			{
				stringBuilder.Append("\"");
			}
			if (!string.IsNullOrEmpty(arguments))
			{
				stringBuilder.Append(" ");
				stringBuilder.Append(arguments);
			}
			return stringBuilder;
		}

		private SafeFileHandle GetSafeFileHandleForRedirection(string RedirectionPath, int dwCreationDisposition)
		{
			ProcessNativeMethods.SECURITY_ATTRIBUTES sECURITYATTRIBUTE = new ProcessNativeMethods.SECURITY_ATTRIBUTES();
			IntPtr intPtr = ProcessNativeMethods.CreateFileW(RedirectionPath, ProcessNativeMethods.GENERIC_READ | ProcessNativeMethods.GENERIC_WRITE, ProcessNativeMethods.FILE_SHARE_WRITE | ProcessNativeMethods.FILE_SHARE_READ, sECURITYATTRIBUTE, dwCreationDisposition, ProcessNativeMethods.FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);
			if (intPtr == IntPtr.Zero)
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				Win32Exception win32Exception = new Win32Exception(lastWin32Error);
				string str = StringUtil.Format(ProcessResources.InvalidStartProcess, win32Exception.Message);
				ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str), "InvalidOperationException", ErrorCategory.InvalidOperation, null);
				base.ThrowTerminatingError(errorRecord);
			}
			SafeFileHandle safeFileHandle = new SafeFileHandle(intPtr, true);
			return safeFileHandle;
		}

		private void LoadEnvironmentVariable(ProcessStartInfo startinfo, IDictionary EnvironmentVariables)
		{
			foreach (DictionaryEntry environmentVariable in EnvironmentVariables)
			{
				if (startinfo.EnvironmentVariables.ContainsKey(environmentVariable.Key.ToString()))
				{
					startinfo.EnvironmentVariables.Remove(environmentVariable.Key.ToString());
				}
				if (!environmentVariable.Key.ToString().Equals("PATH"))
				{
					startinfo.EnvironmentVariables.Add(environmentVariable.Key.ToString(), environmentVariable.Value.ToString());
				}
				else
				{
					startinfo.EnvironmentVariables.Add(environmentVariable.Key.ToString(), string.Concat(Environment.GetEnvironmentVariable(environmentVariable.Key.ToString(), EnvironmentVariableTarget.Machine), ";", Environment.GetEnvironmentVariable(environmentVariable.Key.ToString(), EnvironmentVariableTarget.User)));
				}
			}
		}

		private string ResolveFilePath(string path)
		{
			string str = PathUtils.ResolveFilePath(path, this);
			return str;
		}

		private Process start(ProcessStartInfo startInfo)
		{
			int num;
			Process processById = null;
			if (!startInfo.UseShellExecute)
			{
				num = this.StartWithCreateProcess(startInfo);
			}
			else
			{
				num = this.StartWithShellExecute(startInfo);
			}
			if (num != -1)
			{
				processById = Process.GetProcessById(num);
			}
			return processById;
		}

		private int StartWithCreateProcess(ProcessStartInfo startinfo)
		{
			string str;
			bool flag;
			ProcessNativeMethods.STARTUPINFO sTARTUPINFO = new ProcessNativeMethods.STARTUPINFO();
			SafeNativeMethods.PROCESS_INFORMATION pROCESSINFORMATION = new SafeNativeMethods.PROCESS_INFORMATION();
			int lastWin32Error = 0;
			GCHandle gCHandle = new GCHandle();
			StringBuilder stringBuilder = StartProcessCommand.BuildCommandLine(startinfo.FileName, startinfo.Arguments);
			try
			{
				if (this._redirectstandardinput == null)
				{
					sTARTUPINFO.hStdInput = new SafeFileHandle(ProcessNativeMethods.GetStdHandle(-10), false);
				}
				else
				{
					startinfo.RedirectStandardInput = true;
					this._redirectstandardinput = this.ResolveFilePath(this._redirectstandardinput);
					sTARTUPINFO.hStdInput = this.GetSafeFileHandleForRedirection(this._redirectstandardinput, ProcessNativeMethods.OPEN_EXISTING);
				}
				if (this._redirectstandardoutput == null)
				{
					sTARTUPINFO.hStdOutput = new SafeFileHandle(ProcessNativeMethods.GetStdHandle(-11), false);
				}
				else
				{
					startinfo.RedirectStandardOutput = true;
					this._redirectstandardoutput = this.ResolveFilePath(this._redirectstandardoutput);
					sTARTUPINFO.hStdOutput = this.GetSafeFileHandleForRedirection(this._redirectstandardoutput, ProcessNativeMethods.CREATE_ALWAYS);
				}
				if (this._redirectstandarderror == null)
				{
					sTARTUPINFO.hStdError = new SafeFileHandle(ProcessNativeMethods.GetStdHandle(-12), false);
				}
				else
				{
					startinfo.RedirectStandardError = true;
					this._redirectstandarderror = this.ResolveFilePath(this._redirectstandarderror);
					sTARTUPINFO.hStdError = this.GetSafeFileHandleForRedirection(this._redirectstandarderror, ProcessNativeMethods.CREATE_ALWAYS);
				}
				sTARTUPINFO.dwFlags = 0x100;
				int num = 0;
				if (!startinfo.CreateNoWindow)
				{
					num = num | 16;
					ProcessNativeMethods.STARTUPINFO sTARTUPINFO1 = sTARTUPINFO;
					sTARTUPINFO1.dwFlags = sTARTUPINFO1.dwFlags | 1;
					ProcessWindowStyle windowStyle = startinfo.WindowStyle;
					switch (windowStyle)
					{
						case ProcessWindowStyle.Normal:
						{
							sTARTUPINFO.wShowWindow = 1;
							break;
						}
						case ProcessWindowStyle.Hidden:
						{
							sTARTUPINFO.wShowWindow = 0;
							break;
						}
						case ProcessWindowStyle.Minimized:
						{
							sTARTUPINFO.wShowWindow = 2;
							break;
						}
						case ProcessWindowStyle.Maximized:
						{
							sTARTUPINFO.wShowWindow = 3;
							break;
						}
					}
				}
				else
				{
					num = 0;
				}
				IntPtr zero = IntPtr.Zero;
				if (startinfo.EnvironmentVariables != null && this.UseNewEnvironment)
				{
					bool flag1 = false;
					if (StartProcessCommand.ProcessManager.IsNt)
					{
						num = num | 0x400;
						flag1 = true;
					}
					gCHandle = GCHandle.Alloc(StartProcessCommand.EnvironmentBlock.ToByteArray(startinfo.EnvironmentVariables, flag1), GCHandleType.Pinned);
					zero = gCHandle.AddrOfPinnedObject();
				}
				if (this._credential == null)
				{
					ProcessNativeMethods.SECURITY_ATTRIBUTES sECURITYATTRIBUTE = new ProcessNativeMethods.SECURITY_ATTRIBUTES();
					ProcessNativeMethods.SECURITY_ATTRIBUTES sECURITYATTRIBUTE1 = new ProcessNativeMethods.SECURITY_ATTRIBUTES();
					flag = ProcessNativeMethods.CreateProcess(null, stringBuilder, sECURITYATTRIBUTE, sECURITYATTRIBUTE1, true, num, zero, startinfo.WorkingDirectory, sTARTUPINFO, pROCESSINFORMATION);
					if (!flag)
					{
						lastWin32Error = Marshal.GetLastWin32Error();
					}
					if (!flag)
					{
						Win32Exception win32Exception = new Win32Exception(lastWin32Error);
						str = StringUtil.Format(ProcessResources.InvalidStartProcess, win32Exception.Message);
						ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str), "InvalidOperationException", ErrorCategory.InvalidOperation, null);
						base.ThrowTerminatingError(errorRecord);
					}
				}
				else
				{
					ProcessNativeMethods.LogonFlags logonFlag = 0;
					if (startinfo.LoadUserProfile)
					{
						logonFlag = ProcessNativeMethods.LogonFlags.LOGON_WITH_PROFILE;
					}
					IntPtr coTaskMemUnicode = IntPtr.Zero;
					try
					{
						if (startinfo.Password != null)
						{
							coTaskMemUnicode = Marshal.SecureStringToCoTaskMemUnicode(startinfo.Password);
						}
						else
						{
							coTaskMemUnicode = Marshal.StringToCoTaskMemUni(string.Empty);
						}
						flag = ProcessNativeMethods.CreateProcessWithLogonW(startinfo.UserName, startinfo.Domain, coTaskMemUnicode, logonFlag, null, stringBuilder, num, zero, startinfo.WorkingDirectory, sTARTUPINFO, pROCESSINFORMATION);
						if (!flag)
						{
							lastWin32Error = Marshal.GetLastWin32Error();
						}
						if (!flag)
						{
							if (lastWin32Error == 193)
							{
								str = StringUtil.Format(ProcessResources.InvalidApplication, this._path);
								ErrorRecord errorRecord1 = new ErrorRecord(new InvalidOperationException(str), "InvalidOperationException", ErrorCategory.InvalidOperation, null);
								base.ThrowTerminatingError(errorRecord1);
							}
							Win32Exception win32Exception1 = new Win32Exception(lastWin32Error);
							str = StringUtil.Format(ProcessResources.InvalidStartProcess, win32Exception1.Message);
							ErrorRecord errorRecord2 = new ErrorRecord(new InvalidOperationException(str), "InvalidOperationException", ErrorCategory.InvalidOperation, null);
							base.ThrowTerminatingError(errorRecord2);
						}
					}
					finally
					{
						if (coTaskMemUnicode != IntPtr.Zero)
						{
							Marshal.ZeroFreeCoTaskMemUnicode(coTaskMemUnicode);
						}
					}
				}
			}
			finally
			{
				if (gCHandle.IsAllocated)
				{
					gCHandle.Free();
				}
				sTARTUPINFO.Dispose();
			}
			return pROCESSINFORMATION.dwProcessId;
		}

		private int StartWithShellExecute(ProcessStartInfo startInfo)
		{
			int id = -1;
			try
			{
				Process process = Process.Start(startInfo);
				if (process != null)
				{
					id = process.Id;
				}
			}
			catch (Win32Exception win32Exception1)
			{
				Win32Exception win32Exception = win32Exception1;
				string str = StringUtil.Format(ProcessResources.InvalidStartProcess, win32Exception.Message);
				ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str), "InvalidOperationException", ErrorCategory.InvalidOperation, null);
				base.ThrowTerminatingError(errorRecord);
			}
			return id;
		}

		protected override void StopProcessing()
		{
			if (this.waithandle != null)
			{
				this.waithandle.Set();
			}
		}

		internal static class EnvironmentBlock
		{
			public static byte[] ToByteArray(StringDictionary sd, bool unicode)
			{
				byte[] bytes;
				string[] strArrays = new string[sd.Count];
				sd.Keys.CopyTo(strArrays, 0);
				string[] strArrays1 = new string[sd.Count];
				sd.Values.CopyTo(strArrays1, 0);
				Array.Sort<string, string>(strArrays, strArrays1, StringComparer.OrdinalIgnoreCase);
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < sd.Count; i++)
				{
					stringBuilder.Append(strArrays[i]);
					stringBuilder.Append('=');
					stringBuilder.Append(strArrays1[i]);
					stringBuilder.Append('\0');
				}
				stringBuilder.Append('\0');
				if (!unicode)
				{
					bytes = Encoding.Default.GetBytes(stringBuilder.ToString());
				}
				else
				{
					bytes = Encoding.Unicode.GetBytes(stringBuilder.ToString());
				}
				if ((int)bytes.Length <= 0xffff)
				{
					return bytes;
				}
				else
				{
					throw new InvalidOperationException("EnvironmentBlockTooLong");
				}
			}
		}

		internal static class ProcessManager
		{
			public static bool IsNt
			{
				get
				{
					return Environment.OSVersion.Platform == PlatformID.Win32NT;
				}
			}

		}
	}
}