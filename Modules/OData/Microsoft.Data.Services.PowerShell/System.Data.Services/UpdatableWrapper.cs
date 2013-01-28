namespace System.Data.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Parsing;
    using System.Data.Services.Providers;
    using System.Linq;

    internal class UpdatableWrapper
    {
        private readonly IDataService service;
        private IUpdatable updateProvider;

        internal UpdatableWrapper(IDataService serviceInstance)
        {
            this.service = serviceInstance;
        }

        internal void AddReferenceToCollection(object targetResource, string propertyName, object resourceToBeAdded)
        {
            this.UpdateProvider.AddReferenceToCollection(targetResource, propertyName, resourceToBeAdded);
        }

        internal void ClearChanges()
        {
            this.UpdateProvider.ClearChanges();
        }

        internal object CreateResource(string containerName, string fullTypeName)
        {
            object obj2 = this.UpdateProvider.CreateResource(containerName, fullTypeName);
            if (obj2 == null)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.BadProvider_CreateResourceReturnedNull);
            }
            return obj2;
        }

        internal void DeleteResource(object targetResource)
        {
            this.UpdateProvider.DeleteResource(targetResource);
        }

        internal void DisposeProvider()
        {
            if (this.updateProvider != null)
            {
                WebUtil.Dispose(this.updateProvider);
                this.updateProvider = null;
            }
        }

        internal IUpdatable GetOrLoadUpdateProvider()
        {
            if (this.updateProvider == null)
            {
                this.updateProvider = this.service.Provider.GetService<IDataServiceUpdateProvider2>();
                if ((this.updateProvider == null) && this.service.Provider.IsV1Provider)
                {
                    this.updateProvider = this.service.Provider.GetService<IUpdatable>();
                }
                if (this.updateProvider == null)
                {
                    this.updateProvider = this.service.Provider.GetService<IDataServiceUpdateProvider>();
                }
            }
            return this.updateProvider;
        }

        internal object GetResource(IQueryable query, string fullTypeName)
        {
            object resource;
            try
            {
                resource = this.UpdateProvider.GetResource(query, fullTypeName);
            }
            catch (ArgumentException exception)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_InvalidUriSpecified, exception);
            }
            return resource;
        }

        internal object GetValue(object targetResource, string propertyName)
        {
            return this.UpdateProvider.GetValue(targetResource, propertyName);
        }

        private static IEnumerable<KeyValuePair<string, object>> ParseETagValue(IList<ResourceProperty> etagProperties, string ifMatchHeaderValue)
        {
            bool flag;
            if (ifMatchHeaderValue == "*")
            {
                return WebUtil.EmptyKeyValuePairStringObject;
            }
            string stringToUnescape = ifMatchHeaderValue.Substring("W/\"".Length, (ifMatchHeaderValue.Length - "W/\"".Length) - 1);
            KeyInstance instance = null;
            Exception innerException = null;
            try
            {
                flag = KeyInstance.TryParseNullableTokens(Uri.UnescapeDataString(stringToUnescape), out instance);
            }
            catch (DataServiceException exception2)
            {
                flag = false;
                innerException = exception2;
            }
            if (!flag)
            {
                throw DataServiceException.CreatePreConditionFailedError(System.Data.Services.Strings.Serializer_ETagValueDoesNotMatch, innerException);
            }
            if (instance.PositionalValues.Count != etagProperties.Count)
            {
                throw DataServiceException.CreatePreConditionFailedError(System.Data.Services.Strings.Serializer_ETagValueDoesNotMatch);
            }
            KeyValuePair<string, object>[] pairArray = new KeyValuePair<string, object>[etagProperties.Count];
            for (int i = 0; i < pairArray.Length; i++)
            {
                ResourceProperty property = etagProperties[i];
                object targetValue = null;
                string text = (string) instance.PositionalValues[i];
                if (text != "null")
                {
                    try
                    {
                        flag = WebConvert.TryKeyStringToPrimitive(text, property.Type, out targetValue);
                    }
                    catch (OverflowException exception3)
                    {
                        flag = false;
                        innerException = exception3;
                    }
                    if (!flag)
                    {
                        throw DataServiceException.CreatePreConditionFailedError(System.Data.Services.Strings.Serializer_ETagValueDoesNotMatch, innerException);
                    }
                }
                pairArray[i] = new KeyValuePair<string, object>(etagProperties[i].Name, targetValue);
            }
            return pairArray;
        }

        internal void RemoveReferenceFromCollection(object targetResource, string propertyName, object resourceToBeRemoved)
        {
            this.UpdateProvider.RemoveReferenceFromCollection(targetResource, propertyName, resourceToBeRemoved);
        }

        internal object ResetResource(object resource)
        {
            object obj2 = this.UpdateProvider.ResetResource(resource);
            if (obj2 == null)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.BadProvider_ResetResourceReturnedNull);
            }
            return obj2;
        }

        internal object ResolveResource(object resource)
        {
            object obj2 = this.UpdateProvider.ResolveResource(resource);
            if (obj2 == null)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.BadProvider_ResolveResourceReturnedNull);
            }
            return obj2;
        }

        internal void SaveChanges()
        {
            this.UpdateProvider.SaveChanges();
        }

        internal void ScheduleInvokable(IDataServiceInvokable invokable)
        {
            this.UpdateProvider2.ScheduleInvokable(invokable);
        }

        internal void SetETagValues(object resourceCookie, ResourceSetWrapper container)
        {
            DataServiceHostWrapper host = this.service.OperationContext.Host;
            object obj2 = this.ResolveResource(resourceCookie);
            ResourceType nonPrimitiveResourceType = WebUtil.GetNonPrimitiveResourceType(this.service.Provider, obj2);
            IList<ResourceProperty> eTagProperties = this.service.Provider.GetETagProperties(container.Name, nonPrimitiveResourceType);
            if (eTagProperties.Count == 0)
            {
                if (!string.IsNullOrEmpty(host.RequestIfMatch))
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.Serializer_NoETagPropertiesForType);
                }
            }
            else
            {
                IDataServiceUpdateProvider updateProvider = this.updateProvider as IDataServiceUpdateProvider;
                if (updateProvider != null)
                {
                    IEnumerable<KeyValuePair<string, object>> emptyKeyValuePairStringObject;
                    bool? checkForEquality = null;
                    if (!string.IsNullOrEmpty(host.RequestIfMatch))
                    {
                        checkForEquality = true;
                        emptyKeyValuePairStringObject = ParseETagValue(eTagProperties, host.RequestIfMatch);
                    }
                    else
                    {
                        emptyKeyValuePairStringObject = WebUtil.EmptyKeyValuePairStringObject;
                    }
                    updateProvider.SetConcurrencyValues(resourceCookie, checkForEquality, emptyKeyValuePairStringObject);
                }
                else
                {
                    if (string.IsNullOrEmpty(host.RequestIfMatch))
                    {
                        throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_CannotPerformOperationWithoutETag(nonPrimitiveResourceType.FullName));
                    }
                    if ((host.RequestIfMatch != "*") && (WebUtil.GetETagValue(resourceCookie, nonPrimitiveResourceType, eTagProperties, this.service, false) != host.RequestIfMatch))
                    {
                        throw DataServiceException.CreatePreConditionFailedError(System.Data.Services.Strings.Serializer_ETagValueDoesNotMatch);
                    }
                }
            }
        }

        internal void SetReference(object targetResource, string propertyName, object propertyValue)
        {
            this.UpdateProvider.SetReference(targetResource, propertyName, propertyValue);
        }

        internal void SetValue(object targetResource, string propertyName, object propertyValue)
        {
            this.UpdateProvider.SetValue(targetResource, propertyName, propertyValue);
        }

        private IUpdatable UpdateProvider
        {
            get
            {
                if (this.GetOrLoadUpdateProvider() != null)
                {
                    return this.updateProvider;
                }
                if (this.service.Provider.IsV1Provider)
                {
                    throw DataServiceException.CreateMethodNotImplemented(System.Data.Services.Strings.UpdatableWrapper_MissingIUpdatableForV1Provider);
                }
                throw DataServiceException.CreateMethodNotImplemented(System.Data.Services.Strings.UpdatableWrapper_MissingUpdateProviderInterface);
            }
        }

        private IDataServiceUpdateProvider2 UpdateProvider2
        {
            get
            {
                IDataServiceUpdateProvider2 updateProvider = this.UpdateProvider as IDataServiceUpdateProvider2;
                if (updateProvider == null)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.UpdatableWrapper_MustImplementDataServiceUpdateProvider2ToSupportServiceActions);
                }
                return updateProvider;
            }
        }
    }
}

