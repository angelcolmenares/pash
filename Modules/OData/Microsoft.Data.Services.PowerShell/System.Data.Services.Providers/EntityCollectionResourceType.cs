namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Diagnostics;
    using System.Globalization;

    [DebuggerDisplay("{Name}: {InstanceType}, {ResourceTypeKind}")]
    internal class EntityCollectionResourceType : ResourceType
    {
        private readonly ResourceType itemType;

        internal EntityCollectionResourceType(ResourceType itemType) : base(GetInstanceType(itemType), ResourceTypeKind.EntityCollection, string.Empty, GetName(itemType))
        {
            if (itemType.ResourceTypeKind != ResourceTypeKind.EntityType)
            {
                throw new ArgumentException(Strings.ResourceType_CollectionItemCanBeOnlyEntity);
            }
            this.itemType = itemType;
        }

        private static Type GetInstanceType(ResourceType itemType)
        {
            return typeof(IEnumerable<>).MakeGenericType(new Type[] { itemType.InstanceType });
        }

        private static string GetName(ResourceType itemType)
        {
            return string.Format(CultureInfo.InvariantCulture, "Collection({0})", new object[] { itemType.FullName });
        }

        public ResourceType ItemType
        {
            get
            {
                return this.itemType;
            }
        }
    }
}

