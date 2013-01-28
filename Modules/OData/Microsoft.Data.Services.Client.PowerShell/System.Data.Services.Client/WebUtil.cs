

namespace System.Data.Services.Client
{
	using Microsoft.Data.Edm;
    using Microsoft.Data.OData;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Services.Client.Metadata;
    using System.Data.Services.Common;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.CompilerServices;
	using System.Xml;

    internal static class WebUtil
    {
        private static bool? dataServiceCollectionAvailable = null;
        internal const int DefaultBufferSizeForStreamCopy = 0x10000;
        private static MethodInfo getDefaultValueMethodInfo = ((MethodInfo) typeof(WebUtil).GetMember("GetDefaultValue", BindingFlags.NonPublic | BindingFlags.Static).Single<MemberInfo>(m => (((MethodInfo) m).GetGenericArguments().Count<Type>() == 1)));


        internal static void ApplyHeadersToRequest(Dictionary<string, string> headers, ODataRequestMessageWrapper requestMessage, bool ignoreAcceptHeader)
        {
            foreach (KeyValuePair<string, string> pair in headers)
            {
                if (!string.Equals(pair.Key, "Accept", StringComparison.Ordinal) || !ignoreAcceptHeader)
                {
                    requestMessage.SetHeader(pair.Key, pair.Value);
                }
            }
        }

        internal static IAsyncResult BeginGetRequestStream(ODataRequestMessageWrapper request, AsyncCallback callback, object state)
        {
            return request.BeginGetRequestStream(callback, state);
        }

        internal static IAsyncResult BeginGetResponse(ODataRequestMessageWrapper request, AsyncCallback callback, object state)
        {
            return request.BeginGetResponse(callback, state);
        }

        internal static long CopyStream(Stream input, Stream output, ref byte[] refBuffer)
        {
            long num = 0L;
            byte[] buffer = refBuffer;
            if (buffer == null)
            {
                refBuffer = buffer = new byte[0x3e8];
            }
            int count = 0;
            while (input.CanRead && (0 < (count = input.Read(buffer, 0, buffer.Length))))
            {
                output.Write(buffer, 0, count);
                num += count;
            }
            return num;
        }

        internal static ODataMessageReaderSettings CreateODataMessageReaderSettings(ResponseInfo responseInfo, Func<ODataEntry, XmlReader, Uri, XmlReader> entryXmlCustomizer, bool projectionQuery)
        {
            ODataMessageReaderSettings settings = new ODataMessageReaderSettings();
            responseInfo.TypeResolver = new TypeResolver(responseInfo, projectionQuery);
            settings.EnableWcfDataServicesClientBehavior(new Func<IEdmType, string, IEdmType>(responseInfo.TypeResolver.ResolveWireTypeName), responseInfo.DataNamespace, CommonUtil.UriToString(responseInfo.TypeScheme), entryXmlCustomizer);
            settings.BaseUri = (responseInfo.BaseUriResolver.GetRawBaseUriValue() != null) ? responseInfo.BaseUriResolver.GetBaseUriWithSlash() : null;
            settings.UndeclaredPropertyBehaviorKinds = ODataUndeclaredPropertyBehaviorKinds.ReportUndeclaredLinkProperty;
            settings.MaxProtocolVersion = CommonUtil.ConvertToODataVersion(responseInfo.MaxProtocolVersion);
            if (responseInfo.IgnoreMissingProperties)
            {
                settings.UndeclaredPropertyBehaviorKinds |= ODataUndeclaredPropertyBehaviorKinds.IgnoreUndeclaredValueProperty;
            }
            settings.MessageQuotas.MaxEntityPropertyMappingsPerType = 0x7fffffff;
            settings.MessageQuotas.MaxNestingDepth = 0x7fffffff;
            settings.MessageQuotas.MaxOperationsPerChangeset = 0x7fffffff;
            settings.MessageQuotas.MaxPartsPerBatch = 0x7fffffff;
            settings.MessageQuotas.MaxReceivedMessageSize = 0x7fffffffffffffffL;
            return settings;
        }

        internal static Stream EndGetRequestStream(ODataRequestMessageWrapper request, IAsyncResult asyncResult, DataServiceContext context)
        {
            Stream requestStream = request.EndGetRequestStream(asyncResult);
            return context.InternalGetRequestWrappingStream(requestStream);
        }

        internal static HttpWebResponse EndGetResponse(ODataRequestMessageWrapper request, IAsyncResult asyncResult, DataServiceContext context)
        {
            return GetResponseHelper(request, context, asyncResult, true);
        }

        internal static Type GetBackingTypeForCollectionProperty(Type collectionPropertyType, Type collectionItemType)
        {
            if (collectionPropertyType.IsInterface())
            {
                return typeof(ObservableCollection<>).MakeGenericType(new Type[] { collectionItemType });
            }
            return collectionPropertyType;
        }

        internal static string GetCollectionItemWireTypeName(string wireTypeName)
        {
            return CommonUtil.GetCollectionItemTypeName(wireTypeName, false);
        }

        internal static Type GetDataServiceCollectionOfT(params Type[] typeArguments)
        {
            if (DataServiceCollectionAvailable)
            {
                return GetDataServiceCollectionOfTType().MakeGenericType(typeArguments);
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Type GetDataServiceCollectionOfTType()
        {
            return typeof(DataServiceCollection<>);
        }

        internal static T GetDefaultValue<T>()
        {
            return default(T);
        }

        internal static object GetDefaultValue(Type type)
        {
            return getDefaultValueMethodInfo.MakeGenericMethod(new Type[] { type }).Invoke(null, null);
        }

        internal static void GetHttpWebResponse(InvalidOperationException exception, ref HttpWebResponse response)
        {
            if (response == null)
            {
                WebException exception2 = exception as WebException;
                if (exception2 != null)
                {
                    response = (HttpWebResponse) exception2.Response;
                }
            }
        }

        internal static string GetPreferHeaderAndRequestVersion(DataServiceResponsePreference responsePreference, ref Version requestVersion)
        {
            string str = null;
            if (responsePreference != DataServiceResponsePreference.None)
            {
                if (responsePreference == DataServiceResponsePreference.IncludeContent)
                {
                    str = "return-content";
                }
                else
                {
                    str = "return-no-content";
                }
                RaiseVersion(ref requestVersion, Util.DataServiceVersion3);
            }
            return str;
        }

        internal static Stream GetRequestStream(ODataRequestMessageWrapper request, DataServiceContext context)
        {
            Stream requestStream = request.GetRequestStream();
            return context.InternalGetRequestWrappingStream(requestStream);
        }

        internal static HttpWebResponse GetResponse(ODataRequestMessageWrapper request, DataServiceContext context, bool handleWebException)
        {
            return GetResponseHelper(request, context, null, handleWebException);
        }

        private static HttpWebResponse GetResponseHelper(ODataRequestMessageWrapper request, DataServiceContext context, IAsyncResult asyncResult, bool handleWebException)
        {
            HttpWebResponse response = null;
            try
            {
                if (asyncResult == null)
                {
                    return request.GetResponse();
                }
                return request.EndGetResponse(asyncResult);
            }
            catch (WebException exception)
            {
                response = (HttpWebResponse) exception.Response;
                if (!handleWebException)
                {
                    throw;
                }
                if (response == null)
                {
                    throw;
                }
            }
            finally
            {
                context.InternalSendResponse(response);
            }
            return response;
        }

        internal static Stream GetResponseStream(HttpWebResponse response, DataServiceContext context)
        {
            Stream responseStream = response.GetResponseStream();
            if (context == null)
            {
                return responseStream;
            }
            return context.InternalGetResponseWrappingStream(responseStream);
        }

        internal static bool IsCLRTypeCollection(Type type, DataServiceProtocolVersion maxProtocolVersion)
        {
            if (!PrimitiveType.IsKnownNullableType(type))
            {
                Type implementationType = ClientTypeUtil.GetImplementationType(type, typeof(ICollection<>));
                if ((implementationType != null) && !ClientTypeUtil.TypeIsEntity(implementationType.GetGenericArguments()[0], maxProtocolVersion))
                {
                    if (maxProtocolVersion <= DataServiceProtocolVersion.V2)
                    {
                        throw new InvalidOperationException(System.Data.Services.Client.Strings.WebUtil_CollectionTypeNotSupportedInV2OrBelow(type.FullName));
                    }
                    return true;
                }
            }
            return false;
        }

        internal static bool IsDataServiceCollectionType(Type t)
        {
            return (DataServiceCollectionAvailable && (t == GetDataServiceCollectionOfTType()));
        }

        internal static bool IsWireTypeCollection(string wireTypeName)
        {
            return (CommonUtil.GetCollectionItemTypeName(wireTypeName, false) != null);
        }

        internal static void RaiseVersion(ref Version version, Version minimalVersion)
        {
            if ((version == null) || (version < minimalVersion))
            {
                version = minimalVersion;
            }
        }

        internal static void SetOperationVersionHeaders(ODataRequestMessageWrapper requestMessage, Version requestVersion, Version maxProtocolVersion)
        {
            if (requestVersion != null)
            {
                if (requestVersion > maxProtocolVersion)
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_RequestVersionIsBiggerThanProtocolVersion(requestVersion.ToString(), maxProtocolVersion.ToString()));
                }
                if (requestVersion.Major > 0)
                {
                    requestMessage.SetHeader("DataServiceVersion", requestVersion.ToString() + ";NetFx");
                }
            }
            requestMessage.SetHeader("MaxDataServiceVersion", maxProtocolVersion.ToString() + ";NetFx");
        }

        internal static bool SuccessStatusCode(HttpStatusCode status)
        {
            return ((HttpStatusCode.OK <= status) && (status < HttpStatusCode.MultipleChoices));
        }

        internal static void ValidateCollection(Type collectionItemType, object propertyValue, string propertyName)
        {
            if (!PrimitiveType.IsKnownNullableType(collectionItemType) && (collectionItemType.GetInterfaces().SingleOrDefault<Type>(t => (t == typeof(IEnumerable))) != null))
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.ClientType_CollectionOfCollectionNotSupported);
            }
            if (propertyValue == null)
            {
                if (propertyName != null)
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Collection_NullCollectionNotSupported(propertyName));
                }
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Collection_NullNonPropertyCollectionNotSupported(collectionItemType));
            }
        }

        internal static void ValidateCollectionItem(object itemValue)
        {
            if (itemValue == null)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Collection_NullCollectionItemsNotSupported);
            }
        }

        internal static void ValidateComplexCollectionItem(object itemValue, string propertyName, Type collectionItemType)
        {
            Type type = itemValue.GetType();
            if (PrimitiveType.IsKnownNullableType(type))
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Collection_PrimitiveTypesInCollectionOfComplexTypesNotAllowed);
            }
            if (type != collectionItemType)
            {
                if (propertyName != null)
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.WebUtil_TypeMismatchInCollection(propertyName));
                }
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.WebUtil_TypeMismatchInNonPropertyCollection(collectionItemType));
            }
        }

        internal static void ValidateIdentityValue(string identity)
        {
            if (!Util.CreateUri(identity, UriKind.RelativeOrAbsolute).IsAbsoluteUri)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_TrackingExpectsAbsoluteUri);
            }
        }

        internal static Uri ValidateLocationHeader(string location)
        {
            Uri uri = Util.CreateUri(location, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri)
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Context_LocationHeaderExpectsAbsoluteUri);
            }
            return uri;
        }

        internal static void ValidatePrimitiveCollectionItem(object itemValue, string propertyName, Type collectionItemType)
        {
            Type type = itemValue.GetType();
            if (!PrimitiveType.IsKnownNullableType(type))
            {
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.Collection_ComplexTypesInCollectionOfPrimitiveTypesNotAllowed);
            }
            if (!collectionItemType.IsAssignableFrom(type))
            {
                if (propertyName != null)
                {
                    throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.WebUtil_TypeMismatchInCollection(propertyName));
                }
                throw System.Data.Services.Client.Error.InvalidOperation(System.Data.Services.Client.Strings.WebUtil_TypeMismatchInNonPropertyCollection(collectionItemType));
            }
        }

        private static Dictionary<string, string> WrapHttpHeaders(WebHeaderCollection headers)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>(EqualityComparer<string>.Default);
            if (headers != null)
            {
                foreach (string str in headers.AllKeys)
                {
                    dictionary.Add(str, headers[str]);
                }
            }
            return dictionary;
        }

        internal static Dictionary<string, string> WrapResponseHeaders(HttpWebResponse response)
        {
            return WrapHttpHeaders((response != null) ? response.Headers : null);
        }

        private static bool DataServiceCollectionAvailable
        {
            get
            {
                if (!dataServiceCollectionAvailable.HasValue)
                {
                    try
                    {
                        dataServiceCollectionAvailable = new bool?(GetDataServiceCollectionOfTType() != null);
                    }
                    catch (FileNotFoundException)
                    {
                        dataServiceCollectionAvailable = false;
                    }
                }
                return dataServiceCollectionAvailable.Value;
            }
        }
    }
}

