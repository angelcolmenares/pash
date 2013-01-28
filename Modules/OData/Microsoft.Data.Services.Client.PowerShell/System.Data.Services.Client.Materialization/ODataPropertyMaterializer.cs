namespace System.Data.Services.Client.Materialization
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Data.Services.Client.Metadata;

    internal sealed class ODataPropertyMaterializer : ODataMessageReaderMaterializer
    {
        private object currentValue;

        public ODataPropertyMaterializer(ODataMessageReader reader, ResponseInfo info, Type expectedType, bool? singleResult) : base(reader, info, expectedType, singleResult)
        {
        }

        protected override void ReadFromMessageReader(ODataMessageReader reader, IEdmTypeReference expectedType)
        {
            ODataProperty collectionProperty = reader.ReadProperty(expectedType);
            Type type = Nullable.GetUnderlyingType(base.ExpectedType) ?? base.ExpectedType;
            object obj2 = collectionProperty.Value;
            if (expectedType.IsCollection())
            {
                object obj3;
                Type collectionItemType = type;
                Type implementationType = ClientTypeUtil.GetImplementationType(type, typeof(ICollection<>));
                if (implementationType != null)
                {
                    collectionItemType = implementationType.GetGenericArguments()[0];
                    obj3 = ODataMaterializer.CreateCollectionInstance(collectionProperty, type, base.ResponseInfo);
                }
                else
                {
                    implementationType = typeof(ICollection<>).MakeGenericType(new Type[] { collectionItemType });
                    obj3 = ODataMaterializer.CreateCollectionInstance(collectionProperty, implementationType, base.ResponseInfo);
                }
                ODataMaterializer.ApplyCollectionDataValues(collectionProperty, base.ResponseInfo.IgnoreMissingProperties, base.ResponseInfo, obj3, collectionItemType, ODataMaterializer.GetAddToCollectionDelegate(implementationType));
                this.currentValue = obj3;
            }
            else if (expectedType.IsComplex())
            {
                ODataComplexValue complexValue = obj2 as ODataComplexValue;
                ODataMaterializer.MaterializeComplexTypeProperty(type, complexValue, base.ResponseInfo.IgnoreMissingProperties, base.ResponseInfo);
                this.currentValue = complexValue.GetMaterializedValue();
            }
            else
            {
                ODataMaterializer.MaterializePrimitiveDataValue(base.ExpectedType, collectionProperty);
                this.currentValue = collectionProperty.GetMaterializedValue();
            }
        }

        internal override object CurrentValue
        {
            get
            {
                return this.currentValue;
            }
        }
    }
}

