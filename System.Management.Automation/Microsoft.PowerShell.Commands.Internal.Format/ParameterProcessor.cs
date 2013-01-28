namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Text;

    internal sealed class ParameterProcessor
    {
        private CommandParameterDefinition paramDef;
        [TraceSource("ParameterProcessor", "ParameterProcessor")]
        internal static PSTraceSource tracer = PSTraceSource.GetTracer("ParameterProcessor", "ParameterProcessor");

        internal ParameterProcessor(CommandParameterDefinition p)
        {
            this.paramDef = p;
        }

        internal static string CatenateStringArray(string[] arr)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            for (int i = 0; i < arr.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }
                builder.Append(arr[i]);
            }
            builder.Append("}");
            return builder.ToString();
        }

        private static string CatenateTypeArray(Type[] arr)
        {
            string[] strArray = new string[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                strArray[i] = arr[i].FullName;
            }
            return CatenateStringArray(strArray);
        }

        private static bool MatchesAllowedTypes(Type t, Type[] allowedTypes)
        {
            for (int i = 0; i < allowedTypes.Length; i++)
            {
                if (allowedTypes[i].IsAssignableFrom(t))
                {
                    return true;
                }
            }
            return false;
        }

        private static void ProcessDuplicateHashTableKey(TerminatingErrorContext invocationContext, string duplicateKey, string existingKey)
        {
            string msg = StringUtil.Format(FormatAndOut_MshParameter.DuplicateKeyError, duplicateKey, existingKey);
            ThrowParameterBindingException(invocationContext, "DictionaryKeyDuplicate", msg);
        }

        private static void ProcessIllegalHashTableKeyValue(TerminatingErrorContext invocationContext, string key, Type actualType, Type[] allowedTypes)
        {
            string str;
            string str2;
            if (allowedTypes.Length > 1)
            {
                string str3 = CatenateTypeArray(allowedTypes);
                str = StringUtil.Format(FormatAndOut_MshParameter.IllegalTypeMultiError, new object[] { key, actualType.FullName, str3 });
                str2 = "DictionaryKeyIllegalValue1";
            }
            else
            {
                str = StringUtil.Format(FormatAndOut_MshParameter.IllegalTypeSingleError, new object[] { key, actualType.FullName, allowedTypes[0] });
                str2 = "DictionaryKeyIllegalValue2";
            }
            ThrowParameterBindingException(invocationContext, str2, str);
        }

        private static void ProcessMissingKeyValue(TerminatingErrorContext invocationContext, string keyName)
        {
            string msg = StringUtil.Format(FormatAndOut_MshParameter.MissingKeyValueError, keyName);
            ThrowParameterBindingException(invocationContext, "DictionaryKeyMissingValue", msg);
        }

        private static void ProcessMissingMandatoryKey(TerminatingErrorContext invocationContext, string keyName)
        {
            string msg = StringUtil.Format(FormatAndOut_MshParameter.MissingKeyMandatoryEntryError, keyName);
            ThrowParameterBindingException(invocationContext, "DictionaryKeyMandatoryEntry", msg);
        }

        private static void ProcessNonStringHashTableKey(TerminatingErrorContext invocationContext, object key)
        {
            string msg = StringUtil.Format(FormatAndOut_MshParameter.DictionaryKeyNonStringError, key.GetType().Name);
            ThrowParameterBindingException(invocationContext, "DictionaryKeyNonString", msg);
        }

        private static void ProcessNullHashTableKey(TerminatingErrorContext invocationContext)
        {
            string msg = StringUtil.Format(FormatAndOut_MshParameter.DictionaryKeyNullError, new object[0]);
            ThrowParameterBindingException(invocationContext, "DictionaryKeyNull", msg);
        }

        internal List<MshParameter> ProcessParameters(object[] p, TerminatingErrorContext invocationContext)
        {
            if ((p == null) || (p.Length == 0))
            {
                return null;
            }
            List<MshParameter> list = new List<MshParameter>();
            bool originalParameterWasHashTable = false;
            for (int i = 0; i < p.Length; i++)
            {
                MshParameter parameter = this.paramDef.CreateInstance();
                object val = PSObject.Base(p[i]);
                if (val is IDictionary)
                {
                    originalParameterWasHashTable = true;
                    parameter.hash = this.VerifyHashTable((IDictionary) val, invocationContext);
                }
                else if ((val != null) && MatchesAllowedTypes(val.GetType(), this.paramDef.hashEntries[0].AllowedTypes))
                {
                    parameter.hash = this.paramDef.hashEntries[0].CreateHashtableFromSingleType(val);
                }
                else
                {
                    ProcessUnknownParameterType(invocationContext, val, this.paramDef.hashEntries[0].AllowedTypes);
                }
                this.VerifyAndNormalizeParameter(parameter, invocationContext, originalParameterWasHashTable);
                list.Add(parameter);
            }
            return list;
        }

        private static void ProcessUnknownParameterType(TerminatingErrorContext invocationContext, object actualObject, Type[] allowedTypes)
        {
            string str2;
            string str = CatenateTypeArray(allowedTypes);
            if (actualObject != null)
            {
                str2 = StringUtil.Format(FormatAndOut_MshParameter.UnknownParameterTypeError, actualObject.GetType().FullName, str);
            }
            else
            {
                str2 = StringUtil.Format(FormatAndOut_MshParameter.NullParameterTypeError, str);
            }
            ThrowParameterBindingException(invocationContext, "DictionaryKeyUnknownType", str2);
        }

        internal static void ThrowParameterBindingException(TerminatingErrorContext invocationContext, string errorId, string msg)
        {
            ErrorRecord errorRecord = new ErrorRecord(new NotSupportedException(), errorId, ErrorCategory.InvalidArgument, null) {
                ErrorDetails = new ErrorDetails(msg)
            };
            invocationContext.ThrowTerminatingError(errorRecord);
        }

        private void VerifyAndNormalizeParameter(MshParameter parameter, TerminatingErrorContext invocationContext, bool originalParameterWasHashTable)
        {
            for (int i = 0; i < this.paramDef.hashEntries.Count; i++)
            {
                if (parameter.hash.ContainsKey(this.paramDef.hashEntries[i].KeyName))
                {
                    object val = parameter.hash[this.paramDef.hashEntries[i].KeyName];
                    object obj3 = this.paramDef.hashEntries[i].Verify(val, invocationContext, originalParameterWasHashTable);
                    if (obj3 != null)
                    {
                        parameter.hash[this.paramDef.hashEntries[i].KeyName] = obj3;
                    }
                }
                else
                {
                    object obj4 = this.paramDef.hashEntries[i].ComputeDefaultValue();
                    if (obj4 != AutomationNull.Value)
                    {
                        parameter.hash[this.paramDef.hashEntries[i].KeyName] = obj4;
                    }
                    else if (this.paramDef.hashEntries[i].Mandatory)
                    {
                        ProcessMissingMandatoryKey(invocationContext, this.paramDef.hashEntries[i].KeyName);
                    }
                }
            }
        }

        private Hashtable VerifyHashTable(IDictionary hash, TerminatingErrorContext invocationContext)
        {
            Hashtable hashtable = new Hashtable();
            foreach (DictionaryEntry entry in hash)
            {
                if (entry.Key == null)
                {
                    ProcessNullHashTableKey(invocationContext);
                }
                string key = entry.Key as string;
                if (key == null)
                {
                    ProcessNonStringHashTableKey(invocationContext, entry.Key);
                }
                HashtableEntryDefinition definition = this.paramDef.MatchEntry(key, invocationContext);
                if (hashtable.Contains(definition.KeyName))
                {
                    ProcessDuplicateHashTableKey(invocationContext, key, definition.KeyName);
                }
                bool flag = false;
                if ((definition.AllowedTypes == null) || (definition.AllowedTypes.Length == 0))
                {
                    flag = true;
                }
                else
                {
                    for (int i = 0; i < definition.AllowedTypes.Length; i++)
                    {
                        if (entry.Value == null)
                        {
                            ProcessMissingKeyValue(invocationContext, key);
                        }
                        if (definition.AllowedTypes[i].IsAssignableFrom(entry.Value.GetType()))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (!flag)
                {
                    ProcessIllegalHashTableKeyValue(invocationContext, key, entry.Value.GetType(), definition.AllowedTypes);
                }
                hashtable.Add(definition.KeyName, entry.Value);
            }
            return hashtable;
        }
    }
}

