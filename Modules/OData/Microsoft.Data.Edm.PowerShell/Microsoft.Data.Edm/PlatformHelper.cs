using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Xml;

namespace Microsoft.Data.Edm
{
	internal static class PlatformHelper
	{
		internal readonly static Type[] EmptyTypes;

		internal readonly static string UriSchemeHttp;

		internal readonly static string UriSchemeHttps;

		static PlatformHelper()
		{
			PlatformHelper.EmptyTypes = new Type[0];
			PlatformHelper.UriSchemeHttp = Uri.UriSchemeHttp;
			PlatformHelper.UriSchemeHttps = Uri.UriSchemeHttps;
		}

		internal static ReadOnlyCollection<T> AsReadOnly<T>(this T[] array)
		{
			return Array.AsReadOnly<T>(array);
		}

		internal static bool ContainsGenericParameters(this Type type)
		{
			return type.ContainsGenericParameters;
		}

		internal static bool ContainsGenericParametersEx(this Type type)
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
			BindingFlags bindingFlag;
			BindingFlags bindingFlag1 = BindingFlags.Instance;
			BindingFlags bindingFlag2 = bindingFlag1;
			if (isPublic)
			{
				bindingFlag = BindingFlags.Public;
			}
			else
			{
				bindingFlag = BindingFlags.NonPublic;
			}
			bindingFlag1 = bindingFlag2 | bindingFlag;
			return type.GetConstructor(bindingFlag1, null, argTypes, null);
		}

		internal static IEnumerable<ConstructorInfo> GetInstanceConstructors(this Type type, bool isPublic)
		{
			BindingFlags bindingFlag;
			BindingFlags bindingFlag1 = BindingFlags.Instance;
			BindingFlags bindingFlag2 = bindingFlag1;
			if (isPublic)
			{
				bindingFlag = BindingFlags.Public;
			}
			else
			{
				bindingFlag = BindingFlags.NonPublic;
			}
			bindingFlag1 = bindingFlag2 | bindingFlag;
			return type.GetConstructors(bindingFlag1);
		}

		internal static MethodInfo GetMethod(this Type type, string name, bool isPublic, bool isStatic)
		{
			BindingFlags bindingFlag;
			BindingFlags bindingFlag1;
			BindingFlags bindingFlag2 = BindingFlags.Default;
			BindingFlags bindingFlag3 = bindingFlag2;
			if (isPublic)
			{
				bindingFlag = BindingFlags.Public;
			}
			else
			{
				bindingFlag = BindingFlags.NonPublic;
			}
			bindingFlag2 = bindingFlag3 | bindingFlag;
			BindingFlags bindingFlag4 = bindingFlag2;
			if (isStatic)
			{
				bindingFlag1 = BindingFlags.Static;
			}
			else
			{
				bindingFlag1 = BindingFlags.Instance;
			}
			bindingFlag2 = bindingFlag4 | bindingFlag1;
			return type.GetMethod(name, bindingFlag2);
		}

		internal static MethodInfo GetMethod(this Type type, string name, Type[] types, bool isPublic, bool isStatic)
		{
			BindingFlags bindingFlag;
			BindingFlags bindingFlag1;
			BindingFlags bindingFlag2 = BindingFlags.Default;
			BindingFlags bindingFlag3 = bindingFlag2;
			if (isPublic)
			{
				bindingFlag = BindingFlags.Public;
			}
			else
			{
				bindingFlag = BindingFlags.NonPublic;
			}
			bindingFlag2 = bindingFlag3 | bindingFlag;
			BindingFlags bindingFlag4 = bindingFlag2;
			if (isStatic)
			{
				bindingFlag1 = BindingFlags.Static;
			}
			else
			{
				bindingFlag1 = BindingFlags.Instance;
			}
			bindingFlag2 = bindingFlag4 | bindingFlag1;
			return type.GetMethod(name, bindingFlag2, null, types, null);
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
			BindingFlags bindingFlag = BindingFlags.Instance | BindingFlags.Public;
			if (!instanceOnly)
			{
				bindingFlag = bindingFlag | BindingFlags.Static;
			}
			if (declaredOnly)
			{
				bindingFlag = bindingFlag | BindingFlags.DeclaredOnly;
			}
			return type.GetProperties(bindingFlag);
		}

		internal static IEnumerable<PropertyInfo> GetPublicPropertiesEx(this Type type, bool instanceOnly, bool declaredOnly)
		{
			BindingFlags bindingFlag = BindingFlags.Instance | BindingFlags.Public;
			if (!instanceOnly)
			{
				bindingFlag = bindingFlag | BindingFlags.Static;
			}
			if (declaredOnly)
			{
				bindingFlag = bindingFlag | BindingFlags.DeclaredOnly;
			}
			return type.GetProperties(bindingFlag);
		}

		internal static IEnumerable<MethodInfo> GetPublicStaticMethods(this Type type)
		{
			return type.GetMethods(BindingFlags.Static | BindingFlags.Public);
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

		internal static bool IsGenericTypeEx(this Type type)
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

		internal static bool IsInterfaceEx(this Type type)
		{
			return type.IsInterface;
		}

		internal static bool IsMethod(MemberInfo member)
		{
			return member.MemberType == MemberTypes.Method;
		}

		internal static bool IsPrimitive(this Type type)
		{
			return type.IsPrimitive;
		}

		internal static bool IsProperty(MemberInfo member)
		{
			return member.MemberType == MemberTypes.Property;
		}

		internal static bool IsSealed(this Type type)
		{
			return type.IsSealed;
		}

		internal static bool IsValueType(this Type type)
		{
			return type.IsValueType;
		}

		internal static bool IsValueTypeEx(this Type type)
		{
			return type.IsValueType;
		}

		internal static bool IsVisible(this Type type)
		{
			return type.IsVisible;
		}
	}
}