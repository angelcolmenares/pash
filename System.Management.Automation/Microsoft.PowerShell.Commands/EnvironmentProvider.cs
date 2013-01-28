namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Provider;

    [CmdletProvider("Environment", ProviderCapabilities.ShouldProcess)]
    public sealed class EnvironmentProvider : SessionStateProviderBase
    {
        public const string ProviderName = "Environment";

        internal override object GetSessionStateItem(string name)
        {
            object obj2 = null;
            string environmentVariable = Environment.GetEnvironmentVariable(name);
            if (environmentVariable != null)
            {
                obj2 = new DictionaryEntry(name, environmentVariable);
            }
            return obj2;
        }

        internal override IDictionary GetSessionStateTable()
        {
            Dictionary<string, DictionaryEntry> dictionary = new Dictionary<string, DictionaryEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                dictionary.Add((string) entry.Key, entry);
            }
            return dictionary;
        }

        internal override object GetValueOfItem(object item)
        {
            object obj2 = item;
            if (item is DictionaryEntry)
            {
                DictionaryEntry entry = (DictionaryEntry) item;
                obj2 = entry.Value;
            }
            return obj2;
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            string environmentDriveDescription = SessionStateStrings.EnvironmentDriveDescription;
            PSDriveInfo info = new PSDriveInfo("Env", base.ProviderInfo, string.Empty, environmentDriveDescription, null);
            return new Collection<PSDriveInfo> { info };
        }

        internal override void RemoveSessionStateItem(string name)
        {
            Environment.SetEnvironmentVariable(name, null);
        }

        internal override void SetSessionStateItem(string name, object value, bool writeItem)
        {
            if (value == null)
            {
                Environment.SetEnvironmentVariable(name, null);
            }
            else
            {
                if (value is DictionaryEntry)
                {
                    DictionaryEntry entry2 = (DictionaryEntry) value;
                    value = entry2.Value;
                }
                string str = value as string;
                if (str == null)
                {
                    str = PSObject.AsPSObject(value).ToString();
                }
                Environment.SetEnvironmentVariable(name, str);
                DictionaryEntry item = new DictionaryEntry(name, str);
                if (writeItem)
                {
                    base.WriteItemObject(item, name, false);
                }
            }
        }
    }
}

