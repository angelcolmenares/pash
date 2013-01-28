namespace System.Data.Services.Client.Materialization
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Data.Services.Client;
    using System.Data.Services.Client.Metadata;

    internal sealed class ODataLinksMaterializer : ODataMessageReaderMaterializer
    {
        private ODataEntityReferenceLinks links;

        public ODataLinksMaterializer(ODataMessageReader reader, ResponseInfo info, Type expectedType, bool? singleResult) : base(reader, info, expectedType, singleResult)
        {
        }

        protected override void ReadFromMessageReader(ODataMessageReader reader, IEdmTypeReference expectedType)
        {
            this.ReadLinks();
            Type type = Nullable.GetUnderlyingType(base.ExpectedType) ?? base.ExpectedType;
            ClientEdmModel model = ClientEdmModel.GetModel(base.ResponseInfo.MaxProtocolVersion);
            ClientTypeAnnotation clientTypeAnnotation = model.GetClientTypeAnnotation(model.GetOrCreateEdmType(type));
            if (clientTypeAnnotation.IsEntityType)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.AtomMaterializer_InvalidEntityType(clientTypeAnnotation.ElementTypeName));
            }
            throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Deserialize_MixedTextWithComment);
        }

        private void ReadLinks()
        {
            try
            {
                if (this.links == null)
                {
                    this.links = base.messageReader.ReadEntityReferenceLinks();
                }
            }
            catch (ODataErrorException exception)
            {
                throw new DataServiceClientException(System.Data.Services.Client.Strings.Deserialize_ServerException(exception.Error.Message), exception);
            }
            catch (ODataException exception2)
            {
                throw new InvalidOperationException(exception2.Message, exception2);
            }
        }

        internal override long CountValue
        {
            get
            {
                if ((this.links == null) && !this.IsDisposed)
                {
                    this.ReadLinks();
                }
                if ((this.links == null) || !this.links.Count.HasValue)
                {
                    throw new InvalidOperationException(System.Data.Services.Client.Strings.MaterializeFromAtom_CountNotPresent);
                }
                return this.links.Count.Value;
            }
        }

        internal override object CurrentValue
        {
            get
            {
                return null;
            }
        }

        internal override bool IsCountable
        {
            get
            {
                return true;
            }
        }
    }
}

