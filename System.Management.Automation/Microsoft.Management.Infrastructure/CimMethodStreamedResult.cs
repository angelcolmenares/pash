namespace Microsoft.Management.Infrastructure
{
    using System;
    using System.Runtime.CompilerServices;

    public class CimMethodStreamedResult : CimMethodResultBase
    {
        internal CimMethodStreamedResult(string parameterName, object parameterValue, CimType parameterType)
        {
            this.ParameterName = parameterName;
            this.ItemValue = parameterValue;
            this.ItemType = parameterType;
        }

        public CimType ItemType { get; private set; }

        public object ItemValue { get; private set; }

        public string ParameterName { get; private set; }
    }
}

