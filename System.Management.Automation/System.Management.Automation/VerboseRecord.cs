namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class VerboseRecord : InformationalRecord
    {
        public VerboseRecord(PSObject record) : base(record)
        {
        }

        public VerboseRecord(string message) : base(message)
        {
        }
    }
}

