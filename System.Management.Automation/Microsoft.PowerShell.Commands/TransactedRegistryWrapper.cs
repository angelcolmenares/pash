namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal;
    using Microsoft.Win32;
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Provider;
    using System.Security.AccessControl;

    internal class TransactedRegistryWrapper : IRegistryWrapper
    {
        private CmdletProvider provider;
        private TransactedRegistryKey txRegKey;

        internal TransactedRegistryWrapper(TransactedRegistryKey txRegKey, CmdletProvider provider)
        {
            this.txRegKey = txRegKey;
            this.provider = provider;
        }

        public void Close()
        {
            using (this.provider.CurrentPSTransaction)
            {
                this.txRegKey.Close();
            }
        }

        public IRegistryWrapper CreateSubKey(string subkey)
        {
            using (this.provider.CurrentPSTransaction)
            {
                TransactedRegistryKey txRegKey = this.txRegKey.CreateSubKey(subkey);
                if (txRegKey == null)
                {
                    return null;
                }
                return new TransactedRegistryWrapper(txRegKey, this.provider);
            }
        }

        public void DeleteSubKeyTree(string subkey)
        {
            using (this.provider.CurrentPSTransaction)
            {
                this.txRegKey.DeleteSubKeyTree(subkey);
            }
        }

        public void DeleteValue(string name)
        {
            using (this.provider.CurrentPSTransaction)
            {
                this.txRegKey.DeleteValue(name);
            }
        }

        public ObjectSecurity GetAccessControl(AccessControlSections includeSections)
        {
            using (this.provider.CurrentPSTransaction)
            {
                return this.txRegKey.GetAccessControl(includeSections);
            }
        }

        public string[] GetSubKeyNames()
        {
            using (this.provider.CurrentPSTransaction)
            {
                return this.txRegKey.GetSubKeyNames();
            }
        }

        public object GetValue(string name)
        {
            using (this.provider.CurrentPSTransaction)
            {
                object obj2 = this.txRegKey.GetValue(name);
                try
                {
                    obj2 = RegistryWrapperUtils.ConvertValueToUIntFromRegistryIfNeeded(name, obj2, this.GetValueKind(name));
                }
                catch (IOException)
                {
                }
                return obj2;
            }
        }

        public object GetValue(string name, object defaultValue, RegistryValueOptions options)
        {
            using (this.provider.CurrentPSTransaction)
            {
                object obj2 = this.txRegKey.GetValue(name, defaultValue, options);
                try
                {
                    obj2 = RegistryWrapperUtils.ConvertValueToUIntFromRegistryIfNeeded(name, obj2, this.GetValueKind(name));
                }
                catch (IOException)
                {
                }
                return obj2;
            }
        }

        public RegistryValueKind GetValueKind(string name)
        {
            using (this.provider.CurrentPSTransaction)
            {
                return this.txRegKey.GetValueKind(name);
            }
        }

        public string[] GetValueNames()
        {
            using (this.provider.CurrentPSTransaction)
            {
                return this.txRegKey.GetValueNames();
            }
        }

        public IRegistryWrapper OpenSubKey(string name, bool writable)
        {
            using (this.provider.CurrentPSTransaction)
            {
                TransactedRegistryKey txRegKey = this.txRegKey.OpenSubKey(name, writable);
                if (txRegKey == null)
                {
                    return null;
                }
                return new TransactedRegistryWrapper(txRegKey, this.provider);
            }
        }

        public void SetAccessControl(ObjectSecurity securityDescriptor)
        {
            using (this.provider.CurrentPSTransaction)
            {
                this.txRegKey.SetAccessControl((TransactedRegistrySecurity) securityDescriptor);
            }
        }

        public void SetValue(string name, object value)
        {
            using (this.provider.CurrentPSTransaction)
            {
                this.txRegKey.SetValue(name, value);
            }
        }

        public void SetValue(string name, object value, RegistryValueKind valueKind)
        {
            using (this.provider.CurrentPSTransaction)
            {
                value = PSObject.Base(value);
                value = RegistryWrapperUtils.ConvertUIntToValueForRegistryIfNeeded(value, valueKind);
                this.txRegKey.SetValue(name, value, valueKind);
            }
        }

        public string Name
        {
            get
            {
                using (this.provider.CurrentPSTransaction)
                {
                    return this.txRegKey.Name;
                }
            }
        }

        public object RegistryKey
        {
            get
            {
                return this.txRegKey;
            }
        }

        public int SubKeyCount
        {
            get
            {
                using (this.provider.CurrentPSTransaction)
                {
                    return this.txRegKey.SubKeyCount;
                }
            }
        }
    }
}

