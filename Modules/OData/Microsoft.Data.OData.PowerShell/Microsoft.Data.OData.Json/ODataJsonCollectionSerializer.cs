namespace Microsoft.Data.OData.Json
{
    using Microsoft.Data.OData;
    using System;

    internal sealed class ODataJsonCollectionSerializer : ODataJsonPropertyAndValueSerializer
    {
        internal ODataJsonCollectionSerializer(ODataJsonOutputContext jsonOutputContext) : base(jsonOutputContext)
        {
        }

        internal void WriteCollectionEnd()
        {
            base.JsonWriter.EndArrayScope();
            if (base.WritingResponse && (base.Version >= ODataVersion.V2))
            {
                base.JsonWriter.EndObjectScope();
            }
        }

        internal void WriteCollectionStart()
        {
            if (base.WritingResponse && (base.Version >= ODataVersion.V2))
            {
                base.JsonWriter.StartObjectScope();
                base.JsonWriter.WriteDataArrayName();
            }
            base.JsonWriter.StartArrayScope();
        }
    }
}

