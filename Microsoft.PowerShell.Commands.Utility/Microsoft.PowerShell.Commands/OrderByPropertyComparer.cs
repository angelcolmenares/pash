namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    internal class OrderByPropertyComparer : IComparer<OrderByPropertyEntry>
    {
        private ObjectCommandComparer[] propertyComparers;

        internal OrderByPropertyComparer(bool[] ascending, CultureInfo cultureInfo, bool caseSensitive)
        {
            this.propertyComparers = new ObjectCommandComparer[ascending.Length];
            for (int i = 0; i < ascending.Length; i++)
            {
                this.propertyComparers[i] = new ObjectCommandComparer(ascending[i], cultureInfo, caseSensitive);
            }
        }

        public int Compare(OrderByPropertyEntry firstEntry, OrderByPropertyEntry secondEntry)
        {
            int num = 0;
            for (int i = 0; i < this.propertyComparers.Length; i++)
            {
                ObjectCommandPropertyValue first = (i < firstEntry.orderValues.Count) ? firstEntry.orderValues[i] : ObjectCommandPropertyValue.NonExistingProperty;
                ObjectCommandPropertyValue second = (i < secondEntry.orderValues.Count) ? secondEntry.orderValues[i] : ObjectCommandPropertyValue.NonExistingProperty;
                num = this.propertyComparers[i].Compare(first, second);
                if (num != 0)
                {
                    return num;
                }
            }
            return num;
        }

        internal static OrderByPropertyComparer CreateComparer(List<OrderByPropertyEntry> orderMatrix, bool ascendingFlag, bool?[] ascendingOverrides, CultureInfo cultureInfo, bool caseSensitive)
        {
            if (orderMatrix.Count == 0)
            {
                return null;
            }
            int count = 0;
            foreach (OrderByPropertyEntry entry in orderMatrix)
            {
                if (entry.orderValues.Count > count)
                {
                    count = entry.orderValues.Count;
                }
            }
            if (count == 0)
            {
                return null;
            }
            bool[] ascending = new bool[count];
            for (int i = 0; i < count; i++)
            {
                if ((ascendingOverrides != null) && ascendingOverrides[i].HasValue)
                {
                    ascending[i] = ascendingOverrides[i].Value;
                }
                else
                {
                    ascending[i] = ascendingFlag;
                }
            }
            return new OrderByPropertyComparer(ascending, cultureInfo, caseSensitive);
        }
    }
}

