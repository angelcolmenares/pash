namespace System.Data.Services
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Data.Services.Common;
    using System.Data.Services.Internal;
    using System.Data.Services.Parsing;
    using System.Data.Services.Providers;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Spatial;
    using System.Text;
    using System.Threading;
    using System.Xml;

    internal static class WebUtil
    {
        internal static readonly IEnumerable<KeyValuePair<string, object>> EmptyKeyValuePairStringObject = new KeyValuePair<string, object>[0];
        internal static readonly object[] EmptyObjectArray = new object[0];
        internal static readonly string[] EmptyStringArray = new string[0];
        private static readonly ExpandWrapperTypeWithIndex[] GenericExpandedWrapperTypes;
        private static readonly KeyValuePair<Type, ContentFormat>[] PrimitiveTypesContentFormatMapping = new KeyValuePair<Type, ContentFormat>[] { new KeyValuePair<Type, ContentFormat>(typeof(byte[]), ContentFormat.Binary), new KeyValuePair<Type, ContentFormat>(typeof(Binary), ContentFormat.Binary) };
        private static readonly KeyValuePair<Type, string>[] PrimitiveTypesMimeTypeMapping = new KeyValuePair<Type, string>[] { new KeyValuePair<Type, string>(typeof(byte[]), "application/octet-stream"), new KeyValuePair<Type, string>(typeof(Binary), "application/octet-stream") };
        internal const BindingFlags PublicInstanceBindingFlags = (BindingFlags.Public | BindingFlags.Instance);

        static WebUtil()
        {
            ExpandWrapperTypeWithIndex[] indexArray = new ExpandWrapperTypeWithIndex[12];
            ExpandWrapperTypeWithIndex index = new ExpandWrapperTypeWithIndex {
                Type = typeof(ExpandedWrapper<,>),
                Index = 1
            };
            indexArray[0] = index;
            ExpandWrapperTypeWithIndex index2 = new ExpandWrapperTypeWithIndex {
                Type = typeof(ExpandedWrapper<,,>),
                Index = 2
            };
            indexArray[1] = index2;
            ExpandWrapperTypeWithIndex index3 = new ExpandWrapperTypeWithIndex {
                Type = typeof(ExpandedWrapper<,,,>),
                Index = 3
            };
            indexArray[2] = index3;
            ExpandWrapperTypeWithIndex index4 = new ExpandWrapperTypeWithIndex {
                Type = typeof(ExpandedWrapper<,,,,>),
                Index = 4
            };
            indexArray[3] = index4;
            ExpandWrapperTypeWithIndex index5 = new ExpandWrapperTypeWithIndex {
                Type = typeof(ExpandedWrapper<,,,,,>),
                Index = 5
            };
            indexArray[4] = index5;
            ExpandWrapperTypeWithIndex index6 = new ExpandWrapperTypeWithIndex {
                Type = typeof(ExpandedWrapper<,,,,,,>),
                Index = 6
            };
            indexArray[5] = index6;
            ExpandWrapperTypeWithIndex index7 = new ExpandWrapperTypeWithIndex {
                Type = typeof(ExpandedWrapper<,,,,,,,>),
                Index = 7
            };
            indexArray[6] = index7;
            ExpandWrapperTypeWithIndex index8 = new ExpandWrapperTypeWithIndex {
                Type = typeof(ExpandedWrapper<,,,,,,,,>),
                Index = 8
            };
            indexArray[7] = index8;
            ExpandWrapperTypeWithIndex index9 = new ExpandWrapperTypeWithIndex {
                Type = typeof(ExpandedWrapper<,,,,,,,,,>),
                Index = 9
            };
            indexArray[8] = index9;
            ExpandWrapperTypeWithIndex index10 = new ExpandWrapperTypeWithIndex {
                Type = typeof(ExpandedWrapper<,,,,,,,,,,>),
                Index = 10
            };
            indexArray[9] = index10;
            ExpandWrapperTypeWithIndex index11 = new ExpandWrapperTypeWithIndex {
                Type = typeof(ExpandedWrapper<,,,,,,,,,,,>),
                Index = 11
            };
            indexArray[10] = index11;
            ExpandWrapperTypeWithIndex index12 = new ExpandWrapperTypeWithIndex {
                Type = typeof(ExpandedWrapper<,,,,,,,,,,,,>),
                Index = 12
            };
            indexArray[11] = index12;
            GenericExpandedWrapperTypes = indexArray;
        }

        internal static Uri ApplyHostHeader(Uri baseUri, string requestHost)
        {
            if (!string.IsNullOrEmpty(requestHost))
            {
                string str;
                int num;
                UriBuilder builder = new UriBuilder(baseUri);
                if (GetHostAndPort(requestHost, baseUri.Scheme, out str, out num))
                {
                    builder.Host = str;
                    builder.Port = num;
                }
                else
                {
                    builder.Host = requestHost;
                }
                baseUri = builder.Uri;
            }
            return baseUri;
        }

        internal static T CheckArgumentNull<T>([System.Data.Services.WebUtil.ValidatedNotNull] T value, string parameterName) where T: class
        {
            if (value == null)
            {
                throw System.Data.Services.Error.ArgumentNull(parameterName);
            }
            return value;
        }

        internal static void CheckMaxProtocolVersion(Version featureVersion, Version maxProtocolVersion)
        {
            if (maxProtocolVersion < featureVersion)
            {
                throw new InvalidOperationException(System.Data.Services.Strings.DataServiceConfiguration_ResponseVersionIsBiggerThanProtocolVersion(featureVersion.ToString(), maxProtocolVersion.ToString()));
            }
        }

        internal static void CheckRequestVersion(Version requiredRequestVersion, Version actualRequestVersion)
        {
            if (actualRequestVersion < requiredRequestVersion)
            {
                throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.DataService_DSVTooLow(actualRequestVersion.ToString(2), requiredRequestVersion.Major, requiredRequestVersion.Minor));
            }
        }

        internal static void CheckResourceContainerRights(EntitySetRights rights, string parameterName)
        {
            if ((rights < EntitySetRights.None) || (rights > EntitySetRights.All))
            {
                throw System.Data.Services.Error.ArgumentOutOfRange(parameterName);
            }
        }

        internal static void CheckResourceExists(bool resourceExists, string identifier)
        {
            if (!resourceExists)
            {
                throw DataServiceException.CreateResourceNotFound(identifier);
            }
        }

        internal static void CheckResourceNotCollectionForOpenProperty(ResourceType resourceType, string propertyName)
        {
            if ((resourceType.ResourceTypeKind == ResourceTypeKind.Collection) || (resourceType.ResourceTypeKind == ResourceTypeKind.EntityCollection))
            {
                throw DataServiceException.CreateSyntaxError(System.Data.Services.Strings.InvalidUri_OpenPropertiesCannotBeCollection(propertyName));
            }
        }

        internal static void CheckResourceTypeKind(ResourceTypeKind kind, string parameterName)
        {
            if ((kind < ResourceTypeKind.EntityType) || (kind > ResourceTypeKind.EntityCollection))
            {
                throw new ArgumentException(System.Data.Services.Strings.InvalidEnumValue(kind.GetType().Name), parameterName);
            }
        }

        internal static void CheckServiceActionRights(ServiceActionRights rights, string parameterName)
        {
            if ((rights < ServiceActionRights.None) || (rights > ServiceActionRights.Invoke))
            {
                throw System.Data.Services.Error.ArgumentOutOfRange(parameterName);
            }
        }

        internal static void CheckServiceOperationResultKind(ServiceOperationResultKind kind, string parameterName)
        {
            if ((kind < ServiceOperationResultKind.DirectValue) || (kind > ServiceOperationResultKind.Void))
            {
                throw new ArgumentException(System.Data.Services.Strings.InvalidEnumValue(kind.GetType().Name), parameterName);
            }
        }

        internal static void CheckServiceOperationRights(ServiceOperationRights rights, string parameterName)
        {
            if ((rights < ServiceOperationRights.None) || (rights > (ServiceOperationRights.OverrideEntitySetRights | ServiceOperationRights.AllRead)))
            {
                throw System.Data.Services.Error.ArgumentOutOfRange(parameterName);
            }
        }

        internal static string CheckStringArgumentNullOrEmpty([System.Data.Services.WebUtil.ValidatedNotNull] string value, string parameterName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(parameterName, System.Data.Services.Strings.WebUtil_ArgumentNullOrEmpty);
            }
            return value;
        }

        internal static void CheckSyntaxValid(bool valid)
        {
            if (!valid)
            {
                throw DataServiceException.CreateSyntaxError();
            }
        }

        internal static string CompareAndGetETag(object parentEntityResource, object parentEntityToken, ResourceSetWrapper container, IDataService service, out bool writeResponseForGetMethods)
        {
            DataServiceHostWrapper host = service.OperationContext.Host;
            writeResponseForGetMethods = true;
            string str = null;
            if (parentEntityResource == null)
            {
                if (!string.IsNullOrEmpty(host.RequestIfMatch))
                {
                    throw DataServiceException.CreatePreConditionFailedError(System.Data.Services.Strings.Serializer_ETagValueDoesNotMatch);
                }
                return str;
            }
            ResourceType nonPrimitiveResourceType = GetNonPrimitiveResourceType(service.Provider, parentEntityResource);
            ICollection<ResourceProperty> eTagProperties = service.Provider.GetETagProperties(container.Name, nonPrimitiveResourceType);
            if (eTagProperties.Count == 0)
            {
                if (!string.IsNullOrEmpty(host.RequestIfMatch))
                {
                    throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.Serializer_NoETagPropertiesForType);
                }
            }
            else if ((!string.IsNullOrEmpty(host.RequestIfMatch) || !string.IsNullOrEmpty(host.RequestIfNoneMatch)) && (host.RequestIfMatch != "*"))
            {
                if (host.RequestIfNoneMatch == "*")
                {
                    writeResponseForGetMethods = false;
                }
                else
                {
                    str = GetETagValue(parentEntityToken, nonPrimitiveResourceType, eTagProperties, service, true);
                    if (string.IsNullOrEmpty(host.RequestIfMatch))
                    {
                        if (host.RequestIfNoneMatch == str)
                        {
                            writeResponseForGetMethods = false;
                        }
                    }
                    else if (str != host.RequestIfMatch)
                    {
                        throw DataServiceException.CreatePreConditionFailedError(System.Data.Services.Strings.Serializer_ETagValueDoesNotMatch);
                    }
                }
            }
            if ((str == null) && (eTagProperties.Count != 0))
            {
                str = GetETagValue(parentEntityResource, nonPrimitiveResourceType, eTagProperties, service, true);
            }
            return str;
        }

        internal static bool CompareMimeType(string mimeType1, string mimeType2)
        {
            return string.Equals(mimeType1, mimeType2, StringComparison.OrdinalIgnoreCase);
        }

        internal static long CopyStream(Stream input, Stream output, int bufferSize)
        {
            byte[] buffer = new byte[(bufferSize <= 0) ? 0x10000 : bufferSize];
            return CopyStream(input, output, buffer);
        }

        internal static long CopyStream(Stream input, Stream output, byte[] buffer)
        {
            int num2;
            long num = 0L;
            while (0 < (num2 = input.Read(buffer, 0, buffer.Length)))
            {
                output.Write(buffer, 0, num2);
                num += num2;
            }
            return num;
        }

        internal static string CreateFullNameForCustomAnnotation(string namespaceName, string name)
        {
            if (!string.IsNullOrEmpty(namespaceName))
            {
                return (namespaceName + ":" + name);
            }
            return name;
        }

        internal static void CreateIfNull<T>(ref T value) where T: new()
        {
            if (((T) value) == null)
            {
                value = (default(T) == null) ? Activator.CreateInstance<T>() : default(T);
            }
        }

        internal static ODataMessageReaderSettings CreateMessageReaderSettings(IDataService dataService, bool enableWcfDataServicesServerBehavior)
        {
            ODataMessageReaderSettings settings = new ODataMessageReaderSettings {
                BaseUri = dataService.OperationContext.AbsoluteServiceUri,
                CheckCharacters = false,
                DisableMessageStreamDisposal = true,
                DisablePrimitiveTypeConversion = !dataService.Configuration.EnableTypeConversion
            };
            DataServiceProtocolVersion maxProtocolVersion = dataService.Configuration.DataServiceBehavior.MaxProtocolVersion;
            settings.MaxProtocolVersion = (maxProtocolVersion == DataServiceProtocolVersion.V1) ? ODataVersion.V2 : CommonUtil.ConvertToODataVersion(dataService.Configuration.DataServiceBehavior.MaxProtocolVersion);
            settings.MessageQuotas.MaxReceivedMessageSize = 0x7fffffffffffffffL;
            if (enableWcfDataServicesServerBehavior)
            {
                settings.EnableWcfDataServicesServerBehavior(dataService.Provider.IsV1Provider);
            }
            return settings;
        }

        internal static Delegate CreateNewInstanceConstructor(Type type, string fullName, Type targetType)
        {
            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                fullName = fullName ?? type.FullName;
                throw new InvalidOperationException(System.Data.Services.Strings.NoEmptyConstructorFoundForType(fullName));
            }
            DynamicMethod method = new DynamicMethod("invoke_constructor", targetType, Type.EmptyTypes, false);
            ILGenerator iLGenerator = method.GetILGenerator();
            iLGenerator.Emit(OpCodes.Newobj, constructor);
            if (targetType.IsValueType)
            {
                iLGenerator.Emit(OpCodes.Box);
            }
            iLGenerator.Emit(OpCodes.Ret);
            return method.CreateDelegate(typeof(Func<>).MakeGenericType(new Type[] { targetType }));
        }

        [Conditional("DEBUG")]
        internal static void DebugEnumIsDefined<T>(T value)
        {
        }

        internal static void Dispose(object o)
        {
            IDisposable disposable = o as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        internal static ResourceType ElementType(this ResourceType type)
        {
            if (type.ResourceTypeKind == ResourceTypeKind.EntityCollection)
            {
                return ((EntityCollectionResourceType) type).ItemType;
            }
            if (type.ResourceTypeKind == ResourceTypeKind.Collection)
            {
                return ((CollectionResourceType) type).ItemType;
            }
            return type;
        }

        internal static Uri EnsureLastSegmentEmpty(Uri absoluteUri)
        {
            string[] segments = absoluteUri.Segments;
            if (segments.Length > 0)
            {
                string str = segments[segments.Length - 1];
                if ((str.Length > 0) && (str[str.Length - 1] != '/'))
                {
                    absoluteUri = new Uri(absoluteUri, str + "/");
                }
            }
            return absoluteUri;
        }

        internal static string GetAttributeEx(XmlReader reader, string attributeName, string namespaceUri)
        {
            return (reader.GetAttribute(attributeName, namespaceUri) ?? reader.GetAttribute(attributeName));
        }

        private static string[] GetAvailableMediaTypesForV2(bool isEntityOrFeed)
        {
            if (isEntityOrFeed)
            {
                return new string[] { "application/atom+xml", "application/json;odata=verbose", "application/json" };
            }
            return new string[] { "application/xml", "text/xml", "application/json;odata=verbose", "application/json" };
        }

        private static string[] GetAvailableMediaTypesForV3WithJsonLight(bool isEntityOrFeed)
        {
            if (isEntityOrFeed)
            {
                return new string[] { "application/atom+xml", "application/json;odata=light", "application/json", "application/json;odata=verbose" };
            }
            return new string[] { "application/xml", "text/xml", "application/json;odata=light", "application/json", "application/json;odata=verbose" };
        }

        private static string[] GetAvailableMediaTypesForV3WithoutJsonLight(bool isEntityOrFeed)
        {
            if (isEntityOrFeed)
            {
                return new string[] { "application/atom+xml", "application/json;odata=verbose" };
            }
            return new string[] { "application/xml", "text/xml", "application/json;odata=verbose" };
        }

        internal static ContentFormat GetContentFormat(string mime)
        {
            if (CompareMimeType(mime, "application/json;odata=verbose") || CompareMimeType(mime, "application/json"))
            {
                return ContentFormat.VerboseJson;
            }
            if (CompareMimeType(mime, "application/atom+xml"))
            {
                return ContentFormat.Atom;
            }
            return ContentFormat.PlainXml;
        }

        internal static Version GetEffectiveMaxResponseVersion(DataServiceProtocolVersion maxProtocolVersion, Version requestMaxVersion)
        {
            Version version = maxProtocolVersion.ToVersion();
            if (requestMaxVersion >= version)
            {
                return version;
            }
            return requestMaxVersion;
        }

        internal static Action<Stream> GetEmptyStreamWriter()
        {
            return delegate (Stream stream) {
            };
        }

        internal static string GetETagValue(IDataService service, object resource, ResourceType resourceType, ResourceSetWrapper container)
        {
            ICollection<ResourceProperty> eTagProperties = service.Provider.GetETagProperties(container.Name, resourceType);
            if (eTagProperties.Count != 0)
            {
                return GetETagValue(resource, resourceType, eTagProperties, service, true);
            }
            return null;
        }

        internal static string GetETagValue(object resource, ResourceType resourceType, ICollection<ResourceProperty> etagProperties, IDataService service, bool getMethod)
        {
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            builder.Append("W/\"");
            foreach (ResourceProperty property in etagProperties)
            {
                object obj2;
                string str;
                if (getMethod)
                {
                    obj2 = GetPropertyValue(service.Provider, resource, resourceType, property, null);
                }
                else
                {
                    obj2 = service.Updatable.GetValue(resource, property.Name);
                }
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    builder.Append(',');
                }
                if (obj2 == null)
                {
                    str = "null";
                }
                else if (!WebConvert.TryKeyPrimitiveToString(obj2, out str))
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.Serializer_CannotConvertValue(obj2));
                }
                builder.Append(str);
            }
            builder.Append('"');
            return builder.ToString();
        }

        private static bool GetHostAndPort(string hostHeader, string scheme, out string host, out int port)
        {
            Uri uri;
            if ((scheme != null) && !scheme.EndsWith("://", StringComparison.Ordinal))
            {
                scheme = scheme + "://";
            }
            if (Uri.TryCreate(scheme + hostHeader, UriKind.Absolute, out uri))
            {
                host = uri.Host;
                port = uri.Port;
                return true;
            }
            host = null;
            port = 0;
            return false;
        }

        internal static ResourceType GetNonPrimitiveResourceType(DataServiceProviderWrapper provider, object obj)
        {
            IProjectedResult result = obj as IProjectedResult;
            ResourceType type = (result != null) ? provider.TryResolveResourceType(result.ResourceTypeName) : provider.GetResourceType(obj);
            if (type == null)
            {
                throw new DataServiceException(500, System.Data.Services.Strings.BadProvider_InvalidTypeSpecified(obj.GetType().FullName));
            }
            return type;
        }

        internal static object GetPropertyValue(DataServiceProviderWrapper provider, object resource, ResourceType resourceType, ResourceProperty resourceProperty, string propertyName)
        {
            IProjectedResult result = resource as IProjectedResult;
            if (result != null)
            {
                object projectedPropertyValue = result.GetProjectedPropertyValue(propertyName ?? resourceProperty.Name);
                if (IsNullValue(projectedPropertyValue))
                {
                    projectedPropertyValue = null;
                }
                return projectedPropertyValue;
            }
            if (resourceProperty != null)
            {
                return provider.GetPropertyValue(resource, resourceProperty, resourceType);
            }
            return provider.GetOpenPropertyValue(resource, propertyName);
        }

        internal static IEnumerator GetRequestEnumerator(IEnumerable enumerable)
        {
            IEnumerator enumerator;
            try
            {
                enumerator = enumerable.GetEnumerator();
            }
            catch (NotImplementedException exception)
            {
                throw new DataServiceException(0x1f5, null, System.Data.Services.Strings.DataService_NotImplementedException, null, exception);
            }
            catch (NotSupportedException exception2)
            {
                throw new DataServiceException(0x1f5, null, System.Data.Services.Strings.DataService_NotImplementedException, null, exception2);
            }
            return enumerator;
        }

        internal static ResourceType GetResourceType(DataServiceProviderWrapper provider, object obj)
        {
            return (ResourceType.PrimitiveResourceTypeMap.GetPrimitive(obj.GetType()) ?? GetNonPrimitiveResourceType(provider, obj));
        }

        internal static ContentFormat GetResponseFormatForPrimitiveValue(ResourceType valueType, out string contentType)
        {
            ContentFormat text = ContentFormat.Text;
            contentType = "text/plain";
            if (valueType != null)
            {
                foreach (KeyValuePair<Type, string> pair in PrimitiveTypesMimeTypeMapping)
                {
                    if (valueType.InstanceType == pair.Key)
                    {
                        contentType = pair.Value;
                        break;
                    }
                }
                foreach (KeyValuePair<Type, ContentFormat> pair2 in PrimitiveTypesContentFormatMapping)
                {
                    if (valueType.InstanceType == pair2.Key)
                    {
                        return pair2.Value;
                    }
                }
            }
            return text;
        }

        internal static T GetService<T>(object target) where T: class
        {
            IServiceProvider provider = target as IServiceProvider;
            if (provider != null)
            {
                object service = provider.GetService(typeof(T));
                if (service != null)
                {
                    return (T) service;
                }
            }
            return default(T);
        }

        internal static Type GetTypeAllowingNull(Type type)
        {
            if (!TypeAllowsNull(type))
            {
                return typeof(Nullable<>).MakeGenericType(new Type[] { type });
            }
            return type;
        }

        internal static string GetTypeName(Type type)
        {
            ResourceType primitive = ResourceType.PrimitiveResourceTypeMap.GetPrimitive(type);
            if (primitive == null)
            {
                return type.FullName;
            }
            return primitive.FullName;
        }

        internal static Type GetWrapperType(Type[] wrapperParameters, Func<object, string> errorGenerator)
        {
            if ((wrapperParameters.Length - 1) > 12)
            {
                throw DataServiceException.CreateBadRequestError(errorGenerator(wrapperParameters.Length - 1));
            }
            return GenericExpandedWrapperTypes.Single<ExpandWrapperTypeWithIndex>(x => (x.Index == (wrapperParameters.Length - 1))).Type.MakeGenericType(wrapperParameters);
        }

        internal static bool HasMediaLinkEntryInHierarchy(ResourceType baseType, DataServiceProviderWrapper provider)
        {
            return (baseType.IsMediaLinkEntry || provider.GetDerivedTypes(baseType).Any<ResourceType>(derivedType => derivedType.IsMediaLinkEntry));
        }

        internal static bool IsAtomResponseFormat(string acceptTypesText, RequestTargetKind targetKind, DataServiceProtocolVersion maxProtocolVersion, Version requestMaxVersion)
        {
            Version effectiveMaxResponseVersion = GetEffectiveMaxResponseVersion(maxProtocolVersion, requestMaxVersion);
            string mime = SelectResponseMediaType(acceptTypesText, targetKind == RequestTargetKind.Resource, effectiveMaxResponseVersion < RequestDescription.Version3Dot0);
            if (mime == null)
            {
                return false;
            }
            return (GetContentFormat(mime) == ContentFormat.Atom);
        }

        internal static bool IsBinaryResourceType(ResourceType resourceType)
        {
            if (((resourceType == null) || (resourceType.ResourceTypeKind != ResourceTypeKind.Primitive)) || (!(resourceType.InstanceType == typeof(byte[])) && !(resourceType.InstanceType == typeof(Binary))))
            {
                return false;
            }
            return true;
        }

        internal static bool IsElementIEnumerable(object element, out IEnumerable enumerable)
        {
            enumerable = element as IEnumerable;
            if (enumerable == null)
            {
                return false;
            }
            Type type = element.GetType();
            return !ResourceType.PrimitiveResourceTypeMap.IsPrimitive(type);
        }

        internal static bool IsETagValueValid(string etag, bool allowStrongEtag)
        {
            if (!string.IsNullOrEmpty(etag) && (etag != "*"))
            {
                int num = 1;
                if (etag.StartsWith("W/\"", StringComparison.Ordinal) && (etag[etag.Length - 1] == '"'))
                {
                    num = 3;
                }
                else if ((!allowStrongEtag || (etag[0] != '"')) || (etag[etag.Length - 1] != '"'))
                {
                    return false;
                }
                for (int i = num; i < (etag.Length - 1); i++)
                {
                    if (etag[i] == '"')
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal static bool IsExpandedWrapperType(Type inputType)
        {
            return (inputType.IsGenericType && (GenericExpandedWrapperTypes.SingleOrDefault<ExpandWrapperTypeWithIndex>(x => (x.Type == inputType.GetGenericTypeDefinition())) != null));
        }

        internal static bool IsNullableType(Type type)
        {
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        private static bool IsNullOrWhitespace(string text)
        {
            return string.IsNullOrWhiteSpace(text);
        }

        internal static bool IsNullValue(object propertyValue)
        {
            if (propertyValue != null)
            {
                return (propertyValue == DBNull.Value);
            }
            return true;
        }

        internal static bool IsPrimitiveType(Type type)
        {
            return ((type != null) && ResourceType.PrimitiveResourceTypeMap.IsPrimitive(type));
        }

        internal static bool IsSpatial(this Type type)
        {
            return typeof(ISpatial).IsAssignableFrom(type);
        }

        internal static bool IsValidMimeType(string mimeType)
        {
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            foreach (char ch in mimeType)
            {
                if (ch == '/')
                {
                    if (!flag || flag2)
                    {
                        return false;
                    }
                    flag2 = true;
                }
                else
                {
                    if (((ch < ' ') || (ch > '\x007f')) || ((ch == ' ') || ("()<>@,;:\\\"/[]?=".IndexOf(ch) >= 0)))
                    {
                        return false;
                    }
                    if (flag2)
                    {
                        flag3 = true;
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }
            return flag3;
        }

        private static bool IsWhitespace(string text)
        {
            if (text != null)
            {
                return text.All<char>(new Func<char, bool>(char.IsWhiteSpace));
            }
            return true;
        }

        internal static MetadataEdmSchemaVersion RaiseMetadataEdmSchemaVersion(MetadataEdmSchemaVersion versionToRaise, MetadataEdmSchemaVersion targetVersion)
        {
            if (targetVersion <= versionToRaise)
            {
                return versionToRaise;
            }
            return targetVersion;
        }

        internal static MetadataEdmSchemaVersion RaiseVersion(MetadataEdmSchemaVersion versionToRaise, MetadataEdmSchemaVersion targetVersion)
        {
            if (targetVersion <= versionToRaise)
            {
                return versionToRaise;
            }
            return targetVersion;
        }

        internal static Version RaiseVersion(Version versionToRaise, Version targetVersion)
        {
            if (targetVersion <= versionToRaise)
            {
                return versionToRaise;
            }
            return targetVersion;
        }

        internal static void RecurseEnter(int recursionLimit, ref int recursionDepth)
        {
            recursionDepth++;
            if (recursionDepth == recursionLimit)
            {
                throw DataServiceException.CreateDeepRecursion(recursionLimit);
            }
        }

        internal static void RecurseEnterQueryParser(int recursionLimit, ref int recursionDepth)
        {
            recursionDepth++;
            if (recursionDepth == recursionLimit)
            {
                throw DataServiceException.CreateDeepRecursion_General();
            }
        }

        internal static void RecurseLeave(ref int recursionDepth)
        {
            recursionDepth--;
        }

        internal static ResourceType ResolveTypeIdentifier(DataServiceProviderWrapper provider, string identifier, ResourceType previousSegmentResourceType, bool previousSegmentIsTypeSegment)
        {
            ResourceType subType = provider.TryResolveResourceType(identifier);
            if (subType != null)
            {
                if (previousSegmentIsTypeSegment)
                {
					throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestUriProcessor_TypeIdentifierCannotBeSpecifiedAfterTypeIdentifier(identifier, previousSegmentResourceType.FullName));
                }
                if (!previousSegmentResourceType.IsAssignableFrom(subType))
                {
					throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.RequestUriProcessor_InvalidTypeIdentifier_MustBeASubType(identifier, previousSegmentResourceType.FullName));
                }
            }
            return subType;
        }

        internal static bool ResponseMediaTypeWouldBeJsonLight(string acceptTypesText, bool isEntityOrFeed)
        {
            string[] availableTypes = GetAvailableMediaTypesForV3WithJsonLight(isEntityOrFeed);
            string b = HttpProcessUtility.SelectMimeType(acceptTypesText, availableTypes);
            if (!string.Equals("application/json;odata=light", b, StringComparison.OrdinalIgnoreCase))
            {
                return string.Equals("application/json", b, StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        internal static ContentFormat SelectRequestFormat(string contentType, RequestDescription description)
        {
            ContentFormat verboseJson;
            if (CompareMimeType(contentType, "application/json;odata=verbose"))
            {
                verboseJson = ContentFormat.VerboseJson;
            }
            else if (CompareMimeType(contentType, "text/xml") || CompareMimeType(contentType, "application/xml"))
            {
                verboseJson = ContentFormat.PlainXml;
            }
            else
            {
                if (!CompareMimeType(contentType, "application/atom+xml") && !CompareMimeType(contentType, "*/*"))
                {
                    return ContentFormat.Unsupported;
                }
                verboseJson = ContentFormat.Atom;
            }
            if (description.LinkUri)
            {
                if (verboseJson == ContentFormat.Atom)
                {
                    throw new DataServiceException(0x19f, System.Data.Services.Strings.BadRequest_InvalidContentTypeForRequestUri(contentType, string.Format(CultureInfo.InvariantCulture, "'{0}', '{1}', '{2}'", new object[] { "application/json;odata=verbose", "application/xml", "text/xml" })));
                }
                return verboseJson;
            }
            if ((description.TargetKind == RequestTargetKind.Resource) || description.LastSegmentInfo.HasKeyValues)
            {
                if (verboseJson == ContentFormat.PlainXml)
                {
                    throw new DataServiceException(0x19f, System.Data.Services.Strings.BadRequest_InvalidContentTypeForRequestUri(contentType, string.Format(CultureInfo.InvariantCulture, "'{0}', '{1}', '{2}'", new object[] { "application/json;odata=verbose", "application/atom+xml", "*/*" })));
                }
                return verboseJson;
            }
            if ((description.TargetKind != RequestTargetKind.OpenProperty) && (verboseJson == ContentFormat.Atom))
            {
                throw new DataServiceException(0x19f, System.Data.Services.Strings.BadRequest_InvalidContentTypeForRequestUri(contentType, string.Format(CultureInfo.InvariantCulture, "'{0}', '{1}', '{2}'", new object[] { "application/json;odata=verbose", "application/xml", "text/xml" })));
            }
            return verboseJson;
        }

        internal static string SelectResponseMediaType(string acceptTypesText, bool entityTarget, bool effectiveMaxResponseVersionIsLessThanThree)
        {
            string[] strArray;
            if (effectiveMaxResponseVersionIsLessThanThree)
            {
                strArray = GetAvailableMediaTypesForV2(entityTarget);
            }
            else
            {
                strArray = GetAvailableMediaTypesForV3WithoutJsonLight(entityTarget);
            }
            string str = HttpProcessUtility.SelectMimeType(acceptTypesText, strArray);
            if (CompareMimeType(str, "application/json"))
            {
                str = "application/json;odata=verbose";
            }
            return str;
        }

        internal static void SetResponseHeadersForBatchRequests(ODataBatchOperationResponseMessage operationResponseMessage, BatchServiceHost batchHost)
        {
            IDataServiceHost2 host = batchHost;
            operationResponseMessage.StatusCode = host.ResponseStatusCode;
            if (batchHost.ContentId != null)
            {
                operationResponseMessage.SetHeader("Content-ID", batchHost.ContentId);
            }
            WebHeaderCollection responseHeaders = host.ResponseHeaders;
            foreach (string str in responseHeaders.AllKeys)
            {
                string str2 = responseHeaders[str];
                if (!string.IsNullOrEmpty(str2))
                {
                    operationResponseMessage.SetHeader(str, str2);
                }
            }
        }

        internal static void SkipInsignificantNodes(this XmlReader reader)
        {
            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.None:
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Comment:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.XmlDeclaration:
                        break;

                    case XmlNodeType.Attribute:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.EntityReference:
                    case XmlNodeType.Entity:
                        return;

                    case XmlNodeType.Text:
                        if (IsNullOrWhitespace(reader.Value))
                        {
                            break;
                        }
                        return;

                    default:
                        return;
                }
            }
            while (reader.Read());
        }

        internal static string[] StringToSimpleArray(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                return text.Split(new char[] { ',' }, StringSplitOptions.None);
            }
            return EmptyStringArray;
        }

        internal static Version ToVersion(this DataServiceProtocolVersion protocolVersion)
        {
            switch (protocolVersion)
            {
                case DataServiceProtocolVersion.V1:
                    return RequestDescription.Version1Dot0;

                case DataServiceProtocolVersion.V2:
                    return RequestDescription.Version2Dot0;
            }
            return RequestDescription.Version3Dot0;
        }

        internal static ResourceType TryResolveResourceType(DataServiceProviderWrapper provider, string typeName)
        {
            return (ResourceType.PrimitiveResourceTypeMap.GetPrimitive(typeName) ?? provider.TryResolveResourceType(typeName));
        }

        internal static bool TypeAllowsNull(Type type)
        {
            if (type.IsValueType)
            {
                return IsNullableType(type);
            }
            return true;
        }

        internal static void ValidateAndAddAnnotation(ref Dictionary<string, object> customAnnotations, string namespaceName, string name, object annotation)
        {
            if (customAnnotations == null)
            {
                customAnnotations = new Dictionary<string, object>(StringComparer.Ordinal);
            }
            string.IsNullOrEmpty(namespaceName);
            string key = CreateFullNameForCustomAnnotation(namespaceName, name);
            customAnnotations.Add(key, annotation);
        }

        internal static void WriteETagValueInResponseHeader(RequestDescription requestDescription, string etagValue, DataServiceHostWrapper host)
        {
            if (!string.IsNullOrEmpty(etagValue))
            {
                host.ResponseETag = etagValue;
            }
        }

        internal static bool XmlReaderEnsureElement(XmlReader reader)
        {
            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.None:
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Comment:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.XmlDeclaration:
                        break;

                    case XmlNodeType.Element:
                        return true;

                    case XmlNodeType.Text:
                        if (IsWhitespace(reader.Value))
                        {
                            break;
                        }
                        return false;

                    default:
                        return false;
                }
            }
            while (reader.Read());
            return false;
        }

        internal static IEnumerable<TResult> Zip<T1, T2, TResult>(IEnumerable<T1> left, IEnumerable<T2> right, Func<T1, T2, TResult> resultSelector)
        {
            if ((left != null) && (right != null))
            {
                if (resultSelector == null) resultSelector = (x, y) => default(TResult);
                using (IEnumerator<T1> iteratorVariable0 = left.GetEnumerator())
                {
                    using (IEnumerator<T2> iteratorVariable1 = right.GetEnumerator())
                    {
                        while (iteratorVariable0.MoveNext() && iteratorVariable1.MoveNext())
                        {
                            yield return resultSelector(iteratorVariable0.Current, iteratorVariable1.Current);
                        }
                    }
                }
            }
        }

        
        private sealed class ExpandWrapperTypeWithIndex
        {
            internal int Index { get; set; }

            internal System.Type Type { get; set; }
        }

        private sealed class ValidatedNotNullAttribute : System.Attribute
        {
        }
    }
}

