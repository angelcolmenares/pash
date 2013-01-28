namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Runtime.InteropServices;

    internal sealed class OrderByProperty
    {
        private List<MshParameter> _mshParameterList;
        private OrderByPropertyComparer comparer;
        private List<OrderByPropertyEntry> orderMatrix;

        internal OrderByProperty(PSCmdlet cmdlet, List<PSObject> inputObjects, object[] expr, bool ascending, CultureInfo cultureInfo, bool caseSensitive)
        {
            ProcessExpressionParameter(inputObjects, cmdlet, expr, out this._mshParameterList);
            this.orderMatrix = CreateOrderMatrix(cmdlet, inputObjects, this._mshParameterList);
            this.comparer = CreateComparer(this.orderMatrix, this._mshParameterList, ascending, cultureInfo, caseSensitive);
        }

        private static OrderByPropertyComparer CreateComparer(List<OrderByPropertyEntry> orderMatrix, List<MshParameter> mshParameterList, bool ascending, CultureInfo cultureInfo, bool caseSensitive)
        {
            if ((orderMatrix == null) || (orderMatrix.Count == 0))
            {
                return null;
            }
            bool?[] ascendingOverrides = null;
            if ((mshParameterList != null) && (mshParameterList.Count != 0))
            {
                ascendingOverrides = new bool?[mshParameterList.Count];
                for (int i = 0; i < ascendingOverrides.Length; i++)
                {
                    object entry = mshParameterList[i].GetEntry("ascending");
                    object orderEntryKey = mshParameterList[i].GetEntry("descending");
                    bool flag = isOrderEntryKeyDefined(entry);
                    bool flag2 = isOrderEntryKeyDefined(orderEntryKey);
                    if (!flag && !flag2)
                    {
                        ascendingOverrides[i] = null;
                    }
                    else if ((flag && flag2) && (((bool) entry) == ((bool) orderEntryKey)))
                    {
                        ascendingOverrides[i] = null;
                    }
                    else if (flag)
                    {
                        ascendingOverrides[i] = new bool?((bool) entry);
                    }
                    else
                    {
                        ascendingOverrides[i] = new bool?(!((bool) orderEntryKey));
                    }
                }
            }
            return OrderByPropertyComparer.CreateComparer(orderMatrix, ascending, ascendingOverrides, cultureInfo, caseSensitive);
        }

        internal static List<OrderByPropertyEntry> CreateOrderMatrix(PSCmdlet cmdlet, List<PSObject> inputObjects, List<MshParameter> mshParameterList)
        {
            List<OrderByPropertyEntry> list = new List<OrderByPropertyEntry>();
            foreach (PSObject obj2 in inputObjects)
            {
                if ((obj2 != null) && (obj2 != AutomationNull.Value))
                {
                    List<ErrorRecord> errors = new List<ErrorRecord>();
                    List<string> propertyNotFoundMsgs = new List<string>();
                    OrderByPropertyEntry item = OrderByPropertyEntryEvaluationHelper.ProcessObject(obj2, mshParameterList, errors, propertyNotFoundMsgs);
                    foreach (ErrorRecord record in errors)
                    {
                        cmdlet.WriteError(record);
                    }
                    foreach (string str in propertyNotFoundMsgs)
                    {
                        cmdlet.WriteDebug(str);
                    }
                    list.Add(item);
                }
            }
            return list;
        }

        private static List<MshParameter> ExpandExpressions(List<PSObject> inputObjects, List<MshParameter> unexpandedParameterList)
        {
            List<MshParameter> list = new List<MshParameter>();
            if (unexpandedParameterList != null)
            {
                foreach (MshParameter parameter in unexpandedParameterList)
                {
                    MshExpression entry = (MshExpression) parameter.GetEntry("expression");
                    if (!entry.HasWildCardCharacters)
                    {
                        list.Add(parameter);
                    }
                    else
                    {
                        SortedDictionary<string, MshExpression> dictionary = new SortedDictionary<string, MshExpression>(StringComparer.OrdinalIgnoreCase);
                        if (inputObjects != null)
                        {
                            foreach (object obj2 in inputObjects)
                            {
                                if (obj2 != null)
                                {
                                    foreach (MshExpression expression2 in entry.ResolveNames(PSObject.AsPSObject(obj2)))
                                    {
                                        dictionary[expression2.ToString()] = expression2;
                                    }
                                }
                            }
                        }
                        foreach (MshExpression expression3 in dictionary.Values)
                        {
                            MshParameter item = new MshParameter {
                                hash = (Hashtable) parameter.hash.Clone()
                            };
                            item.hash["expression"] = expression3;
                            list.Add(item);
                        }
                    }
                }
            }
            return list;
        }

        private static string[] GetDefaultKeyPropertySet(PSObject mshObj)
        {
            PSMemberSet pSStandardMembers = mshObj.PSStandardMembers;
            if (pSStandardMembers == null)
            {
                return null;
            }
            PSPropertySet set2 = pSStandardMembers.Members["DefaultKeyPropertySet"] as PSPropertySet;
            if (set2 == null)
            {
                return null;
            }
            string[] array = new string[set2.ReferencedPropertyNames.Count];
            set2.ReferencedPropertyNames.CopyTo(array, 0);
            return array;
        }

        private static bool isOrderEntryKeyDefined(object orderEntryKey)
        {
            return ((orderEntryKey != null) && (orderEntryKey != AutomationNull.Value));
        }

        private static void ProcessExpressionParameter(List<PSObject> inputObjects, PSCmdlet cmdlet, object[] expr, out List<MshParameter> mshParameterList)
        {
            mshParameterList = null;
            TerminatingErrorContext invocationContext = new TerminatingErrorContext(cmdlet);
            ParameterProcessor processor = (cmdlet is SortObjectCommand) ? new ParameterProcessor(new SortObjectExpressionParameterDefinition()) : new ParameterProcessor(new GroupObjectExpressionParameterDefinition());
            if (((expr == null) && (inputObjects != null)) && (inputObjects.Count > 0))
            {
                expr = GetDefaultKeyPropertySet(inputObjects[0]);
            }
            if (expr != null)
            {
                List<MshParameter> unexpandedParameterList = processor.ProcessParameters(expr, invocationContext);
                mshParameterList = ExpandExpressions(inputObjects, unexpandedParameterList);
            }
        }

        internal OrderByPropertyComparer Comparer
        {
            get
            {
                return this.comparer;
            }
        }

        internal List<MshParameter> MshParameterList
        {
            get
            {
                return this._mshParameterList;
            }
        }

        internal List<OrderByPropertyEntry> OrderMatrix
        {
            get
            {
                return this.orderMatrix;
            }
        }
    }
}

