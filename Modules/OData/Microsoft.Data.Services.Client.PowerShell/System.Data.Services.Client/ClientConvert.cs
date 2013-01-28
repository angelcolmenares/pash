namespace System.Data.Services.Client
{
    using System;
    using System.Data.Services.Client.Parsing;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal static class ClientConvert
    {
        internal static object ChangeType(string propertyValue, Type propertyType)
        {
            PrimitiveType type;
            if (PrimitiveType.TryGetPrimitiveType(propertyType, out type) && (type.TypeConverter != null))
            {
                try
                {
                    return type.TypeConverter.Parse(propertyValue);
                }
                catch (FormatException exception)
                {
                    propertyValue = (propertyValue.Length == 0) ? "String.Empty" : "String";
                    throw Error.InvalidOperation(Strings.Deserialize_Current(propertyType.ToString(), propertyValue), exception);
                }
                catch (OverflowException exception2)
                {
                    propertyValue = (propertyValue.Length == 0) ? "String.Empty" : "String";
                    throw Error.InvalidOperation(Strings.Deserialize_Current(propertyType.ToString(), propertyValue), exception2);
                }
            }
            return propertyValue;
        }

        internal static string GetEdmType(Type propertyType)
        {
            PrimitiveType type;
            if (!PrimitiveType.TryGetPrimitiveType(propertyType, out type))
            {
                return null;
            }
            if (type.EdmTypeName == null)
            {
                throw new NotSupportedException(Strings.ALinq_CantCastToUnsupportedPrimitive(propertyType.Name));
            }
            return type.EdmTypeName;
        }

        internal static bool IsBinaryValue(object value)
        {
            PrimitiveType type;
            return (PrimitiveType.TryGetPrimitiveType(value.GetType(), out type) && (value.GetType() == BinaryTypeConverter.BinaryType));
        }

        internal static bool ToNamedType(string typeName, out Type type)
        {
            PrimitiveType type2;
            type = typeof(string);
            if (string.IsNullOrEmpty(typeName))
            {
                return true;
            }
            if (PrimitiveType.TryGetPrimitiveType(typeName, out type2))
            {
                type = type2.ClrType;
                return true;
            }
            return false;
        }

        internal static string ToString(object propertyValue)
        {
            PrimitiveType type;
            if (PrimitiveType.TryGetPrimitiveType(propertyValue.GetType(), out type) && (type.TypeConverter != null))
            {
                return type.TypeConverter.ToString(propertyValue);
            }
            return propertyValue.ToString();
        }

        internal static bool TryKeyBinaryToString(object binaryValue, out string result)
        {
            byte[] buffer = (byte[]) binaryValue.GetType().InvokeMember("ToArray", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, binaryValue, null, CultureInfo.InvariantCulture);
            return WebConvert.TryKeyPrimitiveToString(buffer, out result);
        }

        internal static bool TryKeyPrimitiveToString(object value, out string result)
        {
            if (IsBinaryValue(value))
            {
                return TryKeyBinaryToString(value, out result);
            }
            return WebConvert.TryKeyPrimitiveToString(value, out result);
        }
    }
}

