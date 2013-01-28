namespace System.Management.Automation
{
    using Microsoft.Management.Infrastructure;
    using Microsoft.Management.Infrastructure.Serialization;
    using Microsoft.PowerShell;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Tracing;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;

    internal class InternalDeserializer
    {
        private readonly DeserializationContext _context;
        private readonly XmlReader _reader;
        [TraceSource("InternalDeserializer", "InternalDeserializer class")]
        private static readonly PSTraceSource _trace = PSTraceSource.GetTracer("InternalDeserializer", "InternalDeserializer class");
        private System.Management.Automation.Runspaces.TypeTable _typeTable;
        private Version _version;
        internal const string CimClassMetadataProperty = "__ClassMetadata";
        internal const string CimClassNameProperty = "ClassName";
        private static Lazy<CimDeserializer> cimDeserializer = new Lazy<CimDeserializer>(new Func<CimDeserializer>(CimDeserializer.Create));
        internal const string CimHashCodeProperty = "Hash";
        internal const string CimInstanceMetadataProperty = "__InstanceMetadata";
        internal const string CimMiXmlProperty = "MiXml";
        internal const string CimModifiedProperties = "Modified";
        internal const string CimNamespaceProperty = "Namespace";
        internal const string CimServerNameProperty = "ServerName";
        private int depthBelowTopLevel;
        private bool isStopping;
        private const int MaxDepthBelowTopLevel = 50;
        private readonly ReferenceIdHandlerForDeserializer<object> objectRefIdHandler;
        private readonly ReferenceIdHandlerForDeserializer<ConsolidatedString> typeRefIdHandler;
        private static readonly XmlReaderSettings xmlReaderSettingsForCliXml = GetXmlReaderSettingsForCliXml();
        private static readonly XmlReaderSettings xmlReaderSettingsForUntrustedXmlDocument = GetXmlReaderSettingsForUntrustedXmlDocument();

        internal InternalDeserializer(XmlReader reader, DeserializationContext context)
        {
            this._reader = reader;
            this._context = context;
            this.objectRefIdHandler = new ReferenceIdHandlerForDeserializer<object>();
            this.typeRefIdHandler = new ReferenceIdHandlerForDeserializer<ConsolidatedString>();
        }

        private void CheckIfStopping()
        {
            if (this.isStopping)
            {
                throw PSTraceSource.NewInvalidOperationException("Serialization", "Stopping", new object[0]);
            }
        }

        private static string DecodeString(string s)
        {
            return XmlConvert.DecodeName(s);
        }

        internal static object DeserializeBoolean(InternalDeserializer deserializer)
        {
            object obj2;
            try
            {
                obj2 = XmlConvert.ToBoolean(deserializer._reader.ReadElementString());
            }
            catch (FormatException exception)
            {
                throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, exception, new object[] { typeof(bool).FullName });
            }
            return obj2;
        }

        internal static object DeserializeByte(InternalDeserializer deserializer)
        {
            Exception innerException = null;
            try
            {
                return XmlConvert.ToByte(deserializer._reader.ReadElementString());
            }
            catch (FormatException exception2)
            {
                innerException = exception2;
            }
            catch (OverflowException exception3)
            {
                innerException = exception3;
            }
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, innerException, new object[] { typeof(byte).FullName });
        }

        internal static object DeserializeByteArray(InternalDeserializer deserializer)
        {
            object obj2;
            try
            {
                obj2 = Convert.FromBase64String(deserializer._reader.ReadElementString());
            }
            catch (FormatException exception)
            {
                throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, exception, new object[] { typeof(byte[]).FullName });
            }
            return obj2;
        }

        internal static object DeserializeChar(InternalDeserializer deserializer)
        {
            Exception innerException = null;
            try
            {
                return (char) XmlConvert.ToUInt16(deserializer._reader.ReadElementString());
            }
            catch (FormatException exception2)
            {
                innerException = exception2;
            }
            catch (OverflowException exception3)
            {
                innerException = exception3;
            }
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, innerException, new object[] { typeof(char).FullName });
        }

        internal static object DeserializeDateTime(InternalDeserializer deserializer)
        {
            object obj2;
            try
            {
                obj2 = XmlConvert.ToDateTime(deserializer._reader.ReadElementString(), XmlDateTimeSerializationMode.RoundtripKind);
            }
            catch (FormatException exception)
            {
                throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, exception, new object[] { typeof(DateTime).FullName });
            }
            return obj2;
        }

        internal static object DeserializeDecimal(InternalDeserializer deserializer)
        {
            Exception innerException = null;
            try
            {
                return XmlConvert.ToDecimal(deserializer._reader.ReadElementString());
            }
            catch (FormatException exception2)
            {
                innerException = exception2;
            }
            catch (OverflowException exception3)
            {
                innerException = exception3;
            }
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, innerException, new object[] { typeof(decimal).FullName });
        }

        internal static object DeserializeDouble(InternalDeserializer deserializer)
        {
            Exception innerException = null;
            try
            {
                return XmlConvert.ToDouble(deserializer._reader.ReadElementString());
            }
            catch (FormatException exception2)
            {
                innerException = exception2;
            }
            catch (OverflowException exception3)
            {
                innerException = exception3;
            }
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, innerException, new object[] { typeof(double).FullName });
        }

        internal static object DeserializeGuid(InternalDeserializer deserializer)
        {
            Exception innerException = null;
            try
            {
                return XmlConvert.ToGuid(deserializer._reader.ReadElementString());
            }
            catch (FormatException exception2)
            {
                innerException = exception2;
            }
            catch (OverflowException exception3)
            {
                innerException = exception3;
            }
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, innerException, new object[] { typeof(Guid).FullName });
        }

        internal static object DeserializeInt16(InternalDeserializer deserializer)
        {
            Exception innerException = null;
            try
            {
                return XmlConvert.ToInt16(deserializer._reader.ReadElementString());
            }
            catch (FormatException exception2)
            {
                innerException = exception2;
            }
            catch (OverflowException exception3)
            {
                innerException = exception3;
            }
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, innerException, new object[] { typeof(short).FullName });
        }

        internal static object DeserializeInt32(InternalDeserializer deserializer)
        {
            Exception innerException = null;
            try
            {
                return XmlConvert.ToInt32(deserializer._reader.ReadElementString());
            }
            catch (FormatException exception2)
            {
                innerException = exception2;
            }
            catch (OverflowException exception3)
            {
                innerException = exception3;
            }
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, innerException, new object[] { typeof(int).FullName });
        }

        internal static object DeserializeInt64(InternalDeserializer deserializer)
        {
            Exception innerException = null;
            try
            {
                return XmlConvert.ToInt64(deserializer._reader.ReadElementString());
            }
            catch (FormatException exception2)
            {
                innerException = exception2;
            }
            catch (OverflowException exception3)
            {
                innerException = exception3;
            }
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, innerException, new object[] { typeof(long).FullName });
        }

        internal static object DeserializeProgressRecord(InternalDeserializer deserializer)
        {
            ProgressRecordType type;
            object obj3;
            deserializer.ReadStartElement("PR");
            string activity = null;
            string str2 = null;
            string str3 = null;
            string statusDescription = null;
            int activityId = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            Exception innerException = null;
            try
            {
                activity = deserializer.ReadDecodedElementString("AV");
                activityId = int.Parse(deserializer.ReadDecodedElementString("AI"), CultureInfo.InvariantCulture);
                object obj2 = deserializer.ReadOneObject();
                str2 = (obj2 == null) ? null : obj2.ToString();
                num2 = int.Parse(deserializer.ReadDecodedElementString("PI"), CultureInfo.InvariantCulture);
                num3 = int.Parse(deserializer.ReadDecodedElementString("PC"), CultureInfo.InvariantCulture);
                str3 = deserializer.ReadDecodedElementString("T");
                num4 = int.Parse(deserializer.ReadDecodedElementString("SR"), CultureInfo.InvariantCulture);
                statusDescription = deserializer.ReadDecodedElementString("SD");
            }
            catch (FormatException exception2)
            {
                innerException = exception2;
            }
            catch (OverflowException exception3)
            {
                innerException = exception3;
            }
            if (innerException != null)
            {
                throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, innerException, new object[] { typeof(ulong).FullName });
            }
            deserializer.ReadEndElement();
            try
            {
                type = (ProgressRecordType) Enum.Parse(typeof(ProgressRecordType), str3, true);
            }
            catch (ArgumentException exception4)
            {
                throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, exception4, new object[] { typeof(ProgressRecord).FullName });
            }
            try
            {
                ProgressRecord record = new ProgressRecord(activityId, activity, statusDescription);
                if (!string.IsNullOrEmpty(str2))
                {
                    record.CurrentOperation = str2;
                }
                record.ParentActivityId = num2;
                record.PercentComplete = num3;
                record.RecordType = type;
                record.SecondsRemaining = num4;
                obj3 = record;
            }
            catch (ArgumentException exception5)
            {
                throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, exception5, new object[] { typeof(ProgressRecord).FullName });
            }
            return obj3;
        }

        internal static object DeserializeSByte(InternalDeserializer deserializer)
        {
            Exception innerException = null;
            try
            {
                return XmlConvert.ToSByte(deserializer._reader.ReadElementString());
            }
            catch (FormatException exception2)
            {
                innerException = exception2;
            }
            catch (OverflowException exception3)
            {
                innerException = exception3;
            }
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, innerException, new object[] { typeof(sbyte).FullName });
        }

        internal static object DeserializeScriptBlock(InternalDeserializer deserializer)
        {
            string script = deserializer.ReadDecodedElementString("SBK");
            if (DeserializationOptions.DeserializeScriptBlocks == (deserializer._context.options & DeserializationOptions.DeserializeScriptBlocks))
            {
                return ScriptBlock.Create(script);
            }
            return script;
        }

        internal static object DeserializeSecureString(InternalDeserializer deserializer)
        {
            return deserializer.ReadSecureString();
        }

        internal static object DeserializeSingle(InternalDeserializer deserializer)
        {
            Exception innerException = null;
            try
            {
                return XmlConvert.ToSingle(deserializer._reader.ReadElementString());
            }
            catch (FormatException exception2)
            {
                innerException = exception2;
            }
            catch (OverflowException exception3)
            {
                innerException = exception3;
            }
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, innerException, new object[] { typeof(float).FullName });
        }

        internal static object DeserializeString(InternalDeserializer deserializer)
        {
            return deserializer.ReadDecodedElementString("S");
        }

        internal static object DeserializeTimeSpan(InternalDeserializer deserializer)
        {
            object obj2;
            try
            {
                obj2 = XmlConvert.ToTimeSpan(deserializer._reader.ReadElementString());
            }
            catch (FormatException exception)
            {
                throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, exception, new object[] { typeof(TimeSpan).FullName });
            }
            return obj2;
        }

        internal static object DeserializeUInt16(InternalDeserializer deserializer)
        {
            Exception innerException = null;
            try
            {
                return XmlConvert.ToUInt16(deserializer._reader.ReadElementString());
            }
            catch (FormatException exception2)
            {
                innerException = exception2;
            }
            catch (OverflowException exception3)
            {
                innerException = exception3;
            }
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, innerException, new object[] { typeof(ushort).FullName });
        }

        internal static object DeserializeUInt32(InternalDeserializer deserializer)
        {
            Exception innerException = null;
            try
            {
                return XmlConvert.ToUInt32(deserializer._reader.ReadElementString());
            }
            catch (FormatException exception2)
            {
                innerException = exception2;
            }
            catch (OverflowException exception3)
            {
                innerException = exception3;
            }
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, innerException, new object[] { typeof(int).FullName });
        }

        internal static object DeserializeUInt64(InternalDeserializer deserializer)
        {
            Exception innerException = null;
            try
            {
                return XmlConvert.ToUInt64(deserializer._reader.ReadElementString());
            }
            catch (FormatException exception2)
            {
                innerException = exception2;
            }
            catch (OverflowException exception3)
            {
                innerException = exception3;
            }
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, innerException, new object[] { typeof(ulong).FullName });
        }

        internal static object DeserializeUri(InternalDeserializer deserializer)
        {
            object obj2;
            try
            {
                obj2 = new Uri(deserializer.ReadDecodedElementString("URI"), UriKind.RelativeOrAbsolute);
            }
            catch (UriFormatException exception)
            {
                throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, exception, new object[] { typeof(Uri).FullName });
            }
            return obj2;
        }

        internal static object DeserializeVersion(InternalDeserializer deserializer)
        {
            Exception innerException = null;
            try
            {
                return new Version(deserializer._reader.ReadElementString());
            }
            catch (ArgumentException exception2)
            {
                innerException = exception2;
            }
            catch (FormatException exception3)
            {
                innerException = exception3;
            }
            catch (OverflowException exception4)
            {
                innerException = exception4;
            }
            throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, innerException, new object[] { typeof(Version).FullName });
        }

        internal static object DeserializeXmlDocument(InternalDeserializer deserializer)
        {
            object obj2;
            string xmlContents = deserializer.ReadDecodedElementString("XD");
            try
            {
                int? maxCharactersInDocument = null;
                if (deserializer._context.MaximumAllowedMemory.HasValue)
                {
                    maxCharactersInDocument = new int?(deserializer._context.MaximumAllowedMemory.Value / 2);
                }
                XmlDocument document = LoadUnsafeXmlDocument(xmlContents, true, maxCharactersInDocument);
                deserializer._context.LogExtraMemoryUsage((xmlContents.Length - document.OuterXml.Length) * 2);
                obj2 = document;
            }
            catch (XmlException exception)
            {
                throw deserializer.NewXmlException(Serialization.InvalidPrimitiveType, exception, new object[] { typeof(XmlDocument).FullName });
            }
            return obj2;
        }

        private static XmlReaderSettings GetXmlReaderSettingsForCliXml()
        {
            return new XmlReaderSettings { CheckCharacters = false, CloseInput = false, ConformanceLevel = ConformanceLevel.Document, IgnoreComments = true, IgnoreProcessingInstructions = true, IgnoreWhitespace = false, MaxCharactersFromEntities = 0x400L, Schemas = null, ValidationFlags = XmlSchemaValidationFlags.None, ValidationType = ValidationType.None, XmlResolver = null };
        }

        private static XmlReaderSettings GetXmlReaderSettingsForUntrustedXmlDocument()
        {
            return new XmlReaderSettings { CheckCharacters = false, ConformanceLevel = ConformanceLevel.Auto, IgnoreComments = true, IgnoreProcessingInstructions = true, IgnoreWhitespace = true, MaxCharactersFromEntities = 0x400L, MaxCharactersInDocument = 0x20000000L, DtdProcessing = DtdProcessing.Parse, ValidationFlags = XmlSchemaValidationFlags.None, ValidationType = ValidationType.None, XmlResolver = null };
        }

        private bool IsKnownContainerTag(out ContainerType ct)
        {
            if (this.IsNextElement("DCT"))
            {
                ct = ContainerType.Dictionary;
            }
            else if (this.IsNextElement("QUE"))
            {
                ct = ContainerType.Queue;
            }
            else if (this.IsNextElement("STK"))
            {
                ct = ContainerType.Stack;
            }
            else if (this.IsNextElement("LST"))
            {
                ct = ContainerType.List;
            }
            else if (this.IsNextElement("IE"))
            {
                ct = ContainerType.Enumerable;
            }
            else
            {
                ct = ContainerType.None;
            }
            return (ct != ContainerType.None);
        }

        private bool IsNextElement(string tag)
        {
            if (!(this._reader.LocalName == tag))
            {
                return false;
            }
            if ((this._context.options & DeserializationOptions.NoNamespace) == DeserializationOptions.None)
            {
                return (this._reader.NamespaceURI == "http://schemas.microsoft.com/powershell/2004/04");
            }
            return true;
        }

        internal static XmlDocument LoadUnsafeXmlDocument(FileInfo xmlPath, bool preserveNonElements, int? maxCharactersInDocument)
        {
            XmlDocument document = null;
            using (Stream stream = new FileStream(xmlPath.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                document = LoadUnsafeXmlDocument(stream, preserveNonElements, maxCharactersInDocument);
                stream.Close();
            }
            return document;
        }

        internal static XmlDocument LoadUnsafeXmlDocument(Stream stream, bool preserveNonElements, int? maxCharactersInDocument)
        {
            using (TextReader reader = new StreamReader(stream))
            {
                return LoadUnsafeXmlDocument(reader, preserveNonElements, maxCharactersInDocument);
            }
        }

        internal static XmlDocument LoadUnsafeXmlDocument(TextReader textReader, bool preserveNonElements, int? maxCharactersInDocument)
        {
            XmlReaderSettings xmlReaderSettingsForUntrustedXmlDocument;
            XmlDocument document2;
            if (maxCharactersInDocument.HasValue || preserveNonElements)
            {
                xmlReaderSettingsForUntrustedXmlDocument = XmlReaderSettingsForUntrustedXmlDocument.Clone();
                if (maxCharactersInDocument.HasValue)
                {
                    xmlReaderSettingsForUntrustedXmlDocument.MaxCharactersInDocument = (long) maxCharactersInDocument.Value;
                }
                if (preserveNonElements)
                {
                    xmlReaderSettingsForUntrustedXmlDocument.IgnoreWhitespace = false;
                    xmlReaderSettingsForUntrustedXmlDocument.IgnoreProcessingInstructions = false;
                    xmlReaderSettingsForUntrustedXmlDocument.IgnoreComments = false;
                }
            }
            else
            {
                xmlReaderSettingsForUntrustedXmlDocument = XmlReaderSettingsForUntrustedXmlDocument;
            }
            try
            {
                XmlReader reader = XmlReader.Create(textReader, xmlReaderSettingsForUntrustedXmlDocument);
                XmlDocument document = new XmlDocument {
                    PreserveWhitespace = preserveNonElements
                };
                document.Load(reader);
                document2 = document;
            }
            catch (InvalidOperationException exception)
            {
                throw new XmlException(exception.Message, exception);
            }
            return document2;
        }

        internal static XmlDocument LoadUnsafeXmlDocument(string xmlContents, bool preserveNonElements, int? maxCharactersInDocument)
        {
            using (TextReader reader = new StringReader(xmlContents))
            {
                return LoadUnsafeXmlDocument(reader, preserveNonElements, maxCharactersInDocument);
            }
        }

        private XmlException NewXmlException(string resourceString, Exception innerException, params object[] args)
        {
            string message = StringUtil.Format(resourceString, args);
            XmlException exception = null;
            XmlTextReader reader = this._reader as XmlTextReader;
            if ((reader != null) && reader.HasLineInfo())
            {
                exception = new XmlException(message, innerException, reader.LineNumber, reader.LinePosition);
            }
            if (exception == null)
            {
                exception = new XmlException(message, innerException);
            }
            return exception;
        }

        private PSObject ReadAttributeAndCreatePSObject()
        {
            string attribute = this._reader.GetAttribute("RefId");
            PSObject o = new PSObject();
            if (attribute != null)
            {
                _trace.WriteLine("Read PSObject with refId: {0}", new object[] { attribute });
                this.objectRefIdHandler.SetRefId(o, attribute, this.DuplicateRefIdsAllowed);
            }
            return o;
        }

        private string ReadDecodedElementString(string element)
        {
            this.CheckIfStopping();
            string s = null;
            if (DeserializationOptions.NoNamespace == (this._context.options & DeserializationOptions.NoNamespace))
            {
                s = this._reader.ReadElementString(element);
            }
            else
            {
                s = this._reader.ReadElementString(element, "http://schemas.microsoft.com/powershell/2004/04");
            }
            this._reader.MoveToContent();
            return DecodeString(s);
        }

        private object ReadDictionary(ContainerType ct)
        {
            Hashtable hashtable = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
            int num = 0;
            if (this.ReadStartElementAndHandleEmpty("DCT"))
            {
                while (this._reader.NodeType == XmlNodeType.Element)
                {
                    this.ReadStartElement("En");
                    if (this._reader.NodeType != XmlNodeType.Element)
                    {
                        throw this.NewXmlException(Serialization.DictionaryKeyNotSpecified, null, new object[0]);
                    }
                    if (string.Compare(this.ReadNameAttribute(), "Key", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        throw this.NewXmlException(Serialization.InvalidDictionaryKeyName, null, new object[0]);
                    }
                    object key = this.ReadOneObject();
                    if (key == null)
                    {
                        throw this.NewXmlException(Serialization.NullAsDictionaryKey, null, new object[0]);
                    }
                    if (this._reader.NodeType != XmlNodeType.Element)
                    {
                        throw this.NewXmlException(Serialization.DictionaryValueNotSpecified, null, new object[0]);
                    }
                    if (string.Compare(this.ReadNameAttribute(), "Value", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        throw this.NewXmlException(Serialization.InvalidDictionaryValueName, null, new object[0]);
                    }
                    object obj3 = this.ReadOneObject();
                    if (hashtable.ContainsKey(key) && (num == 0))
                    {
                        num++;
                        Hashtable hashtable2 = new Hashtable();
                        foreach (DictionaryEntry entry in hashtable)
                        {
                            hashtable2.Add(entry.Key, entry.Value);
                        }
                        hashtable = hashtable2;
                    }
                    if (hashtable.ContainsKey(key) && (num == 1))
                    {
                        num++;
                        IEqualityComparer equalityComparer = new ReferenceEqualityComparer();
                        Hashtable hashtable3 = new Hashtable(equalityComparer);
                        foreach (DictionaryEntry entry2 in hashtable)
                        {
                            hashtable3.Add(entry2.Key, entry2.Value);
                        }
                        hashtable = hashtable3;
                    }
                    try
                    {
                        hashtable.Add(key, obj3);
                    }
                    catch (ArgumentException exception)
                    {
                        throw this.NewXmlException(Serialization.InvalidPrimitiveType, exception, new object[] { typeof(Hashtable) });
                    }
                    this.ReadEndElement();
                }
                this.ReadEndElement();
            }
            return hashtable;
        }

        private void ReadEndElement()
        {
            this._reader.ReadEndElement();
            this._reader.MoveToContent();
        }

        private object ReadKnownContainer(ContainerType ct)
        {
            switch (ct)
            {
                case ContainerType.Dictionary:
                    return this.ReadDictionary(ct);

                case ContainerType.Queue:
                case ContainerType.Stack:
                case ContainerType.List:
                case ContainerType.Enumerable:
                    return this.ReadListContainer(ct);
            }
            return null;
        }

        private object ReadListContainer(ContainerType ct)
        {
            ArrayList col = new ArrayList();
            if (this.ReadStartElementAndHandleEmpty(this._reader.LocalName))
            {
                while (this._reader.NodeType == XmlNodeType.Element)
                {
                    col.Add(this.ReadOneObject());
                }
                this.ReadEndElement();
            }
            if (ct == ContainerType.Stack)
            {
                col.Reverse();
                return new Stack(col);
            }
            if (ct == ContainerType.Queue)
            {
                return new Queue(col);
            }
            return col;
        }

        private void ReadMemberSet(PSMemberInfoCollection<PSMemberInfo> collection)
        {
            if (this.ReadStartElementAndHandleEmpty("MS"))
            {
                while (this._reader.NodeType == XmlNodeType.Element)
                {
                    if (this.IsNextElement("MS"))
                    {
                        string name = this.ReadNameAttribute();
                        PSMemberSet member = new PSMemberSet(name);
                        collection.Add(member);
                        this.ReadMemberSet(member.Members);
                        PSGetMemberBinder.SetHasInstanceMember(name);
                    }
                    else
                    {
                        PSNoteProperty property = this.ReadNoteProperty();
                        collection.Add(property);
                        PSGetMemberBinder.SetHasInstanceMember(property.Name);
                    }
                }
                this.ReadEndElement();
            }
        }

        private string ReadNameAttribute()
        {
            string attribute = this._reader.GetAttribute("N");
            if (attribute == null)
            {
                throw this.NewXmlException(Serialization.AttributeExpected, null, new object[] { "N" });
            }
            return DecodeString(attribute);
        }

        private PSNoteProperty ReadNoteProperty()
        {
            string name = this.ReadNameAttribute();
            return new PSNoteProperty(name, this.ReadOneObject());
        }

        private object ReadOneDeserializedObject(out string streamName, out bool isKnownPrimitiveType)
        {
            if (this._reader.NodeType != XmlNodeType.Element)
            {
                throw this.NewXmlException(Serialization.InvalidNodeType, null, new object[] { this._reader.NodeType.ToString(), XmlNodeType.Element.ToString() });
            }
            _trace.WriteLine("Processing start node {0}", new object[] { this._reader.LocalName });
            streamName = this._reader.GetAttribute("S");
            isKnownPrimitiveType = false;
            if (this.IsNextElement("Nil"))
            {
                this.Skip();
                return null;
            }
            if (this.IsNextElement("Ref"))
            {
                string attribute = this._reader.GetAttribute("RefId");
                if (attribute == null)
                {
                    throw this.NewXmlException(Serialization.AttributeExpected, null, new object[] { "RefId" });
                }
                object referencedObject = this.objectRefIdHandler.GetReferencedObject(attribute);
                if (referencedObject == null)
                {
                    throw this.NewXmlException(Serialization.InvalidReferenceId, null, new object[] { attribute });
                }
                this.Skip();
                return referencedObject;
            }
            TypeSerializationInfo typeSerializationInfoFromItemTag = KnownTypes.GetTypeSerializationInfoFromItemTag(this._reader.LocalName);
            if (typeSerializationInfoFromItemTag != null)
            {
                _trace.WriteLine("Primitive Knowntype Element {0}", new object[] { typeSerializationInfoFromItemTag.ItemTag });
                isKnownPrimitiveType = true;
                return this.ReadPrimaryKnownType(typeSerializationInfoFromItemTag);
            }
            if (this.IsNextElement("Obj"))
            {
                _trace.WriteLine("PSObject Element", new object[0]);
                return this.ReadPSObject();
            }
            _trace.TraceError("Invalid element {0} tag found", new object[] { this._reader.LocalName });
            throw this.NewXmlException(Serialization.InvalidElementTag, null, new object[] { this._reader.LocalName });
        }

        private object ReadOneObject()
        {
            string str;
            return this.ReadOneObject(out str);
        }

        internal object ReadOneObject(out string streamName)
        {
            object obj5;
            this.CheckIfStopping();
            try
            {
                bool flag;
                this.depthBelowTopLevel++;
                if (this.depthBelowTopLevel == 50)
                {
                    throw this.NewXmlException(Serialization.DeserializationTooDeep, null, new object[0]);
                }
                object obj2 = this.ReadOneDeserializedObject(out streamName, out flag);
                if (obj2 == null)
                {
                    return null;
                }
                if (!flag)
                {
                    PSObject o = PSObject.AsPSObject(obj2);
                    if (Deserializer.IsDeserializedInstanceOfType(o, typeof(CimInstance)))
                    {
                        return this.RehydrateCimInstance(o);
                    }
                    Type targetTypeForDeserialization = o.GetTargetTypeForDeserialization(this._typeTable);
                    if (null != targetTypeForDeserialization)
                    {
                        Exception exception = null;
                        try
                        {
                            object obj4 = LanguagePrimitives.ConvertTo(obj2, targetTypeForDeserialization, true, CultureInfo.InvariantCulture, this._typeTable);
                            PSEtwLog.LogAnalyticVerbose(PSEventId.Serializer_RehydrationSuccess, PSOpcode.Rehydration, PSTask.Serialization, PSKeyword.Serializer, new object[] { o.InternalTypeNames.Key, targetTypeForDeserialization.FullName, obj4.GetType().FullName });
                            return obj4;
                        }
                        catch (InvalidCastException exception2)
                        {
                            exception = exception2;
                        }
                        catch (ArgumentException exception3)
                        {
                            exception = exception3;
                        }
                        PSEtwLog.LogAnalyticError(PSEventId.Serializer_RehydrationFailure, PSOpcode.Rehydration, PSTask.Serialization, PSKeyword.Serializer, new object[] { o.InternalTypeNames.Key, targetTypeForDeserialization.FullName, exception.ToString(), (exception.InnerException == null) ? string.Empty : exception.InnerException.ToString() });
                    }
                }
                obj5 = obj2;
            }
            finally
            {
                this.depthBelowTopLevel--;
            }
            return obj5;
        }

        private object ReadPrimaryKnownType(TypeSerializationInfo pktInfo)
        {
            object obj2 = pktInfo.Deserializer(this);
            this._reader.MoveToContent();
            return obj2;
        }

        private void ReadProperties(PSObject dso)
        {
            dso.isDeserialized = true;
            dso.adaptedMembers = new PSMemberInfoInternalCollection<PSPropertyInfo>();
            dso.clrMembers = new PSMemberInfoInternalCollection<PSPropertyInfo>();
            if (this.ReadStartElementAndHandleEmpty("Props"))
            {
                while (this._reader.NodeType == XmlNodeType.Element)
                {
                    string name = this.ReadNameAttribute();
                    object serializedValue = this.ReadOneObject();
                    PSProperty member = new PSProperty(name, serializedValue);
                    dso.adaptedMembers.Add(member);
                }
                this.ReadEndElement();
            }
        }

        private PSObject ReadPSObject()
        {
            PSObject dso = this.ReadAttributeAndCreatePSObject();
            if (this.ReadStartElementAndHandleEmpty("Obj"))
            {
                bool overrideTypeInfo = true;
                while (this._reader.NodeType == XmlNodeType.Element)
                {
                    if (this.IsNextElement("TN") || this.IsNextElement("TNRef"))
                    {
                        this.ReadTypeNames(dso);
                        overrideTypeInfo = false;
                    }
                    else
                    {
                        if (this.IsNextElement("Props"))
                        {
                            this.ReadProperties(dso);
                            continue;
                        }
                        if (this.IsNextElement("MS"))
                        {
                            this.ReadMemberSet(dso.InstanceMembers);
                            continue;
                        }
                        if (this.IsNextElement("ToString"))
                        {
                            dso.ToStringFromDeserialization = this.ReadDecodedElementString("ToString");
                            dso.InstanceMembers.Add(PSObject.dotNetInstanceAdapter.GetDotNetMethod<PSMemberInfo>(dso, "ToString"));
                            PSGetMemberBinder.SetHasInstanceMember("ToString");
                            dso.TokenText = dso.ToStringFromDeserialization;
                            continue;
                        }
                        object obj3 = null;
                        ContainerType none = ContainerType.None;
                        TypeSerializationInfo typeSerializationInfoFromItemTag = KnownTypes.GetTypeSerializationInfoFromItemTag(this._reader.LocalName);
                        if (typeSerializationInfoFromItemTag != null)
                        {
                            _trace.WriteLine("Primitive Knowntype Element {0}", new object[] { typeSerializationInfoFromItemTag.ItemTag });
                            obj3 = this.ReadPrimaryKnownType(typeSerializationInfoFromItemTag);
                        }
                        else if (this.IsKnownContainerTag(out none))
                        {
                            _trace.WriteLine("Found container node {0}", new object[] { none });
                            obj3 = this.ReadKnownContainer(none);
                        }
                        else if (this.IsNextElement("Obj"))
                        {
                            _trace.WriteLine("Found PSObject node", new object[0]);
                            obj3 = this.ReadOneObject();
                        }
                        else
                        {
                            _trace.WriteLine("Unknwon tag {0} encountered", new object[] { this._reader.LocalName });
                            if (!this.UnknownTagsAllowed)
                            {
                                throw this.NewXmlException(Serialization.InvalidElementTag, null, new object[] { this._reader.LocalName });
                            }
                            this.Skip();
                        }
                        if (obj3 != null)
                        {
                            dso.SetCoreOnDeserialization(obj3, overrideTypeInfo);
                        }
                    }
                }
                this.ReadEndElement();
                PSObject immediateBaseObject = dso.ImmediateBaseObject as PSObject;
                if (immediateBaseObject != null)
                {
                    PSObject.CopyDeserializerFields(immediateBaseObject, dso);
                }
            }
            return dso;
        }

        private object ReadSecureString()
        {
            object obj3;
            string encryptedString = this._reader.ReadElementString();
            try
            {
                object obj2;
                if (this._context.cryptoHelper != null)
                {
                    obj2 = this._context.cryptoHelper.DecryptSecureString(encryptedString);
                }
                else
                {
                    obj2 = SecureStringHelper.Unprotect(encryptedString);
                }
                this._reader.MoveToContent();
                obj3 = obj2;
            }
            catch (PSCryptoException)
            {
                throw this.NewXmlException(Serialization.DeserializeSecureStringFailed, null, new object[0]);
            }
            return obj3;
        }

        private void ReadStartElement(string element)
        {
            if (DeserializationOptions.NoNamespace == (this._context.options & DeserializationOptions.NoNamespace))
            {
                this._reader.ReadStartElement(element);
            }
            else
            {
                this._reader.ReadStartElement(element, "http://schemas.microsoft.com/powershell/2004/04");
            }
            this._reader.MoveToContent();
        }

        internal bool ReadStartElementAndHandleEmpty(string element)
        {
            bool isEmptyElement = this._reader.IsEmptyElement;
            this.ReadStartElement(element);
            if (!isEmptyElement && (this._reader.NodeType == XmlNodeType.EndElement))
            {
                this.ReadEndElement();
                isEmptyElement = true;
            }
            return !isEmptyElement;
        }

        private void ReadTypeNames(PSObject dso)
        {
            if (!this.IsNextElement("TN"))
            {
                if (this.IsNextElement("TNRef"))
                {
                    string attribute = this._reader.GetAttribute("RefId");
                    _trace.WriteLine("Processing TypeNamesReferenceTag with refId {0}", new object[] { attribute });
                    if (attribute == null)
                    {
                        throw this.NewXmlException(Serialization.AttributeExpected, null, new object[] { "RefId" });
                    }
                    ConsolidatedString referencedObject = this.typeRefIdHandler.GetReferencedObject(attribute);
                    if (referencedObject == null)
                    {
                        throw this.NewXmlException(Serialization.InvalidTypeHierarchyReferenceId, null, new object[] { attribute });
                    }
                    this._context.LogExtraMemoryUsage((referencedObject.Key.Length * 2) - 0x1d);
                    dso.InternalTypeNames = new ConsolidatedString(referencedObject);
                    this.Skip();
                }
            }
            else
            {
                Collection<string> strings = new Collection<string>();
                string refId = this._reader.GetAttribute("RefId");
                _trace.WriteLine("Processing TypeNamesTag with refId {0}", new object[] { refId });
                if (this.ReadStartElementAndHandleEmpty("TN"))
                {
                    while (this._reader.NodeType == XmlNodeType.Element)
                    {
                        if (!this.IsNextElement("T"))
                        {
                            throw this.NewXmlException(Serialization.InvalidElementTag, null, new object[] { this._reader.LocalName });
                        }
                        string str2 = this.ReadDecodedElementString("T");
                        if (!string.IsNullOrEmpty(str2))
                        {
                            Deserializer.AddDeserializationPrefix(ref str2);
                            strings.Add(str2);
                        }
                    }
                    this.ReadEndElement();
                }
                dso.InternalTypeNames = new ConsolidatedString(strings);
                if (refId != null)
                {
                    this.typeRefIdHandler.SetRefId(dso.InternalTypeNames, refId, this.DuplicateRefIdsAllowed);
                }
            }
        }

        private CimClass RehydrateCimClass(PSPropertyInfo classMetadataProperty)
        {
            if ((classMetadataProperty == null) || (classMetadataProperty.Value == null))
            {
                return null;
            }
            IEnumerable enumerable = LanguagePrimitives.GetEnumerable(classMetadataProperty.Value);
            if (enumerable == null)
            {
                return null;
            }
            Stack<KeyValuePair<CimClassSerializationId, CimClass>> stack = new Stack<KeyValuePair<CimClassSerializationId, CimClass>>();
            CimClass parentClass = null;
            CimClass cimClassFromCache = null;
            foreach (object obj2 in enumerable)
            {
                parentClass = cimClassFromCache;
                if (obj2 == null)
                {
                    return null;
                }
                PSObject obj3 = PSObject.AsPSObject(obj2);
                PSPropertyInfo info = obj3.InstanceMembers["Namespace"] as PSPropertyInfo;
                if (info == null)
                {
                    return null;
                }
                string namespaceName = info.Value as string;
                PSPropertyInfo info2 = obj3.InstanceMembers["ClassName"] as PSPropertyInfo;
                if (info2 == null)
                {
                    return null;
                }
                string className = info2.Value as string;
                PSPropertyInfo info3 = obj3.InstanceMembers["ServerName"] as PSPropertyInfo;
                if (info3 == null)
                {
                    return null;
                }
                string computerName = info3.Value as string;
                PSPropertyInfo info4 = obj3.InstanceMembers["Hash"] as PSPropertyInfo;
                if (info4 == null)
                {
                    return null;
                }
                object baseObject = info4.Value;
                if (baseObject == null)
                {
                    return null;
                }
                if (baseObject is PSObject)
                {
                    baseObject = ((PSObject) baseObject).BaseObject;
                }
                if (!(baseObject is int))
                {
                    return null;
                }
                int hashCode = (int) baseObject;
                CimClassSerializationId key = new CimClassSerializationId(className, namespaceName, computerName, hashCode);
                cimClassFromCache = this._context.cimClassSerializationIdCache.GetCimClassFromCache(key);
                if (cimClassFromCache == null)
                {
                    PSPropertyInfo info5 = obj3.InstanceMembers["MiXml"] as PSPropertyInfo;
                    if ((info5 == null) || (info5.Value == null))
                    {
                        return null;
                    }
                    string s = info5.Value.ToString();
                    byte[] bytes = Encoding.Unicode.GetBytes(s);
                    int offset = 0;
                    try
                    {
                        cimClassFromCache = cimDeserializer.Value.DeserializeClass(bytes, ref offset, parentClass, computerName, namespaceName);
                        stack.Push(new KeyValuePair<CimClassSerializationId, CimClass>(key, cimClassFromCache));
                    }
                    catch (CimException)
                    {
                        return null;
                    }
                }
            }
            foreach (KeyValuePair<CimClassSerializationId, CimClass> pair in stack)
            {
                this._context.cimClassSerializationIdCache.AddCimClassToCache(pair.Key, pair.Value);
            }
            return cimClassFromCache;
        }

        private PSObject RehydrateCimInstance(PSObject deserializedObject)
        {
            CimInstance instance;
            if (!(deserializedObject.BaseObject is PSCustomObject))
            {
                return deserializedObject;
            }
            PSPropertyInfo classMetadataProperty = deserializedObject.InstanceMembers["__ClassMetadata"] as PSPropertyInfo;
            CimClass cimClass = this.RehydrateCimClass(classMetadataProperty);
            if (cimClass == null)
            {
                return deserializedObject;
            }
            try
            {
                instance = new CimInstance(cimClass);
            }
            catch (CimException)
            {
                return deserializedObject;
            }
            PSObject obj2 = PSObject.AsPSObject(instance);
            HashSet<string> namesOfModifiedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            PSPropertyInfo info2 = deserializedObject.InstanceMembers["__InstanceMetadata"] as PSPropertyInfo;
            if ((info2 != null) && (info2.Value != null))
            {
                PSPropertyInfo info3 = PSObject.AsPSObject(info2.Value).InstanceMembers["Modified"] as PSPropertyInfo;
                if ((info3 != null) && (info3.Value != null))
                {
                    foreach (string str2 in info3.Value.ToString().Split(new char[] { ' ' }))
                    {
                        namesOfModifiedProperties.Add(str2);
                    }
                }
            }
            if (deserializedObject.adaptedMembers != null)
            {
                foreach (PSMemberInfo info4 in deserializedObject.adaptedMembers)
                {
                    PSPropertyInfo deserializedProperty = info4 as PSPropertyInfo;
                    if ((deserializedProperty != null) && !this.RehydrateCimInstanceProperty(instance, deserializedProperty, namesOfModifiedProperties))
                    {
                        return deserializedObject;
                    }
                }
            }
            foreach (PSMemberInfo info6 in deserializedObject.InstanceMembers)
            {
                PSPropertyInfo info7 = info6 as PSPropertyInfo;
                if (((info7 != null) && ((deserializedObject.adaptedMembers == null) || (deserializedObject.adaptedMembers[info7.Name] == null))) && (!info7.Name.Equals("__ClassMetadata", StringComparison.OrdinalIgnoreCase) && (obj2.Properties[info7.Name] == null)))
                {
                    PSNoteProperty member = new PSNoteProperty(info7.Name, info7.Value);
                    obj2.Properties.Add(member);
                }
            }
            return obj2;
        }

        private bool RehydrateCimInstanceProperty(CimInstance cimInstance, PSPropertyInfo deserializedProperty, HashSet<string> namesOfModifiedProperties)
        {
            if (deserializedProperty.Name.Equals(RemotingConstants.ComputerNameNoteProperty, StringComparison.OrdinalIgnoreCase))
            {
                string computerName = deserializedProperty.Value as string;
                if (computerName != null)
                {
                    cimInstance.SetCimSessionComputerName(computerName);
                }
                return true;
            }
            CimProperty property = cimInstance.CimInstanceProperties[deserializedProperty.Name];
            if (property == null)
            {
                return false;
            }
            object baseObject = deserializedProperty.Value;
            if (baseObject != null)
            {
                PSObject obj3 = PSObject.AsPSObject(baseObject);
                if (obj3.BaseObject is ArrayList)
                {
                    Type type;
                    object obj4;
                    if ((obj3.InternalTypeNames == null) || (obj3.InternalTypeNames.Count == 0))
                    {
                        return false;
                    }
                    string valueToConvert = Deserializer.MaskDeserializationPrefix(obj3.InternalTypeNames[0]);
                    if (valueToConvert == null)
                    {
                        return false;
                    }
                    if (!LanguagePrimitives.TryConvertTo<Type>(valueToConvert, CultureInfo.InvariantCulture, out type))
                    {
                        return false;
                    }
                    if (!type.IsArray)
                    {
                        return false;
                    }
                    if (!LanguagePrimitives.TryConvertTo(baseObject, type, CultureInfo.InvariantCulture, out obj4))
                    {
                        return false;
                    }
                    obj3 = PSObject.AsPSObject(obj4);
                }
                baseObject = obj3.BaseObject;
            }
            try
            {
                property.Value = baseObject;
                if (!namesOfModifiedProperties.Contains(deserializedProperty.Name))
                {
                    property.IsValueModified = false;
                }
            }
            catch (FormatException)
            {
                return false;
            }
            catch (InvalidCastException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (CimException)
            {
                return false;
            }
            return true;
        }

        private void Skip()
        {
            this._reader.Skip();
            this._reader.MoveToContent();
        }

        internal void Stop()
        {
            this.isStopping = true;
        }

        internal void ValidateVersion(string version)
        {
            this._version = null;
            Exception innerException = null;
            try
            {
                this._version = new Version(version);
            }
            catch (ArgumentException exception2)
            {
                innerException = exception2;
            }
            catch (FormatException exception3)
            {
                innerException = exception3;
            }
            if (innerException != null)
            {
                throw this.NewXmlException(Serialization.InvalidVersion, innerException, new object[0]);
            }
            if (this._version.Major != 1)
            {
                throw this.NewXmlException(Serialization.UnexpectedVersion, null, new object[] { this._version.Major });
            }
        }

        private bool DuplicateRefIdsAllowed
        {
            get
            {
                return true;
            }
        }

        internal System.Management.Automation.Runspaces.TypeTable TypeTable
        {
            get
            {
                return this._typeTable;
            }
            set
            {
                this._typeTable = value;
            }
        }

        private bool UnknownTagsAllowed
        {
            get
            {
                return (this._version.Minor > 1);
            }
        }

        internal static XmlReaderSettings XmlReaderSettingsForCliXml
        {
            get
            {
                return xmlReaderSettingsForCliXml;
            }
        }

        internal static XmlReaderSettings XmlReaderSettingsForUntrustedXmlDocument
        {
            get
            {
                return xmlReaderSettingsForUntrustedXmlDocument;
            }
        }
    }
}

