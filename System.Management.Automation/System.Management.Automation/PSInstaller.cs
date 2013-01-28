namespace System.Management.Automation
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration.Install;

    public abstract class PSInstaller : Installer
    {
        protected PSInstaller()
        {
        }

        private RegistryKey GetRegistryKey(string keyPath)
        {
            RegistryKey rootHive = GetRootHive(keyPath);
            if (rootHive == null)
            {
                return null;
            }
            return rootHive.CreateSubKey(GetSubkeyPath(keyPath));
        }

        private static RegistryKey GetRootHive(string keyPath)
        {
            string str;
            int index = keyPath.IndexOf('\\');
            if (index > 0)
            {
                str = keyPath.Substring(0, index);
            }
            else
            {
                str = keyPath;
            }
            switch (str.ToUpperInvariant())
            {
                case "HKEY_CURRENT_USER":
                    return Registry.CurrentUser;

                case "HKEY_LOCAL_MACHINE":
                    return Registry.LocalMachine;

                case "HKEY_CLASSES_ROOT":
                    return Registry.ClassesRoot;

                case "HKEY_CURRENT_CONFIG":
                    return Registry.CurrentConfig;

                case "HKEY_PERFORMANCE_DATA":
                    return Registry.PerformanceData;

                case "HKEY_USERS":
                    return Registry.Users;
            }
            return null;
        }

        private static string GetSubkeyPath(string keyPath)
        {
            int index = keyPath.IndexOf('\\');
            if (index > 0)
            {
                return keyPath.Substring(index + 1);
            }
            return null;
        }

        public sealed override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
            this.WriteRegistry();
        }

        public sealed override void Rollback(IDictionary savedState)
        {
            this.Uninstall(savedState);
        }

        public sealed override void Uninstall(IDictionary savedState)
        {
            string regKey;
            string str3;
            base.Uninstall(savedState);
            if (((base.Context != null) && (base.Context.Parameters != null)) && base.Context.Parameters.ContainsKey("RegFile"))
            {
                string str = base.Context.Parameters["RegFile"];
                if (!string.IsNullOrEmpty(str))
                {
                    return;
                }
            }
            int length = this.RegKey.LastIndexOf('\\');
            if (length >= 0)
            {
                str3 = this.RegKey.Substring(0, length);
                regKey = this.RegKey.Substring(length + 1);
            }
            else
            {
                str3 = "";
                regKey = this.RegKey;
            }
            foreach (string str4 in MshRegistryRoots)
            {
                this.GetRegistryKey(str4 + str3).DeleteSubKey(regKey);
            }
        }

        private void WriteRegistry()
        {
            foreach (string str in MshRegistryRoots)
            {
                RegistryKey registryKey = this.GetRegistryKey(str + this.RegKey);
                foreach (string str2 in this.RegValues.Keys)
                {
                    registryKey.SetValue(str2, this.RegValues[str2]);
                }
            }
        }

        private static string[] MshRegistryRoots
        {
            get
            {
                return new string[] { (@"HKEY_LOCAL_MACHINE\Software\Xamarin\PowerShell\" + PSVersionInfo.RegistryVersion1Key + @"\") };
            }
        }

        internal abstract string RegKey { get; }

        internal abstract Dictionary<string, object> RegValues { get; }
    }
}

