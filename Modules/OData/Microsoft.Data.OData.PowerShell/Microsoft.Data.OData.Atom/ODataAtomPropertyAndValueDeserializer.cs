

namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
	using Microsoft.Data.Edm.Library;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class ODataAtomPropertyAndValueDeserializer : ODataAtomDeserializer
    {
        protected readonly string AtomTypeAttributeName;
        private static readonly IEdmType edmStringType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.String);
        protected readonly string EmptyNamespace;
        protected readonly string ODataCollectionItemElementName;
        protected readonly string ODataNullAttributeName;
        private int recursionDepth;

        internal ODataAtomPropertyAndValueDeserializer(ODataAtomInputContext atomInputContext) : base(atomInputContext)
        {
            XmlNameTable nameTable = base.XmlReader.NameTable;
            this.EmptyNamespace = nameTable.Add(string.Empty);
            this.ODataNullAttributeName = nameTable.Add("null");
            this.ODataCollectionItemElementName = nameTable.Add("element");
            this.AtomTypeAttributeName = nameTable.Add("type");
        }

        [Conditional("DEBUG")]
        private void AssertRecursionDepthIsZero()
        {
        }

        private void DecreaseRecursionDepth()
        {
            this.recursionDepth--;
        }

        protected EdmTypeKind GetNonEntityValueKind()
        {
            EdmTypeKind kind;
            if (base.XmlReader.IsEmptyElement)
            {
                return EdmTypeKind.Primitive;
            }
            base.XmlReader.StartBuffering();
            try
            {
                base.XmlReader.Read();
                bool flag = false;
                do
                {
                    switch (base.XmlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (base.XmlReader.NamespaceEquals(base.XmlReader.ODataNamespace))
                            {
                                if (!base.XmlReader.LocalNameEquals(this.ODataCollectionItemElementName) || (base.Version < ODataVersion.V3))
                                {
                                    return EdmTypeKind.Complex;
                                }
                                flag = true;
                            }
                            base.XmlReader.Skip();
                            break;

                        case XmlNodeType.EndElement:
                            break;

                        default:
                            base.XmlReader.Skip();
                            break;
                    }
                }
                while (base.XmlReader.NodeType != XmlNodeType.EndElement);
                kind = flag ? EdmTypeKind.Collection : EdmTypeKind.Primitive;
            }
            finally
            {
                base.XmlReader.StopBuffering();
            }
            return kind;
        }

        private void IncreaseRecursionDepth()
        {
            ValidationUtils.IncreaseAndValidateRecursionDepth(ref this.recursionDepth, base.MessageReaderSettings.MessageQuotas.MaxNestingDepth);
        }

        private ODataCollectionValue ReadCollectionValue(IEdmCollectionTypeReference collectionTypeReference, string payloadTypeName, SerializationTypeNameAnnotation serializationTypeNameAnnotation)
        {
            this.IncreaseRecursionDepth();
            ODataCollectionValue value2 = new ODataCollectionValue {
                TypeName = (collectionTypeReference == null) ? payloadTypeName : collectionTypeReference.ODataFullName()
            };
            if (serializationTypeNameAnnotation != null)
            {
                value2.SetAnnotation<SerializationTypeNameAnnotation>(serializationTypeNameAnnotation);
            }
            base.XmlReader.MoveToElement();
            List<object> sourceEnumerable = new List<object>();
            if (!base.XmlReader.IsEmptyElement)
            {
                base.XmlReader.ReadStartElement();
                IEdmTypeReference expectedTypeReference = (collectionTypeReference == null) ? null : collectionTypeReference.ElementType();
                DuplicatePropertyNamesChecker duplicatePropertyNamesChecker = base.CreateDuplicatePropertyNamesChecker();
                CollectionWithoutExpectedTypeValidator collectionValidator = null;
                if (collectionTypeReference == null)
                {
                    string itemTypeNameFromCollection = (payloadTypeName == null) ? null : EdmLibraryExtensions.GetCollectionItemTypeName(payloadTypeName);
                    collectionValidator = new CollectionWithoutExpectedTypeValidator(itemTypeNameFromCollection);
                }
                do
                {
                    switch (base.XmlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (base.XmlReader.NamespaceEquals(base.XmlReader.ODataNamespace))
                            {
                                if (!base.XmlReader.LocalNameEquals(this.ODataCollectionItemElementName))
                                {
                                    throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomPropertyAndValueDeserializer_InvalidCollectionElement(base.XmlReader.LocalName, base.XmlReader.ODataNamespace));
                                }
                                object item = this.ReadNonEntityValueImplementation(expectedTypeReference, duplicatePropertyNamesChecker, collectionValidator, true, false);
                                base.XmlReader.Read();
                                ValidationUtils.ValidateCollectionItem(item, false);
                                sourceEnumerable.Add(item);
                            }
                            else
                            {
                                base.XmlReader.Skip();
                            }
                            break;

                        case XmlNodeType.EndElement:
                            break;

                        default:
                            base.XmlReader.Skip();
                            break;
                    }
                }
                while (base.XmlReader.NodeType != XmlNodeType.EndElement);
            }
            value2.Items = new ReadOnlyEnumerable(sourceEnumerable);
            this.DecreaseRecursionDepth();
            return value2;
        }

        private ODataComplexValue ReadComplexValue(IEdmComplexTypeReference complexTypeReference, string payloadTypeName, SerializationTypeNameAnnotation serializationTypeNameAnnotation, DuplicatePropertyNamesChecker duplicatePropertyNamesChecker, bool epmPresent)
        {
            this.IncreaseRecursionDepth();
            ODataComplexValue value2 = new ODataComplexValue();
            IEdmComplexType type = (complexTypeReference == null) ? null : ((IEdmComplexType) complexTypeReference.Definition);
            value2.TypeName = (type == null) ? payloadTypeName : type.ODataFullName();
            if (serializationTypeNameAnnotation != null)
            {
                value2.SetAnnotation<SerializationTypeNameAnnotation>(serializationTypeNameAnnotation);
            }
            base.XmlReader.MoveToElement();
            if (duplicatePropertyNamesChecker == null)
            {
                duplicatePropertyNamesChecker = base.CreateDuplicatePropertyNamesChecker();
            }
            else
            {
                duplicatePropertyNamesChecker.Clear();
            }
            List<ODataProperty> properties = new List<ODataProperty>();
            this.ReadPropertiesImplementation(type, properties, duplicatePropertyNamesChecker, epmPresent);
            value2.Properties = new ReadOnlyEnumerable<ODataProperty>(properties);
            this.DecreaseRecursionDepth();
            return value2;
        }

        internal object ReadNonEntityValue(IEdmTypeReference expectedValueTypeReference, DuplicatePropertyNamesChecker duplicatePropertyNamesChecker, CollectionWithoutExpectedTypeValidator collectionValidator, bool validateNullValue, bool epmPresent)
        {
            return this.ReadNonEntityValueImplementation(expectedValueTypeReference, duplicatePropertyNamesChecker, collectionValidator, validateNullValue, epmPresent);
        }

        protected void ReadNonEntityValueAttributes(out string typeName, out bool isNull)
        {
            typeName = null;
            isNull = false;
            while (base.XmlReader.MoveToNextAttribute())
            {
                if (base.XmlReader.NamespaceEquals(base.XmlReader.ODataMetadataNamespace))
                {
                    if (base.XmlReader.LocalNameEquals(this.AtomTypeAttributeName))
                    {
                        typeName = base.XmlReader.Value;
                        continue;
                    }
                    if (!base.XmlReader.LocalNameEquals(this.ODataNullAttributeName))
                    {
                        continue;
                    }
                    isNull = ODataAtomReaderUtils.ReadMetadataNullAttributeValue(base.XmlReader.Value);
                    break;
                }
                if ((base.UseClientFormatBehavior && base.XmlReader.NamespaceEquals(this.EmptyNamespace)) && base.XmlReader.LocalNameEquals(this.AtomTypeAttributeName))
                {
                    typeName = typeName ?? base.XmlReader.Value;
                }
            }
            base.XmlReader.MoveToElement();
        }

        private object ReadNonEntityValueImplementation(IEdmTypeReference expectedTypeReference, DuplicatePropertyNamesChecker duplicatePropertyNamesChecker, CollectionWithoutExpectedTypeValidator collectionValidator, bool validateNullValue, bool epmPresent)
        {
            string itemTypeNameFromCollection;
            bool flag;
            SerializationTypeNameAnnotation annotation;
            EdmTypeKind kind2;
            this.ReadNonEntityValueAttributes(out itemTypeNameFromCollection, out flag);
            if (flag)
            {
                return this.ReadNullValue(expectedTypeReference, validateNullValue);
            }
            bool flag2 = false;
            if ((collectionValidator != null) && (itemTypeNameFromCollection == null))
            {
                itemTypeNameFromCollection = collectionValidator.ItemTypeNameFromCollection;
                flag2 = collectionValidator.ItemTypeKindFromCollection != EdmTypeKind.None;
            }
            IEdmTypeReference type = ReaderValidationUtils.ResolvePayloadTypeNameAndComputeTargetType(EdmTypeKind.None, edmStringType, expectedTypeReference, itemTypeNameFromCollection, base.Model, base.MessageReaderSettings, base.Version, new Func<EdmTypeKind>(this.GetNonEntityValueKind), out kind2, out annotation);
            if (flag2)
            {
                annotation = new SerializationTypeNameAnnotation {
                    TypeName = null
                };
            }
            if (collectionValidator != null)
            {
                collectionValidator.ValidateCollectionItem(itemTypeNameFromCollection, kind2);
            }
            switch (kind2)
            {
                case EdmTypeKind.Primitive:
                    return this.ReadPrimitiveValue(type.AsPrimitive());

                case EdmTypeKind.Complex:
                    return this.ReadComplexValue((type == null) ? null : type.AsComplex(), itemTypeNameFromCollection, annotation, duplicatePropertyNamesChecker, epmPresent);

                case EdmTypeKind.Collection:
                {
                    IEdmCollectionTypeReference collectionTypeReference = ValidationUtils.ValidateCollectionType(type);
                    return this.ReadCollectionValue(collectionTypeReference, itemTypeNameFromCollection, annotation);
                }
            }
            throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.ODataAtomPropertyAndValueDeserializer_ReadNonEntityValue));
        }

        private object ReadNullValue(IEdmTypeReference expectedTypeReference, bool validateNullValue)
        {
            base.XmlReader.SkipElementContent();
            ReaderValidationUtils.ValidateNullValue(base.Model, expectedTypeReference, base.MessageReaderSettings, validateNullValue, base.Version);
            return null;
        }

        private object ReadPrimitiveValue(IEdmPrimitiveTypeReference actualValueTypeReference)
        {
            return AtomValueUtils.ReadPrimitiveValue(base.XmlReader, actualValueTypeReference);
        }

        protected void ReadProperties(IEdmStructuredType structuredType, List<ODataProperty> properties, DuplicatePropertyNamesChecker duplicatePropertyNamesChecker, bool epmPresent)
        {
            this.ReadPropertiesImplementation(structuredType, properties, duplicatePropertyNamesChecker, epmPresent);
        }

        private void ReadPropertiesImplementation(IEdmStructuredType structuredType, List<ODataProperty> properties, DuplicatePropertyNamesChecker duplicatePropertyNamesChecker, bool epmPresent)
        {
            if (!base.XmlReader.IsEmptyElement)
            {
                base.XmlReader.ReadStartElement();
                do
                {
                    switch (base.XmlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (base.XmlReader.NamespaceEquals(base.XmlReader.ODataNamespace))
                            {
                                IEdmProperty property = null;
                                bool flag = false;
                                bool ignoreProperty = false;
                                if (structuredType != null)
                                {
                                    property = ReaderValidationUtils.ValidateValuePropertyDefined(base.XmlReader.LocalName, structuredType, base.MessageReaderSettings, out ignoreProperty);
                                    if ((property != null) && (property.PropertyKind == EdmPropertyKind.Navigation))
                                    {
                                        throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomPropertyAndValueDeserializer_NavigationPropertyInProperties(property.Name, structuredType));
                                    }
                                    flag = property == null;
                                }
                                if (ignoreProperty)
                                {
                                    base.XmlReader.Skip();
                                }
                                else
                                {
                                    ODataNullValueBehaviorKind nullValueReadBehaviorKind = (base.ReadingResponse || (property == null)) ? ODataNullValueBehaviorKind.Default : base.Model.NullValueReadBehaviorKind(property);
                                    ODataProperty property2 = this.ReadProperty((property == null) ? null : property.Type, nullValueReadBehaviorKind, epmPresent);
                                    if (property2 != null)
                                    {
                                        if (flag)
                                        {
                                            ValidationUtils.ValidateOpenPropertyValue(property2.Name, property2.Value);
                                        }
                                        duplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(property2);
                                        properties.Add(property2);
                                    }
                                }
                            }
                            else
                            {
                                base.XmlReader.Skip();
                            }
                            break;

                        case XmlNodeType.EndElement:
                            break;

                        default:
                            base.XmlReader.Skip();
                            break;
                    }
                }
                while (base.XmlReader.NodeType != XmlNodeType.EndElement);
            }
        }

        private ODataProperty ReadProperty(IEdmTypeReference expectedPropertyTypeReference, ODataNullValueBehaviorKind nullValueReadBehaviorKind, bool epmPresent)
        {
            ODataProperty property = new ODataProperty();
            string localName = base.XmlReader.LocalName;
            ValidationUtils.ValidatePropertyName(localName);
            property.Name = localName;
            object obj2 = this.ReadNonEntityValueImplementation(expectedPropertyTypeReference, null, null, nullValueReadBehaviorKind == ODataNullValueBehaviorKind.Default, epmPresent);
            if ((nullValueReadBehaviorKind == ODataNullValueBehaviorKind.IgnoreValue) && (obj2 == null))
            {
                property = null;
            }
            else
            {
                property.Value = obj2;
            }
            base.XmlReader.Read();
            return property;
        }

        internal ODataProperty ReadTopLevelProperty(IEdmTypeReference expectedPropertyTypeReference)
        {
            base.ReadPayloadStart();
            if (!base.UseServerFormatBehavior && !base.XmlReader.NamespaceEquals(base.XmlReader.ODataNamespace))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomPropertyAndValueDeserializer_TopLevelPropertyElementWrongNamespace(base.XmlReader.NamespaceURI, base.XmlReader.ODataNamespace));
            }
            ODataProperty property = this.ReadProperty(expectedPropertyTypeReference, ODataNullValueBehaviorKind.Default, false);
            base.ReadPayloadEnd();
            return property;
        }
    }
}

