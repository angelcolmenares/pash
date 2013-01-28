namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class DuplicatePropertyNamesChecker
    {
        private readonly bool allowDuplicateProperties;
        private readonly bool isResponse;
        private Dictionary<string, DuplicationRecord> propertyNameCache;

        public DuplicatePropertyNamesChecker(bool allowDuplicateProperties, bool isResponse)
        {
            this.allowDuplicateProperties = allowDuplicateProperties;
            this.isResponse = isResponse;
        }

        private static void ApplyNavigationLinkToDuplicationRecord(DuplicationRecord duplicationRecord, bool isExpanded, bool? isCollection)
        {
            duplicationRecord.NavigationLinkFound = true;
            duplicationRecord.NavigationPropertyIsCollection = GetIsCollectionEffectiveValue(isExpanded, isCollection);
        }

        internal void CheckForDuplicatePropertyNames(ODataAssociationLink associationLink)
        {
            DuplicationRecord record;
            string name = associationLink.Name;
            if (!this.TryGetDuplicationRecord(name, out record))
            {
                DuplicationRecord record2 = new DuplicationRecord(DuplicationKind.NavigationProperty) {
                    AssociationLinkFound = true
                };
                this.propertyNameCache.Add(name, record2);
            }
            else
            {
                if ((record.DuplicationKind != DuplicationKind.NavigationProperty) || record.AssociationLinkFound)
                {
                    throw new ODataException(Strings.DuplicatePropertyNamesChecker_DuplicatePropertyNamesNotAllowed(name));
                }
                record.AssociationLinkFound = true;
            }
        }

        internal void CheckForDuplicatePropertyNames(ODataProperty property)
        {
            DuplicationRecord record;
            string name = property.Name;
            DuplicationKind duplicationKind = GetDuplicationKind(property);
            if (!this.TryGetDuplicationRecord(name, out record))
            {
                this.propertyNameCache.Add(name, new DuplicationRecord(duplicationKind));
            }
            else if ((((record.DuplicationKind == DuplicationKind.Prohibited) || (duplicationKind == DuplicationKind.Prohibited)) || ((record.DuplicationKind == DuplicationKind.NavigationProperty) && record.AssociationLinkFound)) || !this.allowDuplicateProperties)
            {
                throw new ODataException(Strings.DuplicatePropertyNamesChecker_DuplicatePropertyNamesNotAllowed(name));
            }
        }

        internal void CheckForDuplicatePropertyNames(ODataNavigationLink navigationLink, bool isExpanded, bool? isCollection)
        {
            DuplicationRecord record;
            string name = navigationLink.Name;
            if (!this.TryGetDuplicationRecord(name, out record))
            {
                DuplicationRecord duplicationRecord = new DuplicationRecord(DuplicationKind.NavigationProperty);
                ApplyNavigationLinkToDuplicationRecord(duplicationRecord, isExpanded, isCollection);
                this.propertyNameCache.Add(name, duplicationRecord);
            }
            else
            {
                this.CheckNavigationLinkDuplicateNameForExistingDuplicationRecord(name, record);
                if (((record.DuplicationKind == DuplicationKind.NavigationProperty) && record.AssociationLinkFound) && !record.NavigationLinkFound)
                {
                    ApplyNavigationLinkToDuplicationRecord(record, isExpanded, isCollection);
                }
                else if (this.allowDuplicateProperties)
                {
                    record.DuplicationKind = DuplicationKind.NavigationProperty;
                    ApplyNavigationLinkToDuplicationRecord(record, isExpanded, isCollection);
                }
                else
                {
                    bool? isCollectionEffectiveValue = GetIsCollectionEffectiveValue(isExpanded, isCollection);
                    if ((isCollectionEffectiveValue == false) || (record.NavigationPropertyIsCollection == false))
                    {
                        throw new ODataException(Strings.DuplicatePropertyNamesChecker_MultipleLinksForSingleton(name));
                    }
                    if (isCollectionEffectiveValue.HasValue)
                    {
                        record.NavigationPropertyIsCollection = isCollectionEffectiveValue;
                    }
                }
            }
        }

        internal void CheckForDuplicatePropertyNamesOnNavigationLinkStart(ODataNavigationLink navigationLink)
        {
            DuplicationRecord record;
            string name = navigationLink.Name;
            if ((this.propertyNameCache != null) && this.propertyNameCache.TryGetValue(name, out record))
            {
                this.CheckNavigationLinkDuplicateNameForExistingDuplicationRecord(name, record);
            }
        }

        private void CheckNavigationLinkDuplicateNameForExistingDuplicationRecord(string propertyName, DuplicationRecord existingDuplicationRecord)
        {
            if ((((existingDuplicationRecord.DuplicationKind != DuplicationKind.NavigationProperty) || !existingDuplicationRecord.AssociationLinkFound) || existingDuplicationRecord.NavigationLinkFound) && (((existingDuplicationRecord.DuplicationKind == DuplicationKind.Prohibited) || ((existingDuplicationRecord.DuplicationKind == DuplicationKind.PotentiallyAllowed) && !this.allowDuplicateProperties)) || (((existingDuplicationRecord.DuplicationKind == DuplicationKind.NavigationProperty) && this.isResponse) && !this.allowDuplicateProperties)))
            {
                throw new ODataException(Strings.DuplicatePropertyNamesChecker_DuplicatePropertyNamesNotAllowed(propertyName));
            }
        }

        internal void Clear()
        {
            if (this.propertyNameCache != null)
            {
                this.propertyNameCache.Clear();
            }
        }

        private static DuplicationKind GetDuplicationKind(ODataProperty property)
        {
            object obj2 = property.Value;
            if ((obj2 != null) && ((obj2 is ODataStreamReferenceValue) || (obj2 is ODataCollectionValue)))
            {
                return DuplicationKind.Prohibited;
            }
            return DuplicationKind.PotentiallyAllowed;
        }

        private static bool? GetIsCollectionEffectiveValue(bool isExpanded, bool? isCollection)
        {
            if (isExpanded)
            {
                return isCollection;
            }
            if (isCollection != true)
            {
                return null;
            }
            return true;
        }

        private bool TryGetDuplicationRecord(string propertyName, out DuplicationRecord duplicationRecord)
        {
            if (this.propertyNameCache == null)
            {
                this.propertyNameCache = new Dictionary<string, DuplicationRecord>(EqualityComparer<string>.Default);
                duplicationRecord = null;
                return false;
            }
            return this.propertyNameCache.TryGetValue(propertyName, out duplicationRecord);
        }

        private enum DuplicationKind
        {
            Prohibited,
            PotentiallyAllowed,
            NavigationProperty
        }

        private sealed class DuplicationRecord
        {
            public DuplicationRecord(Microsoft.Data.OData.DuplicatePropertyNamesChecker.DuplicationKind duplicationKind)
            {
                this.DuplicationKind = duplicationKind;
            }

            public bool AssociationLinkFound { get; set; }

            public Microsoft.Data.OData.DuplicatePropertyNamesChecker.DuplicationKind DuplicationKind { get; set; }

            public bool NavigationLinkFound { get; set; }

            public bool? NavigationPropertyIsCollection { get; set; }
        }
    }
}

