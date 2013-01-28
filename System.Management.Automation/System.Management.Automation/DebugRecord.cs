namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class DebugRecord : InformationalRecord
    {
        public DebugRecord(PSObject record) : base(record)
        {
        }

        public DebugRecord(string message) : base(message)
        {
        }
    }
}

