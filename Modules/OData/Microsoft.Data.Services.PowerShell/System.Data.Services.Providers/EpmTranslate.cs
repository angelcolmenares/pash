namespace System.Data.Services.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Data.Services.Common;

    internal static class EpmTranslate
    {
        private static readonly string[] syndicationItemToTargetPath = new string[] { string.Empty, "SyndicationAuthorEmail", "SyndicationAuthorName", "SyndicationAuthorUri", "SyndicationContributorEmail", "SyndicationContributorName", "SyndicationContributorUri", "SyndicationUpdated", "SyndicationPublished", "SyndicationRights", "SyndicationSummary", "SyndicationTitle" };
        private static readonly Dictionary<string, SyndicationItemProperty> targetPathToSyndicationItem = new Dictionary<string, SyndicationItemProperty>(EqualityComparer<string>.Default);

        static EpmTranslate()
        {
            foreach (object obj2 in typeof(SyndicationItemProperty).GetEnumValues())
            {
                targetPathToSyndicationItem.Add(syndicationItemToTargetPath[(int) obj2], (SyndicationItemProperty) obj2);
            }
        }

        internal static SyndicationTextContentKind MapEpmContentKindToSyndicationTextContentKind(string strContentKind, string typeName, string memberName)
        {
            switch (strContentKind)
            {
                case "text":
                    return SyndicationTextContentKind.Plaintext;

                case "html":
                    return SyndicationTextContentKind.Html;

                case "xhtml":
                    return SyndicationTextContentKind.Xhtml;
            }
            throw new InvalidOperationException((memberName == null) ? Strings.ObjectContext_InvalidValueForTargetTextContentKindPropertyType(strContentKind, typeName) : Strings.ObjectContext_InvalidValueForTargetTextContentKindPropertyMember(strContentKind, memberName, typeName));
        }

        internal static SyndicationItemProperty MapEpmTargetPathToSyndicationProperty(string targetPath)
        {
            SyndicationItemProperty property;
            if (targetPathToSyndicationItem.TryGetValue(targetPath, out property))
            {
                return property;
            }
            return SyndicationItemProperty.CustomProperty;
        }

        internal static string MapSyndicationPropertyToEpmTargetPath(SyndicationItemProperty property)
        {
            return syndicationItemToTargetPath[(int) property];
        }

        internal static string MapSyndicationTextContentKindToEpmContentKind(SyndicationTextContentKind contentKind)
        {
            switch (contentKind)
            {
                case SyndicationTextContentKind.Plaintext:
                    return "text";

                case SyndicationTextContentKind.Html:
                    return "html";
            }
            return "xhtml";
        }
    }
}

