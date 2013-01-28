namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.OData;
    using System;

    internal sealed class ODataJsonErrorDeserializer : ODataJsonDeserializer
    {
        internal ODataJsonErrorDeserializer(ODataJsonInputContext jsonInputContext) : base(jsonInputContext)
        {
        }

        private ODataInnerError ReadInnerError(int recursionDepth)
        {
            ValidationUtils.IncreaseAndValidateRecursionDepth(ref recursionDepth, base.MessageReaderSettings.MessageQuotas.MaxNestingDepth);
            base.JsonReader.ReadStartObject();
            ODataInnerError error = new ODataInnerError();
            ODataJsonReaderUtils.ErrorPropertyBitMask none = ODataJsonReaderUtils.ErrorPropertyBitMask.None;
            while (base.JsonReader.NodeType == JsonNodeType.Property)
            {
                string str2 = base.JsonReader.ReadPropertyName();
                if (str2 == null)
                {
                    goto Label_010E;
                }
                if (!(str2 == "message"))
                {
                    if (str2 == "type")
                    {
                        goto Label_00A2;
                    }
                    if (str2 == "stacktrace")
                    {
                        goto Label_00C8;
                    }
                    if (str2 == "internalexception")
                    {
                        goto Label_00F1;
                    }
                    goto Label_010E;
                }
                ODataJsonReaderUtils.VerifyErrorPropertyNotFound(ref none, ODataJsonReaderUtils.ErrorPropertyBitMask.MessageValue, "message");
                error.Message = base.JsonReader.ReadStringValue("message");
                continue;
            Label_00A2:
                ODataJsonReaderUtils.VerifyErrorPropertyNotFound(ref none, ODataJsonReaderUtils.ErrorPropertyBitMask.TypeName, "type");
                error.TypeName = base.JsonReader.ReadStringValue("type");
                continue;
            Label_00C8:
                ODataJsonReaderUtils.VerifyErrorPropertyNotFound(ref none, ODataJsonReaderUtils.ErrorPropertyBitMask.StackTrace, "stacktrace");
                error.StackTrace = base.JsonReader.ReadStringValue("stacktrace");
                continue;
            Label_00F1:
                ODataJsonReaderUtils.VerifyErrorPropertyNotFound(ref none, ODataJsonReaderUtils.ErrorPropertyBitMask.InnerError, "internalexception");
                error.InnerError = this.ReadInnerError(recursionDepth);
                continue;
            Label_010E:
                base.JsonReader.SkipValue();
            }
            base.JsonReader.ReadEndObject();
            return error;
        }

        internal ODataError ReadTopLevelError()
        {
            base.JsonReader.DisableInStreamErrorDetection = true;
            ODataError error = new ODataError();
            try
            {
                base.ReadPayloadStart(false, false);
                base.JsonReader.ReadStartObject();
                ODataJsonReaderUtils.ErrorPropertyBitMask none = ODataJsonReaderUtils.ErrorPropertyBitMask.None;
                while (base.JsonReader.NodeType == JsonNodeType.Property)
                {
                    string strB = base.JsonReader.ReadPropertyName();
                    if (string.CompareOrdinal("error", strB) != 0)
                    {
                        throw new ODataException(Strings.ODataJsonErrorDeserializer_TopLevelErrorWithInvalidProperty(strB));
                    }
                    ODataJsonReaderUtils.VerifyErrorPropertyNotFound(ref none, ODataJsonReaderUtils.ErrorPropertyBitMask.Error, "error");
                    base.JsonReader.ReadStartObject();
                    while (base.JsonReader.NodeType == JsonNodeType.Property)
                    {
                        strB = base.JsonReader.ReadPropertyName();
                        string str2 = strB;
                        if (str2 == null)
                        {
                            goto Label_01B8;
                        }
                        if (!(str2 == "code"))
                        {
                            if (str2 == "message")
                            {
                                goto Label_00D9;
                            }
                            if (str2 == "innererror")
                            {
                                goto Label_019B;
                            }
                            goto Label_01B8;
                        }
                        ODataJsonReaderUtils.VerifyErrorPropertyNotFound(ref none, ODataJsonReaderUtils.ErrorPropertyBitMask.Code, "code");
                        error.ErrorCode = base.JsonReader.ReadStringValue("code");
                        continue;
                    Label_00D9:
                        ODataJsonReaderUtils.VerifyErrorPropertyNotFound(ref none, ODataJsonReaderUtils.ErrorPropertyBitMask.Message, "message");
                        base.JsonReader.ReadStartObject();
                        while (base.JsonReader.NodeType == JsonNodeType.Property)
                        {
                            strB = base.JsonReader.ReadPropertyName();
                            string str3 = strB;
                            if (str3 == null)
                            {
                                goto Label_0171;
                            }
                            if (!(str3 == "lang"))
                            {
                                if (str3 == "value")
                                {
                                    goto Label_014B;
                                }
                                goto Label_0171;
                            }
                            ODataJsonReaderUtils.VerifyErrorPropertyNotFound(ref none, ODataJsonReaderUtils.ErrorPropertyBitMask.MessageLanguage, "lang");
                            error.MessageLanguage = base.JsonReader.ReadStringValue("lang");
                            continue;
                        Label_014B:
                            ODataJsonReaderUtils.VerifyErrorPropertyNotFound(ref none, ODataJsonReaderUtils.ErrorPropertyBitMask.MessageValue, "value");
                            error.Message = base.JsonReader.ReadStringValue("value");
                            continue;
                        Label_0171:
                            throw new ODataException(Strings.ODataJsonErrorDeserializer_TopLevelErrorMessageValueWithInvalidProperty(strB));
                        }
                        base.JsonReader.ReadEndObject();
                        continue;
                    Label_019B:
                        ODataJsonReaderUtils.VerifyErrorPropertyNotFound(ref none, ODataJsonReaderUtils.ErrorPropertyBitMask.InnerError, "innererror");
                        error.InnerError = this.ReadInnerError(0);
                        continue;
                    Label_01B8:
                        throw new ODataException(Strings.ODataJsonErrorDeserializer_TopLevelErrorValueWithInvalidProperty(strB));
                    }
                    base.JsonReader.ReadEndObject();
                }
                base.JsonReader.ReadEndObject();
                base.ReadPayloadEnd(false, false);
            }
            finally
            {
                base.JsonReader.DisableInStreamErrorDetection = false;
            }
            return error;
        }
    }
}

