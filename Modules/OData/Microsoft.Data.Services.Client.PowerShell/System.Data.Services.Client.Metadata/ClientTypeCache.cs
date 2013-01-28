namespace System.Data.Services.Client.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [DebuggerDisplay("{PropertyName}")]
    internal static class ClientTypeCache
    {
        private static readonly Dictionary<TypeName, Type> namedTypes = new Dictionary<TypeName, Type>(new TypeNameEqualityComparer());

        internal static Type ResolveFromName(string wireName, Type userType)
        {
            Type type;
            TypeName name;
            bool flag;
            name.Type = userType;
            name.Name = wireName;
            lock (namedTypes)
            {
                flag = namedTypes.TryGetValue(name, out type);
            }
            if (!flag)
            {
                string wireClassName = wireName;
                int num = wireName.LastIndexOf('.');
                if ((0 <= num) && (num < (wireName.Length - 1)))
                {
                    wireClassName = wireName.Substring(num + 1);
                }
                if (userType.Name == wireClassName)
                {
                    type = userType;
                }
                else
                {
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        Type type2 = assembly.GetType(wireName, false);
                        ResolveSubclass(wireClassName, userType, type2, ref type);
                        if (null == type2)
                        {
                            IEnumerable<Type> types = null;
                            try
                            {
                                types = assembly.GetTypes();
                            }
                            catch (ReflectionTypeLoadException)
                            {
                            }
                            if (types != null)
                            {
                                foreach (Type type3 in types)
                                {
                                    ResolveSubclass(wireClassName, userType, type3, ref type);
                                }
                            }
                        }
                    }
                }
                lock (namedTypes)
                {
                    namedTypes[name] = type;
                }
            }
            return type;
        }

        private static void ResolveSubclass(string wireClassName, Type userType, Type type, ref Type existing)
        {
            if (((null != type) && type.IsVisible()) && ((wireClassName == type.Name) && userType.IsAssignableFrom(type)))
            {
                if (null != existing)
                {
                    throw Error.InvalidOperation(Strings.ClientType_Ambiguous(wireClassName, userType));
                }
                existing = type;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TypeName
        {
            internal System.Type Type;
            internal string Name;
        }

        private sealed class TypeNameEqualityComparer : IEqualityComparer<ClientTypeCache.TypeName>
        {
            public bool Equals(ClientTypeCache.TypeName x, ClientTypeCache.TypeName y)
            {
                return ((x.Type == y.Type) && (x.Name == y.Name));
            }

            public int GetHashCode(ClientTypeCache.TypeName obj)
            {
                return (obj.Type.GetHashCode() ^ obj.Name.GetHashCode());
            }
        }
    }
}

