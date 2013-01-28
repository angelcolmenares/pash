using System.Xml;

namespace Microsoft.Data.OData
{
    using System;

    internal sealed class ODataWriterBehavior
    {
        private bool allowDuplicatePropertyNames;
        private bool allowNullValuesForNonNullablePrimitiveTypes;
        private readonly ODataBehaviorKind apiBehaviorKind;
        private static readonly ODataWriterBehavior defaultWriterBehavior = new ODataWriterBehavior(ODataBehaviorKind.Default, ODataBehaviorKind.Default, false, false, false, null, null, "http://schemas.microsoft.com/ado/2007/08/dataservices", "http://schemas.microsoft.com/ado/2007/08/dataservices/scheme");
        private readonly Action<ODataEntry, XmlWriter, XmlWriter> endEntryXmlCustomizationCallback;
        private ODataBehaviorKind formatBehaviorKind;
        private string odataNamespace;
        private readonly Func<ODataEntry, XmlWriter, XmlWriter> startEntryXmlCustomizationCallback;
        private string typeScheme;
        private bool usesV1Provider;

        private ODataWriterBehavior(ODataBehaviorKind formatBehaviorKind, ODataBehaviorKind apiBehaviorKind, bool usesV1Provider, bool allowNullValuesForNonNullablePrimitiveTypes, bool allowDuplicatePropertyNames, Func<ODataEntry, XmlWriter, XmlWriter> startEntryXmlCustomizationCallback, Action<ODataEntry, XmlWriter, XmlWriter> endEntryXmlCustomizationCallback, string odataNamespace, string typeScheme)
        {
            this.formatBehaviorKind = formatBehaviorKind;
            this.apiBehaviorKind = apiBehaviorKind;
            this.usesV1Provider = usesV1Provider;
            this.allowNullValuesForNonNullablePrimitiveTypes = allowNullValuesForNonNullablePrimitiveTypes;
            this.allowDuplicatePropertyNames = allowDuplicatePropertyNames;
            this.startEntryXmlCustomizationCallback = startEntryXmlCustomizationCallback;
            this.endEntryXmlCustomizationCallback = endEntryXmlCustomizationCallback;
            this.odataNamespace = odataNamespace;
            this.typeScheme = typeScheme;
        }

        internal static ODataWriterBehavior CreateWcfDataServicesClientBehavior(Func<ODataEntry, XmlWriter, XmlWriter> startEntryXmlCustomizationCallback, Action<ODataEntry, XmlWriter, XmlWriter> endEntryXmlCustomizationCallback, string odataNamespace, string typeScheme)
        {
            return new ODataWriterBehavior(ODataBehaviorKind.WcfDataServicesClient, ODataBehaviorKind.WcfDataServicesClient, false, false, false, startEntryXmlCustomizationCallback, endEntryXmlCustomizationCallback, odataNamespace, typeScheme);
        }

        internal static ODataWriterBehavior CreateWcfDataServicesServerBehavior(bool usesV1Provider)
        {
            return new ODataWriterBehavior(ODataBehaviorKind.WcfDataServicesServer, ODataBehaviorKind.WcfDataServicesServer, usesV1Provider, true, true, null, null, "http://schemas.microsoft.com/ado/2007/08/dataservices", "http://schemas.microsoft.com/ado/2007/08/dataservices/scheme");
        }

        internal void UseDefaultFormatBehavior()
        {
            this.formatBehaviorKind = ODataBehaviorKind.Default;
            this.usesV1Provider = false;
            this.allowNullValuesForNonNullablePrimitiveTypes = false;
            this.allowDuplicatePropertyNames = false;
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

        internal bool AllowNullValuesForNonNullablePrimitiveTypes
        {
            get
            {
                return this.allowNullValuesForNonNullablePrimitiveTypes;
            }
        }

        internal ODataBehaviorKind ApiBehaviorKind
        {
            get
            {
                return this.apiBehaviorKind;
            }
        }

        internal static ODataWriterBehavior DefaultBehavior
        {
            get
            {
                return defaultWriterBehavior;
            }
        }

        internal Action<ODataEntry, XmlWriter, XmlWriter> EndEntryXmlCustomizationCallback
        {
            get
            {
                return this.endEntryXmlCustomizationCallback;
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

        internal Func<ODataEntry, XmlWriter, XmlWriter> StartEntryXmlCustomizationCallback
        {
            get
            {
                return this.startEntryXmlCustomizationCallback;
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

