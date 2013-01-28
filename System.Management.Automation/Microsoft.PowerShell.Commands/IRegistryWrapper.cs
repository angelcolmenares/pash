namespace Microsoft.PowerShell.Commands
{
    using Microsoft.Win32;
    using System;
    using System.Security.AccessControl;

    internal interface IRegistryWrapper
    {
        void Close();
        IRegistryWrapper CreateSubKey(string subkey);
        void DeleteSubKeyTree(string subkey);
        void DeleteValue(string name);
        ObjectSecurity GetAccessControl(AccessControlSections includeSections);
        string[] GetSubKeyNames();
        object GetValue(string name);
        object GetValue(string name, object defaultValue, RegistryValueOptions options);
        RegistryValueKind GetValueKind(string name);
        string[] GetValueNames();
        IRegistryWrapper OpenSubKey(string name, bool writable);
        void SetAccessControl(ObjectSecurity securityDescriptor);
        void SetValue(string name, object value);
        void SetValue(string name, object value, RegistryValueKind valueKind);

        string Name { get; }

        object RegistryKey { get; }

        int SubKeyCount { get; }
    }
}

