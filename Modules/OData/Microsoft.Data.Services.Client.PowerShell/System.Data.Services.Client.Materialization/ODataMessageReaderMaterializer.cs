namespace System.Data.Services.Client.Materialization
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Data.Services.Client.Metadata;

    internal abstract class ODataMessageReaderMaterializer : ODataMaterializer
    {
        private bool hasReadValue;
        protected ODataMessageReader messageReader;
        protected readonly bool? SingleResult;

        public ODataMessageReaderMaterializer(ODataMessageReader reader, ResponseInfo info, Type expectedType, bool? singleResult) : base(info, expectedType)
        {
            this.messageReader = reader;
            this.SingleResult = singleResult;
        }

        internal sealed override void ApplyLogToContext()
        {
        }

        internal sealed override void ClearLog()
        {
        }

        protected sealed override void OnDispose()
        {
            if (this.messageReader != null)
            {
                this.messageReader.Dispose();
                this.messageReader = null;
            }
        }

        protected abstract void ReadFromMessageReader(ODataMessageReader reader, IEdmTypeReference expectedType);
        protected sealed override bool ReadImplementation()
        {
            if (this.hasReadValue)
            {
                return false;
            }
            try
            {
                ClientEdmModel model = ClientEdmModel.GetModel(base.ResponseInfo.MaxProtocolVersion);
                IEdmTypeReference expectedType = model.GetOrCreateEdmType(base.ExpectedType).ToEdmTypeReference(ClientTypeUtil.CanAssignNull(base.ExpectedType));
                if ((this.SingleResult.HasValue && !this.SingleResult.Value) && (expectedType.Definition.TypeKind != EdmTypeKind.Collection))
                {
                    expectedType = model.GetOrCreateEdmType(typeof(ICollection<>).MakeGenericType(new Type[] { base.ExpectedType })).ToEdmTypeReference(false);
                }
                this.ReadFromMessageReader(this.messageReader, expectedType);
            }
            catch (ODataErrorException exception)
            {
                throw new DataServiceClientException(System.Data.Services.Client.Strings.Deserialize_ServerException(exception.Error.Message), exception);
            }
            catch (ODataException exception2)
            {
                throw new InvalidOperationException(exception2.Message, exception2);
            }
            catch (ArgumentException exception3)
            {
                throw new InvalidOperationException(exception3.Message, exception3);
            }
            finally
            {
                this.hasReadValue = true;
            }
            return true;
        }

        internal override long CountValue
        {
            get
            {
                throw new InvalidOperationException(System.Data.Services.Client.Strings.MaterializeFromAtom_CountNotPresent);
            }
        }

        internal sealed override ODataEntry CurrentEntry
        {
            get
            {
                return null;
            }
        }

        internal sealed override ODataFeed CurrentFeed
        {
            get
            {
                return null;
            }
        }

        protected sealed override bool IsDisposed
        {
            get
            {
                return (this.messageReader == null);
            }
        }

        internal sealed override bool IsEndOfStream
        {
            get
            {
                return this.hasReadValue;
            }
        }

        internal sealed override ProjectionPlan MaterializeEntryPlan
        {
            get
            {
                throw new NotSupportedException();
            }
        }
    }
}

