namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal static class ReaderUtils
    {
        private static readonly ReadOnlyEnumerable<ODataAction> EmptyActionsList = new ReadOnlyEnumerable<ODataAction>();
        private static readonly ReadOnlyEnumerable<ODataAssociationLink> EmptyAssociationLinksList = new ReadOnlyEnumerable<ODataAssociationLink>();
        private static readonly ReadOnlyEnumerable<ODataFunction> EmptyFunctionsList = new ReadOnlyEnumerable<ODataFunction>();

        internal static void AddActionToEntry(ODataEntry entry, ODataAction action)
        {
            if (object.ReferenceEquals(entry.Actions, EmptyActionsList))
            {
                entry.Actions = new ReadOnlyEnumerable<ODataAction>();
            }
            GetSourceListOfEnumerable<ODataAction>(entry.Actions, "Actions").Add(action);
        }

        internal static void AddAssociationLinkToEntry(ODataEntry entry, ODataAssociationLink associationLink)
        {
            if (object.ReferenceEquals(entry.AssociationLinks, EmptyAssociationLinksList))
            {
                entry.AssociationLinks = new ReadOnlyEnumerable<ODataAssociationLink>();
            }
            GetSourceListOfEnumerable<ODataAssociationLink>(entry.AssociationLinks, "AssociationLinks").Add(associationLink);
        }

        internal static void AddFunctionToEntry(ODataEntry entry, ODataFunction function)
        {
            if (object.ReferenceEquals(entry.Functions, EmptyFunctionsList))
            {
                entry.Functions = new ReadOnlyEnumerable<ODataFunction>();
            }
            GetSourceListOfEnumerable<ODataFunction>(entry.Functions, "Functions").Add(function);
        }

        internal static void AddPropertyToPropertiesList(IEnumerable<ODataProperty> properties, ODataProperty propertyToAdd)
        {
            GetPropertiesList(properties).Add(propertyToAdd);
        }

        internal static ODataEntry CreateNewEntry()
        {
            return new ODataEntry { Properties = new ReadOnlyEnumerable<ODataProperty>(), AssociationLinks = EmptyAssociationLinksList, Actions = EmptyActionsList, Functions = EmptyFunctionsList };
        }

        internal static List<ODataProperty> GetPropertiesList(IEnumerable<ODataProperty> properties)
        {
            return GetSourceListOfEnumerable<ODataProperty>(properties, "Properties");
        }

        internal static List<T> GetSourceListOfEnumerable<T>(IEnumerable<T> collection, string collectionName)
        {
            ReadOnlyEnumerable<T> enumerable = collection as ReadOnlyEnumerable<T>;
            if (enumerable == null)
            {
                throw new ODataException(Strings.ReaderUtils_EnumerableModified(collectionName));
            }
            return enumerable.SourceList;
        }

        internal static bool HasFlag(this ODataUndeclaredPropertyBehaviorKinds undeclaredPropertyBehaviorKinds, ODataUndeclaredPropertyBehaviorKinds flag)
        {
            return ((undeclaredPropertyBehaviorKinds & flag) == flag);
        }
    }
}

