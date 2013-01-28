namespace System.Spatial
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Xml;

    internal static class PlatformHelper
    {
        internal static readonly Type[] EmptyTypes = new Type[0];
        internal static readonly string UriSchemeHttp = Uri.UriSchemeHttp;
        internal static readonly string UriSchemeHttps = Uri.UriSchemeHttps;

        internal static ReadOnlyCollection<T> AsReadOnly<T>(this T[] array)
        {
            return Array.AsReadOnly<T>(array);
        }

        internal static bool ContainsGenericParameters(this Type type)
        {
            return type.ContainsGenericParameters;
        }

        internal static string ConvertDateTimeToString(DateTime dateTime)
        {
            return XmlConvert.ToString(dateTime, XmlDateTimeSerializationMode.RoundtripKind);
        }

        internal static DateTime ConvertStringToDateTime(string text)
        {
            return XmlConvert.ToDateTime(text, XmlDateTimeSerializationMode.RoundtripKind);
        }

        internal static Assembly GetAssembly(this Type type)
        {
            return type.Assembly;
        }

        internal static Type GetBaseType(this Type type)
        {
            return type.BaseType;
        }

        internal static ConstructorInfo GetInstanceConstructor(this Type type, bool isPublic, Type[] argTypes)
        {
            BindingFlags instance = BindingFlags.Instance;
            instance |= isPublic ? BindingFlags.Public : BindingFlags.NonPublic;
            return type.GetConstructor(instance, null, argTypes, null);
        }

        internal static IEnumerable<ConstructorInfo> GetInstanceConstructors(this Type type, bool isPublic)
        {
            BindingFlags instance = BindingFlags.Instance;
            instance |= isPublic ? BindingFlags.Public : BindingFlags.NonPublic;
            return type.GetConstructors(instance);
        }

        internal static MethodInfo GetMethod(this Type type, string name, bool isPublic, bool isStatic)
        {
            BindingFlags bindingAttr = BindingFlags.Default;
            bindingAttr |= isPublic ? BindingFlags.Public : BindingFlags.NonPublic;
            bindingAttr |= isStatic ? BindingFlags.Static : BindingFlags.Instance;
            return type.GetMethod(name, bindingAttr);
        }

        internal static MethodInfo GetMethod(this Type type, string name, Type[] types, bool isPublic, bool isStatic)
        {
            BindingFlags bindingAttr = BindingFlags.Default;
            bindingAttr |= isPublic ? BindingFlags.Public : BindingFlags.NonPublic;
            bindingAttr |= isStatic ? BindingFlags.Static : BindingFlags.Instance;
            return type.GetMethod(name, bindingAttr, null, types, null);
        }

        internal static IEnumerable<Type> GetNonPublicNestedTypes(this Type type)
        {
            return type.GetNestedTypes(BindingFlags.NonPublic);
        }

        internal static IEnumerable<PropertyInfo> GetPublicProperties(this Type type, bool instanceOnly)
        {
            return type.GetPublicProperties(instanceOnly, false);
        }

        internal static IEnumerable<PropertyInfo> GetPublicProperties(this Type type, bool instanceOnly, bool declaredOnly)
        {
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance;
            if (!instanceOnly)
            {
                bindingAttr |= BindingFlags.Static;
            }
            if (declaredOnly)
            {
                bindingAttr |= BindingFlags.DeclaredOnly;
            }
            return type.GetProperties(bindingAttr);
        }

        internal static IEnumerable<MethodInfo> GetPublicStaticMethods(this Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Static);
        }

        internal static TypeCode GetTypeCode(Type type)
        {
            return Type.GetTypeCode(type);
        }

        internal static Type GetTypeOrThrow(string typeName)
        {
            return Type.GetType(typeName, true);
        }

        internal static bool IsAbstract(this Type type)
        {
            return type.IsAbstract;
        }

        internal static bool IsClass(this Type type)
        {
            return type.IsClass;
        }

        internal static bool IsEnum(this Type type)
        {
            return type.IsEnum;
        }

        internal static bool IsGenericParameter(this Type type)
        {
            return type.IsGenericParameter;
        }

        internal static bool IsGenericType(this Type type)
        {
            return type.IsGenericType;
        }

        internal static bool IsGenericTypeDefinition(this Type type)
        {
            return type.IsGenericTypeDefinition;
        }

        internal static bool IsInterface(this Type type)
        {
            return type.IsInterface;
        }

        internal static bool IsMethod(MemberInfo member)
        {
            return (member.MemberType == MemberTypes.Method);
        }

        internal static bool IsPrimitive(this Type type)
        {
            return type.IsPrimitive;
        }

        internal static bool IsProperty(MemberInfo member)
        {
            return (member.MemberType == MemberTypes.Property);
        }

        internal static bool IsSealed(this Type type)
        {
            return type.IsSealed;
        }

        internal static bool IsValueType(this Type type)
        {
            return type.IsValueType;
        }

        internal static bool IsVisible(this Type type)
        {
            return type.IsVisible;
        }
    }
}

