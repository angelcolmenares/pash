namespace System.Management.Automation
{
    using System;

    internal class CimClassSerializationId : Tuple<string, string, string, int>
    {
        public CimClassSerializationId(string className, string namespaceName, string computerName, int hashCode) : base(className, namespaceName, computerName, hashCode)
        {
        }

        public int ClassHashCode
        {
            get
            {
                return base.Item4;
            }
        }

        public string ClassName
        {
            get
            {
                return base.Item1;
            }
        }

        public string ComputerName
        {
            get
            {
                return base.Item3;
            }
        }

        public string NamespaceName
        {
            get
            {
                return base.Item2;
            }
        }
    }
}

