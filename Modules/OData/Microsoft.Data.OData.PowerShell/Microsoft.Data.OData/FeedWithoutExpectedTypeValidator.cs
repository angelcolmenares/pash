namespace Microsoft.Data.OData
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData.Metadata;
    using System;

    internal sealed class FeedWithoutExpectedTypeValidator
    {
        private IEdmEntityType itemType;

        internal FeedWithoutExpectedTypeValidator()
        {
        }

        internal void ValidateEntry(IEdmEntityType entityType)
        {
            if (this.itemType == null)
            {
                this.itemType = entityType;
            }
            if (!((IEdmType) this.itemType).IsEquivalentTo(((IEdmType) entityType)))
            {
                IEdmType commonBaseType = this.itemType.GetCommonBaseType(entityType);
                if (commonBaseType == null)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.FeedWithoutExpectedTypeValidator_IncompatibleTypes(entityType.ODataFullName(), this.itemType.ODataFullName()));
                }
                this.itemType = (IEdmEntityType) commonBaseType;
            }
        }
    }
}

