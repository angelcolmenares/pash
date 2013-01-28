namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Xml;

    internal static class StringToBase64Converter
    {
        internal static object[] Base64ToArgsConverter(string base64)
        {
            if (string.IsNullOrEmpty(base64))
            {
                throw PSTraceSource.NewArgumentNullException("base64");
            }
            string s = new string(Encoding.Unicode.GetChars(Convert.FromBase64String(base64)));
            Deserializer deserializer = new Deserializer(XmlReader.Create(new StringReader(s), InternalDeserializer.XmlReaderSettingsForCliXml));
            object obj2 = deserializer.Deserialize();
            if (!deserializer.Done())
            {
                throw PSTraceSource.NewArgumentException("-args");
            }
            PSObject obj3 = obj2 as PSObject;
            if (obj3 == null)
            {
                throw PSTraceSource.NewArgumentException("-args");
            }
            ArrayList baseObject = obj3.BaseObject as ArrayList;
            if (baseObject == null)
            {
                throw PSTraceSource.NewArgumentException("-args");
            }
            return baseObject.ToArray();
        }

        internal static string Base64ToString(string base64)
        {
            if (string.IsNullOrEmpty(base64))
            {
                throw PSTraceSource.NewArgumentNullException("base64");
            }
            return new string(Encoding.Unicode.GetChars(Convert.FromBase64String(base64)));
        }

        internal static string StringToBase64String(string input)
        {
            if (input == null)
            {
                throw PSTraceSource.NewArgumentNullException("input");
            }
            return Convert.ToBase64String(Encoding.Unicode.GetBytes(input.ToCharArray()));
        }
    }
}

