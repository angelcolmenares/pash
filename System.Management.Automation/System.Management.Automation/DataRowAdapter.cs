namespace System.Management.Automation
{
    using Microsoft.PowerShell;
    using System;
    using System.Data;

    internal class DataRowAdapter : PropertyOnlyAdapter
    {
        protected override void DoAddAllProperties<T>(object obj, PSMemberInfoInternalCollection<T> members)
        {
            DataRow row = (DataRow) obj;
            if ((row.Table != null) && (row.Table.Columns != null))
            {
                foreach (DataColumn column in row.Table.Columns)
                {
                    members.Add(new PSProperty(column.ColumnName, this, obj, column.ColumnName) as T);
                }
            }
        }

        protected override PSProperty DoGetProperty(object obj, string propertyName)
        {
            DataRow row = (DataRow) obj;
            if (!row.Table.Columns.Contains(propertyName))
            {
                return null;
            }
            string columnName = row.Table.Columns[propertyName].ColumnName;
            return new PSProperty(columnName, this, obj, columnName);
        }

        protected override object PropertyGet(PSProperty property)
        {
            DataRow baseObject = (DataRow) property.baseObject;
            return baseObject[(string) property.adapterData];
        }

        protected override bool PropertyIsGettable(PSProperty property)
        {
            return true;
        }

        protected override bool PropertyIsSettable(PSProperty property)
        {
            string adapterData = (string) property.adapterData;
            DataRow baseObject = (DataRow) property.baseObject;
            return !baseObject.Table.Columns[adapterData].ReadOnly;
        }

        protected override void PropertySet(PSProperty property, object setValue, bool convertIfPossible)
        {
            DataRow baseObject = (DataRow) property.baseObject;
            baseObject[(string) property.adapterData] = setValue;
        }

        protected override string PropertyType(PSProperty property, bool forDisplay)
        {
            string adapterData = (string) property.adapterData;
            DataRow baseObject = (DataRow) property.baseObject;
            Type dataType = baseObject.Table.Columns[adapterData].DataType;
            if (!forDisplay)
            {
                return dataType.FullName;
            }
            return ToStringCodeMethods.Type(dataType, false);
        }
    }
}

