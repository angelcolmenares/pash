namespace System.Data.Services.Serializers
{
    using Microsoft.Data.OData;
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct TextSerializer
    {
        private readonly ODataMessageWriter writer;
        internal TextSerializer(ODataMessageWriter messageWriter)
        {
            this.writer = messageWriter;
        }

        internal void WriteRequest(object content)
        {
            object primitiveValue = Serializer.GetPrimitiveValue(content);
            this.writer.WriteValue(primitiveValue);
        }
    }
}

