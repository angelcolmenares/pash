namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal static class TypeExtensions
    {
        internal static IEnumerable<T> GetCustomAttributes<T>(this Type type, bool inherit) where T: Attribute
        {
            return (from attr in type.GetCustomAttributes(typeof(T), inherit)
                where attr is T
                select (T) attr);
        }

        internal static bool IsFloating(this Type type)
        {
            return LanguagePrimitives.IsFloating(LanguagePrimitives.GetTypeCode(type));
        }

        internal static bool IsNumeric(this Type type)
        {
            return LanguagePrimitives.IsNumeric(LanguagePrimitives.GetTypeCode(type));
        }

        internal static bool IsNumericOrPrimitive(this Type type)
        {
            if (!type.IsPrimitive)
            {
                return LanguagePrimitives.IsNumeric(LanguagePrimitives.GetTypeCode(type));
            }
            return true;
        }

        internal static bool IsSafePrimitive(this Type type)
        {
            return ((type.IsPrimitive && (type != typeof(IntPtr))) && (type != typeof(UIntPtr)));
        }
    }
}

