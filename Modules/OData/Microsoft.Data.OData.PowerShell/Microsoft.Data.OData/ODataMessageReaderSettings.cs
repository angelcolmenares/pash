using Microsoft.Data.Edm;
using System.Xml;

namespace Microsoft.Data.OData
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class ODataMessageReaderSettings
    {
        private ODataMessageQuotas messageQuotas;
        private ODataReaderBehavior readerBehavior;

        public ODataMessageReaderSettings()
        {
            this.DisablePrimitiveTypeConversion = false;
            this.DisableMessageStreamDisposal = false;
            this.UndeclaredPropertyBehaviorKinds = ODataUndeclaredPropertyBehaviorKinds.None;
            this.CheckCharacters = false;
            this.EnableAtomMetadataReading = false;
            this.readerBehavior = ODataReaderBehavior.DefaultBehavior;
            this.MaxProtocolVersion = ODataVersion.V3;
        }

        public ODataMessageReaderSettings(ODataMessageReaderSettings other)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataMessageReaderSettings>(other, "other");
            this.BaseUri = other.BaseUri;
            this.CheckCharacters = other.CheckCharacters;
            this.DisableMessageStreamDisposal = other.DisableMessageStreamDisposal;
            this.DisablePrimitiveTypeConversion = other.DisablePrimitiveTypeConversion;
            this.EnableAtomMetadataReading = other.EnableAtomMetadataReading;
            this.messageQuotas = new ODataMessageQuotas(other.MessageQuotas);
            this.UndeclaredPropertyBehaviorKinds = other.UndeclaredPropertyBehaviorKinds;
            this.MaxProtocolVersion = other.MaxProtocolVersion;
            this.readerBehavior = other.ReaderBehavior;
        }

        public void EnableDefaultBehavior()
        {
            this.readerBehavior = ODataReaderBehavior.DefaultBehavior;
        }

        public void EnableWcfDataServicesClientBehavior(Func<IEdmType, string, IEdmType> typeResolver, string odataNamespace, string typeScheme, Func<ODataEntry, XmlReader, Uri, XmlReader> entryXmlCustomizationCallback)
        {
            ExceptionUtils.CheckArgumentNotNull<string>(odataNamespace, "odataNamespace");
            ExceptionUtils.CheckArgumentNotNull<string>(typeScheme, "typeScheme");
            this.readerBehavior = ODataReaderBehavior.CreateWcfDataServicesClientBehavior(typeResolver, odataNamespace, typeScheme, entryXmlCustomizationCallback);
        }

        public void EnableWcfDataServicesServerBehavior(bool usesV1Provider)
        {
            this.readerBehavior = ODataReaderBehavior.CreateWcfDataServicesServerBehavior(usesV1Provider);
        }

        public Uri BaseUri { get; set; }

        public bool CheckCharacters { get; set; }

        public bool DisableMessageStreamDisposal { get; set; }

        public bool DisablePrimitiveTypeConversion { get; set; }

        internal bool DisableStrictMetadataValidation
        {
            get
            {
                if (this.ReaderBehavior.ApiBehaviorKind != ODataBehaviorKind.WcfDataServicesServer)
                {
                    return (this.ReaderBehavior.ApiBehaviorKind == ODataBehaviorKind.WcfDataServicesClient);
                }
                return true;
            }
        }

        public bool EnableAtomMetadataReading { get; set; }

        public ODataVersion MaxProtocolVersion { get; set; }

        public ODataMessageQuotas MessageQuotas
        {
            get
            {
                if (this.messageQuotas == null)
                {
                    this.messageQuotas = new ODataMessageQuotas();
                }
                return this.messageQuotas;
            }
            set
            {
                this.messageQuotas = value;
            }
        }

        internal ODataReaderBehavior ReaderBehavior
        {
            get
            {
                return this.readerBehavior;
            }
        }

        public ODataUndeclaredPropertyBehaviorKinds UndeclaredPropertyBehaviorKinds { get; set; }
    }
}

