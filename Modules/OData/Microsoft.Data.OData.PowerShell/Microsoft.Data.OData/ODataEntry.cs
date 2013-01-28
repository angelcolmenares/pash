namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("Id: {Id} TypeName: {TypeName}")]
    internal sealed class ODataEntry : ODataItem
    {
        public IEnumerable<ODataAction> Actions { get; set; }

        public IEnumerable<ODataAssociationLink> AssociationLinks { get; set; }

        public Uri EditLink { get; set; }

        public string ETag { get; set; }

        public IEnumerable<ODataFunction> Functions { get; set; }

        public string Id { get; set; }

        public ODataStreamReferenceValue MediaResource { get; set; }

        public IEnumerable<ODataProperty> Properties { get; set; }

        public Uri ReadLink { get; set; }

        public string TypeName { get; set; }
    }
}

