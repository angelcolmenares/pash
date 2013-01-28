namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;

    internal static class ODataAtomWriterMetadataUtils
    {
        internal static AtomCategoryMetadata MergeCategoryMetadata(AtomCategoryMetadata categoryMetadata, string term, string scheme)
        {
            AtomCategoryMetadata metadata = new AtomCategoryMetadata(categoryMetadata);
            string strB = metadata.Term;
            if (strB != null)
            {
                if (string.CompareOrdinal(term, strB) != 0)
                {
                    throw new ODataException(Strings.ODataAtomWriterMetadataUtils_CategoryTermsMustMatch(term, strB));
                }
            }
            else
            {
                metadata.Term = term;
            }
            string str2 = metadata.Scheme;
            if (str2 == null)
            {
                metadata.Scheme = scheme;
                return metadata;
            }
            if (string.CompareOrdinal(scheme, str2) != 0)
            {
                throw new ODataException(Strings.ODataAtomWriterMetadataUtils_CategorySchemesMustMatch(scheme, str2));
            }
            return metadata;
        }

        internal static AtomLinkMetadata MergeLinkMetadata(AtomLinkMetadata metadata, string relation, Uri href, string title, string mediaType)
        {
            AtomLinkMetadata metadata2 = new AtomLinkMetadata(metadata);
            string strB = metadata2.Relation;
            if (strB != null)
            {
                if (string.CompareOrdinal(relation, strB) != 0)
                {
                    throw new ODataException(Strings.ODataAtomWriterMetadataUtils_LinkRelationsMustMatch(relation, strB));
                }
            }
            else
            {
                metadata2.Relation = relation;
            }
            if (href != null)
            {
                Uri uri = metadata2.Href;
                if (uri != null)
                {
                    if (!href.Equals(uri))
                    {
                        throw new ODataException(Strings.ODataAtomWriterMetadataUtils_LinkHrefsMustMatch(href, uri));
                    }
                }
                else
                {
                    metadata2.Href = href;
                }
            }
            if (title != null)
            {
                string str2 = metadata2.Title;
                if (str2 != null)
                {
                    if (string.CompareOrdinal(title, str2) != 0)
                    {
                        throw new ODataException(Strings.ODataAtomWriterMetadataUtils_LinkTitlesMustMatch(title, str2));
                    }
                }
                else
                {
                    metadata2.Title = title;
                }
            }
            if (mediaType != null)
            {
                string str3 = metadata2.MediaType;
                if (str3 == null)
                {
                    metadata2.MediaType = mediaType;
                    return metadata2;
                }
                if (!HttpUtils.CompareMediaTypeNames(mediaType, str3))
                {
                    throw new ODataException(Strings.ODataAtomWriterMetadataUtils_LinkMediaTypesMustMatch(mediaType, str3));
                }
            }
            return metadata2;
        }
    }
}

