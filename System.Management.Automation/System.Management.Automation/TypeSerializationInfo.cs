namespace System.Management.Automation
{
    using System;

    internal class TypeSerializationInfo
    {
        private readonly TypeDeserializerDelegate _deserializer;
        private readonly string _itemTag;
        private readonly string _propertyTag;
        private readonly TypeSerializerDelegate _serializer;
        private readonly System.Type _type;

        internal TypeSerializationInfo(System.Type type, string itemTag, string propertyTag, TypeSerializerDelegate serializer, TypeDeserializerDelegate deserializer)
        {
            this._type = type;
            this._serializer = serializer;
            this._deserializer = deserializer;
            this._itemTag = itemTag;
            this._propertyTag = propertyTag;
        }

        internal TypeDeserializerDelegate Deserializer
        {
            get
            {
                return this._deserializer;
            }
        }

        internal string ItemTag
        {
            get
            {
                return this._itemTag;
            }
        }

        internal string PropertyTag
        {
            get
            {
                return this._propertyTag;
            }
        }

        internal TypeSerializerDelegate Serializer
        {
            get
            {
                return this._serializer;
            }
        }

        internal System.Type Type
        {
            get
            {
                return this._type;
            }
        }
    }
}

