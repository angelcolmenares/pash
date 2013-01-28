namespace Microsoft.PowerShell.Commands.Diagnostics.Common
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class CommonUtilities
    {
        private const long FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
        private const long FORMAT_MESSAGE_FROM_HMODULE = 0x800;
        private const long FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        private const long FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        private const long LOAD_LIBRARY_AS_DATAFILE = 2;

        private CommonUtilities()
        {
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        private static extern long FormatMessage(long dwFlags, IntPtr lpSource, long dwMessageId, long dwLanguageId, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpBuffer, long nSize, IntPtr Arguments);
        public static long FormatMessageFromModule(long lastError, string moduleName, out string msg)
        {
            msg = string.Empty;
            IntPtr zero = IntPtr.Zero;
            zero = LoadLibraryEx(moduleName, IntPtr.Zero, 2);
            if (zero == IntPtr.Zero)
            {
                return (long) Marshal.GetLastWin32Error();
            }
            try
            {
                long dwFlags = 0x1a00;
                long userDefaultLangID = GetUserDefaultLangID();
                if (Marshal.GetLastWin32Error() != 0)
                {
                    userDefaultLangID = 0;
                }
                StringBuilder lpBuffer = new StringBuilder(0x400);
                if (FormatMessage(dwFlags, zero, lastError, userDefaultLangID, lpBuffer, (long) lpBuffer.Capacity, IntPtr.Zero) == 0)
                {
                    return (long) Marshal.GetLastWin32Error();
                }
                msg = lpBuffer.ToString();
                if (msg.EndsWith(Environment.NewLine, StringComparison.Ordinal))
                {
                    msg = msg.Substring(0, msg.Length - 2);
                }
            }
            finally
            {
                FreeLibrary(zero);
            }
            return 0;
        }

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("kernel32.dll", SetLastError=true)]
        private static extern ushort GetUserDefaultLangID();
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        private static extern IntPtr LoadLibraryEx([MarshalAs(UnmanagedType.LPWStr)] string lpFileName, IntPtr hFile, long dwFlags);
        public static string StringArrayToString(IEnumerable input)
        {
            string str = "";
            foreach (string str2 in input)
            {
                str = str + str2 + ", ";
            }
            return str.TrimEnd(new char[0]).TrimEnd(new char[] { ',' });
        }
    }
}

