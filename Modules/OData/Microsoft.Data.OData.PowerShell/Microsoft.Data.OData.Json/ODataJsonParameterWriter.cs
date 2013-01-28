namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Threading.Tasks;

    internal sealed class ODataJsonParameterWriter : ODataParameterWriterCore
    {
        private readonly ODataJsonOutputContext jsonOutputContext;
        private readonly ODataJsonPropertyAndValueSerializer jsonPropertyAndValueSerializer;

        internal ODataJsonParameterWriter(ODataJsonOutputContext jsonOutputContext, IEdmFunctionImport functionImport) : base(jsonOutputContext, functionImport)
        {
            this.jsonOutputContext = jsonOutputContext;
            this.jsonPropertyAndValueSerializer = new ODataJsonPropertyAndValueSerializer(this.jsonOutputContext);
        }

        protected override ODataCollectionWriter CreateFormatCollectionWriter(string parameterName, IEdmTypeReference expectedItemType)
        {
            this.jsonOutputContext.JsonWriter.WriteName(parameterName);
            return new ODataJsonCollectionWriter(this.jsonOutputContext, expectedItemType, this);
        }

        protected override void EndPayload()
        {
            this.jsonOutputContext.JsonWriter.EndObjectScope();
            this.jsonPropertyAndValueSerializer.WritePayloadEnd();
        }

        protected override Task FlushAsynchronously()
        {
            return this.jsonOutputContext.FlushAsync();
        }

        protected override void FlushSynchronously()
        {
            this.jsonOutputContext.Flush();
        }

        protected override void StartPayload()
        {
            this.jsonPropertyAndValueSerializer.WritePayloadStart();
            this.jsonOutputContext.JsonWriter.StartObjectScope();
        }

        protected override void VerifyNotDisposed()
        {
            this.jsonOutputContext.VerifyNotDisposed();
        }

        protected override void WriteValueParameter(string parameterName, object parameterValue, IEdmTypeReference expectedTypeReference)
        {
            this.jsonOutputContext.JsonWriter.WriteName(parameterName);
            if (parameterValue == null)
            {
                this.jsonOutputContext.JsonWriter.WriteValue((string) null);
            }
            else
            {
                ODataComplexValue complexValue = parameterValue as ODataComplexValue;
                if (complexValue != null)
                {
                    this.jsonPropertyAndValueSerializer.WriteComplexValue(complexValue, expectedTypeReference, false, base.DuplicatePropertyNamesChecker, null);
                    base.DuplicatePropertyNamesChecker.Clear();
                }
                else
                {
                    this.jsonPropertyAndValueSerializer.WritePrimitiveValue(parameterValue, null, expectedTypeReference);
                }
            }
        }
    }
}

