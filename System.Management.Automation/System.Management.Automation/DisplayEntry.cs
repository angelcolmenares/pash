namespace System.Management.Automation
{
    using System;
    using System.Xml;

    public sealed class DisplayEntry
    {
        private static string _safeScriptBlock = ";";
        private static string _tagPropertyName = "PropertyName";
        private static string _tagScriptBlock = "ScriptBlock";
        private DisplayEntryValueType _type;
        private string _value;

        internal DisplayEntry()
        {
        }

        public DisplayEntry(string value, DisplayEntryValueType type)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw PSTraceSource.NewArgumentNullException("value");
            }
            this._value = value;
            this._type = type;
        }

        public override string ToString()
        {
            return this._value;
        }

        internal void WriteToXML(XmlWriter _writer, bool exportScriptBlock)
        {
            if (this._type == DisplayEntryValueType.Property)
            {
                _writer.WriteElementString(_tagPropertyName, this._value);
            }
            else if (this._type == DisplayEntryValueType.ScriptBlock)
            {
                _writer.WriteStartElement(_tagScriptBlock);
                if (exportScriptBlock)
                {
                    _writer.WriteValue(this._value);
                }
                else
                {
                    _writer.WriteValue(_safeScriptBlock);
                }
                _writer.WriteEndElement();
            }
        }

        public string Value
        {
            get
            {
                return this._value;
            }
            internal set
            {
                this._value = value;
            }
        }

        public DisplayEntryValueType ValueType
        {
            get
            {
                return this._type;
            }
            internal set
            {
                this._type = value;
            }
        }
    }
}

