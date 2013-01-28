namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;

    internal sealed class ODataAtomPayloadKindDetectionDeserializer : ODataAtomPropertyAndValueDeserializer
    {
        internal ODataAtomPayloadKindDetectionDeserializer(ODataAtomInputContext atomInputContext) : base(atomInputContext)
        {
        }

        internal IEnumerable<ODataPayloadKind> DetectPayloadKind(ODataPayloadKindDetectionInfo detectionInfo)
        {
            base.XmlReader.DisableInStreamErrorDetection = true;
            try
            {
                if (base.XmlReader.TryReadToNextElement())
                {
                    if (string.CompareOrdinal("http://www.w3.org/2005/Atom", base.XmlReader.NamespaceURI) == 0)
                    {
                        if (string.CompareOrdinal("entry", base.XmlReader.LocalName) == 0)
                        {
                            return new ODataPayloadKind[] { ODataPayloadKind.Entry };
                        }
                        if (base.ReadingResponse && (string.CompareOrdinal("feed", base.XmlReader.LocalName) == 0))
                        {
                            return new ODataPayloadKind[1];
                        }
                    }
                    else
                    {
                        if (string.CompareOrdinal("http://schemas.microsoft.com/ado/2007/08/dataservices", base.XmlReader.NamespaceURI) == 0)
                        {
                            IEnumerable<ODataPayloadKind> possiblePayloadKinds = detectionInfo.PossiblePayloadKinds;
                            IEnumerable<ODataPayloadKind> first = (possiblePayloadKinds.Contains<ODataPayloadKind>(ODataPayloadKind.Property) || possiblePayloadKinds.Contains<ODataPayloadKind>(ODataPayloadKind.Collection)) ? this.DetectPropertyOrCollectionPayloadKind() : Enumerable.Empty<ODataPayloadKind>();
                            if (string.CompareOrdinal("uri", base.XmlReader.LocalName) == 0)
                            {
                                first = first.Concat<ODataPayloadKind>(new ODataPayloadKind[] { ODataPayloadKind.EntityReferenceLink });
                            }
                            if (base.ReadingResponse && (string.CompareOrdinal("links", base.XmlReader.LocalName) == 0))
                            {
                                first = first.Concat<ODataPayloadKind>(new ODataPayloadKind[] { ODataPayloadKind.EntityReferenceLinks });
                            }
                            return first;
                        }
                        if (string.CompareOrdinal("http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", base.XmlReader.NamespaceURI) == 0)
                        {
                            if (base.ReadingResponse && (string.CompareOrdinal("error", base.XmlReader.LocalName) == 0))
                            {
                                return new ODataPayloadKind[] { ODataPayloadKind.Error };
                            }
                            if (string.CompareOrdinal("uri", base.XmlReader.LocalName) == 0)
                            {
                                return new ODataPayloadKind[] { ODataPayloadKind.EntityReferenceLink };
                            }
                        }
                        else if (((string.CompareOrdinal("http://www.w3.org/2007/app", base.XmlReader.NamespaceURI) == 0) && base.ReadingResponse) && (string.CompareOrdinal("service", base.XmlReader.LocalName) == 0))
                        {
                            return new ODataPayloadKind[] { ODataPayloadKind.ServiceDocument };
                        }
                    }
                }
            }
            catch (XmlException)
            {
            }
            finally
            {
                base.XmlReader.DisableInStreamErrorDetection = false;
            }
            return Enumerable.Empty<ODataPayloadKind>();
        }

        private IEnumerable<ODataPayloadKind> DetectPropertyOrCollectionPayloadKind()
        {
            string str;
            bool flag;
            base.ReadNonEntityValueAttributes(out str, out flag);
            if (flag || (str != null))
            {
                return new ODataPayloadKind[] { ODataPayloadKind.Property };
            }
            if ((base.GetNonEntityValueKind() != EdmTypeKind.Collection) || !base.ReadingResponse)
            {
                return new ODataPayloadKind[] { ODataPayloadKind.Property };
            }
            return new ODataPayloadKind[] { ODataPayloadKind.Property, ODataPayloadKind.Collection };
        }
    }
}

