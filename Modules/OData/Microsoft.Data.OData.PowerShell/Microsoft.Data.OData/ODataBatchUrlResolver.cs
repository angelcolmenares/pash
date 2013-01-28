namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;

    internal sealed class ODataBatchUrlResolver : IODataUrlResolver
    {
        private readonly IODataUrlResolver batchMessageUrlResolver;
        private HashSet<string> contentIdCache;

        internal ODataBatchUrlResolver(IODataUrlResolver batchMessageUrlResolver)
        {
            this.batchMessageUrlResolver = batchMessageUrlResolver;
        }

        internal void AddContentId(string contentId)
        {
            if (this.contentIdCache == null)
            {
                this.contentIdCache = new HashSet<string>(StringComparer.Ordinal);
            }
            this.contentIdCache.Add(contentId);
        }

        internal bool ContainsContentId(string contentId)
        {
            if (this.contentIdCache == null)
            {
                return false;
            }
            return this.contentIdCache.Contains(contentId);
        }

        Uri IODataUrlResolver.ResolveUrl(Uri baseUri, Uri payloadUri)
        {
            ExceptionUtils.CheckArgumentNotNull<Uri>(payloadUri, "payloadUri");
            if ((this.contentIdCache != null) && !payloadUri.IsAbsoluteUri)
            {
                string str = UriUtilsCommon.UriToString(payloadUri);
                if ((str.Length > 0) && (str[0] == '$'))
                {
                    string str2;
                    int index = str.IndexOf('/', 1);
                    if (index > 0)
                    {
                        str2 = str.Substring(1, index - 1);
                    }
                    else
                    {
                        str2 = str.Substring(1);
                    }
                    if (this.contentIdCache.Contains(str2))
                    {
                        return payloadUri;
                    }
                }
            }
            if (this.batchMessageUrlResolver != null)
            {
                return this.batchMessageUrlResolver.ResolveUrl(baseUri, payloadUri);
            }
            return null;
        }

        internal void Reset()
        {
            if (this.contentIdCache != null)
            {
                this.contentIdCache.Clear();
            }
        }

        internal IODataUrlResolver BatchMessageUrlResolver
        {
            get
            {
                return this.batchMessageUrlResolver;
            }
        }
    }
}

