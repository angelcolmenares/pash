namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class PSObjectHelper
    {
        internal const string ellipses = "...";
        private static readonly PSObject emptyPSObject = new PSObject("");

        internal static PSObject AsPSObject(object obj)
        {
            if (obj != null)
            {
                return PSObject.AsPSObject(obj);
            }
            return emptyPSObject;
        }

        internal static string FormatField(FieldFormattingDirective directive, object val, int enumerationLimit, StringFormatError formatErrorObject, MshExpressionFactory expressionFactory)
        {
            PSObject so = AsPSObject(val);
            if ((directive != null) && !string.IsNullOrEmpty(directive.formatString))
            {
                try
                {
                    if (directive.formatString.Contains("{0") || directive.formatString.Contains("}"))
                    {
                        return string.Format(CultureInfo.CurrentCulture, directive.formatString, new object[] { so });
                    }
                    return so.ToString(directive.formatString, null);
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                    if (formatErrorObject != null)
                    {
                        formatErrorObject.sourceObject = so;
                        formatErrorObject.exception = exception;
                        formatErrorObject.formatString = directive.formatString;
                        return "";
                    }
                }
            }
            return SmartToString(so, expressionFactory, enumerationLimit, formatErrorObject);
        }

        private static MshExpression GetDefaultNameExpression(PSMemberSet standardMembersSet)
        {
            if (standardMembersSet != null)
            {
                PSNoteProperty property = standardMembersSet.Members["DefaultDisplayProperty"] as PSNoteProperty;
                if (property != null)
                {
                    string str = property.Value.ToString();
                    if (string.IsNullOrEmpty(str))
                    {
                        return null;
                    }
                    return new MshExpression(str);
                }
            }
            return null;
        }

        private static MshExpression GetDefaultNameExpression(PSObject so)
        {
            MshExpression defaultNameExpression = GetDefaultNameExpression(so.PSStandardMembers);
            if (defaultNameExpression == null)
            {
                defaultNameExpression = GetDefaultNameExpression(MaskDeserializedAndGetStandardMembers(so));
            }
            return defaultNameExpression;
        }

        private static List<MshExpression> GetDefaultPropertySet(PSMemberSet standardMembersSet)
        {
            if (standardMembersSet != null)
            {
                PSPropertySet set = standardMembersSet.Members["DefaultDisplayPropertySet"] as PSPropertySet;
                if (set != null)
                {
                    List<MshExpression> list = new List<MshExpression>();
                    foreach (string str in set.ReferencedPropertyNames)
                    {
                        if (!string.IsNullOrEmpty(str))
                        {
                            list.Add(new MshExpression(str));
                        }
                    }
                    return list;
                }
            }
            return new List<MshExpression>();
        }

        internal static List<MshExpression> GetDefaultPropertySet(PSObject so)
        {
            List<MshExpression> defaultPropertySet = GetDefaultPropertySet(so.PSStandardMembers);
            if (defaultPropertySet.Count == 0)
            {
                defaultPropertySet = GetDefaultPropertySet(MaskDeserializedAndGetStandardMembers(so));
            }
            return defaultPropertySet;
        }

        internal static MshExpressionResult GetDisplayName(PSObject target, MshExpressionFactory expressionFactory)
        {
            MshExpression displayNameExpression = GetDisplayNameExpression(target, expressionFactory);
            if (displayNameExpression != null)
            {
                List<MshExpressionResult> values = displayNameExpression.GetValues(target);
                if ((values.Count != 0) && (values[0].Exception == null))
                {
                    return values[0];
                }
            }
            return null;
        }

        internal static MshExpression GetDisplayNameExpression(PSObject target, MshExpressionFactory expressionFactory)
        {
            MshExpression defaultNameExpression = GetDefaultNameExpression(target);
            if (defaultNameExpression != null)
            {
                return defaultNameExpression;
            }
            string[] strArray = new string[] { "name", "id", "key", "*key", "*name", "*id" };
            foreach (string str in strArray)
            {
                List<MshExpression> list = new MshExpression(str).ResolveNames(target);
                while ((list.Count > 0) && ((list[0].ToString().Equals(RemotingConstants.ComputerNameNoteProperty, StringComparison.OrdinalIgnoreCase) || list[0].ToString().Equals(RemotingConstants.ShowComputerNameNoteProperty, StringComparison.OrdinalIgnoreCase)) || (list[0].ToString().Equals(RemotingConstants.RunspaceIdNoteProperty, StringComparison.OrdinalIgnoreCase) || list[0].ToString().Equals(RemotingConstants.SourceJobInstanceId, StringComparison.OrdinalIgnoreCase))))
                {
                    list.RemoveAt(0);
                }
                if (list.Count != 0)
                {
                    return list[0];
                }
            }
            return null;
        }

        internal static IEnumerable GetEnumerable(object obj)
        {
            PSObject obj2 = obj as PSObject;
            if (obj2 != null)
            {
                obj = obj2.BaseObject;
            }
            if (obj is IDictionary)
            {
                return (IEnumerable) obj;
            }
            return LanguagePrimitives.GetEnumerable(obj);
        }

        internal static string GetExpressionDisplayValue(PSObject so, int enumerationLimit, MshExpression ex, FieldFormattingDirective directive, StringFormatError formatErrorObject, MshExpressionFactory expressionFactory, out MshExpressionResult result)
        {
            result = null;
            List<MshExpressionResult> values = ex.GetValues(so);
            if (values.Count == 0)
            {
                return "";
            }
            result = values[0];
            if (result.Exception != null)
            {
                return "";
            }
            return FormatField(directive, result.Result, enumerationLimit, formatErrorObject, expressionFactory);
        }

        private static string GetObjectName(object x, MshExpressionFactory expressionFactory)
        {
            if ((x is PSObject) && ((LanguagePrimitives.IsBoolOrSwitchParameterType(((PSObject) x).BaseObject.GetType()) || LanguagePrimitives.IsNumeric(Type.GetTypeCode(((PSObject) x).BaseObject.GetType()))) || LanguagePrimitives.IsNull(x)))
            {
                return x.ToString();
            }
            if (x == null)
            {
                return "$null";
            }
            MethodInfo info = x.GetType().GetMethod("ToString", Type.EmptyTypes, null);
            if (info.DeclaringType.Equals(info.ReflectedType))
            {
                return AsPSObject(x).ToString();
            }
            MshExpressionResult displayName = GetDisplayName(AsPSObject(x), expressionFactory);
            if ((displayName != null) && (displayName.Exception == null))
            {
                return AsPSObject(displayName.Result).ToString();
            }
            string str = AsPSObject(x).ToString();
            if (str == string.Empty)
            {
                object obj2 = PSObject.Base(x);
                if (obj2 != null)
                {
                    str = obj2.ToString();
                }
            }
            return str;
        }

        private static string GetSmartToStringDisplayName(object x, MshExpressionFactory expressionFactory)
        {
            MshExpressionResult displayName = GetDisplayName(AsPSObject(x), expressionFactory);
            if ((displayName != null) && (displayName.Exception == null))
            {
                return AsPSObject(displayName.Result).ToString();
            }
            return AsPSObject(x).ToString();
        }

        internal static bool IsWriteDebugStream(PSObject so)
        {
            try
            {
                PSPropertyInfo info = so.Properties["WriteDebugStream"];
                return (((info != null) && (info.Value is bool)) && ((bool) info.Value));
            }
            catch (ExtendedTypeSystemException)
            {
                return false;
            }
        }

        internal static bool IsWriteErrorStream(PSObject so)
        {
            try
            {
                PSPropertyInfo info = so.Properties["WriteErrorStream"];
                return (((info != null) && (info.Value is bool)) && ((bool) info.Value));
            }
            catch (ExtendedTypeSystemException)
            {
                return false;
            }
        }

        internal static bool IsWriteVerboseStream(PSObject so)
        {
            try
            {
                PSPropertyInfo info = so.Properties["WriteVerboseStream"];
                return (((info != null) && (info.Value is bool)) && ((bool) info.Value));
            }
            catch (ExtendedTypeSystemException)
            {
                return false;
            }
        }

        internal static bool IsWriteWarningStream(PSObject so)
        {
            try
            {
                PSPropertyInfo info = so.Properties["WriteWarningStream"];
                return (((info != null) && (info.Value is bool)) && ((bool) info.Value));
            }
            catch (ExtendedTypeSystemException)
            {
                return false;
            }
        }

        private static PSMemberSet MaskDeserializedAndGetStandardMembers(PSObject so)
        {
            Collection<string> strings = Deserializer.MaskDeserializationPrefix(so.InternalTypeNames);
            if (strings == null)
            {
                return null;
            }
            TypeTable typeTable = so.GetTypeTable();
            if (typeTable == null)
            {
                return null;
            }
            return (typeTable.GetMembers<PSMemberInfo>(new ConsolidatedString(strings))["PSStandardMembers"] as PSMemberSet);
        }

        internal static bool PSObjectIsEnum(Collection<string> typeNames)
        {
            return (((typeNames.Count >= 2) && !string.IsNullOrEmpty(typeNames[1])) && string.Equals(typeNames[1], "System.Enum", StringComparison.Ordinal));
        }

        internal static string PSObjectIsOfExactType(Collection<string> typeNames)
        {
            if (typeNames.Count != 0)
            {
                return typeNames[0];
            }
            return null;
        }

        internal static bool ShouldShowComputerNameProperty(PSObject so)
        {
            bool result = false;
            if (so != null)
            {
                try
                {
                    PSPropertyInfo info = so.Properties[RemotingConstants.ComputerNameNoteProperty];
                    PSPropertyInfo info2 = so.Properties[RemotingConstants.ShowComputerNameNoteProperty];
                    if ((info != null) && (info2 != null))
                    {
                        LanguagePrimitives.TryConvertTo<bool>(info2.Value, out result);
                    }
                }
                catch (ArgumentException)
                {
                }
                catch (ExtendedTypeSystemException)
                {
                }
            }
            return result;
        }

        internal static string SmartToString(PSObject so, MshExpressionFactory expressionFactory, int enumerationLimit, StringFormatError formatErrorObject)
        {
            if (so == null)
            {
                return "";
            }
            try
            {
                IEnumerable enumerable = GetEnumerable(so);
                if (enumerable != null)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append("{");
                    bool flag = true;
                    int num = 0;
                    IEnumerator enumerator = enumerable.GetEnumerator();
                    if (enumerator != null)
                    {
                        IBlockingEnumerator<object> enumerator2 = enumerator as IBlockingEnumerator<object>;
                        if (enumerator2 != null)
                        {
                            while (enumerator2.MoveNext(false))
                            {
                                if (LocalPipeline.GetExecutionContextFromTLS().CurrentPipelineStopping)
                                {
                                    throw new PipelineStoppedException();
                                }
                                if (enumerationLimit >= 0)
                                {
                                    if (num == enumerationLimit)
                                    {
                                        builder.Append("...");
                                        break;
                                    }
                                    num++;
                                }
                                if (!flag)
                                {
                                    builder.Append(", ");
                                }
                                builder.Append(GetObjectName(enumerator2.Current, expressionFactory));
                                if (flag)
                                {
                                    flag = false;
                                }
                            }
                        }
                        else
                        {
                            foreach (object obj2 in enumerable)
                            {
                                if (LocalPipeline.GetExecutionContextFromTLS().CurrentPipelineStopping)
                                {
                                    throw new PipelineStoppedException();
                                }
                                if (enumerationLimit >= 0)
                                {
                                    if (num == enumerationLimit)
                                    {
                                        builder.Append("...");
                                        break;
                                    }
                                    num++;
                                }
                                if (!flag)
                                {
                                    builder.Append(", ");
                                }
                                builder.Append(GetObjectName(obj2, expressionFactory));
                                if (flag)
                                {
                                    flag = false;
                                }
                            }
                        }
                    }
                    builder.Append("}");
                    return builder.ToString();
                }
                return so.ToString();
            }
            catch (ExtendedTypeSystemException exception)
            {
                if (formatErrorObject != null)
                {
                    formatErrorObject.sourceObject = so;
                    formatErrorObject.exception = exception;
                }
                return "";
            }
        }
    }
}

