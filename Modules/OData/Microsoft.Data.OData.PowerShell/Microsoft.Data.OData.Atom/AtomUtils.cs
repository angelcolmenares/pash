namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Runtime.InteropServices;

    internal static class AtomUtils
    {
        private const int MimeApplicationAtomXmlLength = 20;
        private const int MimeApplicationAtomXmlLengthWithSemicolon = 0x15;
        private const int MimeApplicationAtomXmlTypeEntryLength = 0x1f;
        private const string MimeApplicationAtomXmlTypeEntryParameter = ";type=entry";
        private const int MimeApplicationAtomXmlTypeFeedLength = 30;
        private const string MimeApplicationAtomXmlTypeFeedParameter = ";type=feed";

        internal static string ComputeODataAssociationLinkRelation(ODataAssociationLink associationLink)
        {
            return string.Join("/", new string[] { "http://schemas.microsoft.com/ado/2007/08/dataservices", "relatedlinks", associationLink.Name });
        }

        internal static string ComputeODataNavigationLinkRelation(ODataNavigationLink navigationLink)
        {
            return string.Join("/", new string[] { "http://schemas.microsoft.com/ado/2007/08/dataservices", "related", navigationLink.Name });
        }

        internal static string ComputeODataNavigationLinkType(ODataNavigationLink navigationLink)
        {
            if (!navigationLink.IsCollection.Value)
            {
                return "application/atom+xml;type=entry";
            }
            return "application/atom+xml;type=feed";
        }

        internal static string ComputeStreamPropertyRelation(ODataProperty streamProperty, bool forEditLink)
        {
            string str = forEditLink ? "edit-media" : "mediaresource";
            return string.Join("/", new string[] { "http://schemas.microsoft.com/ado/2007/08/dataservices", str, streamProperty.Name });
        }

        internal static string GetNameFromAtomLinkRelationAttribute(string relation, string namespacePrefix)
        {
            if ((relation != null) && relation.StartsWith(namespacePrefix, StringComparison.Ordinal))
            {
                return relation.Substring(namespacePrefix.Length);
            }
            return null;
        }

        internal static bool IsExactNavigationLinkTypeMatch(string navigationLinkType, out bool hasEntryType, out bool hasFeedType)
        {
            hasEntryType = false;
            hasFeedType = false;
            if (navigationLinkType.StartsWith("application/atom+xml", StringComparison.Ordinal))
            {
                int length = navigationLinkType.Length;
                switch (length)
                {
                    case 20:
                        return true;

                    case 0x15:
                        return (navigationLinkType[length - 1] == ';');

                    case 30:
                        hasFeedType = string.Compare(";type=feed", 0, navigationLinkType, 20, ";type=feed".Length, StringComparison.Ordinal) == 0;
                        return hasFeedType;

                    case 0x1f:
                        hasEntryType = string.Compare(";type=entry", 0, navigationLinkType, 20, ";type=entry".Length, StringComparison.Ordinal) == 0;
                        return hasEntryType;
                }
            }
            return false;
        }

        internal static string UnescapeAtomLinkRelationAttribute(string relation)
        {
            Uri uri;
            if ((!string.IsNullOrEmpty(relation) && Uri.TryCreate(relation, UriKind.RelativeOrAbsolute, out uri)) && uri.IsAbsoluteUri)
            {
                return uri.GetComponents(UriComponents.AbsoluteUri, UriFormat.SafeUnescaped);
            }
            return null;
        }
    }
}

