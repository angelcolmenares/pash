namespace Microsoft.PowerShell.Commands.Internal
{
    using System;
    using System.Runtime.InteropServices;

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("79427A2B-F895-40e0-BE79-B57DC82ED231")]
    internal interface IKernelTransaction
    {
        int GetHandle(out IntPtr pHandle);
    }
}

