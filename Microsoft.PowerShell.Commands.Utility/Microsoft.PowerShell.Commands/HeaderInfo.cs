namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;

    internal class HeaderInfo
    {
        private List<ColumnInfo> columns = new List<ColumnInfo>();

        internal void AddColumn(ColumnInfo col)
        {
            this.columns.Add(col);
        }

        internal PSObject AddColumnsToWindow(OutWindowProxy windowProxy, PSObject liveObject)
        {
            PSObject obj2 = new PSObject();
            int count = this.columns.Count;
            string[] propertyNames = new string[count];
            string[] displayNames = new string[count];
            Type[] types = new Type[count];
            count = 0;
            foreach (ColumnInfo info in this.columns)
            {
                propertyNames[count] = info.StaleObjectPropertyName();
                displayNames[count] = info.DisplayName();
                object columnValue = null;
                types[count] = info.GetValueType(liveObject, out columnValue);
                obj2.Properties.Add(new PSNoteProperty(propertyNames[count], columnValue));
                count++;
            }
            windowProxy.AddColumns(propertyNames, displayNames, types);
            return obj2;
        }

        internal PSObject CreateStalePSObject(PSObject liveObject)
        {
            PSObject obj2 = new PSObject();
            foreach (ColumnInfo info in this.columns)
            {
                obj2.Properties.Add(new PSNoteProperty(info.StaleObjectPropertyName(), info.GetValue(liveObject)));
            }
            return obj2;
        }
    }
}

