using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

namespace Microsoft.PowerShell.Commands
{
	internal static class ProcessNativeMethods
	{
		internal readonly static IntPtr INVALID_HANDLE_VALUE;

		internal static int GENERIC_READ;

		internal static int GENERIC_WRITE;

		internal static int FILE_ATTRIBUTE_NORMAL;

		internal static int CREATE_ALWAYS;

		internal static int FILE_SHARE_WRITE;

		internal static int FILE_SHARE_READ;

		internal static int OF_READWRITE;

		internal static int OPEN_EXISTING;

		static ProcessNativeMethods()
		{
			ProcessNativeMethods.INVALID_HANDLE_VALUE = IntPtr.Zero;
			ProcessNativeMethods.GENERIC_READ = -2147483648;
			ProcessNativeMethods.GENERIC_WRITE = 0x40000000;
			ProcessNativeMethods.FILE_ATTRIBUTE_NORMAL = -2147483648;
			ProcessNativeMethods.CREATE_ALWAYS = 2;
			ProcessNativeMethods.FILE_SHARE_WRITE = 2;
			ProcessNativeMethods.FILE_SHARE_READ = 1;
			ProcessNativeMethods.OF_READWRITE = 2;
			ProcessNativeMethods.OPEN_EXISTING = 3;
		}

		[DllImport("Kernel32.dll", CharSet=CharSet.Unicode)]
		public static extern IntPtr CreateFileW(string lpFileName, int dwDesiredAccess, int dwShareMode, ProcessNativeMethods.SECURITY_ATTRIBUTES lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

		[DllImport("kernel32.dll", CharSet=CharSet.Auto)]
		public static extern bool CreateProcess(string lpApplicationName, StringBuilder lpCommandLine, ProcessNativeMethods.SECURITY_ATTRIBUTES lpProcessAttributes, ProcessNativeMethods.SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, int dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ProcessNativeMethods.STARTUPINFO lpStartupInfo, SafeNativeMethods.PROCESS_INFORMATION lpProcessInformation);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		internal static extern bool CreateProcessWithLogonW(string userName, string domain, IntPtr password, ProcessNativeMethods.LogonFlags logonFlags, string appName, StringBuilder cmdLine, int creationFlags, IntPtr environmentBlock, string lpCurrentDirectory, ProcessNativeMethods.STARTUPINFO lpStartupInfo, SafeNativeMethods.PROCESS_INFORMATION lpProcessInformation);

		[DllImport("kernel32.dll", CharSet=CharSet.Ansi)]
		public static extern IntPtr GetStdHandle(int whichHandle);

		[Flags]
		internal enum LogonFlags
		{
			LOGON_WITH_PROFILE = 1,
			LOGON_NETCREDENTIALS_ONLY = 2
		}

		internal sealed class SafeLocalMemHandle : SafeHandleZeroOrMinusOneIsInvalid
		{
			internal SafeLocalMemHandle() : base(true)
			{
			}

			[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
			internal SafeLocalMemHandle(IntPtr existingHandle, bool ownsHandle) : base(ownsHandle)
			{
				base.SetHandle(existingHandle);
			}

			[DllImport("kernel32.dll", CharSet=CharSet.None)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			private static extern IntPtr LocalFree(IntPtr hMem);

			protected override bool ReleaseHandle()
			{
				return ProcessNativeMethods.SafeLocalMemHandle.LocalFree(this.handle) == IntPtr.Zero;
			}
		}

		internal class SECURITY_ATTRIBUTES
		{
			public int nLength;

			public ProcessNativeMethods.SafeLocalMemHandle lpSecurityDescriptor;

			public bool bInheritHandle;

			public SECURITY_ATTRIBUTES()
			{
				this.nLength = 12;
				this.bInheritHandle = true;
				this.lpSecurityDescriptor = new ProcessNativeMethods.SafeLocalMemHandle(IntPtr.Zero, true);
			}
		}

		internal class STARTUPINFO
		{
			public int cb;

			public IntPtr lpReserved;

			public IntPtr lpDesktop;

			public IntPtr lpTitle;

			public int dwX;

			public int dwY;

			public int dwXSize;

			public int dwYSize;

			public int dwXCountChars;

			public int dwYCountChars;

			public int dwFillAttribute;

			public int dwFlags;

			public short wShowWindow;

			public short cbReserved2;

			public IntPtr lpReserved2;

			public SafeFileHandle hStdInput;

			public SafeFileHandle hStdOutput;

			public SafeFileHandle hStdError;

			public STARTUPINFO()
			{
				this.lpReserved = IntPtr.Zero;
				this.lpDesktop = IntPtr.Zero;
				this.lpTitle = IntPtr.Zero;
				this.lpReserved2 = IntPtr.Zero;
				this.hStdInput = new SafeFileHandle(IntPtr.Zero, false);
				this.hStdOutput = new SafeFileHandle(IntPtr.Zero, false);
				this.hStdError = new SafeFileHandle(IntPtr.Zero, false);
				this.cb = Marshal.SizeOf(this);
			}

			public void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (this.hStdInput != null && !this.hStdInput.IsInvalid)
					{
						this.hStdInput.Close();
						this.hStdInput = null;
					}
					if (this.hStdOutput != null && !this.hStdOutput.IsInvalid)
					{
						this.hStdOutput.Close();
						this.hStdOutput = null;
					}
					if (this.hStdError != null && !this.hStdError.IsInvalid)
					{
						this.hStdError.Close();
						this.hStdError = null;
					}
				}
			}

			public void Dispose()
			{
				this.Dispose(true);
			}
		}
	}
}