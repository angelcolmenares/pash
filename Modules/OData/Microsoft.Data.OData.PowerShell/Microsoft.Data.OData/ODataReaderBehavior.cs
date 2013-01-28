
namespace Microsoft.Data.OData
{
    using System;
	using System.Xml;
	using Microsoft.Data.Edm;

    internal sealed class ODataReaderBehavior
    {
        private bool allowDuplicatePropertyNames;
        private readonly ODataBehaviorKind apiBehaviorKind;
        private static readonly ODataReaderBehavior defaultReaderBehavior = new ODataReaderBehavior(ODataBehaviorKind.Default, ODataBehaviorKind.Default, false, false, null, "http://schemas.microsoft.com/ado/2007/08/dataservices", "http://schemas.microsoft.com/ado/2007/08/dataservices/scheme", null);
        private readonly Func<ODataEntry, XmlReader, Uri, XmlReader> entryXmlCustomizationCallback;
        private ODataBehaviorKind formatBehaviorKind;
        private string odataNamespace;
        private readonly Func<IEdmType, string, IEdmType> typeResolver;
        private string typeScheme;
        private bool usesV1Provider;

        private ODataReaderBehavior(ODataBehaviorKind formatBehaviorKind, ODataBehaviorKind apiBehaviorKind, bool allowDuplicatePropertyNames, bool usesV1Provider, Func<IEdmType, string, IEdmType> typeResolver, string odataNamespace, string typeScheme, Func<ODataEntry, XmlReader, Uri, XmlReader> entryXmlCustomizationCallback)
        {
            this.formatBehaviorKind = formatBehaviorKind;
            this.apiBehaviorKind = apiBehaviorKind;
            this.allowDuplicatePropertyNames = allowDuplicatePropertyNames;
            this.usesV1Provider = usesV1Provider;
            this.typeResolver = typeResolver;
            this.entryXmlCustomizationCallback = entryXmlCustomizationCallback;
            this.odataNamespace = odataNamespace;
            this.typeScheme = typeScheme;
        }

        internal static ODataReaderBehavior CreateWcfDataServicesClientBehavior(Func<IEdmType, string, IEdmType> typeResolver, string odataNamespace, string typeScheme, Func<ODataEntry, XmlReader, Uri, XmlReader> entryXmlCustomizationCallback)
        {
            return new ODataReaderBehavior(ODataBehaviorKind.WcfDataServicesClient, ODataBehaviorKind.WcfDataServicesClient, true, false, typeResolver, odataNamespace, typeScheme, entryXmlCustomizationCallback);
        }

        internal static ODataReaderBehavior CreateWcfDataServicesServerBehavior(bool usesV1Provider)
        {
            return new ODataReaderBehavior(ODataBehaviorKind.WcfDataServicesServer, ODataBehaviorKind.WcfDataServicesServer, true, usesV1Provider, null, "http://schemas.microsoft.com/ado/2007/08/dataservices", "http://schemas.microsoft.com/ado/2007/08/dataservices/scheme", null);
        }

        internal void ResetFormatBehavior()
        {
            this.formatBehaviorKind = ODataBehaviorKind.Default;
            this.allowDuplicatePropertyNames = false;
            this.usesV1Provider = false;
            this.odataNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices";
            this.typeScheme = "http://schemas.microsoft.com/ado/2007/08/dataservices/scheme";
        }

        internal bool AllowDuplicatePropertyNames
        {
            get
            {
                return this.allowDuplicatePropertyNames;
            }
        }

        internal ODataBehaviorKind ApiBehaviorKind
        {
            get
            {
                return this.apiBehaviorKind;
            }
        }

        internal static ODataReaderBehavior DefaultBehavior
        {
            get
            {
                return defaultReaderBehavior;
            }
        }

        internal Func<ODataEntry, XmlReader, Uri, XmlReader> EntryXmlCustomizationCallback
        {
            get
            {
                return this.entryXmlCustomizationCallback;
            }
        }

        internal ODataBehaviorKind FormatBehaviorKind
        {
            get
            {
                return this.formatBehaviorKind;
            }
        }

        internal string ODataNamespace
        {
            get
            {
                return this.odataNamespace;
            }
        }

        internal string ODataTypeScheme
        {
            get
            {
                return this.typeScheme;
            }
        }

        internal Func<IEdmType, string, IEdmType> TypeResolver
        {
            get
            {
                return this.typeResolver;
            }
        }

        internal bool UseV1ProviderBehavior
        {
            get
            {
                return this.usesV1Provider;
            }
        }
    }
}

