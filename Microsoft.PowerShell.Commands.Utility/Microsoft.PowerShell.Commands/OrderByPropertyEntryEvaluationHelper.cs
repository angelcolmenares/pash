namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Runtime.InteropServices;

    internal static class OrderByPropertyEntryEvaluationHelper
    {
        private static void EvaluateSortingExpression(MshParameter p, PSObject inputObject, List<ObjectCommandPropertyValue> orderValues, List<ErrorRecord> errors, out string propertyNotFoundMsg)
        {
            MshExpression entry = p.GetEntry("expression") as MshExpression;
            List<MshExpressionResult> list = entry.GetValues(inputObject, false, true);
            if (list.Count == 0)
            {
                orderValues.Add(ObjectCommandPropertyValue.NonExistingProperty);
                propertyNotFoundMsg = StringUtil.Format(SortObjectStrings.PropertyNotFound, entry.ToString());
            }
            else
            {
                propertyNotFoundMsg = null;
                foreach (MshExpressionResult result in list)
                {
                    if (result.Exception == null)
                    {
                        orderValues.Add(new ObjectCommandPropertyValue(result.Result));
                    }
                    else
                    {
                        ErrorRecord item = new ErrorRecord(result.Exception, "ExpressionEvaluation", ErrorCategory.InvalidResult, inputObject);
                        errors.Add(item);
                        orderValues.Add(ObjectCommandPropertyValue.ExistingNullProperty);
                    }
                }
            }
        }

        internal static OrderByPropertyEntry ProcessObject(PSObject inputObject, List<MshParameter> mshParameterList, List<ErrorRecord> errors, List<string> propertyNotFoundMsgs)
        {
            OrderByPropertyEntry entry = new OrderByPropertyEntry {
                inputObject = inputObject
            };
            if ((mshParameterList == null) || (mshParameterList.Count == 0))
            {
                entry.orderValues.Add(new ObjectCommandPropertyValue(inputObject));
                return entry;
            }
            foreach (MshParameter parameter in mshParameterList)
            {
                string propertyNotFoundMsg = null;
                EvaluateSortingExpression(parameter, inputObject, entry.orderValues, errors, out propertyNotFoundMsg);
                if (!string.IsNullOrEmpty(propertyNotFoundMsg))
                {
                    propertyNotFoundMsgs.Add(propertyNotFoundMsg);
                }
            }
            return entry;
        }
    }
}

