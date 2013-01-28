using System;
using System.Runtime.InteropServices;

namespace System.Management.Automation
{
	public static class OSHelper
	{
		public static PlatformID Platform {
			get { return Environment.OSVersion.Platform; }
		}

		public static bool IsUnix 
		{
			get { return Platform == PlatformID.MacOSX || Platform == PlatformID.Unix; }
		}

		public static bool IsMacOSX
		{
			get { return IsUnix && Kernel == KernelVersion.MacOSX; }
		}

		public static bool IsWindows {
			get { return !IsUnix; }
		}

		private static KernelVersion _kernel;

		public static KernelVersion Kernel {
			get {
				if (_kernel == KernelVersion.Undefined)
				{
					var str = DetectUnixKernel ();
					switch(str) {
						case "Linux":
							_kernel = KernelVersion.Linux;
							break;
						case "FreeBSD":
							_kernel = KernelVersion.FreeBSD;
							break;
						case "Darwin":
							_kernel = KernelVersion.MacOSX;
							break;

						default:
							_kernel = KernelVersion.Unix;
							break;
					}
				}
				return _kernel;
			}
		}

		#region private static string DetectUnixKernel()
		 
         [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
         struct utsname
         {
	             [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	             public string sysname;
	 
	             [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	             public string nodename;
	 
	             [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	             public string release;
	 
	             [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	             public string version;
	 
	             [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	             public string machine;
	 
	             [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
	             public string extraJustInCase;
	 
         }
		 
         private static string DetectUnixKernel()
         {
	             utsname uts = new utsname();
	             uname(out uts);
	 			 /*
		         Debug.WriteLine("System:");
	             Debug.Indent();
	             Debug.WriteLine(uts.sysname);
	             Debug.WriteLine(uts.nodename);
	             Debug.WriteLine(uts.release);
	             Debug.WriteLine(uts.version);
	             Debug.WriteLine(uts.machine);
	             Debug.Unindent();
	             */
	 
	            return uts.sysname.ToString();
		}
	
     	[DllImport("libc")]
        private static extern void uname(out utsname uname_struct);
		 
         #endregion

		public enum KernelVersion
		{
			Undefined,
			Linux,
			FreeBSD,
			Unix,
			MacOSX
		}
	}


}

