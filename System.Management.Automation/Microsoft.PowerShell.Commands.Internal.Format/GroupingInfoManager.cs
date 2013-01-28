namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    internal sealed class GroupingInfoManager
    {
        private object currentGroupingKeyPropertyValue = AutomationNull.Value;
        private string groupingKeyDisplayName;
        private MshExpression groupingKeyExpression;
        private string label;

        internal void Initialize(MshExpression groupingExpression, string displayLabel)
        {
            this.groupingKeyExpression = groupingExpression;
            this.label = displayLabel;
        }

        private static bool IsEqual(object first, object second)
        {
            try
            {
                return (LanguagePrimitives.Compare(first, second, true, CultureInfo.CurrentCulture) == 0);
            }
            catch (InvalidCastException)
            {
            }
            catch (ArgumentException)
            {
            }
            string strA = PSObject.AsPSObject(first).ToString();
            string strB = PSObject.AsPSObject(second).ToString();
            return (string.Compare(strA, strB, true, CultureInfo.CurrentCulture) == 0);
        }

        internal bool UpdateGroupingKeyValue(PSObject so)
        {
            if (this.groupingKeyExpression == null)
            {
                return false;
            }
            List<MshExpressionResult> values = this.groupingKeyExpression.GetValues(so);
            if ((values.Count <= 0) || (values[0].Exception != null))
            {
                return false;
            }
            object result = values[0].Result;
            object currentGroupingKeyPropertyValue = this.currentGroupingKeyPropertyValue;
            this.currentGroupingKeyPropertyValue = result;
            bool flag = !IsEqual(this.currentGroupingKeyPropertyValue, currentGroupingKeyPropertyValue) && !IsEqual(currentGroupingKeyPropertyValue, this.currentGroupingKeyPropertyValue);
            if (flag && (this.label == null))
            {
                this.groupingKeyDisplayName = values[0].ResolvedExpression.ToString();
            }
            return flag;
        }

        internal object CurrentGroupingKeyPropertyValue
        {
            get
            {
                return this.currentGroupingKeyPropertyValue;
            }
        }

        internal string GroupingKeyDisplayName
        {
            get
            {
                if (this.label != null)
                {
                    return this.label;
                }
                return this.groupingKeyDisplayName;
            }
        }
    }
}

