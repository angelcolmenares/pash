namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.OData;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal static class JsonReaderExtensions
    {
        [Conditional("DEBUG")]
        internal static void AssertBuffering(this BufferingJsonReader bufferedJsonReader)
        {
        }

        [Conditional("DEBUG")]
        internal static void AssertNotBuffering(this BufferingJsonReader bufferedJsonReader)
        {
        }

        internal static ODataException CreateException(string exceptionMessage)
        {
            return new ODataException(exceptionMessage);
        }

        internal static string GetPropertyName(this JsonReader jsonReader)
        {
            return (string) jsonReader.Value;
        }

        internal static double? ReadDoubleValue(this JsonReader jsonReader)
        {
            object obj2 = jsonReader.ReadPrimitiveValue();
            double? nullable = obj2 as double?;
            if ((obj2 == null) || nullable.HasValue)
            {
                return nullable;
            }
            int? nullable2 = obj2 as int?;
            if (!nullable2.HasValue)
            {
                throw CreateException(Strings.JsonReaderExtensions_CannotReadValueAsDouble(obj2));
            }
            return new double?((double) nullable2.Value);
        }

        internal static void ReadEndArray(this JsonReader jsonReader)
        {
            jsonReader.ReadNext(JsonNodeType.EndArray);
        }

        internal static void ReadEndObject(this JsonReader jsonReader)
        {
            jsonReader.ReadNext(JsonNodeType.EndObject);
        }

        internal static JsonNodeType ReadNext(this JsonReader jsonReader)
        {
            jsonReader.Read();
            return jsonReader.NodeType;
        }

        private static void ReadNext(this JsonReader jsonReader, JsonNodeType expectedNodeType)
        {
            jsonReader.ValidateNodeType(expectedNodeType);
            jsonReader.Read();
        }

        internal static object ReadPrimitiveValue(this JsonReader jsonReader)
        {
            object obj2 = jsonReader.Value;
            jsonReader.ReadNext(JsonNodeType.PrimitiveValue);
            return obj2;
        }

        internal static string ReadPropertyName(this JsonReader jsonReader)
        {
            jsonReader.ValidateNodeType(JsonNodeType.Property);
            string propertyName = jsonReader.GetPropertyName();
            jsonReader.ReadNext();
            return propertyName;
        }

        internal static void ReadStartArray(this JsonReader jsonReader)
        {
            jsonReader.ReadNext(JsonNodeType.StartArray);
        }

        internal static void ReadStartObject(this JsonReader jsonReader)
        {
            jsonReader.ReadNext(JsonNodeType.StartObject);
        }

        internal static string ReadStringValue(this JsonReader jsonReader)
        {
            object obj2 = jsonReader.ReadPrimitiveValue();
            string str = obj2 as string;
            if ((obj2 != null) && (str == null))
            {
                throw CreateException(Strings.JsonReaderExtensions_CannotReadValueAsString(obj2));
            }
            return str;
        }

        internal static string ReadStringValue(this JsonReader jsonReader, string propertyName)
        {
            object obj2 = jsonReader.ReadPrimitiveValue();
            string str = obj2 as string;
            if ((obj2 != null) && (str == null))
            {
                throw CreateException(Strings.JsonReaderExtensions_CannotReadPropertyValueAsString(obj2, propertyName));
            }
            return str;
        }

        internal static void SkipValue(this JsonReader jsonReader)
        {
            int num = 0;
            do
            {
                switch (jsonReader.NodeType)
                {
                    case JsonNodeType.StartObject:
                    case JsonNodeType.StartArray:
                        num++;
                        break;

                    case JsonNodeType.EndObject:
                    case JsonNodeType.EndArray:
                        num--;
                        break;
                }
                jsonReader.ReadNext();
            }
            while (num > 0);
        }

        private static void ValidateNodeType(this JsonReader jsonReader, JsonNodeType expectedNodeType)
        {
            if (jsonReader.NodeType != expectedNodeType)
            {
                throw CreateException(Strings.JsonReaderExtensions_UnexpectedNodeDetected(expectedNodeType, jsonReader.NodeType));
            }
        }
    }
}

