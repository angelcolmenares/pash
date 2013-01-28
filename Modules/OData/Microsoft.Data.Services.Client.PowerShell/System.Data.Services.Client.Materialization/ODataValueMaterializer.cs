namespace System.Data.Services.Client.Materialization
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Data.Services.Client;

    internal sealed class ODataValueMaterializer : ODataMessageReaderMaterializer
    {
        private object currentValue;

        public ODataValueMaterializer(ODataMessageReader reader, ResponseInfo info, Type expectedType, bool? singleResult) : base(reader, info, expectedType, singleResult)
        {
        }

        protected override void ReadFromMessageReader(ODataMessageReader reader, IEdmTypeReference expectedType)
        {
            object obj2 = reader.ReadValue(expectedType);
            ODataMaterializer.MaterializePrimitiveDataValue(base.ExpectedType, null, obj2, base.ResponseInfo, () => "TODO: Is this reachable?", out this.currentValue);
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

