namespace Microsoft.Data.OData
{
    using System;

    internal static class TypeUtils
    {
        internal static bool AreTypesEquivalent(Type typeA, Type typeB)
        {
            return (((typeA != null) && (typeB != null)) && typeA.IsEquivalentTo(typeB));
        }

        internal static Type GetNonNullableType(Type type)
        {
            return (Nullable.GetUnderlyingType(type) ?? type);
        }

        internal static Type GetNullableType(Type type)
        {
            if (!TypeAllowsNull(type))
            {
                type = typeof(Nullable<>).MakeGenericType(new Type[] { type });
            }
            return type;
        }

        internal static bool IsNullableType(Type type)
        {
            return (type.IsGenericType() && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        internal static bool TypeAllowsNull(Type type)
        {
            if (type.IsValueType())
            {
                return IsNullableType(type);
            }
            return true;
        }
    }
}

