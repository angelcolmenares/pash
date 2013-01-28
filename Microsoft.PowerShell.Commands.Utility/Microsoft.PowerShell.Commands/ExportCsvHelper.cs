namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Text;

    internal class ExportCsvHelper : IDisposable
    {
        private PSCmdlet _cmdlet;
        private bool _disposed;
        private char Delimiter;

        internal ExportCsvHelper(PSCmdlet cmdlet, char delimiter)
        {
            this._cmdlet = cmdlet;
            this.Delimiter = delimiter;
        }

        internal IList<string> BuildPropertyNames(PSObject source, IList<string> propertyNames)
        {
            PSMemberInfoCollection<PSPropertyInfo> infos = new PSMemberInfoIntegratingCollection<PSPropertyInfo>(source, PSObject.GetPropertyCollection(PSMemberViewTypes.Adapted | PSMemberViewTypes.Extended));
            propertyNames = new Collection<string>();
            foreach (PSPropertyInfo info in infos)
            {
                propertyNames.Add(info.Name);
            }
            return propertyNames;
        }

        internal string ConvertPropertyNamesCSV(IList<string> propertyNames)
        {
            StringBuilder dest = new StringBuilder();
            bool flag = true;
            foreach (string str in propertyNames)
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    dest.Append(this.Delimiter);
                }
                EscapeAndAppendString(dest, str);
            }
            return dest.ToString();
        }

        internal string ConvertPSObjectToCSV(PSObject mshObject, IList<string> propertyNames)
        {
            StringBuilder dest = new StringBuilder();
            bool flag = true;
            foreach (string str in propertyNames)
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    dest.Append(this.Delimiter);
                }
                PSPropertyInfo property = mshObject.Properties[str];
                string source = null;
                if (property != null)
                {
                    source = this.GetToStringValueForProperty(property);
                }
                EscapeAndAppendString(dest, source);
            }
            return dest.ToString();
        }

        public void Dispose()
        {
            if (!this._disposed)
            {
                GC.SuppressFinalize(this);
            }
            this._disposed = true;
        }

        internal static void EscapeAndAppendString(StringBuilder dest, string source)
        {
            if (source != null)
            {
                dest.Append('"');
                for (int i = 0; i < source.Length; i++)
                {
                    char ch = source[i];
                    if (ch == '"')
                    {
                        dest.Append('"');
                    }
                    dest.Append(ch);
                }
                dest.Append('"');
            }
        }

        internal string GetToStringValueForProperty(PSPropertyInfo property)
        {
            string str = null;
            try
            {
                object obj2 = property.Value;
                if (obj2 != null)
                {
                    str = obj2.ToString();
                }
            }
            catch (Exception exception)
            {
                UtilityCommon.CheckForSevereException(this._cmdlet, exception);
            }
            return str;
        }

        internal string GetTypeString(PSObject source)
        {
            Collection<string> typeNames = source.TypeNames;
            if ((typeNames == null) || (typeNames.Count == 0))
            {
                return "#TYPE";
            }
            string str2 = typeNames[0];
            if (str2.StartsWith("CSV:", StringComparison.OrdinalIgnoreCase))
            {
                str2 = str2.Substring(4);
            }
            return string.Format(CultureInfo.InvariantCulture, "#TYPE {0}", new object[] { str2 });
        }
    }
}

