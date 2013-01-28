namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal sealed class ODataAtomCollectionDeserializer : ODataAtomPropertyAndValueDeserializer
    {
        private readonly DuplicatePropertyNamesChecker duplicatePropertyNamesChecker;

        internal ODataAtomCollectionDeserializer(ODataAtomInputContext atomInputContext) : base(atomInputContext)
        {
            this.duplicatePropertyNamesChecker = base.CreateDuplicatePropertyNamesChecker();
        }

        internal void ReadCollectionEnd()
        {
            base.XmlReader.Read();
        }

        internal object ReadCollectionItem(IEdmTypeReference expectedItemType, CollectionWithoutExpectedTypeValidator collectionValidator)
        {
            if (!base.XmlReader.LocalNameEquals(base.ODataCollectionItemElementName))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomCollectionDeserializer_WrongCollectionItemElementName(base.XmlReader.LocalName, base.XmlReader.ODataNamespace));
            }
            object obj2 = base.ReadNonEntityValue(expectedItemType, this.duplicatePropertyNamesChecker, collectionValidator, true, false);
            base.XmlReader.Read();
            return obj2;
        }

        internal ODataCollectionStart ReadCollectionStart(out bool isCollectionElementEmpty)
        {
            if (!base.XmlReader.NamespaceEquals(base.XmlReader.ODataNamespace))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomCollectionDeserializer_TopLevelCollectionElementWrongNamespace(base.XmlReader.NamespaceURI, base.XmlReader.ODataNamespace));
            }
            while (base.XmlReader.MoveToNextAttribute())
            {
                if (base.XmlReader.NamespaceEquals(base.XmlReader.ODataMetadataNamespace) && (base.XmlReader.LocalNameEquals(base.AtomTypeAttributeName) || base.XmlReader.LocalNameEquals(base.ODataNullAttributeName)))
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomCollectionDeserializer_TypeOrNullAttributeNotAllowed);
                }
            }
            base.XmlReader.MoveToElement();
            ODataCollectionStart start = new ODataCollectionStart {
                Name = base.XmlReader.LocalName
            };
            isCollectionElementEmpty = base.XmlReader.IsEmptyElement;
            if (!isCollectionElementEmpty)
            {
                base.XmlReader.Read();
            }
            return start;
        }

        internal void SkipToElementInODataNamespace()
        {
            do
            {
                switch (base.XmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (base.XmlReader.NamespaceEquals(base.XmlReader.ODataNamespace))
                        {
                            return;
                        }
                        base.XmlReader.Skip();
                        break;

                    case XmlNodeType.EndElement:
                        return;

                    default:
                        base.XmlReader.Skip();
                        break;
                }
            }
            while (!base.XmlReader.EOF);
        }
    }
}

