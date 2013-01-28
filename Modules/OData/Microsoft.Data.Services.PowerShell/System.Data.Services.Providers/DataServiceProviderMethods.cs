namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class DataServiceProviderMethods
    {
        internal static readonly MethodInfo ConvertMethodInfo = typeof(DataServiceProviderMethods).GetMethod("Convert", BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo GetSequenceValueMethodInfo = typeof(DataServiceProviderMethods).GetMethod("GetSequenceValue", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object), typeof(ResourceProperty) }, null);
        internal static readonly MethodInfo GetValueMethodInfo = typeof(DataServiceProviderMethods).GetMethod("GetValue", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object), typeof(ResourceProperty) }, null);
        internal static readonly MethodInfo OfTypeIEnumerableMethodInfo;
        internal static readonly MethodInfo OfTypeIQueryableMethodInfo;
        internal static readonly MethodInfo TypeAsMethodInfo = typeof(DataServiceProviderMethods).GetMethod("TypeAs", BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo TypeIsMethodInfo = typeof(DataServiceProviderMethods).GetMethod("TypeIs", BindingFlags.Public | BindingFlags.Static);

        static DataServiceProviderMethods()
        {
            MethodInfo[] infoArray = (MethodInfo[]) typeof(DataServiceProviderMethods).GetMember("OfType", MemberTypes.Method, BindingFlags.Public | BindingFlags.Static);
            if (infoArray[0].GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IQueryable<>))
            {
                OfTypeIQueryableMethodInfo = infoArray[0];
                OfTypeIEnumerableMethodInfo = infoArray[1];
            }
            else
            {
                OfTypeIEnumerableMethodInfo = infoArray[0];
                OfTypeIQueryableMethodInfo = infoArray[1];
            }
        }

        public static bool AreByteArraysEqual(byte[] left, byte[] right)
        {
            if (left != right)
            {
                if ((left == null) || (right == null))
                {
                    return false;
                }
                if (left.Length != right.Length)
                {
                    return false;
                }
                for (int i = 0; i < left.Length; i++)
                {
                    if (left[i] != right[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool AreByteArraysNotEqual(byte[] left, byte[] right)
        {
            return !AreByteArraysEqual(left, right);
        }

        public static int Compare(bool? left, bool? right)
        {
            return Comparer<bool?>.Default.Compare(left, right);
        }

        public static int Compare(Guid? left, Guid? right)
        {
            return Comparer<Guid?>.Default.Compare(left, right);
        }

        public static int Compare(bool left, bool right)
        {
            return Comparer<bool>.Default.Compare(left, right);
        }

        public static int Compare(Guid left, Guid right)
        {
            return Comparer<Guid>.Default.Compare(left, right);
        }

        public static int Compare(string left, string right)
        {
            return Comparer<string>.Default.Compare(left, right);
        }

        public static object Convert(object value, ResourceType type)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<T> GetSequenceValue<T>(object value, ResourceProperty property)
        {
            throw new NotImplementedException();
        }

        public static object GetValue(object value, ResourceProperty property)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<TResult> OfType<TSource, TResult>(IEnumerable<TSource> query, ResourceType resourceType)
        {
            throw new NotImplementedException();
        }

        public static IQueryable<TResult> OfType<TSource, TResult>(IQueryable<TSource> query, ResourceType resourceType)
        {
            throw new NotImplementedException();
        }

        public static T TypeAs<T>(object value, ResourceType type)
        {
            throw new NotImplementedException();
        }

        public static bool TypeIs(object value, ResourceType type)
        {
            throw new NotImplementedException();
        }
    }
}

