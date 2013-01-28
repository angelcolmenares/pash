namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Runtime.InteropServices;

    internal class NewObjectNativeMethods
    {
        private NewObjectNativeMethods()
        {
        }

        [DllImport("ole32.dll")]
        internal static extern int CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] string lpszProgID, out Guid pclsid);
    }
}

