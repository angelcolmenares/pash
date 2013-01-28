namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Runtime.InteropServices;

    internal abstract class ColumnInfo
    {
        protected string displayName;
        protected string staleObjectPropertyName;

        internal ColumnInfo(string staleObjectPropertyName, string displayName)
        {
            this.displayName = displayName;
            this.staleObjectPropertyName = GraphicalHostReflectionWrapper.EscapeBinding(staleObjectPropertyName);
        }

        internal string DisplayName()
        {
            return this.displayName;
        }

        internal abstract object GetValue(PSObject liveObject);
        internal Type GetValueType(PSObject liveObject, out object columnValue)
        {
            columnValue = this.GetValue(liveObject);
            if ((columnValue != null) && (columnValue is IComparable))
            {
                return columnValue.GetType();
            }
            return typeof(string);
        }

        internal static object LimitString(object src)
        {
            string source = src as string;
            if (source == null)
            {
                return src;
            }
            return HostUtilities.GetMaxLines(source, 10);
        }

        internal string StaleObjectPropertyName()
        {
            return this.staleObjectPropertyName;
        }
    }
}

