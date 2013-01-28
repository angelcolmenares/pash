namespace Microsoft.Powershell.Commands.GetCounter.PdhNative
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct CounterHandleNInstance
    {
        public IntPtr hCounter;
        public string InstanceName;
    }
}

