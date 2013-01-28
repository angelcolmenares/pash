namespace System.Data.Services.Client
{
    using System;

    internal class UriResolver
    {
        private readonly Uri baseUri;
        private readonly Uri baseUriWithSlash;
        private readonly Func<string, Uri> resolveEntitySet;

        private UriResolver(Uri baseUri, Func<string, Uri> resolveEntitySet)
        {
            this.baseUri = baseUri;
            this.resolveEntitySet = resolveEntitySet;
            if (this.baseUri != null)
            {
                this.baseUriWithSlash = Util.ForceSlashTerminatedUri(this.baseUri);
            }
        }

        internal UriResolver CloneWithOverrideValue(Func<string, Uri> overrideResolveEntitySetValue)
        {
            return new UriResolver(this.baseUri, overrideResolveEntitySetValue);
        }

        internal UriResolver CloneWithOverrideValue(Uri overrideBaseUriValue, string parameterName)
        {
            ConvertToAbsoluteAndValidateBaseUri(ref overrideBaseUriValue, parameterName);
            return new UriResolver(overrideBaseUriValue, this.resolveEntitySet);
        }

        private static void ConvertToAbsoluteAndValidateBaseUri(ref Uri baseUri, string parameterName)
        {
            baseUri = ConvertToAbsoluteUri(baseUri);
            if (!IsValidBaseUri(baseUri))
            {
                if (parameterName != null)
                {
                    throw Error.Argument(Strings.Context_BaseUri, parameterName);
                }
                throw Error.InvalidOperation(Strings.Context_BaseUri);
            }
        }

        private static Uri ConvertToAbsoluteUri(Uri baseUri)
        {
            if (baseUri == null)
            {
                return null;
            }
            return baseUri;
        }

        internal Uri CreateAbsoluteUriIfNeeded(Uri requestUri)
        {
            Util.CheckArgumentNull<Uri>(requestUri, "requestUri");
            if (!requestUri.IsAbsoluteUri)
            {
                requestUri = Util.AppendBaseUriAndRelativeUri(this.baseUri, requestUri);
            }
            return requestUri;
        }

        internal static UriResolver CreateFromBaseUri(Uri baseUri, string parameterName)
        {
            ConvertToAbsoluteAndValidateBaseUri(ref baseUri, parameterName);
            return new UriResolver(baseUri, null);
        }

        internal Uri GetBaseUriWithSlash()
        {
            return this.GetBaseUriWithSlash(() => Strings.Context_BaseUriRequired);
        }

        internal Uri GetBaseUriWithSlash(Func<string> getErrorMessage)
        {
            if (this.baseUriWithSlash == null)
            {
                throw Error.InvalidOperation(getErrorMessage());
            }
            return this.baseUriWithSlash;
        }

        internal Uri GetEntitySetUri(string entitySetName)
        {
            Uri entitySetUriFromResolver = this.GetEntitySetUriFromResolver(entitySetName);
            if (entitySetUriFromResolver != null)
            {
                return Util.ForceNonSlashTerminatedUri(entitySetUriFromResolver);
            }
            if (this.baseUriWithSlash == null)
            {
                throw Error.InvalidOperation(Strings.Context_ResolveEntitySetOrBaseUriRequired(entitySetName));
            }
            return Util.CreateUri(this.baseUriWithSlash, new Uri(entitySetName, UriKind.Relative));
        }

        private Uri GetEntitySetUriFromResolver(string entitySetName)
        {
            if (this.resolveEntitySet != null)
            {
                Uri baseUri = this.resolveEntitySet(entitySetName);
                if (baseUri != null)
                {
                    if (!IsValidBaseUri(baseUri))
                    {
                        throw Error.InvalidOperation(Strings.Context_ResolveReturnedInvalidUri);
                    }
                    return baseUri;
                }
            }
            return null;
        }

        internal Uri GetRawBaseUriValue()
        {
            return this.baseUri;
        }

        private static bool IsValidBaseUri(Uri baseUri)
        {
            return ((baseUri == null) || ((baseUri.IsAbsoluteUri && Uri.IsWellFormedUriString(CommonUtil.UriToString(baseUri), UriKind.Absolute)) && ((string.IsNullOrEmpty(baseUri.Query) && string.IsNullOrEmpty(baseUri.Fragment)) && (!(baseUri.Scheme != "http") || !(baseUri.Scheme != "https")))));
        }

        public Func<string, Uri> ResolveEntitySet
        {
            get
            {
                return this.resolveEntitySet;
            }
        }
    }
}

