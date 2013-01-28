namespace System.Management.Automation
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate void TypeSerializerDelegate(InternalSerializer serializer, string streamName, string property, object source, TypeSerializationInfo entry);
}

