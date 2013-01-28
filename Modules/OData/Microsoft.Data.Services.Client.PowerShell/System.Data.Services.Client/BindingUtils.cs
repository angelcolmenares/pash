namespace System.Data.Services.Client
{
    using System;

    internal static class BindingUtils
    {
        internal static Type GetCollectionEntityType(Type collectionType)
        {
            while (collectionType != null)
            {
                if (collectionType.IsGenericType() && WebUtil.IsDataServiceCollectionType(collectionType.GetGenericTypeDefinition()))
                {
                    return collectionType.GetGenericArguments()[0];
                }
                collectionType = collectionType.BaseType;
            }
            return null;
        }

        internal static void ValidateEntitySetName(string entitySetName, object entity)
        {
            if (string.IsNullOrEmpty(entitySetName))
            {
                throw new InvalidOperationException(Strings.DataBinding_Util_UnknownEntitySetName(entity.GetType().FullName));
            }
        }

        internal static void VerifyObserverNotPresent<T>(object oec, string sourceProperty, Type sourceType)
        {
            DataServiceCollection<T> services = oec as DataServiceCollection<T>;
            if (services.Observer != null)
            {
                throw new InvalidOperationException(Strings.DataBinding_CollectionPropertySetterValueHasObserver(sourceProperty, sourceType));
            }
        }
    }
}

