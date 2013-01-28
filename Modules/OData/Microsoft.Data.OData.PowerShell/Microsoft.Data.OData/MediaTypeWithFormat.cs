namespace Microsoft.Data.OData
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class MediaTypeWithFormat
    {
        public ODataFormat Format { get; set; }

        public Microsoft.Data.OData.MediaType MediaType { get; set; }
    }
}

