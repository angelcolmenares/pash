namespace System.Management.Automation
{
    using Microsoft.PowerShell;
    using System;
    using System.Data;

    internal class DataRowViewAdapter : PropertyOnlyAdapter
    {
        protected override void DoAddAllProperties<T>(object obj, PSMemberInfoInternalCollection<T> members)
        {
            DataRowView view = (DataRowView) obj;
            if (((view.Row != null) && (view.Row.Table != null)) && (view.Row.Table.Columns != null))
            {
                foreach (DataColumn column in view.Row.Table.Columns)
                {
                    members.Add(new PSProperty(column.ColumnName, this, obj, column.ColumnName) as T);
                }
            }
        }

        protected override PSProperty DoGetProperty(object obj, string propertyName)
        {
            DataRowView view = (DataRowView) obj;
            if (!view.Row.Table.Columns.Contains(propertyName))
            {
                return null;
            }
            string columnName = view.Row.Table.Columns[propertyName].ColumnName;
            return new PSProperty(columnName, this, obj, columnName);
        }

        protected override object PropertyGet(PSProperty property)
        {
            DataRowView baseObject = (DataRowView) property.baseObject;
            return baseObject[(string) property.adapterData];
        }

        protected override bool PropertyIsGettable(PSProperty property)
        {
            return true;
        }

        protected override bool PropertyIsSettable(PSProperty property)
        {
            string adapterData = (string) property.adapterData;
            DataRowView baseObject = (DataRowView) property.baseObject;
            return !baseObject.Row.Table.Columns[adapterData].ReadOnly;
        }

        protected override void PropertySet(PSProperty property, object setValue, bool convertIfPossible)
        {
            DataRowView baseObject = (DataRowView) property.baseObject;
            baseObject[(string) property.adapterData] = setValue;
        }

        protected override string PropertyType(PSProperty property, bool forDisplay)
        {
            string adapterData = (string) property.adapterData;
            DataRowView baseObject = (DataRowView) property.baseObject;
            Type dataType = baseObject.Row.Table.Columns[adapterData].DataType;
            if (!forDisplay)
            {
                return dataType.FullName;
            }
            return ToStringCodeMethods.Type(dataType, false);
        }
    }
}

