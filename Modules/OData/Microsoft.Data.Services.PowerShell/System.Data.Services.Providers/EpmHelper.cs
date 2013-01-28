namespace System.Data.Services.Providers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Services;
    using System.Data.Services.Common;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal static class EpmHelper
    {
        internal static IEnumerable<EpmPropertyInformation> GetEpmInformationFromProperty(EdmMember edmMember)
        {
            return GetEpmPropertyInformation(edmMember, edmMember.DeclaringType.Name, edmMember.Name);
        }

        internal static IEnumerable<EpmPropertyInformation> GetEpmInformationFromType(StructuralType structuralType)
        {
            return GetEpmPropertyInformation(structuralType, structuralType.Name, null);
        }

        private static IEnumerable<EpmPropertyInformation> GetEpmPropertyInformation(MetadataItem metadataItem, string typeName, string memberName)
        {
            EpmAttributeNameBuilder iteratorVariable0 = new EpmAttributeNameBuilder();
            while (true)
            {
                string iteratorVariable6;
                bool iteratorVariable1 = true;
                MetadataProperty iteratorVariable2 = EdmUtil.FindExtendedProperty(metadataItem, iteratorVariable0.EpmTargetPath);
                if (iteratorVariable2 == null)
                {
                    break;
                }
                bool result = true;
                MetadataProperty iteratorVariable4 = EdmUtil.FindExtendedProperty(metadataItem, iteratorVariable0.EpmKeepInContent);
                if ((iteratorVariable4 != null) && !bool.TryParse(Convert.ToString(iteratorVariable4.Value, CultureInfo.InvariantCulture), out result))
                {
                    throw new InvalidOperationException((memberName == null) ? Strings.ObjectContext_InvalidValueForEpmPropertyType(iteratorVariable0.EpmKeepInContent, typeName) : Strings.ObjectContext_InvalidValueForEpmPropertyMember(iteratorVariable0.EpmKeepInContent, memberName, typeName));
                }
                MetadataProperty iteratorVariable5 = EdmUtil.FindExtendedProperty(metadataItem, iteratorVariable0.EpmSourcePath);
                if (iteratorVariable5 == null)
                {
                    if (memberName == null)
                    {
                        throw new InvalidOperationException(Strings.ObjectContext_MissingExtendedAttributeType(iteratorVariable0.EpmSourcePath, typeName));
                    }
                    iteratorVariable1 = false;
                    iteratorVariable6 = memberName;
                }
                else
                {
                    iteratorVariable6 = Convert.ToString(iteratorVariable5.Value, CultureInfo.InvariantCulture);
                }
                string targetPath = Convert.ToString(iteratorVariable2.Value, CultureInfo.InvariantCulture);
                SyndicationItemProperty iteratorVariable8 = EpmTranslate.MapEpmTargetPathToSyndicationProperty(targetPath);
                MetadataProperty iteratorVariable9 = EdmUtil.FindExtendedProperty(metadataItem, iteratorVariable0.EpmContentKind);
                MetadataProperty iteratorVariable10 = EdmUtil.FindExtendedProperty(metadataItem, iteratorVariable0.EpmNsPrefix);
                MetadataProperty iteratorVariable11 = EdmUtil.FindExtendedProperty(metadataItem, iteratorVariable0.EpmNsUri);
                if ((iteratorVariable9 != null) && ((iteratorVariable10 != null) || (iteratorVariable11 != null)))
                {
                    string str = (iteratorVariable10 != null) ? iteratorVariable0.EpmNsPrefix : iteratorVariable0.EpmNsUri;
                    throw new InvalidOperationException((memberName == null) ? Strings.ObjectContext_InvalidAttributeForNonSyndicationItemsType(str, typeName) : Strings.ObjectContext_InvalidAttributeForNonSyndicationItemsMember(str, memberName, typeName));
                }
                if (((iteratorVariable10 != null) || (iteratorVariable11 != null)) || (iteratorVariable8 == SyndicationItemProperty.CustomProperty))
                {
                    string iteratorVariable12 = (iteratorVariable10 != null) ? Convert.ToString(iteratorVariable10.Value, CultureInfo.InvariantCulture) : null;
                    string iteratorVariable13 = (iteratorVariable11 != null) ? Convert.ToString(iteratorVariable11.Value, CultureInfo.InvariantCulture) : null;
                    EpmPropertyInformation iteratorVariable14 = new EpmPropertyInformation {
                        IsAtom = false,
                        KeepInContent = result,
                        SourcePath = iteratorVariable6,
                        PathGiven = iteratorVariable1,
                        TargetPath = targetPath,
                        NsPrefix = iteratorVariable12,
                        NsUri = iteratorVariable13
                    };
                    yield return iteratorVariable14;
                }
                else
                {
                    SyndicationTextContentKind plaintext;
                    if (iteratorVariable9 != null)
                    {
                        plaintext = EpmTranslate.MapEpmContentKindToSyndicationTextContentKind(Convert.ToString(iteratorVariable9.Value, CultureInfo.InvariantCulture), typeName, memberName);
                    }
                    else
                    {
                        plaintext = SyndicationTextContentKind.Plaintext;
                    }
                    EpmPropertyInformation iteratorVariable16 = new EpmPropertyInformation {
                        IsAtom = true,
                        KeepInContent = result,
                        SourcePath = iteratorVariable6,
                        PathGiven = iteratorVariable1,
                        SyndicationItem = iteratorVariable8,
                        ContentKind = plaintext
                    };
                    yield return iteratorVariable16;
                }
                iteratorVariable0.MoveNext();
            }
        }

        

        internal sealed class EpmPropertyInformation
        {
            internal SyndicationTextContentKind ContentKind { get; set; }

            internal bool IsAtom { get; set; }

            internal bool KeepInContent { get; set; }

            internal string NsPrefix { get; set; }

            internal string NsUri { get; set; }

            internal bool PathGiven { get; set; }

            internal string SourcePath { get; set; }

            internal SyndicationItemProperty SyndicationItem { get; set; }

            internal string TargetPath { get; set; }
        }
    }
}

