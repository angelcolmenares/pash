namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Text;

    public class GroupInfo
    {
        internal int count;
        internal Collection<PSObject> group = new Collection<PSObject>();
        private OrderByPropertyEntry groupValue;
        private string name;

        internal GroupInfo(OrderByPropertyEntry groupValue)
        {
            this.Add(groupValue.inputObject);
            this.groupValue = groupValue;
            this.name = BuildName(groupValue.orderValues);
        }

        internal virtual void Add(PSObject groupValue)
        {
            this.group.Add(groupValue);
            this.count++;
        }

        private static string BuildName(List<ObjectCommandPropertyValue> propValues)
        {
            StringBuilder builder = new StringBuilder();
            foreach (ObjectCommandPropertyValue value2 in propValues)
            {
                if ((value2 != null) && (value2.PropertyValue != null))
                {
                    ICollection propertyValue = value2.PropertyValue as ICollection;
                    if (propertyValue != null)
                    {
                        builder.Append("{");
                        int length = builder.Length;
                        foreach (object obj2 in propertyValue)
                        {
                            builder.Append(string.Format(CultureInfo.InvariantCulture, "{0}, ", new object[] { obj2.ToString() }));
                        }
                        builder = (builder.Length > length) ? builder.Remove(builder.Length - 2, 2) : builder;
                        builder.Append("}, ");
                    }
                    else
                    {
                        builder.Append(string.Format(CultureInfo.InvariantCulture, "{0}, ", new object[] { value2.PropertyValue.ToString() }));
                    }
                }
            }
            if (builder.Length < 2)
            {
                return string.Empty;
            }
            return builder.Remove(builder.Length - 2, 2).ToString();
        }

        public int Count
        {
            get
            {
                return this.count;
            }
        }

        public Collection<PSObject> Group
        {
            get
            {
                return this.group;
            }
        }

        internal OrderByPropertyEntry GroupValue
        {
            get
            {
                return this.groupValue;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public ArrayList Values
        {
            get
            {
                ArrayList list = new ArrayList();
                foreach (ObjectCommandPropertyValue value2 in this.groupValue.orderValues)
                {
                    list.Add(value2.PropertyValue);
                }
                return list;
            }
        }
    }
}

