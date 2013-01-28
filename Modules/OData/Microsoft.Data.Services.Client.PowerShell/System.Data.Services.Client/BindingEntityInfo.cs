namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Services.Client.Metadata;
    using System.Data.Services.Common;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class BindingEntityInfo
    {
        private static readonly Dictionary<Type, BindingEntityInfoPerType> bindingEntityInfos = new Dictionary<Type, BindingEntityInfoPerType>(EqualityComparer<Type>.Default);
        private static readonly object FalseObject = new object();
        private static readonly HashSet<Type> knownNonEntityTypes = new HashSet<Type>(EqualityComparer<Type>.Default);
        private static readonly Dictionary<Type, object> knownObservableCollectionTypes = new Dictionary<Type, object>(EqualityComparer<Type>.Default);
        private static readonly ReaderWriterLockSlim metadataCacheLock = new ReaderWriterLockSlim();
        private static readonly object TrueObject = new object();

        private static bool CanBeComplexType(Type type)
        {
            return typeof(INotifyPropertyChanged).IsAssignableFrom(type);
        }

        private static BindingEntityInfoPerType GetBindingEntityInfoFor(Type entityType, DataServiceProtocolVersion maxProtocolVersion)
        {
            BindingEntityInfoPerType type;
            metadataCacheLock.EnterReadLock();
            try
            {
                if (bindingEntityInfos.TryGetValue(entityType, out type))
                {
                    return type;
                }
            }
            finally
            {
                metadataCacheLock.ExitReadLock();
            }
            type = new BindingEntityInfoPerType();
            EntitySetAttribute attribute = (EntitySetAttribute) entityType.GetCustomAttributes(typeof(EntitySetAttribute), true).SingleOrDefault<object>();
            ClientEdmModel model = ClientEdmModel.GetModel(maxProtocolVersion);
            type.EntitySet = (attribute != null) ? attribute.EntitySet : null;
            type.ClientType = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(entityType));
            foreach (ClientPropertyAnnotation annotation in type.ClientType.Properties())
            {
                BindingPropertyInfo item = null;
                Type propertyType = annotation.PropertyType;
                if (!annotation.IsStreamLinkProperty)
                {
                    if (annotation.IsPrimitiveOrComplexCollection)
                    {
                        item = new BindingPropertyInfo {
                            PropertyKind = BindingPropertyKind.BindingPropertyKindPrimitiveOrComplexCollection
                        };
                    }
                    else if (annotation.IsEntityCollection)
                    {
                        if (IsDataServiceCollection(propertyType, maxProtocolVersion))
                        {
                            item = new BindingPropertyInfo {
                                PropertyKind = BindingPropertyKind.BindingPropertyKindDataServiceCollection
                            };
                        }
                    }
                    else if (IsEntityType(propertyType, maxProtocolVersion))
                    {
                        item = new BindingPropertyInfo {
                            PropertyKind = BindingPropertyKind.BindingPropertyKindEntity
                        };
                    }
                    else if (CanBeComplexType(propertyType))
                    {
                        item = new BindingPropertyInfo {
                            PropertyKind = BindingPropertyKind.BindingPropertyKindComplex
                        };
                    }
                    if (item != null)
                    {
                        item.PropertyInfo = annotation;
                        if ((type.ClientType.IsEntityType || (item.PropertyKind == BindingPropertyKind.BindingPropertyKindComplex)) || (item.PropertyKind == BindingPropertyKind.BindingPropertyKindPrimitiveOrComplexCollection))
                        {
                            type.ObservableProperties.Add(item);
                        }
                    }
                }
            }
            metadataCacheLock.EnterWriteLock();
            try
            {
                if (!bindingEntityInfos.ContainsKey(entityType))
                {
                    bindingEntityInfos[entityType] = type;
                }
            }
            finally
            {
                metadataCacheLock.ExitWriteLock();
            }
            return type;
        }

        internal static ClientTypeAnnotation GetClientType(Type entityType, DataServiceProtocolVersion maxProtocolVersion)
        {
            return GetBindingEntityInfoFor(entityType, maxProtocolVersion).ClientType;
        }

        internal static string GetEntitySet(object target, string targetEntitySet, DataServiceProtocolVersion maxProtocolVersion)
        {
            if (!string.IsNullOrEmpty(targetEntitySet))
            {
                return targetEntitySet;
            }
            return GetEntitySetAttribute(target.GetType(), maxProtocolVersion);
        }

        private static string GetEntitySetAttribute(Type entityType, DataServiceProtocolVersion maxProtocolVersion)
        {
            return GetBindingEntityInfoFor(entityType, maxProtocolVersion).EntitySet;
        }

        internal static IList<BindingPropertyInfo> GetObservableProperties(Type entityType, DataServiceProtocolVersion maxProtocolVersion)
        {
            return GetBindingEntityInfoFor(entityType, maxProtocolVersion).ObservableProperties;
        }

        internal static bool IsDataServiceCollection(Type collectionType, DataServiceProtocolVersion maxProtocolVersion)
        {
            metadataCacheLock.EnterReadLock();
            try
            {
                object obj2;
                if (knownObservableCollectionTypes.TryGetValue(collectionType, out obj2))
                {
                    return (obj2 == TrueObject);
                }
            }
            finally
            {
                metadataCacheLock.ExitReadLock();
            }
            Type baseType = collectionType;
            bool flag = false;
            while (baseType != null)
            {
                if (baseType.IsGenericType())
                {
                    Type[] genericArguments = baseType.GetGenericArguments();
                    if (((genericArguments != null) && (genericArguments.Length == 1)) && IsEntityType(genericArguments[0], maxProtocolVersion))
                    {
                        Type dataServiceCollectionOfT = WebUtil.GetDataServiceCollectionOfT(genericArguments);
                        if ((dataServiceCollectionOfT != null) && dataServiceCollectionOfT.IsAssignableFrom(baseType))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                baseType = baseType.BaseType;
            }
            metadataCacheLock.EnterWriteLock();
            try
            {
                if (!knownObservableCollectionTypes.ContainsKey(collectionType))
                {
                    knownObservableCollectionTypes[collectionType] = flag ? TrueObject : FalseObject;
                }
            }
            finally
            {
                metadataCacheLock.ExitWriteLock();
            }
            return flag;
        }

        internal static bool IsEntityType(Type type, DataServiceProtocolVersion maxProtocolVersion)
        {
            bool flag;
            metadataCacheLock.EnterReadLock();
            try
            {
                if (knownNonEntityTypes.Contains(type))
                {
                    return false;
                }
            }
            finally
            {
                metadataCacheLock.ExitReadLock();
            }
            try
            {
                if (IsDataServiceCollection(type, maxProtocolVersion))
                {
                    return false;
                }
                flag = ClientTypeUtil.TypeOrElementTypeIsEntity(type);
            }
            catch (InvalidOperationException)
            {
                flag = false;
            }
            if (!flag)
            {
                metadataCacheLock.EnterWriteLock();
                try
                {
                    if (!knownNonEntityTypes.Contains(type))
                    {
                        knownNonEntityTypes.Add(type);
                    }
                }
                finally
                {
                    metadataCacheLock.ExitWriteLock();
                }
            }
            return flag;
        }

        internal static bool TryGetPropertyValue(object source, string sourceProperty, DataServiceProtocolVersion maxProtocolVersion, out BindingPropertyInfo bindingPropertyInfo, out ClientPropertyAnnotation clientProperty, out object propertyValue)
        {
            Type entityType = source.GetType();
            bindingPropertyInfo = GetObservableProperties(entityType, maxProtocolVersion).SingleOrDefault<BindingPropertyInfo>(x => x.PropertyInfo.PropertyName == sourceProperty);
            bool flag = bindingPropertyInfo != null;
            if (!flag)
            {
                clientProperty = GetClientType(entityType, maxProtocolVersion).GetProperty(sourceProperty, true);
                flag = clientProperty != null;
                if (!flag)
                {
                    propertyValue = null;
                    return flag;
                }
                propertyValue = clientProperty.GetValue(source);
                return flag;
            }
            clientProperty = null;
            propertyValue = bindingPropertyInfo.PropertyInfo.GetValue(source);
            return flag;
        }

        private sealed class BindingEntityInfoPerType
        {
            private List<BindingEntityInfo.BindingPropertyInfo> observableProperties = new List<BindingEntityInfo.BindingPropertyInfo>();

            public ClientTypeAnnotation ClientType { get; set; }

            public string EntitySet { get; set; }

            public List<BindingEntityInfo.BindingPropertyInfo> ObservableProperties
            {
                get
                {
                    return this.observableProperties;
                }
            }
        }

        internal class BindingPropertyInfo
        {
            public ClientPropertyAnnotation PropertyInfo { get; set; }

            public BindingPropertyKind PropertyKind { get; set; }
        }
    }
}

