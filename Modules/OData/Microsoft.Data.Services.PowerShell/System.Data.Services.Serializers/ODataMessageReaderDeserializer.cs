namespace System.Data.Services.Serializers
{
    using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Data.Services;
    using System.Data.Services.Providers;
    using System.Xml;
    using System.Xml.Linq;

    internal abstract class ODataMessageReaderDeserializer : Deserializer
    {
        private System.Data.Services.ContentFormat contentFormat;
        private readonly ODataMessageReader messageReader;

        internal ODataMessageReaderDeserializer(bool update, IDataService dataService, UpdateTracker tracker, RequestDescription requestDescription, bool enableWcfDataServicesServerBehavior) : base(update, dataService, tracker, requestDescription)
        {
            System.Data.Services.ODataRequestMessage requestMessage = new System.Data.Services.ODataRequestMessage(dataService.OperationContext.Host);
            if (WebUtil.CompareMimeType(requestMessage.ContentType, "*/*"))
            {
                requestMessage.ContentType = "application/atom+xml";
            }
            this.messageReader = new ODataMessageReader(requestMessage, WebUtil.CreateMessageReaderSettings(dataService, enableWcfDataServicesServerBehavior), dataService.Provider.GetMetadataModel(base.Service.OperationContext));
            this.contentFormat = System.Data.Services.ContentFormat.Unknown;
        }

        protected void ApplyProperty(ODataProperty property, ResourceType resourceType, object resource)
        {
            ResourceType type;
            string name = property.Name;
            ResourceProperty resourceProperty = resourceType.TryResolvePropertyName(name);
            if (resourceProperty == null)
            {
                type = null;
            }
            else
            {
                if (resourceProperty.Kind == ResourcePropertyKind.Stream)
                {
                    return;
                }
                if (base.Update && resourceProperty.IsOfKind(ResourcePropertyKind.Key))
                {
                    return;
                }
                type = resourceProperty.ResourceType;
            }
            object propertyValue = this.ConvertValue(property.Value, ref type);
            if (resourceProperty == null)
            {
                Deserializer.SetOpenPropertyValue(resource, name, propertyValue, base.Service);
            }
            else
            {
                Deserializer.SetPropertyValue(resourceProperty, resource, propertyValue, base.Service);
            }
        }

        private object ConvertCollection(ODataCollectionValue collection, ResourceType resourceType)
        {
            CollectionResourceType type = resourceType as CollectionResourceType;
            IList list = Deserializer.CreateNewCollection();
            base.RecurseEnter();
            foreach (object obj2 in collection.Items)
            {
                ResourceType itemType = type.ItemType;
                list.Add(this.ConvertValue(obj2, ref itemType));
            }
            base.RecurseLeave();
            return Deserializer.GetReadOnlyCollection(list);
        }

        private object ConvertComplexValue(ODataComplexValue complexValue, ref ResourceType complexResourceType)
        {
            if (complexResourceType == null)
            {
                complexResourceType = base.Service.Provider.TryResolveResourceType(complexValue.TypeName);
            }
            base.CheckAndIncrementObjectCount();
            base.RecurseEnter();
            object resource = base.Updatable.CreateResource(null, complexResourceType.FullName);
            foreach (ODataProperty property in complexValue.Properties)
            {
                this.ApplyProperty(property, complexResourceType, resource);
            }
            base.RecurseLeave();
            return resource;
        }

        protected static object ConvertPrimitiveValue(object value, ref ResourceType resourceType)
        {
            if (value != null)
            {
                if (resourceType == null)
                {
                    resourceType = ResourceType.GetPrimitiveResourceType(value.GetType());
                    return value;
                }
                Type instanceType = resourceType.InstanceType;
                if (instanceType == typeof(XElement))
                {
                    string text = value as string;
                    value = XElement.Parse(text, LoadOptions.PreserveWhitespace);
                    return value;
                }
                if (instanceType == typeof(Binary))
                {
                    byte[] buffer = value as byte[];
                    value = new Binary(buffer);
                }
            }
            return value;
        }

        protected object ConvertValue(object odataValue, ref ResourceType resourceType)
        {
            if (odataValue == null)
            {
                return null;
            }
            ODataComplexValue complexValue = odataValue as ODataComplexValue;
            if (complexValue != null)
            {
                return this.ConvertComplexValue(complexValue, ref resourceType);
            }
            ODataCollectionValue collection = odataValue as ODataCollectionValue;
            if (collection != null)
            {
                return this.ConvertCollection(collection, resourceType);
            }
            return ConvertPrimitiveValue(odataValue, ref resourceType);
        }

        protected sealed override object Deserialize(System.Data.Services.SegmentInfo segmentInfo)
        {
            object obj2;
            try
            {
                obj2 = this.Read(segmentInfo);
            }
            catch (XmlException exception)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataServiceException_GeneralError, exception);
            }
            catch (ODataContentTypeException exception2)
            {
                throw new DataServiceException(0x19f, null, System.Data.Services.Strings.DataServiceException_UnsupportedMediaType, null, exception2);
            }
            catch (ODataException exception3)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataServiceException_GeneralError, exception3);
            }
            return obj2;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                this.messageReader.Dispose();
            }
        }

        protected abstract System.Data.Services.ContentFormat GetContentFormat();
        protected IEdmFunctionImport GetFunctionImport(OperationWrapper serviceOperation)
        {
            return base.Service.Provider.GetMetadataProviderEdmModel(base.Service.OperationContext).EnsureDefaultEntityContainer().EnsureFunctionImport(serviceOperation);
        }

        protected IEdmSchemaType GetSchemaType(ResourceType resourceType)
        {
            return base.Service.Provider.GetMetadataProviderEdmModel(base.Service.OperationContext).EnsureSchemaType(resourceType);
        }

        protected IEdmTypeReference GetTypeReference(ResourceType resourceType, List<KeyValuePair<string, object>> customAnnotations)
        {
            return base.Service.Provider.GetMetadataProviderEdmModel(base.Service.OperationContext).EnsureTypeReference(resourceType, customAnnotations);
        }

        protected abstract object Read(System.Data.Services.SegmentInfo segmentInfo);

        protected sealed override System.Data.Services.ContentFormat ContentFormat
        {
            get
            {
                if (this.contentFormat == System.Data.Services.ContentFormat.Unknown)
                {
                    this.contentFormat = this.GetContentFormat();
                }
                return this.contentFormat;
            }
        }

        protected ODataMessageReader MessageReader
        {
            get
            {
                return this.messageReader;
            }
        }
    }
}

