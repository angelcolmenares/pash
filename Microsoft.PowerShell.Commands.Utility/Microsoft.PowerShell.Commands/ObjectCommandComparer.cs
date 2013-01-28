namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Management.Automation;
    using System.Threading;

    internal class ObjectCommandComparer : IComparer
    {
        private bool ascendingOrder = true;
        private bool caseSensitive;
        private CultureInfo cultureInfo;

        internal ObjectCommandComparer(bool ascending, CultureInfo cultureInfo, bool caseSensitive)
        {
            this.ascendingOrder = ascending;
            this.cultureInfo = cultureInfo;
            if (this.cultureInfo == null)
            {
                this.cultureInfo = Thread.CurrentThread.CurrentCulture;
            }
            this.caseSensitive = caseSensitive;
        }

        internal int Compare(ObjectCommandPropertyValue first, ObjectCommandPropertyValue second)
        {
            if (first.IsExistingProperty && second.IsExistingProperty)
            {
                return this.Compare(first.PropertyValue, second.PropertyValue);
            }
            if (first.IsExistingProperty)
            {
                return -1;
            }
            if (second.IsExistingProperty)
            {
                return 1;
            }
            return 0;
        }

        public int Compare(object first, object second)
        {
            if (IsValueNull(first) && IsValueNull(second))
            {
                return 0;
            }
            PSObject obj2 = first as PSObject;
            if (obj2 != null)
            {
                first = obj2.BaseObject;
            }
            PSObject obj3 = second as PSObject;
            if (obj3 != null)
            {
                second = obj3.BaseObject;
            }
            try
            {
                return (LanguagePrimitives.Compare(first, second, !this.caseSensitive, this.cultureInfo) * (this.ascendingOrder ? 1 : -1));
            }
            catch (InvalidCastException)
            {
            }
            catch (ArgumentException)
            {
            }
            string strA = PSObject.AsPSObject(first).ToString();
            string strB = PSObject.AsPSObject(second).ToString();
            return (string.Compare(strA, strB, !this.caseSensitive, this.cultureInfo) * (this.ascendingOrder ? 1 : -1));
        }

        private static bool IsValueNull(object value)
        {
            return (PSObject.Base(value) == null);
        }
    }
}

