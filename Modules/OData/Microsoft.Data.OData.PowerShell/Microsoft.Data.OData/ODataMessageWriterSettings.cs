using System.Xml;

namespace Microsoft.Data.OData
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class ODataMessageWriterSettings
    {
        private string acceptCharSets;
        private string acceptMediaTypes;
        private ODataFormat format;
        private ODataMessageQuotas messageQuotas;
        private bool? useFormat;
        private ODataWriterBehavior writerBehavior;

        public ODataMessageWriterSettings()
        {
            this.CheckCharacters = true;
            this.writerBehavior = ODataWriterBehavior.DefaultBehavior;
        }

        public ODataMessageWriterSettings(ODataMessageWriterSettings settings)
        {
            ExceptionUtils.CheckArgumentNotNull<ODataMessageWriterSettings>(settings, "settings");
            this.acceptCharSets = settings.acceptCharSets;
            this.acceptMediaTypes = settings.acceptMediaTypes;
            this.BaseUri = settings.BaseUri;
            this.CheckCharacters = settings.CheckCharacters;
            this.DisableMessageStreamDisposal = settings.DisableMessageStreamDisposal;
            this.format = settings.format;
            this.Indent = settings.Indent;
            this.messageQuotas = new ODataMessageQuotas(settings.MessageQuotas);
            this.useFormat = settings.useFormat;
            this.Version = settings.Version;
            this.writerBehavior = settings.writerBehavior;
        }

        public void EnableDefaultBehavior()
        {
            this.writerBehavior = ODataWriterBehavior.DefaultBehavior;
        }

        public void EnableWcfDataServicesClientBehavior(Func<ODataEntry, XmlWriter, XmlWriter> startEntryXmlCustomizationCallback, Action<ODataEntry, XmlWriter, XmlWriter> endEntryXmlCustomizationCallback, string odataNamespace, string typeScheme)
        {
            ExceptionUtils.CheckArgumentNotNull<string>(odataNamespace, "odataNamespace");
            ExceptionUtils.CheckArgumentNotNull<string>(typeScheme, "typeScheme");
            if (((startEntryXmlCustomizationCallback == null) && (endEntryXmlCustomizationCallback != null)) || ((startEntryXmlCustomizationCallback != null) && (endEntryXmlCustomizationCallback == null)))
            {
                throw new ODataException(Strings.ODataMessageWriterSettings_MessageWriterSettingsXmlCustomizationCallbacksMustBeSpecifiedBoth);
            }
            this.writerBehavior = ODataWriterBehavior.CreateWcfDataServicesClientBehavior(startEntryXmlCustomizationCallback, endEntryXmlCustomizationCallback, odataNamespace, typeScheme);
        }

        public void EnableWcfDataServicesServerBehavior(bool usesV1Provider)
        {
            this.writerBehavior = ODataWriterBehavior.CreateWcfDataServicesServerBehavior(usesV1Provider);
        }

        public void SetContentType(ODataFormat payloadFormat)
        {
            this.acceptCharSets = null;
            this.acceptMediaTypes = null;
            this.format = payloadFormat;
            this.useFormat = true;
        }

        public void SetContentType(string acceptableMediaTypes, string acceptableCharSets)
        {
            this.acceptMediaTypes = acceptableMediaTypes;
            this.acceptCharSets = acceptableCharSets;
            this.format = null;
            this.useFormat = false;
        }

        internal string AcceptableCharsets
        {
            get
            {
                return this.acceptCharSets;
            }
        }

        internal string AcceptableMediaTypes
        {
            get
            {
                return this.acceptMediaTypes;
            }
        }

        public Uri BaseUri { get; set; }

        public bool CheckCharacters { get; set; }

        public bool DisableMessageStreamDisposal { get; set; }

        internal ODataFormat Format
        {
            get
            {
                return this.format;
            }
        }

        public bool Indent { get; set; }

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

        internal bool? UseFormat
        {
            get
            {
                return this.useFormat;
            }
        }

        public ODataVersion? Version { get; set; }

        internal ODataWriterBehavior WriterBehavior
        {
            get
            {
                return this.writerBehavior;
            }
        }
    }
}

