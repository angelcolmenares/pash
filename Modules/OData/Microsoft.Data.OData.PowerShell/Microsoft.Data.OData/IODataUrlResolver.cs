namespace Microsoft.Data.OData
{
    using System;

    internal interface IODataUrlResolver
    {
        Uri ResolveUrl(Uri baseUri, Uri payloadUri);
    }
}

