namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Xml;

    internal sealed class ODataAtomErrorDeserializer : ODataAtomDeserializer
    {
        internal ODataAtomErrorDeserializer(ODataAtomInputContext atomInputContext) : base(atomInputContext)
        {
        }

        internal static ODataError ReadErrorElement(BufferingXmlReader xmlReader, int maxInnerErrorDepth)
        {
            ODataError error = new ODataError();
            DuplicateErrorElementPropertyBitMask none = DuplicateErrorElementPropertyBitMask.None;
            if (xmlReader.IsEmptyElement)
            {
                return error;
            }
            xmlReader.Read();
        Label_001A:
            switch (xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    string str;
                    if (xmlReader.NamespaceEquals(xmlReader.ODataMetadataNamespace) && ((str = xmlReader.LocalName) != null))
                    {
                        if (!(str == "code"))
                        {
                            if (str == "message")
                            {
                                VerifyErrorElementNotFound(ref none, DuplicateErrorElementPropertyBitMask.Message, "message");
                                error.MessageLanguage = xmlReader.GetAttribute(xmlReader.XmlLangAttributeName, xmlReader.XmlNamespace);
                                error.Message = xmlReader.ReadElementValue();
                                goto Label_00EA;
                            }
                            if (str == "innererror")
                            {
                                VerifyErrorElementNotFound(ref none, DuplicateErrorElementPropertyBitMask.InnerError, "innererror");
                                error.InnerError = ReadInnerErrorElement(xmlReader, 0, maxInnerErrorDepth);
                                goto Label_00EA;
                            }
                        }
                        else
                        {
                            VerifyErrorElementNotFound(ref none, DuplicateErrorElementPropertyBitMask.Code, "code");
                            error.ErrorCode = xmlReader.ReadElementValue();
                            goto Label_00EA;
                        }
                    }
                    break;

                case XmlNodeType.EndElement:
                    goto Label_00EA;
            }
            xmlReader.Skip();
        Label_00EA:
            if (xmlReader.NodeType != XmlNodeType.EndElement)
            {
                goto Label_001A;
            }
            return error;
        }

        private static ODataInnerError ReadInnerErrorElement(BufferingXmlReader xmlReader, int recursionDepth, int maxInnerErrorDepth)
        {
            ValidationUtils.IncreaseAndValidateRecursionDepth(ref recursionDepth, maxInnerErrorDepth);
            ODataInnerError error = new ODataInnerError();
            DuplicateInnerErrorElementPropertyBitMask none = DuplicateInnerErrorElementPropertyBitMask.None;
            if (xmlReader.IsEmptyElement)
            {
                goto Label_010F;
            }
            xmlReader.Read();
        Label_0022:
            switch (xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    string str;
                    if (xmlReader.NamespaceEquals(xmlReader.ODataMetadataNamespace) && ((str = xmlReader.LocalName) != null))
                    {
                        if (!(str == "message"))
                        {
                            if (str == "type")
                            {
                                VerifyInnerErrorElementNotFound(ref none, DuplicateInnerErrorElementPropertyBitMask.TypeName, "type");
                                error.TypeName = xmlReader.ReadElementValue();
                                goto Label_0102;
                            }
                            if (str == "stacktrace")
                            {
                                VerifyInnerErrorElementNotFound(ref none, DuplicateInnerErrorElementPropertyBitMask.StackTrace, "stacktrace");
                                error.StackTrace = xmlReader.ReadElementValue();
                                goto Label_0102;
                            }
                            if (str == "internalexception")
                            {
                                VerifyInnerErrorElementNotFound(ref none, DuplicateInnerErrorElementPropertyBitMask.InternalException, "internalexception");
                                error.InnerError = ReadInnerErrorElement(xmlReader, recursionDepth, maxInnerErrorDepth);
                                goto Label_0102;
                            }
                        }
                        else
                        {
                            VerifyInnerErrorElementNotFound(ref none, DuplicateInnerErrorElementPropertyBitMask.Message, "message");
                            error.Message = xmlReader.ReadElementValue();
                            goto Label_0102;
                        }
                    }
                    break;

                case XmlNodeType.EndElement:
                    goto Label_0102;
            }
            xmlReader.Skip();
        Label_0102:
            if (xmlReader.NodeType != XmlNodeType.EndElement)
            {
                goto Label_0022;
            }
        Label_010F:
            xmlReader.Read();
            return error;
        }

        internal ODataError ReadTopLevelError()
        {
            ODataError error2;
            try
            {
                base.XmlReader.DisableInStreamErrorDetection = true;
                base.ReadPayloadStart();
                if (!base.XmlReader.NamespaceEquals(base.XmlReader.ODataMetadataNamespace) || !base.XmlReader.LocalNameEquals(base.XmlReader.ODataErrorElementName))
                {
                    throw new ODataErrorException(Strings.ODataAtomErrorDeserializer_InvalidRootElement(base.XmlReader.Name, base.XmlReader.NamespaceURI));
                }
                ODataError error = ReadErrorElement(base.XmlReader, base.MessageReaderSettings.MessageQuotas.MaxNestingDepth);
                base.XmlReader.Read();
                base.ReadPayloadEnd();
                error2 = error;
            }
            finally
            {
                base.XmlReader.DisableInStreamErrorDetection = false;
            }
            return error2;
        }

        private static void VerifyErrorElementNotFound(ref DuplicateErrorElementPropertyBitMask elementsFoundBitField, DuplicateErrorElementPropertyBitMask elementFoundBitMask, string elementName)
        {
            if ((elementsFoundBitField & elementFoundBitMask) == elementFoundBitMask)
            {
                throw new ODataException(Strings.ODataAtomErrorDeserializer_MultipleErrorElementsWithSameName(elementName));
            }
            elementsFoundBitField |= elementFoundBitMask;
        }

        private static void VerifyInnerErrorElementNotFound(ref DuplicateInnerErrorElementPropertyBitMask elementsFoundBitField, DuplicateInnerErrorElementPropertyBitMask elementFoundBitMask, string elementName)
        {
            if ((elementsFoundBitField & elementFoundBitMask) == elementFoundBitMask)
            {
                throw new ODataException(Strings.ODataAtomErrorDeserializer_MultipleInnerErrorElementsWithSameName(elementName));
            }
            elementsFoundBitField |= elementFoundBitMask;
        }

        [Flags]
        private enum DuplicateErrorElementPropertyBitMask
        {
            Code = 1,
            InnerError = 4,
            Message = 2,
            None = 0
        }

        [Flags]
        private enum DuplicateInnerErrorElementPropertyBitMask
        {
            InternalException = 8,
            Message = 1,
            None = 0,
            StackTrace = 4,
            TypeName = 2
        }
    }
}

