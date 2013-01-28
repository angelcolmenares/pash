namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal static class JsonWriterExtensions
    {
        private static void WriteJsonArrayValue(this JsonWriter jsonWriter, IEnumerable arrayValue, ODataVersion odataVersion)
        {
            jsonWriter.StartArrayScope();
            foreach (object obj2 in arrayValue)
            {
                jsonWriter.WriteJsonValue(obj2, odataVersion);
            }
            jsonWriter.EndArrayScope();
        }

        internal static void WriteJsonObjectValue(this JsonWriter jsonWriter, IDictionary<string, object> jsonObjectValue, string typeName, ODataVersion odataVersion)
        {
            jsonWriter.StartObjectScope();
            if (typeName != null)
            {
                jsonWriter.WriteName("__metadata");
                jsonWriter.StartObjectScope();
                jsonWriter.WriteName("type");
                jsonWriter.WriteValue(typeName);
                jsonWriter.EndObjectScope();
            }
            foreach (KeyValuePair<string, object> pair in jsonObjectValue)
            {
                jsonWriter.WriteName(pair.Key);
                jsonWriter.WriteJsonValue(pair.Value, odataVersion);
            }
            jsonWriter.EndObjectScope();
        }

        private static void WriteJsonValue(this JsonWriter jsonWriter, object propertyValue, ODataVersion odataVersion)
        {
            if (propertyValue == null)
            {
                jsonWriter.WriteValue((string) null);
            }
            else if (EdmLibraryExtensions.IsPrimitiveType(propertyValue.GetType()))
            {
                jsonWriter.WritePrimitiveValue(propertyValue, odataVersion);
            }
            else
            {
                IDictionary<string, object> jsonObjectValue = propertyValue as IDictionary<string, object>;
                if (jsonObjectValue != null)
                {
                    jsonWriter.WriteJsonObjectValue(jsonObjectValue, null, odataVersion);
                }
                else
                {
                    IEnumerable arrayValue = propertyValue as IEnumerable;
                    jsonWriter.WriteJsonArrayValue(arrayValue, odataVersion);
                }
            }
        }

        internal static void WritePrimitiveValue(this JsonWriter jsonWriter, object value, ODataVersion odataVersion)
        {
            switch (PlatformHelper.GetTypeCode(value.GetType()))
            {
                case TypeCode.Boolean:
                    jsonWriter.WriteValue((bool) value);
                    return;

                case TypeCode.SByte:
                    jsonWriter.WriteValue((sbyte) value);
                    return;

                case TypeCode.Byte:
                    jsonWriter.WriteValue((byte) value);
                    return;

                case TypeCode.Int16:
                    jsonWriter.WriteValue((short) value);
                    return;

                case TypeCode.Int32:
                    jsonWriter.WriteValue((int) value);
                    return;

                case TypeCode.Int64:
                    jsonWriter.WriteValue((long) value);
                    return;

                case TypeCode.Single:
                    jsonWriter.WriteValue((float) value);
                    return;

                case TypeCode.Double:
                    jsonWriter.WriteValue((double) value);
                    return;

                case TypeCode.Decimal:
                    jsonWriter.WriteValue((decimal) value);
                    return;

                case TypeCode.DateTime:
                    jsonWriter.WriteValue((DateTime) value, odataVersion);
                    return;

                case TypeCode.String:
                    jsonWriter.WriteValue((string) value);
                    return;
            }
            byte[] inArray = value as byte[];
            if (inArray != null)
            {
                jsonWriter.WriteValue(Convert.ToBase64String(inArray));
            }
            else if (value is DateTimeOffset)
            {
                jsonWriter.WriteValue((DateTimeOffset) value, odataVersion);
            }
            else if (value is Guid)
            {
                jsonWriter.WriteValue((Guid) value);
            }
            else
            {
                if (!(value is TimeSpan))
                {
                    throw new ODataException(Strings.ODataJsonWriter_UnsupportedValueType(value.GetType().FullName));
                }
                jsonWriter.WriteValue((TimeSpan) value);
            }
        }
    }
}

