namespace Microsoft.PowerShell.Commands.Internal
{
    using System;

    internal sealed class BaseRegistryKeys
    {
        internal static readonly IntPtr HKEY_CLASSES_ROOT = new IntPtr(-2147483648);
        internal static readonly IntPtr HKEY_CURRENT_CONFIG = new IntPtr(-2147483643);
        internal static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(-2147483647);
        internal static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(-2147483646);
        internal static readonly IntPtr HKEY_USERS = new IntPtr(-2147483645);
    }
}

