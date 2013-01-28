namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;

    [DebuggerDisplay("{Name}: {InstanceType}, {ResourceTypeKind}")]
    internal class CollectionResourceType : ResourceType
    {
        private readonly ResourceType itemType;

        internal CollectionResourceType(ResourceType itemType) : base(GetInstanceType(itemType), ResourceTypeKind.Collection, string.Empty, GetName(itemType))
        {
            if ((itemType.ResourceTypeKind != ResourceTypeKind.Primitive) && (itemType.ResourceTypeKind != ResourceTypeKind.ComplexType))
            {
                throw new ArgumentException(Strings.ResourceType_CollectionItemCanBeOnlyPrimitiveOrComplex);
            }
            if (itemType == ResourceType.GetPrimitiveResourceType(typeof(Stream)))
            {
                throw new ArgumentException(Strings.ResourceType_CollectionItemCannotBeStream(itemType.FullName), "itemType");
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

