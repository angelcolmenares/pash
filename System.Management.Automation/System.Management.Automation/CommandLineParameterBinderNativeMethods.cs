namespace System.Management.Automation
{
    using System;
    using System.Runtime.InteropServices;

    internal static class CommandLineParameterBinderNativeMethods
    {
        [DllImport("shell32.dll", SetLastError=true)]
        private static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);
        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);
        public static string[] PreParseCommandLine(string commandLine)
        {
            string[] strArray2;
            int pNumArgs = 0;
            IntPtr ptr = CommandLineToArgvW(commandLine, out pNumArgs);
            if (ptr == IntPtr.Zero)
            {
                return null;
            }
            try
            {
                string[] strArray = new string[pNumArgs - 1];
                for (int i = 1; i < pNumArgs; i++)
                {
                    strArray[i - 1] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(ptr, i * IntPtr.Size));
                }
                strArray2 = strArray;
            }
            finally
            {
                LocalFree(ptr);
            }
            return strArray2;
        }
    }
}

