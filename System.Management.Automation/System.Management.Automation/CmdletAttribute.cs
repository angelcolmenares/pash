namespace System.Management.Automation
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CmdletAttribute : CmdletCommonMetadataAttribute
    {
        private string nounName;
        private string verbName;

        public CmdletAttribute(string verbName, string nounName)
        {
            if ((nounName == null) || (nounName.Length == 0))
            {
                throw PSTraceSource.NewArgumentException("nounName");
            }
            if ((verbName == null) || (verbName.Length == 0))
            {
                throw PSTraceSource.NewArgumentException("verbName");
            }
            this.nounName = nounName;
            this.verbName = verbName;
        }

        public string NounName
        {
            get
            {
                return this.nounName;
            }
        }

        public string VerbName
        {
            get
            {
                return this.verbName;
            }
        }
    }
}

