namespace System.Data.Services.Client
{
    using Microsoft.Data.OData;
    using System;
    using System.Data.Services.Common;
	using System.Dynamic;
    using System.Linq;
    using System.Text;
	using System.Threading;

    internal static class CommonUtil
    {
        private static readonly Type OutOfMemoryType = typeof(OutOfMemoryException);
        private static readonly Type StackOverflowType = typeof(StackOverflowException);
        private static readonly Type ThreadAbortType = typeof(ThreadAbortException);
        private static readonly Type[] unsupportedTypes = new Type[] { typeof(IDynamicMetaObjectProvider), typeof(Tuple<>), typeof(Tuple<,>), typeof(Tuple<,,>), typeof(Tuple<,,,>), typeof(Tuple<,,,,>), typeof(Tuple<,,,,,>), typeof(Tuple<,,,,,,>), typeof(Tuple<,,,,,,,>) };

        internal static ODataVersion ConvertToODataVersion(DataServiceProtocolVersion maxProtocolVersion)
        {
            switch (maxProtocolVersion)
            {
                case DataServiceProtocolVersion.V1:
                    return ODataVersion.V1;

                case DataServiceProtocolVersion.V2:
                    return ODataVersion.V2;

                case DataServiceProtocolVersion.V3:
                    return ODataVersion.V3;
            }
            return ~ODataVersion.V1;
        }

        internal static ODataVersion ConvertToODataVersion(Version version)
        {
            if ((version.Major == 1) && (version.Minor == 0))
            {
                return ODataVersion.V1;
            }
            if ((version.Major == 2) && (version.Minor == 0))
            {
                return ODataVersion.V2;
            }
            return ODataVersion.V3;
        }

        internal static string GetCollectionItemTypeName(string typeName, bool isNested)
        {
            if (((typeName == null) || !typeName.StartsWith("Collection(", StringComparison.Ordinal)) || ((typeName[typeName.Length - 1] != ')') || (typeName.Length == "Collection()".Length)))
            {
                return null;
            }
            if (isNested)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.ClientType_CollectionOfCollectionNotSupported);
            }
            string str = typeName.Substring("Collection(".Length, typeName.Length - "Collection()".Length);
            GetCollectionItemTypeName(str, true);
            return str;
        }

        internal static string GetModelTypeName(Type type)
        {
            if (type.IsGenericType())
            {
                Type[] genericArguments = type.GetGenericArguments();
                StringBuilder builder = new StringBuilder((type.Name.Length * 2) * (1 + genericArguments.Length));
                if (type.IsNested)
                {
                    builder.Append(GetModelTypeName(type.DeclaringType));
                    builder.Append('_');
                }
                builder.Append(type.Name);
                builder.Append('[');
                for (int i = 0; i < genericArguments.Length; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(' ');
                    }
                    if (genericArguments[i].IsGenericParameter)
                    {
                        builder.Append(genericArguments[i].Name);
                    }
                    else
                    {
                        string modelTypeNamespace = GetModelTypeNamespace(genericArguments[i]);
                        if (!string.IsNullOrEmpty(modelTypeNamespace))
                        {
                            builder.Append(modelTypeNamespace);
                            builder.Append('.');
                        }
                        builder.Append(GetModelTypeName(genericArguments[i]));
                    }
                }
                builder.Append(']');
                return builder.ToString();
            }
            if (type.IsNested)
            {
                return (GetModelTypeName(type.DeclaringType) + "_" + type.Name);
            }
            return type.Name;
        }

        internal static string GetModelTypeNamespace(Type type)
        {
            return (type.Namespace ?? string.Empty);
        }

        internal static bool IsCatchableExceptionType(Exception e)
        {
            if (e == null)
            {
                return true;
            }
            Type type = e.GetType();
            return (((type != ThreadAbortType) && (type != StackOverflowType)) && (type != OutOfMemoryType));
        }

        internal static bool IsUnsupportedType(Type type)
        {
            if (type.IsGenericType())
            {
                type = type.GetGenericTypeDefinition();
            }
            return unsupportedTypes.Any<Type>(t => t.IsAssignableFrom(type));
        }

        internal static string UriToString(Uri uri)
        {
            if (!uri.IsAbsoluteUri)
            {
                return uri.OriginalString;
            }
            return uri.AbsoluteUri;
        }
    }
}

