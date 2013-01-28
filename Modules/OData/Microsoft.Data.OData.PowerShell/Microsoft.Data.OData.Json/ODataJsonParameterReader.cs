namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;

    internal sealed class ODataJsonParameterReader : ODataParameterReaderCore
    {
        private readonly ODataJsonInputContext jsonInputContext;
        private readonly ODataJsonPropertyAndValueDeserializer jsonPropertyAndValueDeserializer;

        internal ODataJsonParameterReader(ODataJsonInputContext jsonInputContext, IEdmFunctionImport functionImport) : base(jsonInputContext, functionImport)
        {
            this.jsonInputContext = jsonInputContext;
            this.jsonPropertyAndValueDeserializer = new ODataJsonPropertyAndValueDeserializer(jsonInputContext);
        }

        protected override ODataCollectionReader CreateCollectionReader(IEdmTypeReference expectedItemTypeReference)
        {
            return new ODataJsonCollectionReader(this.jsonInputContext, expectedItemTypeReference, this);
        }

        private bool EndOfParameters()
        {
            return (this.jsonPropertyAndValueDeserializer.JsonReader.NodeType == JsonNodeType.EndObject);
        }

        protected override bool ReadAtStartImplementation()
        {
            this.jsonPropertyAndValueDeserializer.ReadPayloadStart(false);
            if (this.jsonPropertyAndValueDeserializer.JsonReader.NodeType == JsonNodeType.EndOfInput)
            {
                base.PopScope(ODataParameterReaderState.Start);
                base.EnterScope(ODataParameterReaderState.Completed, null, null);
                return false;
            }
            this.jsonPropertyAndValueDeserializer.JsonReader.ReadStartObject();
            if (this.EndOfParameters())
            {
                this.ReadParametersEnd();
                return false;
            }
            this.ReadNextParameter();
            return true;
        }

        private void ReadNextParameter()
        {
            ODataParameterReaderState collection;
            object obj2;
            string parameterName = this.jsonPropertyAndValueDeserializer.JsonReader.ReadPropertyName();
            IEdmTypeReference parameterTypeReference = base.GetParameterTypeReference(parameterName);
            switch (parameterTypeReference.TypeKind())
            {
                case EdmTypeKind.Primitive:
                {
                    IEdmPrimitiveTypeReference type = parameterTypeReference.AsPrimitive();
                    if (type.PrimitiveKind() == EdmPrimitiveTypeKind.Stream)
                    {
                        throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonParameterReader_UnsupportedPrimitiveParameterType(parameterName, type.PrimitiveKind()));
                    }
                    obj2 = this.jsonPropertyAndValueDeserializer.ReadNonEntityValue(type, null, null, true);
                    collection = ODataParameterReaderState.Value;
                    break;
                }
                case EdmTypeKind.Complex:
                    obj2 = this.jsonPropertyAndValueDeserializer.ReadNonEntityValue(parameterTypeReference, null, null, true);
                    collection = ODataParameterReaderState.Value;
                    break;

                case EdmTypeKind.Collection:
                    obj2 = null;
                    if (this.jsonPropertyAndValueDeserializer.JsonReader.NodeType == JsonNodeType.PrimitiveValue)
                    {
                        obj2 = this.jsonPropertyAndValueDeserializer.JsonReader.ReadPrimitiveValue();
                        if (obj2 != null)
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonParameterReader_NullCollectionExpected(JsonNodeType.PrimitiveValue, obj2));
                        }
                        collection = ODataParameterReaderState.Value;
                    }
                    else
                    {
                        if (((IEdmCollectionType) parameterTypeReference.Definition).ElementType.TypeKind() == EdmTypeKind.Entity)
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonParameterReader_UnsupportedParameterTypeKind(parameterName, "Entity Collection"));
                        }
                        collection = ODataParameterReaderState.Collection;
                    }
                    break;

                default:
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataJsonParameterReader_UnsupportedParameterTypeKind(parameterName, parameterTypeReference.TypeKind()));
            }
            base.EnterScope(collection, parameterName, obj2);
        }

        protected override bool ReadNextParameterImplementation()
        {
            base.PopScope(this.State);
            if (this.EndOfParameters())
            {
                this.ReadParametersEnd();
                return false;
            }
            this.ReadNextParameter();
            return true;
        }

        private void ReadParametersEnd()
        {
            this.jsonPropertyAndValueDeserializer.JsonReader.ReadEndObject();
            this.jsonPropertyAndValueDeserializer.ReadPayloadEnd(false);
            base.PopScope(ODataParameterReaderState.Start);
            base.EnterScope(ODataParameterReaderState.Completed, null, null);
        }
    }
}

