namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using Microsoft.Data.OData;
    using Microsoft.Data.OData.Metadata;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal abstract class EpmReader
    {
        private readonly ODataAtomInputContext atomInputContext;
        private readonly IODataAtomReaderEntryState entryState;

        protected EpmReader(IODataAtomReaderEntryState entryState, ODataAtomInputContext inputContext)
        {
            this.entryState = entryState;
            this.atomInputContext = inputContext;
        }

        private void AddEpmPropertyValue(List<ODataProperty> properties, string propertyName, object propertyValue, bool checkDuplicateEntryPropertyNames)
        {
            ODataProperty property = new ODataProperty {
                Name = propertyName,
                Value = propertyValue
            };
            if (checkDuplicateEntryPropertyNames)
            {
                this.entryState.DuplicatePropertyNamesChecker.CheckForDuplicatePropertyNames(property);
            }
            properties.Add(property);
        }

        protected void SetEntryEpmValue(EntityPropertyMappingInfo epmInfo, object propertyValue)
        {
            this.SetEpmValue(ReaderUtils.GetPropertiesList(this.entryState.Entry.Properties), this.entryState.EntityType.ToTypeReference(), epmInfo, propertyValue);
        }

        protected void SetEpmValue(IList targetList, IEdmTypeReference targetTypeReference, EntityPropertyMappingInfo epmInfo, object propertyValue)
        {
            this.SetEpmValueForSegment(epmInfo, 0, targetTypeReference.AsStructuredOrNull(), (List<ODataProperty>) targetList, propertyValue);
        }

        private void SetEpmValueForSegment(EntityPropertyMappingInfo epmInfo, int propertyValuePathIndex, IEdmStructuredTypeReference segmentStructuralTypeReference, List<ODataProperty> existingProperties, object propertyValue)
        {
            string propertyName = epmInfo.PropertyValuePath[propertyValuePathIndex].PropertyName;
            if (!epmInfo.Attribute.KeepInContent)
            {
                IEdmTypeReference type;
                ODataProperty property = existingProperties.FirstOrDefault<ODataProperty>(p => string.CompareOrdinal(p.Name, propertyName) == 0);
                ODataComplexValue value2 = null;
                if (property != null)
                {
                    value2 = property.Value as ODataComplexValue;
                    if (value2 == null)
                    {
                        return;
                    }
                }
                IEdmProperty property2 = segmentStructuralTypeReference.FindProperty(propertyName);
                if ((property2 == null) && (propertyValuePathIndex != (epmInfo.PropertyValuePath.Length - 1)))
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.EpmReader_OpenComplexOrCollectionEpmProperty(epmInfo.Attribute.SourcePath));
                }
                if ((property2 == null) || (this.MessageReaderSettings.DisablePrimitiveTypeConversion && property2.Type.IsODataPrimitiveTypeKind()))
                {
                    type = EdmCoreModel.Instance.GetString(true);
                }
                else
                {
                    type = property2.Type;
                }
                switch (type.TypeKind())
                {
                    case EdmTypeKind.Primitive:
                        object obj2;
                        if (type.IsStream())
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.EpmReader_SetEpmValueForSegment_StreamProperty));
                        }
                        if (propertyValue == null)
                        {
                            ReaderValidationUtils.ValidateNullValue(this.atomInputContext.Model, type, this.atomInputContext.MessageReaderSettings, true, this.atomInputContext.Version);
                            obj2 = null;
                        }
                        else
                        {
                            obj2 = AtomValueUtils.ConvertStringToPrimitive((string) propertyValue, type.AsPrimitive());
                        }
                        this.AddEpmPropertyValue(existingProperties, propertyName, obj2, segmentStructuralTypeReference.IsODataEntityTypeKind());
                        return;

                    case EdmTypeKind.Complex:
                    {
                        if (value2 == null)
                        {
                            value2 = new ODataComplexValue {
                                TypeName = type.ODataFullName(),
                                Properties = new ReadOnlyEnumerable<ODataProperty>()
                            };
                            this.AddEpmPropertyValue(existingProperties, propertyName, value2, segmentStructuralTypeReference.IsODataEntityTypeKind());
                        }
                        IEdmComplexTypeReference reference2 = type.AsComplex();
                        this.SetEpmValueForSegment(epmInfo, propertyValuePathIndex + 1, reference2, ReaderUtils.GetPropertiesList(value2.Properties), propertyValue);
                        return;
                    }
                    case EdmTypeKind.Collection:
                    {
                        ODataCollectionValue value4 = new ODataCollectionValue {
                            TypeName = type.ODataFullName(),
                            Items = new ReadOnlyEnumerable((List<object>) propertyValue)
                        };
                        this.AddEpmPropertyValue(existingProperties, propertyName, value4, segmentStructuralTypeReference.IsODataEntityTypeKind());
                        return;
                    }
                }
                throw new ODataException(Microsoft.Data.OData.Strings.General_InternalError(InternalErrorCodes.EpmReader_SetEpmValueForSegment_TypeKind));
            }
        }

        protected IODataAtomReaderEntryState EntryState
        {
            get
            {
                return this.entryState;
            }
        }

        protected ODataMessageReaderSettings MessageReaderSettings
        {
            get
            {
                return this.atomInputContext.MessageReaderSettings;
            }
        }

        protected ODataVersion Version
        {
            get
            {
                return this.atomInputContext.Version;
            }
        }
    }
}

