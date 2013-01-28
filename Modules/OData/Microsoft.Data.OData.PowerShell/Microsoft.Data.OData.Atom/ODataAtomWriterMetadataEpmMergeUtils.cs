namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal static class ODataAtomWriterMetadataEpmMergeUtils
    {
        private static AtomTextConstruct MergeAtomTextValue(AtomTextConstruct customValue, AtomTextConstruct epmValue, string propertyName)
        {
            AtomTextConstruct construct;
            if (TryMergeIfNull<AtomTextConstruct>(customValue, epmValue, out construct))
            {
                return construct;
            }
            if (customValue.Kind != epmValue.Kind)
            {
                throw new ODataException(Strings.ODataAtomMetadataEpmMerge_TextKindConflict(propertyName, customValue.Kind.ToString(), epmValue.Kind.ToString()));
            }
            if (string.CompareOrdinal(customValue.Text, epmValue.Text) != 0)
            {
                throw new ODataException(Strings.ODataAtomMetadataEpmMerge_TextValueConflict(propertyName, customValue.Text, epmValue.Text));
            }
            return epmValue;
        }

        internal static AtomEntryMetadata MergeCustomAndEpmEntryMetadata(AtomEntryMetadata customEntryMetadata, AtomEntryMetadata epmEntryMetadata, ODataWriterBehavior writerBehavior)
        {
            AtomEntryMetadata metadata;
            if ((customEntryMetadata != null) && (writerBehavior.FormatBehaviorKind == ODataBehaviorKind.WcfDataServicesClient))
            {
                if (customEntryMetadata.Updated.HasValue)
                {
                    customEntryMetadata.UpdatedString = ODataAtomConvert.ToAtomString(customEntryMetadata.Updated.Value);
                }
                if (customEntryMetadata.Published.HasValue)
                {
                    customEntryMetadata.PublishedString = ODataAtomConvert.ToAtomString(customEntryMetadata.Published.Value);
                }
            }
            if (TryMergeIfNull<AtomEntryMetadata>(customEntryMetadata, epmEntryMetadata, out metadata))
            {
                return metadata;
            }
            epmEntryMetadata.Title = MergeAtomTextValue(customEntryMetadata.Title, epmEntryMetadata.Title, "Title");
            epmEntryMetadata.Summary = MergeAtomTextValue(customEntryMetadata.Summary, epmEntryMetadata.Summary, "Summary");
            epmEntryMetadata.Rights = MergeAtomTextValue(customEntryMetadata.Rights, epmEntryMetadata.Rights, "Rights");
            if (writerBehavior.FormatBehaviorKind == ODataBehaviorKind.WcfDataServicesClient)
            {
                epmEntryMetadata.PublishedString = MergeTextValue(customEntryMetadata.PublishedString, epmEntryMetadata.PublishedString, "PublishedString");
                epmEntryMetadata.UpdatedString = MergeTextValue(customEntryMetadata.UpdatedString, epmEntryMetadata.UpdatedString, "UpdatedString");
            }
            else
            {
                epmEntryMetadata.Published = MergeDateTimeValue(customEntryMetadata.Published, epmEntryMetadata.Published, "Published");
                epmEntryMetadata.Updated = MergeDateTimeValue(customEntryMetadata.Updated, epmEntryMetadata.Updated, "Updated");
            }
            epmEntryMetadata.Authors = MergeSyndicationMapping<AtomPersonMetadata>(customEntryMetadata.Authors, epmEntryMetadata.Authors);
            epmEntryMetadata.Contributors = MergeSyndicationMapping<AtomPersonMetadata>(customEntryMetadata.Contributors, epmEntryMetadata.Contributors);
            epmEntryMetadata.Categories = MergeSyndicationMapping<AtomCategoryMetadata>(customEntryMetadata.Categories, epmEntryMetadata.Categories);
            epmEntryMetadata.Links = MergeSyndicationMapping<AtomLinkMetadata>(customEntryMetadata.Links, epmEntryMetadata.Links);
            epmEntryMetadata.Source = customEntryMetadata.Source;
            return epmEntryMetadata;
        }

        private static DateTimeOffset? MergeDateTimeValue(DateTimeOffset? customValue, DateTimeOffset? epmValue, string propertyName)
        {
            DateTimeOffset? nullable;
            if (TryMergeIfNull<DateTimeOffset>(customValue, epmValue, out nullable))
            {
                return nullable;
            }
            if (customValue != epmValue)
            {
                throw new ODataException(Strings.ODataAtomMetadataEpmMerge_TextValueConflict(propertyName, customValue.ToString(), epmValue.ToString()));
            }
            return epmValue;
        }

        private static IEnumerable<T> MergeSyndicationMapping<T>(IEnumerable<T> customValues, IEnumerable<T> epmValues)
        {
            IEnumerable<T> enumerable;
            if (TryMergeIfNull<IEnumerable<T>>(customValues, epmValues, out enumerable))
            {
                return enumerable;
            }
            List<T> list = (List<T>) epmValues;
            foreach (T local in customValues)
            {
                list.Add(local);
            }
            return list;
        }

        private static string MergeTextValue(string customValue, string epmValue, string propertyName)
        {
            string str;
            if (TryMergeIfNull<string>(customValue, epmValue, out str))
            {
                return str;
            }
            if (string.CompareOrdinal(customValue, epmValue) != 0)
            {
                throw new ODataException(Strings.ODataAtomMetadataEpmMerge_TextValueConflict(propertyName, customValue, epmValue));
            }
            return epmValue;
        }

        private static bool TryMergeIfNull<T>(T? customValue, T? epmValue, out T? result) where T: struct
        {
            if (!customValue.HasValue)
            {
                result = epmValue;
                return true;
            }
            if (!epmValue.HasValue)
            {
                result = customValue;
                return true;
            }
            result = null;
            return false;
        }

        private static bool TryMergeIfNull<T>(T customValue, T epmValue, out T result) where T: class
        {
            if (customValue == null)
            {
                result = epmValue;
                return true;
            }
            if (epmValue == null)
            {
                result = customValue;
                return true;
            }
            result = default(T);
            return false;
        }
    }
}

