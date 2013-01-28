namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Security;
    using System.Xml;

    internal static class KnownTypes
    {
        private static readonly Hashtable _knownTableKeyItemTag = new Hashtable();
        private static readonly Hashtable _knownTableKeyType = new Hashtable();
		private static readonly TypeSerializationInfo _xdInfo = new TypeSerializationInfo(typeof(XmlDocument), "XD", "XD", new TypeSerializerDelegate(InternalSerializer.WriteXmlDocument), new TypeDeserializerDelegate(InternalDeserializer.DeserializeXmlDocument));

        private static readonly TypeSerializationInfo[] _TypeSerializationInfo = new TypeSerializationInfo[] { 
            new TypeSerializationInfo(typeof(bool), "B", "B", new TypeSerializerDelegate(InternalSerializer.WriteBoolean), new TypeDeserializerDelegate(InternalDeserializer.DeserializeBoolean)), new TypeSerializationInfo(typeof(byte), "By", "By", null, new TypeDeserializerDelegate(InternalDeserializer.DeserializeByte)), new TypeSerializationInfo(typeof(char), "C", "C", new TypeSerializerDelegate(InternalSerializer.WriteChar), new TypeDeserializerDelegate(InternalDeserializer.DeserializeChar)), new TypeSerializationInfo(typeof(DateTime), "DT", "DT", new TypeSerializerDelegate(InternalSerializer.WriteDateTime), new TypeDeserializerDelegate(InternalDeserializer.DeserializeDateTime)), new TypeSerializationInfo(typeof(decimal), "D", "D", null, new TypeDeserializerDelegate(InternalDeserializer.DeserializeDecimal)), new TypeSerializationInfo(typeof(double), "Db", "Db", new TypeSerializerDelegate(InternalSerializer.WriteDouble), new TypeDeserializerDelegate(InternalDeserializer.DeserializeDouble)), new TypeSerializationInfo(typeof(Guid), "G", "G", null, new TypeDeserializerDelegate(InternalDeserializer.DeserializeGuid)), new TypeSerializationInfo(typeof(short), "I16", "I16", null, new TypeDeserializerDelegate(InternalDeserializer.DeserializeInt16)), new TypeSerializationInfo(typeof(int), "I32", "I32", null, new TypeDeserializerDelegate(InternalDeserializer.DeserializeInt32)), new TypeSerializationInfo(typeof(long), "I64", "I64", null, new TypeDeserializerDelegate(InternalDeserializer.DeserializeInt64)), new TypeSerializationInfo(typeof(sbyte), "SB", "SB", null, new TypeDeserializerDelegate(InternalDeserializer.DeserializeSByte)), new TypeSerializationInfo(typeof(float), "Sg", "Sg", new TypeSerializerDelegate(InternalSerializer.WriteSingle), new TypeDeserializerDelegate(InternalDeserializer.DeserializeSingle)), new TypeSerializationInfo(typeof(ScriptBlock), "SBK", "SBK", new TypeSerializerDelegate(InternalSerializer.WriteScriptBlock), new TypeDeserializerDelegate(InternalDeserializer.DeserializeScriptBlock)), new TypeSerializationInfo(typeof(string), "S", "S", new TypeSerializerDelegate(InternalSerializer.WriteEncodedString), new TypeDeserializerDelegate(InternalDeserializer.DeserializeString)), new TypeSerializationInfo(typeof(TimeSpan), "TS", "TS", new TypeSerializerDelegate(InternalSerializer.WriteTimeSpan), new TypeDeserializerDelegate(InternalDeserializer.DeserializeTimeSpan)), new TypeSerializationInfo(typeof(ushort), "U16", "U16", null, new TypeDeserializerDelegate(InternalDeserializer.DeserializeUInt16)), 
            new TypeSerializationInfo(typeof(uint), "U32", "U32", null, new TypeDeserializerDelegate(InternalDeserializer.DeserializeUInt32)), new TypeSerializationInfo(typeof(ulong), "U64", "U64", null, new TypeDeserializerDelegate(InternalDeserializer.DeserializeUInt64)), new TypeSerializationInfo(typeof(Uri), "URI", "URI", new TypeSerializerDelegate(InternalSerializer.WriteUri), new TypeDeserializerDelegate(InternalDeserializer.DeserializeUri)), new TypeSerializationInfo(typeof(byte[]), "BA", "BA", new TypeSerializerDelegate(InternalSerializer.WriteByteArray), new TypeDeserializerDelegate(InternalDeserializer.DeserializeByteArray)), 
			new TypeSerializationInfo(typeof(Version), "Version", "Version", new TypeSerializerDelegate(InternalSerializer.WriteVersion), new TypeDeserializerDelegate(InternalDeserializer.DeserializeVersion)), _xdInfo, new TypeSerializationInfo(typeof(ProgressRecord), "PR", "PR", new TypeSerializerDelegate(InternalSerializer.WriteProgressRecord), new TypeDeserializerDelegate(InternalDeserializer.DeserializeProgressRecord)), new TypeSerializationInfo(typeof(SecureString), "SS", "SS", new TypeSerializerDelegate(InternalSerializer.WriteSecureString), new TypeDeserializerDelegate(InternalDeserializer.DeserializeSecureString))
         };
        
        static KnownTypes()
        {
            for (int i = 0; i < _TypeSerializationInfo.Length; i++)
            {
                _knownTableKeyType.Add(_TypeSerializationInfo[i].Type.FullName, _TypeSerializationInfo[i]);
                _knownTableKeyItemTag.Add(_TypeSerializationInfo[i].ItemTag, _TypeSerializationInfo[i]);
            }
        }

        internal static TypeSerializationInfo GetTypeSerializationInfo(Type type)
        {
            TypeSerializationInfo info = (TypeSerializationInfo) _knownTableKeyType[type.FullName];
            if ((info == null) && typeof(XmlDocument).IsAssignableFrom(type))
            {
                info = _xdInfo;
            }
            return info;
        }

        internal static TypeSerializationInfo GetTypeSerializationInfoFromItemTag(string itemTag)
        {
            return (TypeSerializationInfo) _knownTableKeyItemTag[itemTag];
        }
    }
}

