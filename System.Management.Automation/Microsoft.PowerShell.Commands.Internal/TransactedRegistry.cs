namespace Microsoft.PowerShell.Commands.Internal
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    internal static class TransactedRegistry
    {
        internal static readonly TransactedRegistryKey ClassesRoot = TransactedRegistryKey.GetBaseKey(BaseRegistryKeys.HKEY_CLASSES_ROOT);
        internal static readonly TransactedRegistryKey CurrentConfig = TransactedRegistryKey.GetBaseKey(BaseRegistryKeys.HKEY_CURRENT_CONFIG);
        internal static readonly TransactedRegistryKey CurrentUser = TransactedRegistryKey.GetBaseKey(BaseRegistryKeys.HKEY_CURRENT_USER);
        internal static readonly TransactedRegistryKey LocalMachine = TransactedRegistryKey.GetBaseKey(BaseRegistryKeys.HKEY_LOCAL_MACHINE);
        private const string resBaseName = "RegistryProviderStrings";
        internal static readonly TransactedRegistryKey Users = TransactedRegistryKey.GetBaseKey(BaseRegistryKeys.HKEY_USERS);
    }
}

