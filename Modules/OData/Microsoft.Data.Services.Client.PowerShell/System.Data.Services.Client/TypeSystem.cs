namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Spatial;

    internal static class TypeSystem
    {
        private static readonly Dictionary<MethodInfo, string> expressionMethodMap = new Dictionary<MethodInfo, string>(0x23, EqualityComparer<MethodInfo>.Default);
        private static readonly Dictionary<string, string> expressionVBMethodMap;
        private static readonly Dictionary<Type, Type> ienumerableElementTypeCache = new Dictionary<Type, Type>(EqualityComparer<Type>.Default);
        private static readonly Dictionary<PropertyInfo, MethodInfo> propertiesAsMethodsMap;
        private const string VisualBasicAssemblyFullName = "Microsoft.VisualBasic, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

        static TypeSystem()
        {
            expressionMethodMap.Add(typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), "substringof");
            expressionMethodMap.Add(typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) }), "endswith");
            expressionMethodMap.Add(typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) }), "startswith");
            expressionMethodMap.Add(typeof(string).GetMethod("IndexOf", new Type[] { typeof(string) }), "indexof");
            expressionMethodMap.Add(typeof(string).GetMethod("Replace", new Type[] { typeof(string), typeof(string) }), "replace");
            expressionMethodMap.Add(typeof(string).GetMethod("Substring", new Type[] { typeof(int) }), "substring");
            expressionMethodMap.Add(typeof(string).GetMethod("Substring", new Type[] { typeof(int), typeof(int) }), "substring");
            expressionMethodMap.Add(typeof(string).GetMethod("ToLower", System.Data.Services.Client.PlatformHelper.EmptyTypes), "tolower");
            expressionMethodMap.Add(typeof(string).GetMethod("ToUpper", System.Data.Services.Client.PlatformHelper.EmptyTypes), "toupper");
            expressionMethodMap.Add(typeof(string).GetMethod("Trim", System.Data.Services.Client.PlatformHelper.EmptyTypes), "trim");
            expressionMethodMap.Add(typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) }), "concat");
            expressionMethodMap.Add(typeof(string).GetProperty("Length", typeof(int)).GetGetMethod(), "length");
            expressionMethodMap.Add(typeof(DateTime).GetProperty("Day", typeof(int)).GetGetMethod(), "day");
            expressionMethodMap.Add(typeof(DateTime).GetProperty("Hour", typeof(int)).GetGetMethod(), "hour");
            expressionMethodMap.Add(typeof(DateTime).GetProperty("Month", typeof(int)).GetGetMethod(), "month");
            expressionMethodMap.Add(typeof(DateTime).GetProperty("Minute", typeof(int)).GetGetMethod(), "minute");
            expressionMethodMap.Add(typeof(DateTime).GetProperty("Second", typeof(int)).GetGetMethod(), "second");
            expressionMethodMap.Add(typeof(DateTime).GetProperty("Year", typeof(int)).GetGetMethod(), "year");
            expressionMethodMap.Add(typeof(DateTimeOffset).GetProperty("Day", typeof(int)).GetGetMethod(), "day");
            expressionMethodMap.Add(typeof(DateTimeOffset).GetProperty("Hour", typeof(int)).GetGetMethod(), "hour");
            expressionMethodMap.Add(typeof(DateTimeOffset).GetProperty("Month", typeof(int)).GetGetMethod(), "month");
            expressionMethodMap.Add(typeof(DateTimeOffset).GetProperty("Minute", typeof(int)).GetGetMethod(), "minute");
            expressionMethodMap.Add(typeof(DateTimeOffset).GetProperty("Second", typeof(int)).GetGetMethod(), "second");
            expressionMethodMap.Add(typeof(DateTimeOffset).GetProperty("Year", typeof(int)).GetGetMethod(), "year");
            expressionMethodMap.Add(typeof(TimeSpan).GetProperty("Hours", typeof(int)).GetGetMethod(), "hour");
            expressionMethodMap.Add(typeof(TimeSpan).GetProperty("Minutes", typeof(int)).GetGetMethod(), "minute");
            expressionMethodMap.Add(typeof(TimeSpan).GetProperty("Seconds", typeof(int)).GetGetMethod(), "second");
            expressionMethodMap.Add(typeof(Math).GetMethod("Round", new Type[] { typeof(double) }), "round");
            expressionMethodMap.Add(typeof(Math).GetMethod("Round", new Type[] { typeof(decimal) }), "round");
            expressionMethodMap.Add(typeof(Math).GetMethod("Floor", new Type[] { typeof(double) }), "floor");
            expressionMethodMap.Add(typeof(Math).GetMethod("Floor", new Type[] { typeof(decimal) }), "floor");
            expressionMethodMap.Add(typeof(Math).GetMethod("Ceiling", new Type[] { typeof(double) }), "ceiling");
            expressionMethodMap.Add(typeof(Math).GetMethod("Ceiling", new Type[] { typeof(decimal) }), "ceiling");
            expressionMethodMap.Add(typeof(GeographyOperationsExtensions).GetMethod("Distance", new Type[] { typeof(GeographyPoint), typeof(GeographyPoint) }, true, true), "geo.distance");
            expressionMethodMap.Add(typeof(GeometryOperationsExtensions).GetMethod("Distance", new Type[] { typeof(GeometryPoint), typeof(GeometryPoint) }, true, true), "geo.distance");
            expressionVBMethodMap = new Dictionary<string, string>(EqualityComparer<string>.Default);
            expressionVBMethodMap.Add("Microsoft.VisualBasic.Strings.Trim", "trim");
            expressionVBMethodMap.Add("Microsoft.VisualBasic.Strings.Len", "length");
            expressionVBMethodMap.Add("Microsoft.VisualBasic.Strings.Mid", "substring");
            expressionVBMethodMap.Add("Microsoft.VisualBasic.Strings.UCase", "toupper");
            expressionVBMethodMap.Add("Microsoft.VisualBasic.Strings.LCase", "tolower");
            expressionVBMethodMap.Add("Microsoft.VisualBasic.DateAndTime.Year", "year");
            expressionVBMethodMap.Add("Microsoft.VisualBasic.DateAndTime.Month", "month");
            expressionVBMethodMap.Add("Microsoft.VisualBasic.DateAndTime.Day", "day");
            expressionVBMethodMap.Add("Microsoft.VisualBasic.DateAndTime.Hour", "hour");
            expressionVBMethodMap.Add("Microsoft.VisualBasic.DateAndTime.Minute", "minute");
            expressionVBMethodMap.Add("Microsoft.VisualBasic.DateAndTime.Second", "second");
            propertiesAsMethodsMap = new Dictionary<PropertyInfo, MethodInfo>(EqualityComparer<PropertyInfo>.Default);
            propertiesAsMethodsMap.Add(typeof(string).GetProperty("Length", typeof(int)), typeof(string).GetProperty("Length", typeof(int)).GetGetMethod());
            propertiesAsMethodsMap.Add(typeof(DateTime).GetProperty("Day", typeof(int)), typeof(DateTime).GetProperty("Day", typeof(int)).GetGetMethod());
            propertiesAsMethodsMap.Add(typeof(DateTime).GetProperty("Hour", typeof(int)), typeof(DateTime).GetProperty("Hour", typeof(int)).GetGetMethod());
            propertiesAsMethodsMap.Add(typeof(DateTime).GetProperty("Minute", typeof(int)), typeof(DateTime).GetProperty("Minute", typeof(int)).GetGetMethod());
            propertiesAsMethodsMap.Add(typeof(DateTime).GetProperty("Second", typeof(int)), typeof(DateTime).GetProperty("Second", typeof(int)).GetGetMethod());
            propertiesAsMethodsMap.Add(typeof(DateTime).GetProperty("Month", typeof(int)), typeof(DateTime).GetProperty("Month", typeof(int)).GetGetMethod());
            propertiesAsMethodsMap.Add(typeof(DateTime).GetProperty("Year", typeof(int)), typeof(DateTime).GetProperty("Year", typeof(int)).GetGetMethod());
            propertiesAsMethodsMap.Add(typeof(DateTimeOffset).GetProperty("Day", typeof(int)), typeof(DateTimeOffset).GetProperty("Day", typeof(int)).GetGetMethod());
            propertiesAsMethodsMap.Add(typeof(DateTimeOffset).GetProperty("Hour", typeof(int)), typeof(DateTimeOffset).GetProperty("Hour", typeof(int)).GetGetMethod());
            propertiesAsMethodsMap.Add(typeof(DateTimeOffset).GetProperty("Minute", typeof(int)), typeof(DateTimeOffset).GetProperty("Minute", typeof(int)).GetGetMethod());
            propertiesAsMethodsMap.Add(typeof(DateTimeOffset).GetProperty("Second", typeof(int)), typeof(DateTimeOffset).GetProperty("Second", typeof(int)).GetGetMethod());
            propertiesAsMethodsMap.Add(typeof(DateTimeOffset).GetProperty("Month", typeof(int)), typeof(DateTimeOffset).GetProperty("Month", typeof(int)).GetGetMethod());
            propertiesAsMethodsMap.Add(typeof(DateTimeOffset).GetProperty("Year", typeof(int)), typeof(DateTimeOffset).GetProperty("Year", typeof(int)).GetGetMethod());
            propertiesAsMethodsMap.Add(typeof(TimeSpan).GetProperty("Hours", typeof(int)), typeof(TimeSpan).GetProperty("Hours", typeof(int)).GetGetMethod());
            propertiesAsMethodsMap.Add(typeof(TimeSpan).GetProperty("Minutes", typeof(int)), typeof(TimeSpan).GetProperty("Minutes", typeof(int)).GetGetMethod());
            propertiesAsMethodsMap.Add(typeof(TimeSpan).GetProperty("Seconds", typeof(int)), typeof(TimeSpan).GetProperty("Seconds", typeof(int)).GetGetMethod());
        }

        internal static Type FindIEnumerable(Type seqType)
        {
            Type type;
            if (((seqType == null) || (seqType == typeof(string))) || ((seqType.IsPrimitive() || seqType.IsValueType()) || (Nullable.GetUnderlyingType(seqType) != null)))
            {
                return null;
            }
            lock (ienumerableElementTypeCache)
            {
                if (!ienumerableElementTypeCache.TryGetValue(seqType, out type))
                {
                    type = FindIEnumerableForNonPrimitiveType(seqType);
                    ienumerableElementTypeCache.Add(seqType, type);
                }
            }
            return type;
        }

        private static Type FindIEnumerableForNonPrimitiveType(Type seqType)
        {
            if (seqType.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(new Type[] { seqType.GetElementType() });
            }
            if (seqType.IsGenericType())
            {
                foreach (Type type in seqType.GetGenericArguments())
                {
                    Type type2 = typeof(IEnumerable<>).MakeGenericType(new Type[] { type });
                    if (type2.IsAssignableFrom(seqType))
                    {
                        return type2;
                    }
                }
            }
            IEnumerable<Type> interfaces = seqType.GetInterfaces();
            if (interfaces != null)
            {
                foreach (Type type3 in interfaces)
                {
                    Type type4 = FindIEnumerable(type3);
                    if (type4 != null)
                    {
                        return type4;
                    }
                }
            }
            if ((seqType.GetBaseType() != null) && (seqType.GetBaseType() != typeof(object)))
            {
                return FindIEnumerable(seqType.GetBaseType());
            }
            return null;
        }

        internal static Type GetElementType(Type seqType)
        {
            Type type = FindIEnumerable(seqType);
            if (type == null)
            {
                return seqType;
            }
            return type.GetGenericArguments()[0];
        }

        internal static bool IsPrivate(PropertyInfo pi)
        {
            MethodInfo info = pi.GetGetMethod() ?? pi.GetSetMethod();
            if (info != null)
            {
                return info.IsPrivate;
            }
            return true;
        }

        internal static bool TryGetPropertyAsMethod(PropertyInfo pi, out MethodInfo mi)
        {
            return propertiesAsMethodsMap.TryGetValue(pi, out mi);
        }

        internal static bool TryGetQueryOptionMethod(MethodInfo mi, out string methodName)
        {
            return (expressionMethodMap.TryGetValue(mi, out methodName) || ((mi.DeclaringType.GetAssembly().FullName == "Microsoft.VisualBasic, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a") && expressionVBMethodMap.TryGetValue(mi.DeclaringType.FullName + "." + mi.Name, out methodName)));
        }
    }
}

