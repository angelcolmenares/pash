namespace Microsoft.PowerShell.Commands
{
    using Microsoft.Win32;
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Security.AccessControl;

    internal class RegistryWrapper : IRegistryWrapper
    {
        private Microsoft.Win32.RegistryKey regKey;

        internal RegistryWrapper(Microsoft.Win32.RegistryKey regKey)
        {
            this.regKey = regKey;
        }

        public void Close()
        {
            this.regKey.Close();
        }

        public IRegistryWrapper CreateSubKey(string subkey)
        {
            Microsoft.Win32.RegistryKey regKey = this.regKey.CreateSubKey(subkey);
            if (regKey == null)
            {
                return null;
            }
            return new RegistryWrapper(regKey);
        }

        public void DeleteSubKeyTree(string subkey)
        {
            this.regKey.DeleteSubKeyTree(subkey);
        }

        public void DeleteValue(string name)
        {
            this.regKey.DeleteValue(name);
        }

        public ObjectSecurity GetAccessControl(AccessControlSections includeSections)
        {
            return this.regKey.GetAccessControl(includeSections);
        }

        public string[] GetSubKeyNames()
        {
            return this.regKey.GetSubKeyNames();
        }

        public object GetValue(string name)
        {
            object obj2 = this.regKey.GetValue(name);
            try
            {
                obj2 = RegistryWrapperUtils.ConvertValueToUIntFromRegistryIfNeeded(name, obj2, this.GetValueKind(name));
            }
            catch (IOException)
            {
            }
            return obj2;
        }

        public object GetValue(string name, object defaultValue, RegistryValueOptions options)
        {
            object obj2 = this.regKey.GetValue(name, defaultValue, options);
            try
            {
                obj2 = RegistryWrapperUtils.ConvertValueToUIntFromRegistryIfNeeded(name, obj2, this.GetValueKind(name));
            }
            catch (IOException)
            {
            }
            return obj2;
        }

        public RegistryValueKind GetValueKind(string name)
        {
            return this.regKey.GetValueKind(name);
        }

        public string[] GetValueNames()
        {
            return this.regKey.GetValueNames();
        }

        public IRegistryWrapper OpenSubKey(string name, bool writable)
        {
            Microsoft.Win32.RegistryKey regKey = this.regKey.OpenSubKey(name, writable);
            if (regKey == null)
            {
                return null;
            }
            return new RegistryWrapper(regKey);
        }

        public void SetAccessControl(ObjectSecurity securityDescriptor)
        {
            this.regKey.SetAccessControl((RegistrySecurity) securityDescriptor);
        }

        public void SetValue(string name, object value)
        {
            this.regKey.SetValue(name, value);
        }

        public void SetValue(string name, object value, RegistryValueKind valueKind)
        {
            value = PSObject.Base(value);
            value = RegistryWrapperUtils.ConvertUIntToValueForRegistryIfNeeded(value, valueKind);
            this.regKey.SetValue(name, value, valueKind);
        }

        public string Name
        {
            get
            {
                return this.regKey.Name;
            }
        }

        public object RegistryKey
        {
            get
            {
                return this.regKey;
            }
        }

        public int SubKeyCount
        {
            get
            {
                return this.regKey.SubKeyCount;
            }
        }
    }
}

