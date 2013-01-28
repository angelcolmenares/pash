namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class MediaTypeUtils
    {
        private static readonly ODataPayloadKind[] allSupportedPayloadKinds;
        private static readonly UTF8Encoding encodingUtf8NoPreamble;

        static MediaTypeUtils()
        {
            ODataPayloadKind[] kindArray = new ODataPayloadKind[13];
            kindArray[1] = ODataPayloadKind.Entry;
            kindArray[2] = ODataPayloadKind.Property;
            kindArray[3] = ODataPayloadKind.MetadataDocument;
            kindArray[4] = ODataPayloadKind.ServiceDocument;
            kindArray[5] = ODataPayloadKind.Value;
            kindArray[6] = ODataPayloadKind.BinaryValue;
            kindArray[7] = ODataPayloadKind.Collection;
            kindArray[8] = ODataPayloadKind.EntityReferenceLinks;
            kindArray[9] = ODataPayloadKind.EntityReferenceLink;
            kindArray[10] = ODataPayloadKind.Batch;
            kindArray[11] = ODataPayloadKind.Error;
            kindArray[12] = ODataPayloadKind.Parameter;
            allSupportedPayloadKinds = kindArray;
            encodingUtf8NoPreamble = new UTF8Encoding(false, true);
        }

        internal static void CheckMediaTypeForWildCards(MediaType mediaType)
        {
            if (HttpUtils.CompareMediaTypeNames("*", mediaType.TypeName) || HttpUtils.CompareMediaTypeNames("*", mediaType.SubTypeName))
            {
                throw new ODataException(Microsoft.Data.OData.Strings.ODataMessageReader_WildcardInContentType(mediaType.FullTypeName));
            }
        }

        private static void FailOnUnsupportedMediaTypes(MediaType contentType, string contentTypeName, ODataPayloadKind[] supportedPayloadKinds, MediaTypeResolver mediaTypeResolver)
        {
            Func<ODataPayloadKind, IEnumerable<string>> selector = null;
            if (((HttpUtils.CompareMediaTypeNames(contentType.SubTypeName, "json") && HttpUtils.CompareMediaTypeNames(contentType.TypeName, "application")) && (contentType.Parameters != null)) && (from p in contentType.Parameters
                where HttpUtils.CompareMediaTypeParameterNames(p.Key, "odata") && (string.Compare("light", p.Value, StringComparison.OrdinalIgnoreCase) == 0)
                select p).Any<KeyValuePair<string, string>>())
            {
                if (selector == null)
                {
                    selector = pk => from mt in mediaTypeResolver.GetMediaTypesForPayloadKind(pk) select mt.MediaType.ToText();
                }
                throw new ODataContentTypeException(Microsoft.Data.OData.Strings.MediaTypeUtils_CannotDetermineFormatFromContentType(string.Join(", ", supportedPayloadKinds.SelectMany<ODataPayloadKind, string>(selector).ToArray<string>()), contentTypeName));
            }
        }

        internal static ODataFormat GetContentTypeFromSettings(ODataMessageWriterSettings settings, ODataPayloadKind payloadKind, MediaTypeResolver mediaTypeResolver, out MediaType mediaType, out Encoding encoding)
        {
            ODataFormat format;
            MediaTypeWithFormat format2;
            MediaTypeWithFormat[] mediaTypesForPayloadKind = mediaTypeResolver.GetMediaTypesForPayloadKind(payloadKind);
            if ((mediaTypesForPayloadKind == null) || (mediaTypesForPayloadKind.Length == 0))
            {
                throw new ODataContentTypeException(Microsoft.Data.OData.Strings.MediaTypeUtils_DidNotFindMatchingMediaType(null, settings.AcceptableMediaTypes));
            }
            if (settings.UseFormat == true)
            {
                mediaType = GetDefaultMediaType(mediaTypesForPayloadKind, settings.Format, out format);
                encoding = mediaType.SelectEncoding();
                return format;
            }
            IList<KeyValuePair<MediaType, string>> specifiedTypes = HttpUtils.MediaTypesFromString(settings.AcceptableMediaTypes);
            if (((ODataVersion) settings.Version) == ODataVersion.V3)
            {
                specifiedTypes = RemoveApplicationJsonFromAcceptableMediaTypes(specifiedTypes, mediaTypesForPayloadKind, settings.AcceptableMediaTypes);
            }
            string str = null;
            if ((specifiedTypes == null) || (specifiedTypes.Count == 0))
            {
                format2 = mediaTypesForPayloadKind[0];
            }
            else
            {
                MediaTypeMatchInfo info = MatchMediaTypes(from kvp in specifiedTypes select kvp.Key, (from smt in mediaTypesForPayloadKind select smt.MediaType).ToArray<MediaType>());
                if (info == null)
                {
                    throw new ODataContentTypeException(Microsoft.Data.OData.Strings.MediaTypeUtils_DidNotFindMatchingMediaType(string.Join(", ", (from mt in mediaTypesForPayloadKind select mt.MediaType.ToText()).ToArray<string>()), settings.AcceptableMediaTypes));
                }
                format2 = mediaTypesForPayloadKind[info.TargetTypeIndex];
                KeyValuePair<MediaType, string> pair = specifiedTypes[info.SourceTypeIndex];
                str = pair.Value;
            }
            format = format2.Format;
            mediaType = format2.MediaType;
            string acceptableCharsets = settings.AcceptableCharsets;
            if (str != null)
            {
                acceptableCharsets = (acceptableCharsets == null) ? str : (str + "," + acceptableCharsets);
            }
            encoding = GetEncoding(acceptableCharsets, payloadKind, mediaType, true);
            return format;
        }

        private static MediaType GetDefaultMediaType(MediaTypeWithFormat[] supportedMediaTypes, ODataFormat specifiedFormat, out ODataFormat actualFormat)
        {
            for (int i = 0; i < supportedMediaTypes.Length; i++)
            {
                MediaTypeWithFormat format = supportedMediaTypes[i];
                if ((specifiedFormat == null) || (format.Format == specifiedFormat))
                {
                    actualFormat = format.Format;
                    return format.MediaType;
                }
            }
            throw new ODataException(Microsoft.Data.OData.Strings.ODataUtils_DidNotFindDefaultMediaType(specifiedFormat));
        }

        private static Encoding GetEncoding(string acceptCharsetHeader, ODataPayloadKind payloadKind, MediaType mediaType, bool useDefaultEncoding)
        {
            if (payloadKind == ODataPayloadKind.BinaryValue)
            {
                return null;
            }
            return HttpUtils.EncodingFromAcceptableCharsets(acceptCharsetHeader, mediaType, encodingUtf8NoPreamble, useDefaultEncoding ? encodingUtf8NoPreamble : null);
        }

        private static ODataFormat GetFormatFromContentType(string contentTypeName, ODataPayloadKind[] supportedPayloadKinds, MediaTypeResolver mediaTypeResolver, out MediaType mediaType, out Encoding encoding, out ODataPayloadKind selectedPayloadKind)
        {
            string str;
            mediaType = ParseContentType(contentTypeName, out str);
            FailOnUnsupportedMediaTypes(mediaType, contentTypeName, supportedPayloadKinds, mediaTypeResolver);
            MediaTypeWithFormat[] mediaTypesForPayloadKind = null;
            for (int i = 0; i < supportedPayloadKinds.Length; i++)
            {
                ODataPayloadKind payloadKind = supportedPayloadKinds[i];
                mediaTypesForPayloadKind = mediaTypeResolver.GetMediaTypesForPayloadKind(payloadKind);
                MediaTypeMatchInfo info = MatchMediaTypes(from smt in mediaTypesForPayloadKind select smt.MediaType, new MediaType[] { mediaType });
                if (info != null)
                {
                    selectedPayloadKind = payloadKind;
                    encoding = GetEncoding(str, selectedPayloadKind, mediaType, false);
                    return mediaTypesForPayloadKind[info.SourceTypeIndex].Format;
                }
            }
            throw new ODataContentTypeException(Microsoft.Data.OData.Strings.MediaTypeUtils_CannotDetermineFormatFromContentType(string.Join(", ", (from pk in supportedPayloadKinds select from mt in mediaTypeResolver.GetMediaTypesForPayloadKind(pk) select mt.MediaType.ToText()).SelectMany (x => x).ToArray<string>()), contentTypeName));
        }

        internal static ODataFormat GetFormatFromContentType(string contentTypeHeader, ODataPayloadKind[] supportedPayloadKinds, MediaTypeResolver mediaTypeResolver, out MediaType mediaType, out Encoding encoding, out ODataPayloadKind selectedPayloadKind, out string batchBoundary)
        {
            ODataFormat format = GetFormatFromContentType(contentTypeHeader, supportedPayloadKinds, mediaTypeResolver, out mediaType, out encoding, out selectedPayloadKind);
            if (selectedPayloadKind == ODataPayloadKind.Batch)
            {
                KeyValuePair<string, string> pair = new KeyValuePair<string, string>();
                IEnumerable<KeyValuePair<string, string>> parameters = mediaType.Parameters;
                if (parameters != null)
                {
                    bool flag = false;
                    foreach (KeyValuePair<string, string> pair2 in from p in parameters
                        where HttpUtils.CompareMediaTypeParameterNames("boundary", p.Key)
                        select p)
                    {
                        if (flag)
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.MediaTypeUtils_BoundaryMustBeSpecifiedForBatchPayloads(contentTypeHeader, "boundary"));
                        }
                        pair = pair2;
                        flag = true;
                    }
                }
                if (pair.Key == null)
                {
                    throw new ODataException(Microsoft.Data.OData.Strings.MediaTypeUtils_BoundaryMustBeSpecifiedForBatchPayloads(contentTypeHeader, "boundary"));
                }
                batchBoundary = pair.Value;
                ValidationUtils.ValidateBoundaryString(batchBoundary);
                return format;
            }
            batchBoundary = null;
            return format;
        }

        internal static IList<ODataPayloadKindDetectionResult> GetPayloadKindsForContentType(string contentTypeHeader, MediaTypeResolver mediaTypeResolver, out MediaType contentType)
        {
            string str;
            contentType = ParseContentType(contentTypeHeader, out str);
            MediaType[] targetTypes = new MediaType[] { contentType };
            List<ODataPayloadKindDetectionResult> list = new List<ODataPayloadKindDetectionResult>();
            MediaTypeWithFormat[] mediaTypesForPayloadKind = null;
            for (int i = 0; i < allSupportedPayloadKinds.Length; i++)
            {
                ODataPayloadKind payloadKind = allSupportedPayloadKinds[i];
                mediaTypesForPayloadKind = mediaTypeResolver.GetMediaTypesForPayloadKind(payloadKind);
                MediaTypeMatchInfo info = MatchMediaTypes(from smt in mediaTypesForPayloadKind select smt.MediaType, targetTypes);
                if (info != null)
                {
                    list.Add(new ODataPayloadKindDetectionResult(payloadKind, mediaTypesForPayloadKind[info.SourceTypeIndex].Format));
                }
            }
            return list;
        }

        private static MediaTypeMatchInfo MatchMediaTypes(IEnumerable<MediaType> sourceTypes, MediaType[] targetTypes)
        {
            MediaTypeMatchInfo info = null;
            int sourceIndex = 0;
            if (sourceTypes != null)
            {
                foreach (MediaType type in sourceTypes)
                {
                    int targetIndex = 0;
                    foreach (MediaType type2 in targetTypes)
                    {
                        MediaTypeMatchInfo other = new MediaTypeMatchInfo(type, type2, sourceIndex, targetIndex);
                        if (!other.IsMatch)
                        {
                            targetIndex++;
                        }
                        else
                        {
                            if (info == null)
                            {
                                info = other;
                            }
                            else if (info.CompareTo(other) < 0)
                            {
                                info = other;
                            }
                            targetIndex++;
                        }
                    }
                    sourceIndex++;
                }
            }
            if (info == null)
            {
                return null;
            }
            return info;
        }

        internal static bool MediaTypeAndSubtypeAreEqual(string firstTypeAndSubtype, string secondTypeAndSubtype)
        {
            ExceptionUtils.CheckArgumentNotNull<string>(firstTypeAndSubtype, "firstTypeAndSubtype");
            ExceptionUtils.CheckArgumentNotNull<string>(secondTypeAndSubtype, "secondTypeAndSubtype");
            return HttpUtils.CompareMediaTypeNames(firstTypeAndSubtype, secondTypeAndSubtype);
        }

        internal static bool MediaTypeStartsWithTypeAndSubtype(string mediaType, string typeAndSubtype)
        {
            ExceptionUtils.CheckArgumentNotNull<string>(mediaType, "mediaType");
            ExceptionUtils.CheckArgumentNotNull<string>(typeAndSubtype, "typeAndSubtype");
            return mediaType.StartsWith(typeAndSubtype, StringComparison.OrdinalIgnoreCase);
        }

        private static MediaType ParseContentType(string contentTypeHeader, out string charset)
        {
            IList<KeyValuePair<MediaType, string>> list = HttpUtils.MediaTypesFromString(contentTypeHeader);
            if (list.Count != 1)
            {
                throw new ODataException(Microsoft.Data.OData.Strings.MediaTypeUtils_NoOrMoreThanOneContentTypeSpecified(contentTypeHeader));
            }
            KeyValuePair<MediaType, string> pair = list[0];
            MediaType key = pair.Key;
            CheckMediaTypeForWildCards(key);
            KeyValuePair<MediaType, string> pair2 = list[0];
            charset = pair2.Value;
            return key;
        }

        private static IList<KeyValuePair<MediaType, string>> RemoveApplicationJsonFromAcceptableMediaTypes(IList<KeyValuePair<MediaType, string>> specifiedTypes, MediaTypeWithFormat[] supportedMediaTypes, string acceptableMediaTypes)
        {
            if (specifiedTypes == null)
            {
                return null;
            }
            List<KeyValuePair<MediaType, string>> list = null;
            for (int i = specifiedTypes.Count - 1; i >= 0; i--)
            {
                KeyValuePair<MediaType, string> pair = specifiedTypes[i];
                MediaType key = pair.Key;
                if (HttpUtils.CompareMediaTypeNames(key.SubTypeName, "json") && HttpUtils.CompareMediaTypeNames(key.TypeName, "application"))
                {
                    if ((key.Parameters != null) && (from p in key.Parameters
                        where HttpUtils.CompareMediaTypeParameterNames(p.Key, "odata")
                        select p).Any<KeyValuePair<string, string>>())
                    {
                        continue;
                    }
                    if (list == null)
                    {
                        list = new List<KeyValuePair<MediaType, string>>(specifiedTypes);
                    }
                    list.RemoveAt(i);
                }
            }
            if (list == null)
            {
                return specifiedTypes;
            }
            if (list.Count == 0)
            {
                throw new ODataContentTypeException(Microsoft.Data.OData.Strings.MediaTypeUtils_DidNotFindMatchingMediaType(string.Join(", ", (from mt in supportedMediaTypes select mt.MediaType.ToText()).ToArray<string>()), acceptableMediaTypes));
            }
            return list;
        }

        internal static UTF8Encoding EncodingUtf8NoPreamble
        {
            get
            {
                return encodingUtf8NoPreamble;
            }
        }

        private sealed class MediaTypeMatchInfo : IComparable<MediaTypeUtils.MediaTypeMatchInfo>
        {
            private const int DefaultQualityValue = 0x3e8;
            private readonly int sourceIndex;
            private readonly int targetIndex;

            public MediaTypeMatchInfo(MediaType sourceType, MediaType targetType, int sourceIndex, int targetIndex)
            {
                this.sourceIndex = sourceIndex;
                this.targetIndex = targetIndex;
                this.MatchTypes(sourceType, targetType);
            }

            public int CompareTo(MediaTypeUtils.MediaTypeMatchInfo other)
            {
                ExceptionUtils.CheckArgumentNotNull<MediaTypeUtils.MediaTypeMatchInfo>(other, "other");
                if (this.MatchingTypeNamePartCount > other.MatchingTypeNamePartCount)
                {
                    return 1;
                }
                if (this.MatchingTypeNamePartCount == other.MatchingTypeNamePartCount)
                {
                    if (this.MatchingParameterCount > other.MatchingParameterCount)
                    {
                        return 1;
                    }
                    if (this.MatchingParameterCount == other.MatchingParameterCount)
                    {
                        int num = this.QualityValue.CompareTo(other.QualityValue);
                        if (num != 0)
                        {
                            return num;
                        }
                        if (other.TargetTypeIndex >= this.TargetTypeIndex)
                        {
                            return 1;
                        }
                        return -1;
                    }
                }
                return -1;
            }

            private static bool IsQualityValueParameter(string parameterName)
            {
                return HttpUtils.CompareMediaTypeParameterNames("q", parameterName);
            }

            private void MatchTypes(MediaType sourceType, MediaType targetType)
            {
                this.MatchingTypeNamePartCount = -1;
                if (sourceType.TypeName == "*")
                {
                    this.MatchingTypeNamePartCount = 0;
                }
                else if (HttpUtils.CompareMediaTypeNames(sourceType.TypeName, targetType.TypeName))
                {
                    if (sourceType.SubTypeName == "*")
                    {
                        this.MatchingTypeNamePartCount = 1;
                    }
                    else if (HttpUtils.CompareMediaTypeNames(sourceType.SubTypeName, targetType.SubTypeName))
                    {
                        this.MatchingTypeNamePartCount = 2;
                    }
                }
                this.QualityValue = 0x3e8;
                this.SourceTypeParameterCountForMatching = 0;
                this.MatchingParameterCount = 0;
                IList<KeyValuePair<string, string>> parameters = sourceType.Parameters;
                IList<KeyValuePair<string, string>> list2 = targetType.Parameters;
                bool flag = (list2 != null) && (list2.Count > 0);
                bool flag2 = (parameters != null) && (parameters.Count > 0);
                if (flag2)
                {
                    for (int i = 0; i < parameters.Count; i++)
                    {
                        string str2;
                        KeyValuePair<string, string> pair = parameters[i];
                        string key = pair.Key;
                        if (IsQualityValueParameter(key))
                        {
                            KeyValuePair<string, string> pair2 = parameters[i];
                            this.QualityValue = ParseQualityValue(pair2.Value.Trim());
                            break;
                        }
                        this.SourceTypeParameterCountForMatching = i + 1;
                        if (flag && TryFindMediaTypeParameter(list2, key, out str2))
                        {
                            KeyValuePair<string, string> pair3 = parameters[i];
                            if (string.Compare(pair3.Value.Trim(), str2.Trim(), StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                this.MatchingParameterCount++;
                            }
                        }
                    }
                }
                if ((!flag2 || (this.SourceTypeParameterCountForMatching == 0)) || (this.MatchingParameterCount == this.SourceTypeParameterCountForMatching))
                {
                    this.MatchingParameterCount = -1;
                }
            }

            private static int ParseQualityValue(string qualityValueText)
            {
                int qualityValue = 0x3e8;
                if (qualityValueText.Length > 0)
                {
                    int textIndex = 0;
                    HttpUtils.ReadQualityValue(qualityValueText, ref textIndex, out qualityValue);
                }
                return qualityValue;
            }

            private static bool TryFindMediaTypeParameter(IList<KeyValuePair<string, string>> parameters, string parameterName, out string parameterValue)
            {
                parameterValue = null;
                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Count; i++)
                    {
                        KeyValuePair<string, string> pair = parameters[i];
                        string key = pair.Key;
                        if (HttpUtils.CompareMediaTypeParameterNames(parameterName, key))
                        {
                            KeyValuePair<string, string> pair2 = parameters[i];
                            parameterValue = pair2.Value;
                            return true;
                        }
                    }
                }
                return false;
            }

            public bool IsMatch
            {
                get
                {
                    if (this.QualityValue == 0)
                    {
                        return false;
                    }
                    if (this.MatchingTypeNamePartCount < 0)
                    {
                        return false;
                    }
                    if (((this.MatchingTypeNamePartCount > 1) && (this.MatchingParameterCount != -1)) && (this.MatchingParameterCount < this.SourceTypeParameterCountForMatching))
                    {
                        return false;
                    }
                    return true;
                }
            }

            public int MatchingParameterCount { get; private set; }

            public int MatchingTypeNamePartCount { get; private set; }

            public int QualityValue { get; private set; }

            public int SourceTypeIndex
            {
                get
                {
                    return this.sourceIndex;
                }
            }

            public int SourceTypeParameterCountForMatching { get; private set; }

            public int TargetTypeIndex
            {
                get
                {
                    return this.targetIndex;
                }
            }
        }
    }
}

