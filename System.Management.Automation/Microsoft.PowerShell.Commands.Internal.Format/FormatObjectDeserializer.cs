namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    internal sealed class FormatObjectDeserializer
    {
        private Microsoft.PowerShell.Commands.Internal.Format.TerminatingErrorContext _errorContext;
        private bool _useBaseObject;
        private const string TabExpansionString = "    ";
        [TraceSource("FormatObjectDeserializer", "FormatObjectDeserializer")]
        internal static PSTraceSource tracer = PSTraceSource.GetTracer("FormatObjectDeserializer", "class to deserialize property bags into formatting objects");

        internal FormatObjectDeserializer(Microsoft.PowerShell.Commands.Internal.Format.TerminatingErrorContext errorContext)
        {
            this._errorContext = errorContext;
        }

        internal object Deserialize(PSObject so)
        {
            if (this._useBaseObject)
            {
                object baseObject = so.BaseObject;
                if ((baseObject != null) && (baseObject is PacketInfoData))
                {
                    return baseObject;
                }
            }
            if (!Deserializer.IsInstanceOfType(so, typeof(FormatInfoData)))
            {
                return so;
            }
            string property = GetProperty(so, "ClassId2e4f51ef21dd47e99d3c952918aff9cd") as string;
            if (property == null)
            {
                return so;
            }
            if (IsClass(property, "033ecb2bc07a4d43b5ef94ed5a35d280"))
            {
                return this.DeserializeObject(so);
            }
            if (IsClass(property, "cf522b78d86c486691226b40aa69e95c"))
            {
                return this.DeserializeObject(so);
            }
            if (IsClass(property, "9e210fe47d09416682b841769c78b8a3"))
            {
                return this.DeserializeObject(so);
            }
            if (IsClass(property, "4ec4f0187cb04f4cb6973460dfe252df"))
            {
                return this.DeserializeObject(so);
            }
            if (IsClass(property, "27c87ef9bbda4f709f6b4002fa4af63c"))
            {
                return this.DeserializeObject(so);
            }
            this.ProcessUnknownInvalidClassId(property, so, "FormatObjectDeserializerDeserializeInvalidClassId");
            return null;
        }

        internal bool DeserializeBoolMemberVariable(PSObject so, string property)
        {
            return (bool) this.DeserializeMemberVariable(so, property, typeof(bool), true);
        }

        internal int DeserializeIntMemberVariable(PSObject so, string property)
        {
            return (int) this.DeserializeMemberVariable(so, property, typeof(int), true);
        }

        internal FormatInfoData DeserializeMandatoryMemberObject(PSObject so, string property)
        {
            FormatInfoData data = this.DeserializeMemberObject(so, property);
            this.VerifyDataNotNull(data, property);
            return data;
        }

        internal FormatInfoData DeserializeMemberObject(PSObject so, string property)
        {
            object obj2 = GetProperty(so, property);
            if (obj2 == null)
            {
                return null;
            }
            if (so == obj2)
            {
                string message = StringUtil.Format(FormatAndOut_format_xxx.FOD_RecursiveProperty, property);
                ErrorRecord errorRecord = new ErrorRecord(PSTraceSource.NewArgumentException("property"), "FormatObjectDeserializerRecursiveProperty", ErrorCategory.InvalidData, so) {
                    ErrorDetails = new ErrorDetails(message)
                };
                this.TerminatingErrorContext.ThrowTerminatingError(errorRecord);
            }
            return this.DeserializeObject(PSObject.AsPSObject(obj2));
        }

        private object DeserializeMemberVariable(PSObject so, string property, Type t, bool cannotBeNull)
        {
            object obj2 = GetProperty(so, property);
            if (cannotBeNull)
            {
                this.VerifyDataNotNull(obj2, property);
            }
            if ((obj2 != null) && (t != obj2.GetType()))
            {
                string message = StringUtil.Format(FormatAndOut_format_xxx.FOD_InvalidPropertyType, t.Name, property);
                ErrorRecord errorRecord = new ErrorRecord(PSTraceSource.NewArgumentException("property"), "FormatObjectDeserializerInvalidPropertyType", ErrorCategory.InvalidData, so) {
                    ErrorDetails = new ErrorDetails(message)
                };
                this.TerminatingErrorContext.ThrowTerminatingError(errorRecord);
            }
            return obj2;
        }

        internal FormatInfoData DeserializeObject(PSObject so)
        {
            FormatInfoData data = FormatInfoDataClassFactory.CreateInstance(so, this);
            if (data != null)
            {
                data.Deserialize(so, this);
            }
            return data;
        }

        internal string DeserializeStringMemberVariable(PSObject so, string property)
        {
            string str = (string) this.DeserializeMemberVariable(so, property, typeof(string), false);
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            return str.Replace("\t", "    ");
        }

        internal string DeserializeStringMemberVariableRaw(PSObject so, string property)
        {
            return (string) this.DeserializeMemberVariable(so, property, typeof(string), false);
        }

        internal WriteStreamType DeserializeWriteStreamTypeMemberVariable(PSObject so)
        {
            object property = GetProperty(so, "writeStream");
            if (property != null)
            {
                if (property is WriteStreamType)
                {
                    return (WriteStreamType) property;
                }
                if (property is string)
                {
                    WriteStreamType none;
                    if (!Enum.TryParse<WriteStreamType>(property as string, true, out none))
                    {
                        none = WriteStreamType.None;
                    }
                    return none;
                }
            }
            return WriteStreamType.None;
        }

        internal static object GetProperty(PSObject so, string name)
        {
            PSMemberInfo info = so.Properties[name];
            if (info == null)
            {
                return null;
            }
            return info.Value;
        }

        private static bool IsClass(string x, string y)
        {
            return (string.Compare(x, y, StringComparison.OrdinalIgnoreCase) == 0);
        }

        internal bool IsFormatInfoData(PSObject so)
        {
            if (this._useBaseObject)
            {
                object baseObject = so.BaseObject;
                if ((baseObject != null) && (baseObject is PacketInfoData))
                {
                    return true;
                }
            }
            if (Deserializer.IsInstanceOfType(so, typeof(FormatInfoData)))
            {
                string property = GetProperty(so, "ClassId2e4f51ef21dd47e99d3c952918aff9cd") as string;
                if (property == null)
                {
                    return false;
                }
                if (IsClass(property, "033ecb2bc07a4d43b5ef94ed5a35d280"))
                {
                    return true;
                }
                if (IsClass(property, "cf522b78d86c486691226b40aa69e95c"))
                {
                    return true;
                }
                if (IsClass(property, "9e210fe47d09416682b841769c78b8a3"))
                {
                    return true;
                }
                if (IsClass(property, "4ec4f0187cb04f4cb6973460dfe252df"))
                {
                    return true;
                }
                if (IsClass(property, "27c87ef9bbda4f709f6b4002fa4af63c"))
                {
                    return true;
                }
                this.ProcessUnknownInvalidClassId(property, so, "FormatObjectDeserializerIsFormatInfoDataInvalidClassId");
            }
            return false;
        }

        private void ProcessUnknownInvalidClassId(string classId, object obj, string errorId)
        {
            string message = StringUtil.Format(FormatAndOut_format_xxx.FOD_ClassIdInvalid, classId);
            ErrorRecord errorRecord = new ErrorRecord(PSTraceSource.NewArgumentException("classId"), errorId, ErrorCategory.InvalidData, obj) {
                ErrorDetails = new ErrorDetails(message)
            };
            this.TerminatingErrorContext.ThrowTerminatingError(errorRecord);
        }

        internal void VerifyDataNotNull(object obj, string name)
        {
            if (obj == null)
            {
                string message = StringUtil.Format(FormatAndOut_format_xxx.FOD_NullDataMember, name);
                ErrorRecord errorRecord = new ErrorRecord(new ArgumentException(), "FormatObjectDeserializerNullDataMember", ErrorCategory.InvalidData, null) {
                    ErrorDetails = new ErrorDetails(message)
                };
                this.TerminatingErrorContext.ThrowTerminatingError(errorRecord);
            }
        }

        internal Microsoft.PowerShell.Commands.Internal.Format.TerminatingErrorContext TerminatingErrorContext
        {
            get
            {
                return this._errorContext;
            }
        }
    }
}

