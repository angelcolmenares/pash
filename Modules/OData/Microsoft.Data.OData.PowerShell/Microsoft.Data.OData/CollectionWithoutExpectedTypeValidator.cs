namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Runtime.InteropServices;

    internal sealed class CollectionWithoutExpectedTypeValidator
    {
        private readonly bool itemTypeDerivedFromCollectionValue;
        private EdmTypeKind itemTypeKind;
        private string itemTypeName;
        private IEdmPrimitiveType primitiveItemType;

        internal CollectionWithoutExpectedTypeValidator(string itemTypeNameFromCollection)
        {
            if (itemTypeNameFromCollection != null)
            {
                this.itemTypeName = itemTypeNameFromCollection;
                this.itemTypeKind = ComputeExpectedTypeKind(this.itemTypeName, out this.primitiveItemType);
                this.itemTypeDerivedFromCollectionValue = true;
            }
        }

        private static EdmTypeKind ComputeExpectedTypeKind(string typeName, out IEdmPrimitiveType primitiveItemType)
        {
            IEdmSchemaType type = EdmCoreModel.Instance.FindDeclaredType(typeName);
            if (type != null)
            {
                primitiveItemType = (IEdmPrimitiveType) type;
                return EdmTypeKind.Primitive;
            }
            primitiveItemType = null;
            return EdmTypeKind.Complex;
        }

        internal void ValidateCollectionItem(string collectionItemTypeName, EdmTypeKind collectionItemTypeKind)
        {
            if ((collectionItemTypeKind != EdmTypeKind.Primitive) && (collectionItemTypeKind != EdmTypeKind.Complex))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.CollectionWithoutExpectedTypeValidator_InvalidItemTypeKind(collectionItemTypeKind));
            }
            if (this.itemTypeDerivedFromCollectionValue)
            {
                collectionItemTypeName = collectionItemTypeName ?? this.itemTypeName;
                this.ValidateCollectionItemTypeNameAndKind(collectionItemTypeName, collectionItemTypeKind);
            }
            else
            {
                if (this.itemTypeKind == EdmTypeKind.None)
                {
                    this.itemTypeKind = (collectionItemTypeName == null) ? collectionItemTypeKind : ComputeExpectedTypeKind(collectionItemTypeName, out this.primitiveItemType);
                    if (collectionItemTypeName == null)
                    {
                        this.itemTypeKind = collectionItemTypeKind;
                        if (this.itemTypeKind == EdmTypeKind.Primitive)
                        {
                            this.itemTypeName = "Edm.String";
                            this.primitiveItemType = EdmCoreModel.Instance.GetString(false).PrimitiveDefinition();
                        }
                        else
                        {
                            this.itemTypeName = null;
                            this.primitiveItemType = null;
                        }
                    }
                    else
                    {
                        this.itemTypeKind = ComputeExpectedTypeKind(collectionItemTypeName, out this.primitiveItemType);
                        this.itemTypeName = collectionItemTypeName;
                    }
                }
                if ((collectionItemTypeName == null) && (collectionItemTypeKind == EdmTypeKind.Primitive))
                {
                    collectionItemTypeName = "Edm.String";
                }
                this.ValidateCollectionItemTypeNameAndKind(collectionItemTypeName, collectionItemTypeKind);
            }
        }

        private void ValidateCollectionItemTypeNameAndKind(string collectionItemTypeName, EdmTypeKind collectionItemTypeKind)
        {
            if (this.itemTypeKind != collectionItemTypeKind)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.CollectionWithoutExpectedTypeValidator_IncompatibleItemTypeKind(collectionItemTypeKind, this.itemTypeKind));
            }
            if (this.itemTypeKind == EdmTypeKind.Primitive)
            {
                if (string.CompareOrdinal(this.itemTypeName, collectionItemTypeName) == 0)
                {
                    return;
                }
                if (this.primitiveItemType.IsSpatial())
                {
                    EdmPrimitiveTypeKind primitiveTypeKind = EdmCoreModel.Instance.GetPrimitiveTypeKind(collectionItemTypeName);
                    IEdmPrimitiveType primitiveType = EdmCoreModel.Instance.GetPrimitiveType(primitiveTypeKind);
                    if (this.itemTypeDerivedFromCollectionValue)
                    {
                        if (this.primitiveItemType.IsAssignableFrom(primitiveType))
                        {
                            return;
                        }
                    }
                    else
                    {
                        IEdmPrimitiveType commonBaseType = this.primitiveItemType.GetCommonBaseType(primitiveType);
                        if (commonBaseType != null)
                        {
                            this.primitiveItemType = commonBaseType;
                            this.itemTypeName = commonBaseType.ODataFullName();
                            return;
                        }
                    }
                }
                throw new ODataException(Microsoft.Data.OData.Strings.CollectionWithoutExpectedTypeValidator_IncompatibleItemTypeName(collectionItemTypeName, this.itemTypeName));
            }
            if (string.CompareOrdinal(this.itemTypeName, collectionItemTypeName) != 0)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.CollectionWithoutExpectedTypeValidator_IncompatibleItemTypeName(collectionItemTypeName, this.itemTypeName));
            }
        }

        internal EdmTypeKind ItemTypeKindFromCollection
        {
            get
            {
                if (!this.itemTypeDerivedFromCollectionValue)
                {
                    return EdmTypeKind.None;
                }
                return this.itemTypeKind;
            }
        }

        internal string ItemTypeNameFromCollection
        {
            get
            {
                if (!this.itemTypeDerivedFromCollectionValue)
                {
                    return null;
                }
                return this.itemTypeName;
            }
        }
    }
}

