namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;

    internal abstract class FormatInfoData
    {
        private string classId;
        private const int classIdLength = 0x20;
        internal const string classidProperty = "ClassId2e4f51ef21dd47e99d3c952918aff9cd";

        protected FormatInfoData(string classId)
        {
            this.classId = classId;
        }

        internal virtual void Deserialize(PSObject so, FormatObjectDeserializer deserializer)
        {
        }

        public string ClassId2e4f51ef21dd47e99d3c952918aff9cd
        {
            get
            {
                return this.classId;
            }
        }
    }
}

